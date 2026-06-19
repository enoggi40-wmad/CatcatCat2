using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip jumpSound;

    [Header("Animation")]
    public Animator playerAnimator;

    [Header("Jump Gauge")]
    public GameObject jumpGaugeObject;
    public Slider jumpGaugeSlider;

    private float jumpGaugeTimer = 0.0f;

    public static float ACCELERATION = 10.0f; // 가속도
    public static float SPEED_MIN = 4.0f;     // 속도의 최솟값
    public static float SPEED_MAX = 8.0f;     // 속도의 최댓값

    // [점프 설정]
    public static float JUMP_SPEED = 5.0f;          // 점프 중 위로 올라가는 속도
    public static float JUMP_TIME_MAX = 0.35f;      // 마우스를 꾹 누르고 있을 때 최대 상승 시간
    public static float JUMP_RELEASE_REDUCE = 0.2f; // 현재는 사용 안 함
    public static float JUMP_LAND_IGNORE_TIME = 0.15f; // 점프 직후 착지 판정을 무시하는 시간

    // [슬라이드 설정]
    public static float SLIDE_DOWN_SPEED = 12.0f; // 슬라이드 시 아래로 내려가는 속도

    // [발판 옆면에 닿았을 때 떨어지는 속도]
    [Header("SIDE_FALL")]
    public static float SIDE_FALL_SPEED = 12.0f; // 옆면에 붙었을 때 아래로 떨어지게 하는 속도

    [Header("Movement")]
    public float current_speed = 0.0f;        // 현재 속도
    public LevelControl level_control = null; // LevelControl 저장

    // [에어 슬라이딩]
    public bool isAirSliding = false;

    private FailManager failManager;

    public enum STEP
    {
        NONE = -1,
        RUN = 0,
        JUMP,
        MISS,
        NUM,
    };

    public STEP step = STEP.NONE;      // 현재 상태
    public STEP next_step = STEP.NONE; // 다음 상태
    public float step_timer = 0.0f;     // 현재 상태가 시작된 뒤 지난 시간

    private bool is_landed = false;       // 착지했는가
    private bool is_colided = false;      // 뭔가와 충돌했는가
    private bool is_key_released = false; // 버튼이 떨어졌는가

    public static float NARAKU_HEIGHT = -5.0f; // 이 높이보다 아래로 떨어지면 MISS

    void Start()
    {
        this.next_step = STEP.RUN;

        failManager = FindObjectOfType<FailManager>();

        if (jumpGaugeObject != null)
        {
            jumpGaugeObject.SetActive(false);
        }

        if (jumpGaugeSlider != null)
        {
            jumpGaugeSlider.value = 0.0f;
        }
    }

    private void check_landed()
    {
        this.is_landed = false;

        do
        {
            Vector3 s = this.transform.position;
            Vector3 e = s + Vector3.down * 0.55f;

            RaycastHit hit;

            // 아래 방향으로 선을 쏴서 바닥이 있는지 확인
            // Trigger는 착지 판정에서 무시
            if (!Physics.Linecast(s, e, out hit, ~0, QueryTriggerInteraction.Ignore))
            {
                break;
            }

            // 장애물은 바닥으로 인정하지 않음
            if (hit.collider.CompareTag("Obstacle"))
            {
                break;
            }

            // 벽이나 옆면은 바닥으로 인정하지 않음
            if (hit.normal.y < 0.7f)
            {
                break;
            }

            // 점프 직후에는 바로 착지 판정이 날 수 있으므로 잠깐 무시
            if (this.step == STEP.JUMP)
            {
                if (this.step_timer < PlayerControl.JUMP_LAND_IGNORE_TIME)
                {
                    break;
                }
            }

            this.is_landed = true;

        } while (false);
    }

    // 발판 옆면에 닿았을 때 아래로 떨어지게 함
    private void OnCollisionStay(Collision collision)
    {
        // MISS 상태에서는 따로 처리하지 않음
        if (this.step == STEP.MISS)
        {
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            // 바닥 윗면이면 normal.y가 1에 가까움
            // 발판 옆면이면 normal.y가 0에 가까움
            if (contact.normal.y < 0.3f)
            {
                Rigidbody rb = this.GetComponent<Rigidbody>();
                Vector3 v = rb.velocity;

                // 옆면에 닿았을 때 아래로 떨어지도록 y속도 부여
                if (v.y > -PlayerControl.SIDE_FALL_SPEED)
                {
                    v.y = -PlayerControl.SIDE_FALL_SPEED;
                }

                rb.velocity = v;

                Debug.Log("발판 옆면 접촉 → 아래로 떨어지게 처리");

                break;
            }
        }
    }

    private void Update()
    {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        Vector3 velocity = rb.velocity;

        // 현재 목표 속도 가져오기
        this.current_speed = this.level_control.getPlayerSpeed();

        // 착지 확인
        this.check_landed();
        
        // 현재 상태 유지 시간 증가
        this.step_timer += Time.deltaTime;

        // 낭떠러지 아래로 떨어지면 MISS 상태로 전환
        if (this.transform.position.y < NARAKU_HEIGHT && this.step != STEP.MISS)
        {
            this.next_step = STEP.MISS;

            if (failManager != null)
            {
                failManager.GoToFailScene();
            }
        }

        // 상태별 기본 처리
        switch (this.step)
        {
            case STEP.RUN:
                // 달리는 중에는 x속도를 항상 목표 속도로 고정
                velocity.x = this.current_speed;

                // RUN 상태에서는 S키 하강 처리하지 않음
                // 에어 슬라이드는 JUMP 상태에서만 처리
                break;

            case STEP.JUMP:
                // 점프 중에도 x속도를 항상 목표 속도로 고정
                velocity.x = this.current_speed;

                // 점프 중에는 게이지 갱신
                if (Input.GetKey(KeyCode.Space) && this.step_timer < PlayerControl.JUMP_TIME_MAX)
                {
                    UpdateJumpGauge();
                }

                // 스페이스바를 떼거나 최대 점프 시간이 끝나면 게이지 숨김
                if (Input.GetKeyUp(KeyCode.Space) || this.step_timer >= PlayerControl.JUMP_TIME_MAX)
                {
                    HideJumpGauge();
                }

                // 공중에서 S / 아래 방향키를 한 번 누르면 에어 슬라이드 시작
                if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !this.isAirSliding)
                {
                    this.isAirSliding = true;

                    if (playerAnimator != null)
                    {
                        playerAnimator.SetTrigger("Slide");
                    }
                }

                // 에어 슬라이드가 시작되면 키를 떼도 착지 전까지 계속 아래로 내려감
                if (this.isAirSliding)
                {
                    velocity.y = -PlayerControl.SLIDE_DOWN_SPEED;
                }
                else
                {
                    // 스페이스바를 누르고 있고, 최대 점프 시간이 지나지 않았으면 계속 상승
                    if (Input.GetKey(KeyCode.Space) && this.step_timer < PlayerControl.JUMP_TIME_MAX)
                    {
                        velocity.y = PlayerControl.JUMP_SPEED;
                    }
                }
                break;

            case STEP.MISS:
                // 실패 상태에서는 점점 감속
                velocity.x -= PlayerControl.ACCELERATION * Time.deltaTime;

                if (velocity.x < 0.0f)
                {
                    velocity.x = 0.0f;
                }

                break;

         
        }

        // 상태 전이 판정
        if (this.next_step == STEP.NONE)
        {
            switch (this.step)
            {
                case STEP.RUN:
                    // 땅에 닿아 있을 때 좌클릭을 누르면 바로 점프 시작
                    if (Input.GetKeyDown(KeyCode.Space) && this.is_landed)
                    {
                        this.next_step = STEP.JUMP;

                        ShowJumpGauge();

                        if (audioSource != null && jumpSound != null)
                        {
                            audioSource.PlayOneShot(jumpSound, 0.4f);
                        }

                        if (playerAnimator != null)
                        {
                            playerAnimator.SetTrigger("Jump");
                        }     
                    }
                    break;

                case STEP.JUMP:
                    // 점프 후 다시 땅에 닿으면 RUN 상태로 복귀
                    if (this.is_landed && this.step_timer > PlayerControl.JUMP_LAND_IGNORE_TIME)
                    {
                        this.next_step = STEP.RUN;
                    }
                    break;

                case STEP.MISS:
                    break;
            }
        }

        // 상태 변경 처리
        while (this.next_step != STEP.NONE)
        {
            this.step = this.next_step;
            this.next_step = STEP.NONE;

            switch (this.step)
            {
                case STEP.JUMP:
                    // 좌클릭 누르는 순간 바로 점프 시작
                    velocity.y = PlayerControl.JUMP_SPEED;

                    // 점프 직후 S키를 이미 누르고 있으면 바로 에어 슬라이드
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    {
                        velocity.y = -PlayerControl.SLIDE_DOWN_SPEED;
                        this.isAirSliding = true;
                    }

                    this.is_key_released = false;
                    break;

                case STEP.RUN:
                    HideJumpGauge();
                    this.isAirSliding = false;
                    this.is_key_released = false;
                    break;

                case STEP.MISS:
                    this.is_key_released = true;
                    break;
            }

            // 상태가 바뀌었으므로 타이머 초기화
            this.step_timer = 0.0f;
        }

        // 최종 속도 적용
        rb.velocity = velocity;
        

    }

    private void ShowJumpGauge()
    {
        jumpGaugeTimer = 0.0f;

        if (jumpGaugeObject != null)
        {
            jumpGaugeObject.SetActive(true);
        }

        if (jumpGaugeSlider != null)
        {
            jumpGaugeSlider.value = 0.0f;
        }
    }

    private void HideJumpGauge()
    {
        jumpGaugeTimer = 0.0f;

        if (jumpGaugeSlider != null)
        {
            jumpGaugeSlider.value = 0.0f;
        }

        if (jumpGaugeObject != null)
        {
            jumpGaugeObject.SetActive(false);
        }
    }

    private void UpdateJumpGauge()
    {
        if (jumpGaugeSlider == null)
        {
            return;
        }

        jumpGaugeTimer += Time.deltaTime;

        float ratio = jumpGaugeTimer / JUMP_TIME_MAX;
        ratio = Mathf.Clamp01(ratio);

        jumpGaugeSlider.value = ratio;
    }
}
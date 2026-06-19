using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Damage Sound")]
    public AudioClip damageSound;
    public float damageSoundVolume = 1.0f;

    [Header("Obstacle Destroy Sound")]
    public AudioClip obstacleDestroySound;
    public float obstacleDestroySoundVolume = 0.5f;

    [Header("HP")]
    public float maxHP = 100.0f;       // 최대 체력
    public float currentHP = 100.0f;   // 현재 체력
    public Slider healthBar;           // 체력바 UI

    [Header("Damage")]
    public float obstacleDamage = 20.0f; // 장애물에 닿았을 때 데미지

    [Header("Invincible Material")]
    public Renderer playerRenderer;       // 플레이어 Renderer
    public Material normalMaterial;       // 평소 Material
    public Material invincibleMaterial;   // 무적 상태 Material

    private Material materialBeforeInvincible;

    private bool isDead = false;          // 죽었는지 여부
    private bool isInvincible = false;    // 무적 상태 여부

    [Header("Scene")]
    private FailManager failManager;

    [Header("Invincible UI")]
    public GameObject invincibleTextObject;

    public void SetMaterialAfterInvincible(Material newMaterial)
    {
        if (newMaterial == null)
        {
            return;
        }

        // 무적이 끝났을 때 돌아갈 Material을 고양이 색으로 갱신
        materialBeforeInvincible = newMaterial;

        // 무적 상태가 아니라면 바로 색 변경
        if (!isInvincible && playerRenderer != null)
        {
            playerRenderer.material = newMaterial;
        }
    }


    private void Start()
    {
        currentHP = maxHP;
       
        failManager = FindObjectOfType<FailManager>();

        if (failManager == null)
        {
            Debug.LogWarning("[PlayerHealth] FailManager를 찾지 못함");
        }
        else
        {
            Debug.Log("[PlayerHealth] FailManager 찾음");
        }

        // 평소 Material을 따로 안 넣었다면 현재 Material을 자동 저장
        if (playerRenderer != null && normalMaterial == null)
        {
            normalMaterial = playerRenderer.material;
        }

        if (invincibleTextObject != null)
        {
            invincibleTextObject.SetActive(false);
        }

        UpdateHealthBar();
    }

    public void HitObstacle(GameObject obstacle)
    {
        if (obstacle == null)
        {
            return;
        }

        PlayerControl playerControl = GetComponent<PlayerControl>();

        bool isAirSliding = false;

        if (playerControl != null)
        {
            isAirSliding = playerControl.isAirSliding;
        }

        // 무적 상태이거나 공중 슬라이드 상태면 장애물 제거
        if (isInvincible || isAirSliding)
        {
            if (obstacleDestroySound != null)
            {
                AudioSource.PlayClipAtPoint(
                    obstacleDestroySound,
                    obstacle.transform.position,
                    obstacleDestroySoundVolume
                );
            }

            Destroy(obstacle);
            Debug.Log("무적/공중슬라이드 상태로 장애물 파괴!");
            return;
        }

        TakeDamage(obstacleDamage);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        // 무적 상태면 데미지 무시
        if (isInvincible)
        {
            Debug.Log("무적 상태라 데미지 무시!");
            return;
        }

        currentHP -= damage;

        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(
                damageSound,
                this.transform.position,
                damageSoundVolume
            );
        }

        if (currentHP <= 0.0f)
        {
            isDead = true;
            Debug.Log("[PlayerHealth] 게임 오버 조건 도달");

            if (failManager != null)
            {
                Debug.Log("[PlayerHealth] Fail Scene 이동 시도");
                failManager.GoToFailScene();
            }
            else
            {
                Debug.LogWarning("[PlayerHealth] failManager가 null이라 씬 이동 못함");
            }
        }

        UpdateHealthBar();

        Debug.Log("플레이어 체력: " + currentHP);

        
    }

    public void Heal(float amount)
    {
        if (isDead)
        {
            return;
        }

        currentHP += amount;

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        UpdateHealthBar();

        Debug.Log("체력 회복: " + amount + " / 현재 체력: " + currentHP);
    }

    public void StartInvincible(float duration)
    {
        // 이미 무적이면 또 실행하지 않음
        if (isInvincible)
        {
            return;
        }

        StartCoroutine(InvincibleCoroutine(duration));
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        isInvincible = true;
        Debug.Log("무적 시작!");

        // 무적 텍스트 켜기
        if (invincibleTextObject != null)
        {
            invincibleTextObject.SetActive(true);
        }

        // 무적 시작 직전의 Material 저장
        if (playerRenderer != null)
        {
            materialBeforeInvincible = playerRenderer.material;
        }

        // 무적 Material로 변경
        if (playerRenderer != null && invincibleMaterial != null)
        {
            playerRenderer.material = invincibleMaterial;
        }

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        Debug.Log("무적 종료!");

        // 원래 기본 Material이 아니라, 무적 직전 Material로 복구
        if (playerRenderer != null && materialBeforeInvincible != null)
        {
            playerRenderer.material = materialBeforeInvincible;
        }

        // 무적 텍스트 끄기
        if (invincibleTextObject != null)
        {
            invincibleTextObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHP;
        }
    }
}
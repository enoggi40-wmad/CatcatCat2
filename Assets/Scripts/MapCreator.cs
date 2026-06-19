using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Block 클래스 추가
public class Block
{
    // 블록의 종류를 나타내는 열거체
    public enum TYPE
    {
        NONE = -1, // 없음
        FLOOR = 0, // 마루
        HOLE, // 구멍
        NUM, // 블록이 몇 종류인지(＝2)
    };
};

public class MapCreator : MonoBehaviour
{
    public static float BLOCK_WIDTH = 1.0f;    // 블록의 폭
    public static float BLOCK_HEIGHT = 0.5f;   // 블록의 높이
    public static int BLOCK_NUM_IN_SCREEN = 24;// 화면 내에 들어가는 블록의 개수

    // 블록에 관한 정보를 모아서 관리하는 구조체
    // (여러 개의 정보를 하나로 묶을 때 사용)
    private struct FloorBlock
    {
        public bool is_created; // 블록이 만들어졌는가
        public Vector3 position; // 블록의 위치
    };
    private FloorBlock last_block; // 마지막에 생성한 블록
    private PlayerControl player = null;// scene상의 Player를 보관
    private BlockCreator block_creator; // BlockCreator를 보관

    private LevelControl level_control = null;
    private float play_timer = 0.0f;

    [Header("UI")]
    public TMP_Text levelText; // Level / BreakTime 표시 텍스트

    [Header("Goal")]
    public GameObject goalTriggerPrefab;
    private bool goalCreated = false;

    // 맴버 변수 부분에 아래 추가
    public TextAsset level_data_text = null;
    private GameRoot game_root = null;

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player")
        .GetComponent<PlayerControl>();
        this.last_block.is_created = false;
        this.block_creator = this.gameObject.GetComponent<BlockCreator>();

        this.level_control = new LevelControl();
        this.level_control.initialize();

        this.level_control.loadLevelData(this.level_data_text);

        this.game_root = this.gameObject.GetComponent<GameRoot>(); // 끝에 추가

        this.player.level_control = this.level_control;
    }

    private void create_floor_block()
    {
        Vector3 block_position; // 이제부터 만들 블록의 위치
        if (!this.last_block.is_created)
        { // last_block이 생성되지 않은 경우
          // 블록의 위치를 일단 Player와 같게
            block_position = this.player.transform.position;
            // 그러고 나서 블록의 X 위치를 화면 절반만큼 왼쪽으로 이동
            block_position.x -= BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 2.0f);
            // 블록의 Y위치는 0으로
            block_position.y = 0.0f;
        }
        else
        { // last_block이 생성된 경우
          // 이번에 만들 블록의 위치를 직전에 만든 블록과 같게
            block_position = this.last_block.position;
        }

        this.last_block.position = block_position; // last_block의 위치를 갱신
        this.last_block.is_created = true; // 블록이 생성됨

        block_position.x += BLOCK_WIDTH; // 블록을 1블럭만큼 오른쪽으로 이동
        //this.level_control.update(); // LevelControl을 갱신
        this.level_control.update(this.play_timer);

        // 지금 만들 블록 정보의height(높이)를 scene 상의 좌표로 변환
        block_position.y = level_control.current_block.height * BLOCK_HEIGHT;
        // 지금 만들 블록에 관한 정보를 변수 current에 넣음
        LevelControl.CreationInfo current = this.level_control.current_block;

        // 지금 만들 블록이 바닥이면
        if (current.block_type == Block.TYPE.FLOOR)
        {
            // 바닥 블록 생성
            this.block_creator.createBlock(block_position);

            // Level 5 끝 시간 이후 GoalTrigger를 한 번만 생성
            if (!goalCreated && this.play_timer >= this.level_control.getLevelEndTime(5))
            {
                if (goalTriggerPrefab != null)
                {
                    Vector3 goalPosition = block_position;
                    goalPosition.y += 1.0f;

                    GameObject goal = Instantiate(goalTriggerPrefab);
                    goal.transform.position = goalPosition;

                    goalCreated = true;

                    Debug.Log("Level 5 끝 지점에 GoalTrigger 생성");
                }
                else
                {
                    Debug.LogWarning("GoalTrigger Prefab이 연결되지 않음");
                }
            }

            // FLOOR 구간의 첫 번째 칸이면 긴 투명 Collider 생성
            if (current.current_count == 1)
            {
                this.block_creator.createFloorCollider(block_position, current.max_count);
            }

            // 현재 구간이 BreakTime인지 확인
            int currentLevel = this.level_control.getLevelFromTime(this.play_timer);
            bool isBreakTime = currentLevel == 3;

            // 바닥 위에 먹이 생성 시도
            this.block_creator.createFood(block_position, isBreakTime);

            // 바닥 위에 스킬포인트 생성 시도
            this.block_creator.createSkillPoint(block_position,isBreakTime);

            // 바닥 위에 공중 장애물 생성 시도
            this.block_creator.createObstacle(block_position,current,this.level_control.level, isBreakTime);
        }
        else if (current.block_type == Block.TYPE.HOLE)
        {
            // HOLE 구간의 중간 칸에서만 고양이 생성
            int middle = current.max_count / 2;

            if (current.current_count == middle)
            {
                this.block_creator.createCat(block_position);
            }
        }

        

        this.last_block.position = block_position;
        this.last_block.is_created = true;
    }

    void Update()
    {
        // 플레이 타임
        this.play_timer += Time.deltaTime;

        // 플레이어의 X위치를 가져옴
        float block_generate_x = this.player.transform.position.x;
        // 그리고 대략 반 화면만큼 오른쪽으로 이동
        // 이 위치가 블록을 생성하는 문턱 값
        block_generate_x += BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN + 1) / 2.0f;

        // 마지막에 만든 블록의 위치가 문턱 값보다 작으면
        while (this.last_block.position.x < block_generate_x)
        {
            // 블록을 만듬
            this.create_floor_block();
        }

        // Level / BreakTime 텍스트 표시
        if (this.levelText != null && this.level_control != null)
        {
            int currentLevel = this.level_control.getLevelFromTime(this.play_timer);

            if (currentLevel <= 2)
            {
                // index 0, 1, 2 → Level 1, 2, 3
                this.levelText.text = "Level: " + (currentLevel + 1);
            }
            else if (currentLevel == 3)
            {
                // index 3 → BreakTime
                this.levelText.text = "BreakTime";
            }
            else
            {
                // index 4, 5 → Level 4, 5
                // BreakTime 줄이 하나 끼어 있으므로 그대로 표시
                this.levelText.text = "Level " + currentLevel;
            }
        }
    }

    public bool isDelete(GameObject block_object)
    {
        bool ret = false; // 반환값
        float left_limit = this.player.transform.position.x - BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 2.0f); // 삭제 문턱 값
                                                                                                                 // 블록의 위치가 문턱 값보다 작으면(왼쪽),
        if (block_object.transform.position.x < left_limit)
        {
            ret = true; // 반환값을 true(사라져도 좋다)로
        }
        return (ret); // 판정 결과를 돌려줌
    }

    public LevelControl GetLevelControl()
    {
        return this.level_control;
    }

    public float GetPlayTime()
    {
        return this.game_root.getPlayTime();
    }
}

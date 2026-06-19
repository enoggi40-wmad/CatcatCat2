using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCreator : MonoBehaviour
{
    [Header("Block")]
    public GameObject[] blockPrefabs; // 블록을 저장할 배열
    private int block_count = 0; // 생성한 블록의 개수

    [Header("Floor Collider")]
    public float floorColliderXOffset = -0.5f;
    public float floorColliderYOffset = 0.0f;

    [Header("Food")]
    public GameObject foodPrefab;      // 먹이 프리팹
    public float foodRate = 0.11f;     // 먹이 생성 확률
    public float foodYOffset = 0.8f;   // 바닥 위 먹이 높이

    [Header("Skill Point Item")]
    public GameObject skillPointPrefab;
    public float skillPointRate = 0.35f;
    public float skillPointYOffset = 1.5f;
    public int skillPointInterval = 8;

    private int skillPoint_wait_count = 0;

    [Header("Cat")]
    public GameObject[] catPrefabs; // 프리팹 배열      // 고양이 프리팹
    public float catRate = 0.1f;     // 고양이 생성 확률
    public float catYOffset = 1.8f;   // 바닥 위 고양이 높이
    public int catInterval = 20;      // 고양이 사이 최소 간격

    private int cat_wait_count = 0;   // 고양이 연속 생성 방지용

    [Header("Obstacle")]
    public GameObject airObstaclePrefab;      // 공중 장애물
    public GameObject groundObstaclePrefab;   // 바닥 장애물

    public float obstacleRate = 0.15f;
    public int obstacleInterval = 4;

    public float airObstacleYOffsetMin = 1.6f;
    public float airObstacleYOffsetMax = 2.8f;

    public float groundObstacleYOffset = 0.5f;

    private int obstacle_wait_count = 0;

    public void createBlock(Vector3 block_position)
    {
        // 만들어야 할 블록의 종류(흰색인가 빨간색인가)를 구함
        int next_block_type = this.block_count % this.blockPrefabs.Length; // %: 나머지를 구하는 연산자

        // 블록을 생성하고 go에 보관
        GameObject go = GameObject.Instantiate(this.blockPrefabs[next_block_type]) as GameObject;
        go.transform.position = block_position; // 블록의 위치를 이동
        this.block_count++; // 블록의 개수를 증가
    }

    public void createFood(Vector3 block_position, bool isBreakTime)
    {
        // 먹이 프리팹이 없으면 생성하지 않음
        if (this.foodPrefab == null)
        {
            return;
        }

        // 초반 몇 칸은 먹이가 안 나오게 함
        if (this.block_count < 5)
        {
            return;
        }

        // 기본 먹이 확률
        float currentFoodRate = this.foodRate;

        // BreakTime이면 먹이 확률 증가
        if (isBreakTime)
        {
            currentFoodRate = 0.75f;
        }

        // 랜덤 확률 체크
        if (Random.value > currentFoodRate)
        {
            return;
        }

        Vector3 food_position = block_position;

        // BreakTime이면 먹이 위치를 조금 더 다양하게 배치
        if (isBreakTime)
        {
            food_position.y += Random.Range(0.8f, 1.5f);
        }
        else
        {
            food_position.y += this.foodYOffset;
        }

        GameObject food = GameObject.Instantiate(this.foodPrefab) as GameObject;
        food.transform.position = food_position;
    }

    public void createSkillPoint(Vector3 block_position, bool isBreakTime)
    {
        if (this.skillPointPrefab == null)
        {
            return;
        }

        if (this.block_count < 15)
        {
            return;
        }

        if (this.skillPoint_wait_count > 0)
        {
            this.skillPoint_wait_count--;
            return;
        }

        float currentSkillPointRate = this.skillPointRate;
        int currentSkillPointInterval = this.skillPointInterval;

        // BreakTime에는 스킬포인트가 더 잘 나오게 함
        if (isBreakTime)
        {
            currentSkillPointRate = 0.7f;
            currentSkillPointInterval = 2;
        }

        if (Random.value > currentSkillPointRate)
        {
            return;
        }

        Vector3 skill_position = block_position;
        skill_position.y += this.skillPointYOffset;

        GameObject skillPoint = Instantiate(this.skillPointPrefab);
        skillPoint.transform.position = skill_position;

        this.skillPoint_wait_count = currentSkillPointInterval;

        Debug.Log("스킬포인트 생성! BreakTime: " + isBreakTime);
    }

    public void createCat(Vector3 block_position)
    {
        // 고양이 프리팹 배열이 없거나 비어 있으면 생성하지 않음
        if (this.catPrefabs == null || this.catPrefabs.Length <= 0)
        {
            return;
        }

        // 초반에는 고양이 안 나오게 함
        if (this.block_count < 15)
        {
            return;
        }

        // 고양이가 너무 자주 나오지 않게 대기
        if (this.cat_wait_count > 0)
        {
            this.cat_wait_count--;
            return;
        }

        // 랜덤 확률 체크
        if (Random.value > this.catRate)
        {
            return;
        }

        Vector3 cat_position = block_position;

        // 바닥 위에 고양이 배치
        cat_position.y += this.catYOffset;

        // 여러 고양이 프리팹 중 하나를 랜덤 선택
        int catType = Random.Range(0, this.catPrefabs.Length);

        GameObject cat = GameObject.Instantiate(this.catPrefabs[catType]) as GameObject;
        cat.transform.position = cat_position;

        // 다음 고양이까지 간격 두기
        this.cat_wait_count = this.catInterval;
    }

    public void createObstacle(Vector3 block_position, LevelControl.CreationInfo current, int level, bool isBreakTime)
    {
        if (this.block_count < 30)
        {
            return;
        }

        if (this.obstacle_wait_count > 0)
        {
            this.obstacle_wait_count--;
            return;
        }

        // 레벨이 올라갈수록 장애물 생성 확률 증가
        float currentObstacleRate = this.obstacleRate + level * 0.05f;

        // 확률이 너무 높아지지 않도록 제한
        currentObstacleRate = Mathf.Clamp(currentObstacleRate, 0.0f, 0.5f);

        // BreakTime이면 장애물 생성 안 함
        if (isBreakTime)
        {
            currentObstacleRate = 0.0f;
        }

        if (Random.value > currentObstacleRate)
        {
            return;
        }

        Vector3 obstacle_position = block_position;

        int obstacleType = Random.Range(0, 2);

        if (obstacleType == 0)
        {
            // 바닥 장애물
            if (this.groundObstaclePrefab == null)
            {
                return;
            }

            // 발판 끝 부분에는 장애물 생성 안 함
            if (current.current_count >= current.max_count - 1)
            {
                return;
            }

            // 발판 시작 부분에는 장애물 생성 안 함
            if (current.current_count <= 1)
            {
                return;
            }

            obstacle_position.y += this.groundObstacleYOffset;

            GameObject obstacle = Instantiate(this.groundObstaclePrefab);
            obstacle.transform.position = obstacle_position;
        }
        else
        {
            // 공중 장애물
            if (this.airObstaclePrefab == null)
            {
                return;
            }

            // 발판 시작 부분에는 장애물 생성 안 함
            if (current.current_count <= 1)
            {
                return;
            }

            float randomYOffset = Random.Range(
                this.airObstacleYOffsetMin,
                this.airObstacleYOffsetMax
            );

            obstacle_position.y += randomYOffset;

            GameObject obstacle = Instantiate(this.airObstaclePrefab);
            obstacle.transform.position = obstacle_position;
        }

        // 레벨이 올라갈수록 장애물 간격 감소
        int currentInterval = Mathf.Max(1, this.obstacleInterval - level);

        // BreakTime이면 장애물 간격을 더 넓게 함
        if (isBreakTime)
        {
            currentInterval += 3;
        }

        this.obstacle_wait_count = currentInterval;
    }

    public void createFloorCollider(Vector3 first_block_position, int floor_count)
    {
        GameObject floorColliderObject = new GameObject("FloorCollider");

        Vector3 collider_position = first_block_position;

        // FLOOR 구간의 가운데로 이동
        collider_position.x += MapCreator.BLOCK_WIDTH * (floor_count - 1) / 2.0f;

        // 위치 보정값
        collider_position.x += this.floorColliderXOffset;
        collider_position.y += this.floorColliderYOffset;

        floorColliderObject.transform.position = collider_position;

        BoxCollider box = floorColliderObject.AddComponent<BoxCollider>();

        box.size = new Vector3(
            MapCreator.BLOCK_WIDTH * floor_count,
            MapCreator.BLOCK_HEIGHT * 2,
            1.0f
        );

        floorColliderObject.AddComponent<DeleteControl>();
    }
}

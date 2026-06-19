using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// LevelControlclass 앞에 추가
public class LevelData
{
    public struct Range
    { // 범위를 표현하는 구조체
        public int min; // 범위의 최솟값
        public int max; // 범위의 최댓값
    };

    public float end_time; // 종료 시간
    public float player_speed; // 플레이어의 속도

    public Range floor_count; // 발판 블록 수의 범위
    public Range hole_count; // 구멍의 개수 범위
    public Range height_diff; // 발판의 높이 범위

    public LevelData()
    {
        this.end_time = 15.0f; // 종료 시간 초기화
        this.player_speed = 6.0f; // 플레이어의 속도 초기화
        this.floor_count.min = 10;// 발판 블록 수의 최솟값을 초기화
        this.floor_count.max = 10;// 발판 블록 수의 최댓값을 초기화
        this.hole_count.min = 2; // 구멍 개수의 최솟값을 초기화
        this.hole_count.max = 6; // 구멍 개수의 최댓값을 초기화
        this.height_diff.min = 0; // 발판 높이 변화의 최솟값을 초기화
        this.height_diff.max = 0; // 발판 높이 변화의 최댓값을 초기화
    }
}
public class LevelControl
{
    private List<LevelData> level_datas = new List<LevelData>();
    public int HEIGHT_MAX = 20;
    public int HEIGHT_MIN = -4;

    // 만들어야 할 블록에 관한 정보를 모은 구조체
    public struct CreationInfo
    {
        public Block.TYPE block_type; // 블록의 종류
        public int max_count; // 블록의 최대 개수
        public int height; // 블록을 배치할 높이
        public int current_count; // 작성한 블록의 개수
    };

    public CreationInfo previous_block; // 이전에 어떤 블록을 만들었는가
    public CreationInfo current_block; // 지금 어떤 블록을 만들어야 하는가
    public CreationInfo next_block; // 다음에 어떤 블록을 만들어야 하는가
    public int block_count = 0; // 생성한 블록의 총 수
    public int level = 0; // 난이도

    // 프로필 노트에 실제로 기록
    private void clear_next_block(ref CreationInfo block)
    {
        // 전달받은 블록(block)을 초기화
        block.block_type = Block.TYPE.FLOOR;
        block.max_count = 15;
        block.height = 0;
        block.current_count = 0;
    }

    // 프로필 노트를 초기화
    public void initialize()
    {
        this.block_count = 0; // 블록의 총 수를 초기화

        // 이전, 현재, 다음 블록을 각각
        // clear_next_block()에 넘겨서 초기화
        this.clear_next_block(ref this.previous_block);
        this.clear_next_block(ref this.current_block);
        this.clear_next_block(ref this.next_block);
    }

    private void update_level(ref CreationInfo current, CreationInfo previous, float passage_time)
    {
        float local_time = Mathf.Repeat(passage_time, this.level_datas[this.level_datas.Count - 1].end_time); // Mathf.Repeat: 0~max 사이의 값을 반환
                                                                                                              // 현재 레벨을 구함
        int i;
        for (i = 0; i < this.level_datas.Count - 1; i++)
        {
            if (local_time <= this.level_datas[i].end_time)
            {
                break;
            }
        }
        this.level = i;
        current.block_type = Block.TYPE.FLOOR;
        current.max_count = 1;
        if (this.block_count >= 10)
        {
            // 현재 레벨용 레벨 데이터를 가져옴
            LevelData level_data;
            level_data = this.level_datas[this.level];

            switch (previous.block_type)
            {
                case Block.TYPE.FLOOR:
                    current.block_type = Block.TYPE.HOLE;
                    current.max_count = Random.Range(level_data.hole_count.min, level_data.hole_count.max);
                    current.height = previous.height;
                    break;
                case Block.TYPE.HOLE:
                    current.block_type = Block.TYPE.FLOOR;

                    // 바닥 길이의 최솟값~최댓값 사이의 임의의 값
                    current.max_count = Random.Range(level_data.floor_count.min, level_data.floor_count.max);
                    // 바닥 높이의 최솟값과 최댓값을 구함
                    int height_min = previous.height + level_data.height_diff.min;
                    int height_max = previous.height + level_data.height_diff.max;
                    height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX); // 최소와 최대값 사이를 강제로 지정
                    height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX); // Mathf.clamp: min~max 사이의 값을 반환
                                                                                  // 바닥 높이의 최솟값~최댓값 사이의 임의의 값
                    current.height = Random.Range(height_min, height_max);
                    break;
            }
        }
    }

    public void update(float passage_time)
    { // *Update()가 아님, create_floor_block() 메서드에서 호출
        this.current_block.current_count++;
        // 이번에 만든 블록 개수를 증가
        // 이번에 만든 블록 개수가 max_count 이상이면,
        if (this.current_block.current_count >= this.current_block.max_count)
        {
            this.previous_block = this.current_block;
            this.current_block = this.next_block;
            this.clear_next_block(ref this.next_block);
            this.update_level(ref this.next_block, this.current_block, passage_time);
        }
        this.block_count++;
    }

    // 아래 txt 파일로부터 데이터를 로딩하는 함수 추가
    public void loadLevelData(TextAsset level_data_text)
    {
        string level_texts = level_data_text.text;
        string[] lines = level_texts.Split('\n');
        // lines 내의 각 행에 대해서 차례로 처리해 가는 루프
        foreach (var line in lines)
        {
            if (line == "")
            {
                // 행이 빈 줄이면,
                // 텍스트 데이터를 문자열로 가져옴
                // 개행 코드 ＇\＇마다 분할해서 문자열 배열에 넣음
                // 아래 처리는 하지 않고 반복문의 처음으로 점프
                continue;
            }
            ;
            Debug.Log(line);
            string[] words = line.Split();
            int n = 0;
            // 행의 내용을 디버그 출력
            // 행 내의 워드를 배열에 저장
            // LevelData형 변수를 생성
            // 현재 처리하는 행의 데이터를 넣음
            LevelData level_data = new LevelData();
            // words내의 각 워드에 대해서 순서대로 처리해 가는 루프
            foreach (var word in words)
            {
                if (word.StartsWith("#"))
                { // 워드의 시작문자가 #이면
                    break;
                } // 루프 탈출
                if (word == "")
                { // 워드가 텅 비었으면
                    continue;
                } // 루프의 시작으로 점프
                  // n 값을 0, 1, 2,...7로 변화시켜 감으로써 8항목을 처리
                  // 각 워드를 플롯값으로 변환하고 level_data에 저장
                switch (n)
                {
                    case 0: level_data.end_time = float.Parse(word); break;
                    case 1: level_data.player_speed = float.Parse(word); break;
                    case 2: level_data.floor_count.min = int.Parse(word); break;
                    case 3: level_data.floor_count.max = int.Parse(word); break;
                    case 4: level_data.hole_count.min = int.Parse(word); break;
                    case 5: level_data.hole_count.max = int.Parse(word); break;
                    case 6: level_data.height_diff.min = int.Parse(word); break;
                    case 7: level_data.height_diff.max = int.Parse(word); break;
                }
                n++;
            }
            if (n >= 8)
            { // 8항목(이상)이 제대로 처리되었다면,
                this.level_datas.Add(level_data);
            }
            else
            { // 그렇지 않다면(오류의 가능성이 있음),
                if (n == 0)
                { // 1워드도 처리하지 않은 경우는 주석이므로
                  // 아무것도 하지 않음
                }
                else
                {
                    // List 구조의 level_datas에 level_data를 추가
                    // 그 이외이면 오류로 간주
                    // 데이터 개수가 맞지 않다는 것을 보여주는 오류 메시지를 표시
                    Debug.LogError("[LevelData] Out of parameter.\n");
                }
            }
        }
        if (this.level_datas.Count == 0)
        { // level_datas에 데이터가 하나도 없으면
            Debug.LogError("[LevelData] Has no data.\n"); // 오류 메시지를 표시
            this.level_datas.Add(new LevelData()); // level_datas에 기본 LevelData를 하나 추가
        }
    }

    public float getPlayerSpeed()
    {
        return (this.level_datas[this.level].player_speed);
    }

    public int getLevelFromTime(float passage_time)
    {
        if (this.level_datas.Count == 0)
        {
            return 0;
        }

        float local_time = Mathf.Repeat(
            passage_time,
            this.level_datas[this.level_datas.Count - 1].end_time
        );

        int i;

        for (i = 0; i < this.level_datas.Count - 1; i++)
        {
            if (local_time <= this.level_datas[i].end_time)
            {
                break;
            }
        }

        return i;
    }

    public float getLevelEndTime(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= this.level_datas.Count)
        {
            return 0.0f;
        }

        return this.level_datas[levelIndex].end_time;
    }
}

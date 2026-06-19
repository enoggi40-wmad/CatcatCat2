using UnityEngine;
using TMPro;

public class PointManager : MonoBehaviour
{
    [Header("Food Mission")]
    public int foodCount = 0;       // 미션용 먹이 개수
    public int foodGoal = 30;       // 목표 먹이 개수

    [Header("Skill Point")]
    public int skillPoint = 0;      // 스킬 사용용 재화

    [Header("UI")]
    public TMP_Text pointText;      // 화면에 표시할 Text

    private void Start()
    {
        UpdatePointText();
    }

    public void AddFood()
    {
        foodCount++;
        UpdatePointText();
    }

    public void AddFoodAmount(int amount)
    {
        foodCount += amount;

        UpdatePointText();

        Debug.Log("먹이 " + amount + "개 획득. 현재 먹이: " + foodCount);
    }

    public void AddSkillPoint(int amount)
    {
        skillPoint += amount;

        UpdatePointText();

        Debug.Log("스킬 포인트 " + amount + "개 획득. 현재 스킬 포인트: " + skillPoint);
    }

    public bool ConsumeSkillPoint(int amount)
    {
        if (skillPoint < amount)
        {
            Debug.Log("스킬 포인트가 부족함");
            return false;
        }

        skillPoint -= amount;

        UpdatePointText();

        Debug.Log("스킬 포인트 " + amount + "개 사용. 남은 스킬 포인트: " + skillPoint);

        return true;
    }

    public bool IsFoodMissionClear()
    {
        return foodCount >= foodGoal;
    }

    private void UpdatePointText()
    {
        if (pointText == null)
        {
            return;
        }

        pointText.text =
            "Food: " + foodCount + " / " + foodGoal +
            "\nSkillPoint: " + skillPoint;
    }
}
using UnityEngine;
using TMPro;

public class CatManager : MonoBehaviour
{
    public int rescuedCatCount = 0; // 구조한 고양이 수
    public int rescueGoal = 3;      // 목표 구조 수

    public TMP_Text catText;        // 고양이 구조 UI

    private bool missionClear = false;

    private void Start()
    {
        UpdateCatText();
    }

    public void AddRescuedCat()
    {
        rescuedCatCount++;

        Debug.Log("고양이 구조! 현재 구조 수: " + rescuedCatCount);

        //if (!missionClear && rescuedCatCount >= rescueGoal)
        //{
        //    missionClear = true;
        //    Debug.Log("고양이 구조 미션 완료!");
        //}

        UpdateCatText();
    }

    public bool IsMissionClear()
    {
        return rescuedCatCount >= rescueGoal;
    }

    private void UpdateCatText()
    {
        if (catText == null)
        {
            return;
        }

        if (missionClear)
        {
            catText.text = "Cat Rescue: " + rescuedCatCount + " / " + rescueGoal + "\nRescue Mission Completed!";
        }
        else
        {
            catText.text = "Cat Rescue: " + rescuedCatCount + " / " + rescueGoal;
        }
    }
}
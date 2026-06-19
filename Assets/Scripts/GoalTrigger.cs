using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalTrigger : MonoBehaviour
{
    [Header("Scene Names")]
    public string successSceneName = "SuccessScene";
    public string failSceneName = "FailScene";

    private PointManager pointManager;
    private CatManager catManager;

    private bool isTriggered = false;

    private void Start()
    {
        pointManager = FindObjectOfType<PointManager>();
        catManager = FindObjectOfType<CatManager>();

        if (pointManager == null)
        {
            Debug.LogWarning("[GoalTrigger] FoodManager를 찾지 못함");
        }

        if (catManager == null)
        {
            Debug.LogWarning("[GoalTrigger] CatManager를 찾지 못함");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        isTriggered = true;

        bool foodClear = false;
        bool catClear = false;

        if (pointManager != null)
        {
            foodClear = pointManager.IsFoodMissionClear();
        }

        if (catManager != null)
        {
            catClear = catManager.IsMissionClear();
        }

        Debug.Log("먹이 미션 성공 여부: " + foodClear);
        Debug.Log("고양이 미션 성공 여부: " + catClear);

        if (foodClear && catClear)
        {
            Debug.Log("모든 미션 성공! SuccessScene 이동");
            SceneManager.LoadScene(successSceneName);
        }
        else
        {
            Debug.Log("미션 실패! FailScene 이동");
            SceneManager.LoadScene(failSceneName);
        }
    }
}
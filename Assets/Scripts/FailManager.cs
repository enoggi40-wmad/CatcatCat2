using UnityEngine;
using UnityEngine.SceneManagement;

public class FailManager : MonoBehaviour
{
    public string failSceneName = "Fail";

    private bool isFailed = false;

    public void GoToFailScene()
    {
        if (isFailed)
        {
            return;
        }

        isFailed = true;

        Time.timeScale = 1.0f;
        SceneManager.LoadScene(failSceneName);
    }
}
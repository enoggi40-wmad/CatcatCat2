using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FailSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainSceneName = "Start";

    [Header("Button Sound")]
    public AudioSource audioSource;
    public AudioClip restartButtonSound;
    public AudioClip exitButtonSound;
    public float restartDelay = 0.2f;
    public float exitDelay = 0.2f;

    private bool isRestarting = false;
    private bool isExiting = false;

    public void RestartGame()
    {
        if (isRestarting)
        {
            return;
        }

        isRestarting = true;
        StartCoroutine(RestartGameCoroutine());
    }

    private IEnumerator RestartGameCoroutine()
    {
        if (audioSource != null && restartButtonSound != null)
        {
            audioSource.PlayOneShot(restartButtonSound);
            yield return new WaitForSecondsRealtime(restartDelay);
        }

        Time.timeScale = 1.0f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void ExitGame()
    {
        if (isExiting)
        {
            return;
        }

        isExiting = true;
        StartCoroutine(ExitGameCoroutine());
    }

    private IEnumerator ExitGameCoroutine()
    {
        if (audioSource != null && exitButtonSound != null)
        {
            audioSource.PlayOneShot(exitButtonSound);
            yield return new WaitForSecondsRealtime(exitDelay);
        }

        Debug.Log("게임 종료");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
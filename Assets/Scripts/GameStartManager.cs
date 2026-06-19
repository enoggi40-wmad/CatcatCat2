using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    [Header("Scene")]
    public string mainSceneName = "Main";

    [Header("Button Sound")]
    public AudioSource audioSource;
    public AudioClip startButtonSound;
    public AudioClip quitButtonSound;
    public float sceneLoadDelay = 0.2f;
    public float quitDelay = 0.2f;

    private bool isStarting = false;
    private bool isQuitting = false;

    public void StartGame()
    {
        if (isStarting)
        {
            return;
        }

        isStarting = true;
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        if (audioSource != null && startButtonSound != null)
        {
            audioSource.PlayOneShot(startButtonSound);
            yield return new WaitForSecondsRealtime(sceneLoadDelay);
        }

        Time.timeScale = 1.0f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void QuitGame()
    {
        if (isQuitting)
        {
            return;
        }

        isQuitting = true;
        StartCoroutine(QuitGameCoroutine());
    }

    private IEnumerator QuitGameCoroutine()
    {
        if (audioSource != null && quitButtonSound != null)
        {
            audioSource.PlayOneShot(quitButtonSound);
            yield return new WaitForSecondsRealtime(quitDelay);
        }

        Debug.Log("게임 종료!");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
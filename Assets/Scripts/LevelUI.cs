using UnityEngine;
using TMPro;

public class LevelUI : MonoBehaviour
{
    public TMP_Text levelText; // 레벨 표시 Text

    private MapCreator mapCreator;


    private void Start()
    {
        mapCreator = FindObjectOfType<MapCreator>();
    }

    private void Update()
    {
        if (levelText == null)
        {
            return;
        }

        if (mapCreator == null)
        {
            mapCreator = FindObjectOfType<MapCreator>();
            return;
        }

        LevelControl levelControl = mapCreator.GetLevelControl();

        if (levelControl == null)
        {
            return;
        }

        float playTime = mapCreator.GetPlayTime();

        int levelIndex = levelControl.getLevelFromTime(playTime);

        // 코드상 레벨은 0부터 시작하니까 화면에는 +1
        levelText.text = "Level: " + (levelIndex + 1);
    }
}
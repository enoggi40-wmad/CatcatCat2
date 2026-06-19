using UnityEngine;

public class ObstacleHealth : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip destroySound;
    public float destroySoundVolume = 0.5f;

    public int maxHP = 2;
    private int currentHP;

    [Header("Reward")]
    public int rewardSkillPoint = 7; // 장애물 파괴 시 얻는 먹이 개수

    private PointManager pointManager;

    private void Start()
    {
        currentHP = maxHP;
        pointManager = FindObjectOfType<PointManager>();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log("장애물 HP: " + currentHP);

        if (currentHP <= 0)
        {
            GiveReward();
            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(
                    destroySound,
                    this.transform.position,
                    destroySoundVolume
                );
            }

            Destroy(this.gameObject);
        }
    }

    private void GiveReward()
    {
        if (pointManager != null)
        {
            pointManager.AddSkillPoint(rewardSkillPoint);
        }
        else
        {
            Debug.LogWarning("FoodManager를 찾지 못해서 먹이 보상을 지급하지 못함");
        }
    }
}
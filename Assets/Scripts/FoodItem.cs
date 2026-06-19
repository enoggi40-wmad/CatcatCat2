using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public int foodAmount = 1;

    [Header("Sound")]
    public AudioClip collectSound;
    public float collectSoundVolume = 1.0f;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        // Collider가 여러 개 닿아도 한 번만 처리
        if (isCollected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        isCollected = true;

        PointManager pointManager = FindObjectOfType<PointManager>();

        if (pointManager != null)
        {
            if (foodAmount <= 1)
            {
                pointManager.AddFood();
            }
            else
            {
                pointManager.AddFoodAmount(foodAmount);
            }
        }
        else
        {
            Debug.LogWarning("PointManager를 찾지 못함");
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(
                collectSound,
                this.transform.position,
                collectSoundVolume
            );
        }

        Destroy(this.gameObject);

        Debug.Log("Food 획득!");
    }
}
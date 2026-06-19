using UnityEngine;

public class PlayerBodyHitbox : MonoBehaviour
{
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth를 찾지 못함");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle"))
        {
            return;
        }

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.HitObstacle(other.gameObject);
    }
}
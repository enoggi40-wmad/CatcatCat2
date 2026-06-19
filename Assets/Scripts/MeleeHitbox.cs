using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 3;

    private bool hasHit = false;

    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
        {
            return;
        }

        if (!other.CompareTag("Obstacle"))
        {
            return;
        }

        ObstacleHealth obstacleHealth = other.GetComponent<ObstacleHealth>();

        if (obstacleHealth == null)
        {
            obstacleHealth = other.GetComponentInParent<ObstacleHealth>();
        }

        if (obstacleHealth != null)
        {
            obstacleHealth.TakeDamage(damage);
            hasHit = true;

            Debug.Log("ClawHitbox: 장애물 공격 성공!");
        }
        else
        {
            Debug.LogWarning("ClawHitbox: ObstacleHealth를 찾지 못함 - " + other.gameObject.name);
        }
    }
}
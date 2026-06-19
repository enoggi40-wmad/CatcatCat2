using UnityEngine;

public class NearMissZone : MonoBehaviour
{
    private bool used = false; // 한 장애물에서 무적이 여러 번 발동되는 것 방지

    private void OnTriggerEnter(Collider other)
    {
        if (used)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerControl playerControl = other.GetComponent<PlayerControl>();
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerControl == null || playerHealth == null)
        {
            return;
        }

        // 점프 중 슬라이드로 장애물 근처를 지나갔을 때만 발동
        if (playerControl.isAirSliding)
        {
            used = true;

            playerHealth.StartInvincible(1.0f);

            Debug.Log("아슬아슬 회피 성공! 1초 무적");
        }
    }
}
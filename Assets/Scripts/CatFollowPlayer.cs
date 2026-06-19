using UnityEngine;

public class CatFollowPlayer : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip captureSound;
    public float captureSoundVolume = 1.0f;

    public float followSpeed = 8.0f;
    public float captureCompleteDistance = 0.8f; // 이 거리 안에 들어오면 구조 완료

    private Transform targetPlayer;
    private bool isFollowing = false;
    private bool isCaptured = false; // 중복 구조 방지

    public void StartFollow(Transform player)
    {
        targetPlayer = player;
        isFollowing = true;
    }

    public void StopFollow()
    {
        isFollowing = false;
        targetPlayer = null;
    }

    private void Update()
    {
        if (!isFollowing || targetPlayer == null || isCaptured)
        {
            return;
        }

        // 거리 기준 구조 완료 판정
        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        if (distance <= captureCompleteDistance)
        {
            CompleteCapture();
            return;
        }

        // 플레이어 쪽으로 이동
        Vector3 direction = targetPlayer.position - transform.position;
        direction.Normalize();

        transform.position += direction * followSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFollowing || isCaptured)
        {
            return;
        }

        // Player 본체가 아니라 자식 Collider에 닿아도 찾을 수 있게 InParent 사용
        PlayerCatAbility playerCatAbility = other.GetComponentInParent<PlayerCatAbility>();

        if (playerCatAbility == null)
        {
            return;
        }

        // Trigger로 닿았을 때도 구조 완료
        targetPlayer = playerCatAbility.transform;
        CompleteCapture();
    }

    private void CompleteCapture()
    {
        if (isCaptured)
        {
            return;
        }

        isCaptured = true;
        isFollowing = false;

        if (targetPlayer == null)
        {
            Debug.LogWarning("targetPlayer가 없어서 고양이 구조 실패");
            return;
        }

        PlayerCatAbility playerCatAbility = targetPlayer.GetComponentInParent<PlayerCatAbility>();
        CatType catType = GetComponent<CatType>();

        if (playerCatAbility != null && catType != null)
        {
            playerCatAbility.SetCurrentCat(catType);
        }
        else
        {
            Debug.LogWarning("PlayerCatAbility 또는 CatType을 찾지 못함");
        }

        CatManager catManager = FindObjectOfType<CatManager>();

        if (catManager != null)
        {
            catManager.AddRescuedCat();
        }

        if (captureSound != null)
        {
            AudioSource.PlayClipAtPoint(
                captureSound,
                this.transform.position,
                captureSoundVolume
            );
        }

        Debug.Log("고양이가 플레이어에게 도착해서 구조 완료!");

        Destroy(this.gameObject);
    }
}
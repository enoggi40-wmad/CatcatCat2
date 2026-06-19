using UnityEngine;

public class CatProjectile : MonoBehaviour
{
    public float speed = 12.0f;     // 투사체 속도
    public float lifeTime = 3.0f;   // 몇 초 뒤 자동 삭제
    public int damage = 3;          // 장애물에게 줄 데미지

    private void Start()
    {
        Destroy(this.gameObject, lifeTime);
    }

    private void Update()
    {
        // 오른쪽 방향으로 이동
        transform.position += Vector3.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 장애물이 아니면 무시
        if (!other.CompareTag("Obstacle"))
        {
            return;
        }

        // 장애물 체력 스크립트 가져오기
        ObstacleHealth obstacleHealth = other.GetComponent<ObstacleHealth>();

        if (obstacleHealth != null)
        {
            obstacleHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("장애물에 ObstacleHealth가 없음");
        }

        // 투사체는 한 번 맞으면 사라짐
        Destroy(this.gameObject);

        Debug.Log("투사체가 장애물에 명중!");
    }
}
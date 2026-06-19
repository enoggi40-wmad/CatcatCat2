using UnityEngine;

public class PlayerCatCapture : MonoBehaviour
{
    public float captureRange = 2.0f; // 포획 가능한 거리
    public LayerMask catLayer;        // 고양이 레이어

    private CatFollowPlayer currentFollowingCat = null;

    private void Update()
    {
        // 우클릭을 누르고 있는 동안 고양이를 끌어옴
        if (Input.GetKey(KeyCode.D))
        {
            TryAttractCat();
        }

        // 우클릭을 떼면 고양이 따라오기 중지
        if (Input.GetKeyUp(KeyCode.D))
        {
            StopAttractCat();
        }
    }

    private void TryAttractCat()
    {
        // 이미 따라오는 고양이가 있으면 계속 따라오게만 유지
        if (currentFollowingCat != null)
        {
            currentFollowingCat.StartFollow(this.transform);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(
            this.transform.position,
            captureRange,
            catLayer
        );

        if (hits.Length <= 0)
        {
            return;
        }

        GameObject cat = hits[0].gameObject;

        CatFollowPlayer follow = cat.GetComponent<CatFollowPlayer>();

        if (follow != null)
        {
            currentFollowingCat = follow;
            currentFollowingCat.StartFollow(this.transform);

            Debug.Log("고양이가 플레이어 쪽으로 이동 시작!");
        }
        else
        {
            Debug.LogWarning("고양이에 CatFollowPlayer가 없음");
        }
    }

    private void StopAttractCat()
    {
        if (currentFollowingCat != null)
        {
            currentFollowingCat.StopFollow();
            currentFollowingCat = null;

            Debug.Log("고양이 끌어오기 중지");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, captureRange);
    }
}
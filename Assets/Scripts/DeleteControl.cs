using UnityEngine;

public class DeleteControl : MonoBehaviour
{
    private MapCreator map_creator = null;

    private void Start()
    {
        // 씬에 있는 MapCreator 찾기
        this.map_creator = FindObjectOfType<MapCreator>();
    }

    private void Update()
    {
        if (this.map_creator == null)
        {
            return;
        }

        // 화면 왼쪽 밖으로 지나간 장애물은 삭제
        if (this.map_creator.isDelete(this.gameObject))
        {
            Destroy(this.gameObject);
        }
    }
}
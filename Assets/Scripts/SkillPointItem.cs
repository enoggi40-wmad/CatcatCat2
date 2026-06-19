using UnityEngine;

public class SkillPointItem : MonoBehaviour
{
    public int skillPointAmount = 1;

    [Header("Sound")]
    public AudioClip collectSound;
    public float collectSoundVolume = 1.0f;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
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
            pointManager.AddSkillPoint(skillPointAmount);
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

        Debug.Log("½ºÅ³ Æ÷ÀÎÆ® È¹µæ!");
    }
}
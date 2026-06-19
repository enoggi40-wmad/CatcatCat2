using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerCatAbility : MonoBehaviour
{
    [Header("Ability Sound")]
    public AudioSource audioSource;

    public AudioClip clawSound;
    public float clawSoundVolume = 1.0f;

    public AudioClip roarSound;
    public float roarSoundVolume = 1.0f;

    public AudioClip healSound;
    public float healSoundVolume = 1.0f;


    [Header("Player Material")]
    public Renderer playerRenderer;      // 플레이어 Renderer
    private Material originalMaterial;    // 원래 Material

    [Header("Current Cat")]
    public string currentCatName = "";
    public CatType.AbilityType currentAbilityType;
    public bool hasCatAbility = false;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;       // 발사할 투사체 프리팹
    public Transform projectileSpawnPoint;    // 투사체 생성 위치

    [Header("Melee Attack")]
    public int meleeAttackDamage = 3;

    [Header("Melee Visual")]
    public GameObject meleeStick;
    public float meleeVisualTime = 0.2f;
    public float meleeRotateSpeed = 900.0f;

    [Header("Heal")]
    public PlayerHealth playerHealth;
    public float healAmount = 10.0f;

    [Header("SkillPoint Cost")]
    public PointManager pointManager;
    public int rangedAttackSkillPointCost = 1;
    public int meleeAttackSkillPointCost = 3;
    public int healSkillPointCost = 10;

    [Header("Ability UI")]
    public GameObject abilityUI;
    public Text abilityText;

    private void Start()
    {
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }

        if (abilityUI != null)
        {
            abilityUI.SetActive(false);
        }
    }
    
    public void SetCurrentCat(CatType cat)
    {
        if (cat == null)
        {
            return;
        }

        // CatType 컴포넌트 자체를 저장하지 말고 정보만 복사
        currentCatName = cat.catName;
        currentAbilityType = cat.abilityType;
        hasCatAbility = true;

        // Player 위 능력 UI 갱신
        if (abilityUI != null)
        {
            abilityUI.SetActive(true);
        }

        if (abilityText != null)
        {
            abilityText.text = cat.catName;
            abilityText.color = cat.uiColor;
        }

        // 고양이 색깔을 플레이어에게 적용
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth != null && cat.playerMaterial != null)
        {
            playerHealth.SetMaterialAfterInvincible(cat.playerMaterial);
        }
        else
        {
            if (playerRenderer != null && cat.playerMaterial != null)
            {
                playerRenderer.material = cat.playerMaterial;
            }
        }

        Debug.Log("최근 구조한 고양이: " + cat.catName);
        Debug.Log("고양이 능력 타입: " + cat.abilityType);
    }

    private void Update()
    {
        // E키를 누르면 고양이 능력 사용 시도
        if (Input.GetKeyDown(KeyCode.A))
        {
            UseCatAbility();
        }
    }

    private bool TryConsumeSkillPoint(int cost)
    {
        if (pointManager == null)
        {
            Debug.LogWarning("PointManager가 연결되지 않음");
            return false;
        }

        if (!pointManager.ConsumeSkillPoint(cost))
        {
            Debug.Log("스킬포인트가 부족해서 능력 사용 실패");
            return false;
        }

        return true;
    }

    private void UseCatAbility()
    {
        if (!hasCatAbility)
        {
            Debug.Log("현재 사용할 고양이 능력이 없음");
            return;
        }

        Debug.Log("고양이 능력 사용 시도: " + currentAbilityType);

        switch (currentAbilityType)
        {
            case CatType.AbilityType.Roar:
                UseRangedAttack();
                break;

            case CatType.AbilityType.Claw:
                UseMeleeAttack();
                break;

            case CatType.AbilityType.Heal:
                UseHeal();
                break;
        }
    }

    private void UseRangedAttack()
    {
        if (pointManager == null)
        {
            Debug.LogWarning("PointManager가 연결되지 않음");
            return;
        }

        if (!TryConsumeSkillPoint(rangedAttackSkillPointCost))
        {
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("투사체 프리팹이 연결되지 않음");
            return;
        }

        if (audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound, roarSoundVolume);
        }

        Vector3 spawnPosition = this.transform.position + Vector3.right * 1.0f;

        if (projectileSpawnPoint != null)
        {
            spawnPosition = projectileSpawnPoint.position;
        }

        Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        Debug.Log("원거리 투사체 발사!");
    }

    private void UseMeleeAttack()
    {
        Debug.Log("발톱 공격 함수 실행됨");

        if (!TryConsumeSkillPoint(meleeAttackSkillPointCost))
        {
            return;
        }

        if (meleeStick == null)
        {
            Debug.LogWarning("MeleeStick 또는 MeleeClaw가 연결되지 않음");
            return;
        }

        if (audioSource != null && clawSound != null)
        {
            audioSource.PlayOneShot(clawSound, clawSoundVolume);
        }

        StartCoroutine(MeleeVisualCoroutine());
    }

    private IEnumerator MeleeVisualCoroutine()
    {
        if (meleeStick == null)
        {
            yield break;
        }

        meleeStick.SetActive(true);

        float timer = 0.0f;

        // 시작 회전값 저장
        Quaternion startRotation = meleeStick.transform.localRotation;

        while (timer < meleeVisualTime)
        {
            timer += Time.deltaTime;

            // 휘두르는 느낌
            meleeStick.transform.Rotate(

                
                0.0f,
                -meleeRotateSpeed * Time.deltaTime,
                0.0f,
                
                Space.Self
            );

            yield return null;
        }

        // 원래 회전으로 복구하고 숨김
        meleeStick.transform.localRotation = startRotation;
        meleeStick.SetActive(false);
    }

    private void UseHeal()
    {
        if (!TryConsumeSkillPoint(healSkillPointCost))
        {
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth가 연결되지 않음");
            return;
        }

        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound, healSoundVolume);
        }

        playerHealth.Heal(healAmount);

        Debug.Log("Heal 고양이 능력 사용! 체력 " + healAmount + " 회복");
    }
}
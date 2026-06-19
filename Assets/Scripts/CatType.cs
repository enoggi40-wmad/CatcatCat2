using UnityEngine;

public class CatType : MonoBehaviour
{
    public enum AbilityType
    {
        None,
        Claw,
        Roar,
        Heal
    }

    [Header("Cat Info")]
    public string catName = "Cat";

    [Header("Ability")]
    public AbilityType abilityType;

    [Header("Player Material")]
    public Material playerMaterial; // 이 고양이를 얻었을 때 플레이어에게 입힐 Material

    [Header("Cat UI")]
    public Color uiColor = Color.white;
}
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Survivor/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName = "Default";

    [Header("Base Stats")]
    public float baseMaxHp = 100f;
    public float baseWalkSpeed = 6f;
    public float baseMightMultiplier = 1f;
    public float basePickupRange = 3f;
    public float baseAttackRange = 10f;
}

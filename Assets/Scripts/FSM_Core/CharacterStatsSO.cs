using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Combat/CharacterStats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Base Attributes")]
    public float maxHealth = 100f;
    public float maxToughness = 100f;

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;

    [Header("Combat")]
    public float attackDamage = 20.0f;
    public float toughnessDamage = 25.0f;
}
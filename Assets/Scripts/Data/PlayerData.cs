using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Health")]
    public int startingHealth = 100;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 1500f;
    public float jumpPower = 3f;
    public int maxJumps = 1;
    public float gravityMultiplier = 1f;

    [Header("Combat")]
    public float attackRange = 2f;
    public int attackDamage = 25;
    public float attackSpeed = 1f;
}

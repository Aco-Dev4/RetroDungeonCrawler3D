using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;

    [Header("Prefab")]
    public GameObject prefab;

    [Header("Base Stats")]
    public int maxHealth = 100;
    public int damage = 10;
    public float attackSpeed = 1f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
}

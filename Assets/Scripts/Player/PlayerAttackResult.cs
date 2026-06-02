public struct PlayerAttackResult
{
    public int damage;
    public bool isCrit;
    public bool isGuaranteedCrit;

    public PlayerAttackResult(int damage, bool isCrit, bool isGuaranteedCrit = false)
    {
        this.damage = damage;
        this.isCrit = isCrit;
        this.isGuaranteedCrit = isGuaranteedCrit;
    }
}
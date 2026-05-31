public struct PlayerAttackResult
{
    public int damage;
    public bool isCrit;

    public PlayerAttackResult(int damage, bool isCrit)
    {
        this.damage = damage;
        this.isCrit = isCrit;
    }
}
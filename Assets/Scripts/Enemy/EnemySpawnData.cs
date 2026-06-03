public class EnemySpawnData
{
    public EnemyData enemyData;
    public bool isMiniBoss;

    public EnemySpawnData(EnemyData enemyData, bool isMiniBoss)
    {
        this.enemyData = enemyData;
        this.isMiniBoss = isMiniBoss;
    }
}
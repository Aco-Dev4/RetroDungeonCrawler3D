using UnityEngine;

public class PlayerDeathEffectHandler : MonoBehaviour
{
    private int _revives;

    public void SetRevives(int amount)
    {
        _revives = Mathf.Max(0, amount);
    }

    public bool TryPreventDeath()
    {
        if (_revives <= 0)
            return false;

        _revives--;

        Debug.Log("Guardian Angel prevented death.");

        return true;
    }
}
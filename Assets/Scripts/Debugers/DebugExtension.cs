using UnityEngine;

public static class DebugExtension
{
    public static void DebugWireSphere(Vector3 position, Color color, float radius, float duration = 0f)
    {
        float step = 10f;
        for (float theta = 0; theta < 360; theta += step)
        {
            Vector3 p1 = position + Quaternion.Euler(0, theta, 0) * Vector3.forward * radius;
            Vector3 p2 = position + Quaternion.Euler(0, theta + step, 0) * Vector3.forward * radius;
            Debug.DrawLine(p1, p2, color, duration);
        }
    }
}

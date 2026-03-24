using UnityEngine;

public static class MathHelper
{
    public static float Remap(float value, float fromMin, float fromMax, 
                               float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    public static Vector3 GetRandomPointInCircle(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(0f, radius);
        return new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );
    }

    public static bool IsInRange(Vector3 a, Vector3 b, float range)
    {
        return Vector3.SqrMagnitude(a - b) <= range * range;
    }

    public static float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
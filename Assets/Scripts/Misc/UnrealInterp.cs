using UnityEngine;

public class UnrealInterp : MonoBehaviour
{
    public static Quaternion QInterpTo(Quaternion current, Quaternion target, float deltaTime, float interpSpeed)
    {
        if (interpSpeed <= 0f || Quaternion.Angle(current, target) < 0.01f) return target;

        return Quaternion.Slerp(current, target, Mathf.Clamp01(interpSpeed * deltaTime));
    }

    public static float FInterpTo(float current, float target, float deltaTime, float interpSpeed)
    {
        if (interpSpeed <= 0f) return target;

        float dist = target - current;
        if (Mathf.Abs(dist) < 0.01f) return target;
        float deltaMove = dist * Mathf.Clamp01(deltaTime * interpSpeed);
        return current + deltaMove;
    }
}

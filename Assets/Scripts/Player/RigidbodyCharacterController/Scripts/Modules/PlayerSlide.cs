using UnityEngine;

public class PlayerSlide : MonoBehaviour, IPlayerModule
{
    public Vector3 CalculateVelocity(CharacterMotor motor)
    {
        if (!motor.IsSliding) return Vector3.zero;

        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, motor.GroundHit.normal);
        return slideDirection.normalized * motor.GravityData.SlideSpeed;
    }
}
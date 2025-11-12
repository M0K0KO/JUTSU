using UnityEngine;

public class PlayerMove : MonoBehaviour, IPlayerModule
{
    public Vector3 CalculateVelocity(CharacterMotor motor)
    {
        if (motor.IsDashing || motor.IsSliding) 
        {
            return Vector3.zero;
        }
        
        Vector3 moveDirection = motor.RawMoveDirection;
        Vector3 wallAdjustedDirection = GetWallAdjustedDirection(moveDirection, motor, out float speedMultiplier);
        Vector3 finalDirection = GetSlopeAdjustedDirection(wallAdjustedDirection, motor);
        AdjustMoveSpeed(motor);
        float finalSpeed = motor.MovementData.moveSpeed * speedMultiplier;

        if (motor.IsOnValidGround)
        {
            Vector3 projectedMove = Vector3.ProjectOnPlane(finalDirection, motor.GroundHit.normal);
            return projectedMove.normalized * finalSpeed;
        }
        else
        {
            if (motor.RetainAirMovement)
            {
                return motor.PlayerDash ? motor.PlayerDash.RetainedDashVelocity : Vector3.zero;
            }
            
            Vector3 currentHorizontalVelocity = new Vector3(motor.CurrentVelocity.x, 0, motor.CurrentVelocity.z);
            Vector3 desiredAirVelocity = finalDirection.normalized * finalSpeed;
            Vector3 finalAirVelocity = Vector3.Lerp(
                currentHorizontalVelocity, 
                desiredAirVelocity, 
                motor.MovementData.airControl * Time.fixedDeltaTime
            );
            
            return finalAirVelocity;
        }
    }
    
    public Vector3 GetWallAdjustedDirection(Vector3 direction, CharacterMotor motor, out float speedMultiplier)
    {
        speedMultiplier = 1f;
    
        if (motor.IsAgainstWall)
        {
            float dot = Vector3.Dot(direction, motor.WallHit.normal);
            if (dot < 0)
            {
                const float headOnThreshold = -0.9f;

                if (dot < headOnThreshold)
                {
                    speedMultiplier = 0f;
                    return Vector3.zero;
                }
                else
                {
                    Vector3 impactVelocity = Vector3.Project(direction, motor.WallHit.normal);

                    Vector3 slideDirection = direction - impactVelocity * 0.9f;

                    speedMultiplier = 1f + dot;
                
                    return slideDirection;
                }
            }
        }
    
        return direction; 
    }
    
    public Vector3 GetSlopeAdjustedDirection(Vector3 direction, CharacterMotor motor)
    {
        if (motor.IsOnValidGround)
        {
            return Vector3.ProjectOnPlane(direction, motor.GroundHit.normal);
        }
        return direction;
    }

    private void AdjustMoveSpeed(CharacterMotor motor)
    {
        float targetSpeed = motor._playerInput.RunInput ? motor.MovementData.runSpeed : motor.MovementData.walkSpeed;
        motor.MovementData.moveSpeed = targetSpeed;
    }
}
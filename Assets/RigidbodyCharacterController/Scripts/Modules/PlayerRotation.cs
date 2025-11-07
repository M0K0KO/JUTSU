using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    private CharacterMotor motor;
    
    private void Awake()
    {
        motor = GetComponent<CharacterMotor>();
    }

    private void Update()
    {
        motor.Rb.angularVelocity = Vector3.zero;

        Vector3 lookDirection = motor.RawMoveDirection;
        if (lookDirection.sqrMagnitude < float.Epsilon) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        
        Quaternion newRotation = Quaternion.Slerp(
            motor.Rb.rotation,
            targetRotation, 
            motor.MovementData.rotationSpeed * Time.fixedDeltaTime
        );
        
        motor.Rb.MoveRotation(newRotation);
    }
}

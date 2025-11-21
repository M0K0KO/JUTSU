using UnityEngine;

public class PlayerGravity : MonoBehaviour, IPlayerModule
{
    private float verticalVelocity;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public Vector3 CalculateVelocity(CharacterMotor motor)
    {
        if (motor.IsOnValidGround && verticalVelocity <= 0f)
        {
            if (_playerInput.MoveInput.sqrMagnitude > float.Epsilon)
            {
                verticalVelocity = -1f;
            }
            else
            {
                verticalVelocity = 0f;
            }
        }
        else
        {
            verticalVelocity += motor.GravityData.Gravity * Time.fixedDeltaTime;
            verticalVelocity = Mathf.Max(motor.GravityData.MaxFallSpeed, verticalVelocity);
        }

        return new Vector3(0, verticalVelocity, 0);
    }

    public void SetVerticalVelocity(float velocity)
    {
        verticalVelocity = velocity;
    }

    
    //------------------------------------------------
    
    #if UNITY_EDITOR
    public float GetCurrentVerticalVelocity()
    {
        return verticalVelocity;
    }
    #endif
}

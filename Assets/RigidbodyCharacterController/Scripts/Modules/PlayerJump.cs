using UnityEngine;

public class PlayerJump : MonoBehaviour, IPlayerModule
{
    private PlayerInput _playerInput;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private int jumpCount;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public Vector3 CalculateVelocity(CharacterMotor motor)
    {
        if (motor.IsOnValidGround)
        {
            coyoteTimeCounter = motor.JumpData.CoyoteTime;
            jumpCount = 1;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }


        bool jumpPressed = _playerInput.JumpInput;
        if (jumpPressed)
        {
            _playerInput.ClearJumpInput();
            jumpBufferCounter = motor.JumpData.JumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }

        if (jumpBufferCounter > 0f && jumpCount < motor.JumpData.maxJumps)
        {
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            motor.PlayerGravity.SetVerticalVelocity(motor.JumpData.JumpForce);

            jumpCount++;
        }

        return Vector3.zero;
    }

    //----------------------------------------------------------------
#if UNITY_EDITOR
    public float GetCoyoteTime() => coyoteTimeCounter;
    public float GetJumpBufferTime() => jumpBufferCounter;
#endif
}
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerDash : MonoBehaviour, IPlayerModule
{
    private PlayerInput _playerInput;

    private int dashPhase;
    
    private float dashCooldownTimer;
    private float dashDurationTimer;

    private Coroutine dashChainWindowCoroutine;
    private bool dashChainWindow = false;

    private Vector3 lockedDashDirection;
    public Vector3 RetainedDashVelocity { get; private set; }

    private void Awake()
    {
        dashPhase = 0;
        _playerInput = GetComponent<PlayerInput>();
    }

    public Vector3 CalculateVelocity(CharacterMotor motor)
    {
        // Timer는 돌아가는중임 (항상 돌아가야하는 타이머)
        dashCooldownTimer -= Time.fixedDeltaTime;
        dashDurationTimer -= Time.fixedDeltaTime;

        // Dash 지속시간이 거의 끝났을때부터, Dash가 끝난 이후 약간까지 Window 열기
        if (motor.IsDashing)
        {
            
            if (dashDurationTimer < 0f)
                EndDash(motor);
            else
            {
                if (dashDurationTimer <= motor.DashData.dashChainCoyoteTime && !dashChainWindow)
                    StartCoroutine(OpenDashChainWindow(motor));
            }
        }
        

        bool dashPressed = _playerInput.RollInput;
        if (dashPressed)
        {
            _playerInput.ClearRollInput();
            if (dashPhase == 0 && dashCooldownTimer <= 0) // 첫 대쉬
            {
                StartNewDash(motor);
            }
            else if (dashChainWindow && dashPhase < motor.DashData.MaxDashes) // Window가 열렸고, 추가적으로 Dash 가능한 상태
            {
                StartNewDash(motor);
            }
        }

        if (motor.IsDashing)
        {
            Vector3 wallAdjustedDirection = motor.PlayerMove.GetWallAdjustedDirection(lockedDashDirection, motor, out float speedMultiplier);
            Vector3 finalDashDirection = motor.PlayerMove.GetSlopeAdjustedDirection(wallAdjustedDirection, motor);
            float finalDashSpeed = motor.DashData.dashSpeed * speedMultiplier;
            return finalDashDirection.normalized * finalDashSpeed;
        }

        return Vector3.zero;
    }

    private IEnumerator OpenDashChainWindow(CharacterMotor motor)
    {
        dashChainWindow = true;
        yield return new WaitForSeconds(motor.DashData.dashChainWindow);
        dashChainWindow = false;
        if (!motor.IsDashing) dashPhase = 0;
    }
    
    private void StartNewDash(CharacterMotor motor)
    {
        motor.IsDashing = true;
        motor.RetainAirMovement = false; // 새로운 대시를 시작하면 운동량 보존을 해제
        RetainedDashVelocity = Vector3.zero;
        
        dashChainWindow = false; // 연계에 성공했으므로 창을 닫음
        dashDurationTimer = motor.DashData.dashDuration;
        dashCooldownTimer = motor.DashData.dashCoolDown; // 첫 대시, 연계 대시 모두 쿨타임을 다시 적용

        lockedDashDirection =
            motor.RawMoveDirection.sqrMagnitude > 0.1f ? motor.RawMoveDirection : transform.forward;
        
        dashPhase++;
    }
    
    private void EndDash(CharacterMotor motor)
    {
        motor.IsDashing = false;

        if (!motor.IsOnValidGround)
        {
            motor.RetainAirMovement = true;
            RetainedDashVelocity = motor.CurrentVelocity;
        }
        else
        {
            motor.RetainAirMovement = false;
            RetainedDashVelocity = Vector3.zero;
        }
    }
}

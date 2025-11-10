using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool RollInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool LockOnInput { get; private set; }

    private PlayerInputActions _playerInputActions;

    private InputAction _move;
    private InputAction _run;
    private InputAction _dash;
    private InputAction _look;
    private InputAction _attack;
    private InputAction _lockOn;

    
    #region Built-In Functions
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();

        _move = _playerInputActions.Move.Move;
        _run = _playerInputActions.Move.Run;
        _dash = _playerInputActions.Move.Dash;
        _look = _playerInputActions.Camera.Look;
        _attack = _playerInputActions.Action.Shoot;
        _lockOn = _playerInputActions.Camera.LockOn;
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
        
        _move.performed += OnMove;
        _move.canceled += OnMove;
        
        _run.performed += OnRun;
        _run.canceled += OnRun;

        _dash.performed += OnDash;
        
        _look.performed += OnLook;

        _lockOn.performed += OnLockOn;
        _lockOn.canceled += OnLockOn;

        _attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();
        
        _move.performed -= OnMove;
        _move.canceled -= OnMove;
        
        _run.performed -= OnRun;
        _run.canceled -= OnRun;

        _dash.performed -= OnDash;
        
        _look.performed -= OnLook;
        
        _lockOn.performed -= OnLockOn;
        _lockOn.canceled -= OnLockOn;
        
        _attack.performed -= OnAttack;
    }

    private void OnDestroy()
    {
        _playerInputActions?.Dispose();
    }
    #endregion
    
    #region Call-Back Functions
    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        RunInput = context.ReadValueAsButton();
    }
    
    private void OnDash(InputAction.CallbackContext context)
    {
        RollInput = true;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    private void OnLockOn(InputAction.CallbackContext context)
    {
        LockOnInput = context.ReadValueAsButton();
    }
    
    private void OnAttack(InputAction.CallbackContext context)
    {
        AttackInput = true;
    }
    #endregion
    
    #region ClearInput
    public void ClearRollInput() => RollInput = false;
    public void ClearAttackInput() => AttackInput = false;
    public void ClearLockOnInput() => LockOnInput = false;
    #endregion
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerManager player;
    
    public Vector2 MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool RollInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool LockOnInput { get; private set; }
    public bool JutsuInput { get; private set; }

    private PlayerInputActions _playerInputActions;

    private InputAction _move;
    private InputAction _run;
    private InputAction _dash;
    private InputAction _look;
    private InputAction _attack;
    private InputAction _lockOn;
    private InputAction _jutsu;

    private InputAction _pause;
    
    #region Built-In Functions
    private void Awake()
    {
        player = GetComponent<PlayerManager>();
        
        _playerInputActions = new PlayerInputActions();

        _move = _playerInputActions.Move.Move;
        _run = _playerInputActions.Move.Run;
        _dash = _playerInputActions.Move.Dash;
        _look = _playerInputActions.Camera.Look;
        _attack = _playerInputActions.Action.Shoot;
        _lockOn = _playerInputActions.Camera.LockOn;
        _jutsu = _playerInputActions.Action.Jutsu;
        
        _pause = _playerInputActions.UI.Pause;
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

        _jutsu.performed += OnJutsu;

        _pause.performed += OnPause;

        PauseMenuController.Instance.OnGamePaused += OnGamePaused;
        PauseMenuController.Instance.OnGameResumed += OnGameResumed;
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

        _jutsu.performed -= OnJutsu;

        _pause.performed -= OnPause;

        PauseMenuController.Instance.OnGamePaused -= OnGamePaused;
        PauseMenuController.Instance.OnGameResumed -= OnGameResumed;
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

    private void OnJutsu(InputAction.CallbackContext context)
    {
        if (player.jutsu.isInMuryokusho) return;
        JutsuInput = true;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        PauseMenuController.Instance.OnPauseInputReceived();
    }
    
    #endregion
    
    #region ClearInput
    public void ClearRollInput() => RollInput = false;
    public void ClearAttackInput() => AttackInput = false;
    public void ClearLockOnInput() => LockOnInput = false;
    public void ClearJutsuInput() => JutsuInput = false;
    #endregion

    #region Pause Event Call-Back Functions

    private void OnGamePaused()
    {
        _playerInputActions.Move.Disable();
        _playerInputActions.Action.Disable();
        _playerInputActions.Camera.Disable();
    }

    private void OnGameResumed()
    {
        _playerInputActions.Move.Enable();
        _playerInputActions.Action.Enable();
        _playerInputActions.Camera.Enable();
    }

    #endregion
}

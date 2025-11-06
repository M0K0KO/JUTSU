using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool DashInput { get; private set; }

    private PlayerInputActions _playerInputActions;

    private InputAction _move;
    private InputAction _jump;
    private InputAction _dash;

    
    #region Built-In Functions
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();

        _move = _playerInputActions.Move.Move;
        _jump = _playerInputActions.Move.Jump;
        _dash = _playerInputActions.Move.Dash;
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
        
        _move.performed += OnMove;
        _move.canceled += OnMove;

        _jump.performed += OnJump;

        _dash.performed += OnDash;
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();
        
        _move.performed -= OnMove;
        _move.canceled -= OnMove;

        _jump.performed -= OnJump;

        _dash.performed -= OnDash;
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

    private void OnJump(InputAction.CallbackContext context)
    {
        JumpInput = true;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        DashInput = true;
    }
    #endregion
    
    #region ClearInput

    public void ClearJumpInput()
    {
        JumpInput = false;
    }

    public void ClearDashInput()
    {
        DashInput = false;
    }
    #endregion
}

using System;
using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float walkSpeed;

    [SerializeField] private float runSpeed;

    [Header("Gravity Settings")]
    [SerializeField] private float gravitySpeed;

    [Header("Rotate Settings")]
    [SerializeField] private float rotateSpeed;

    [Header("Camera Rotation Settings")]
    [SerializeField] private GameObject camFollowTarget;

    [Header("Dash Settings")]
    [SerializeField] private float rollSpeed;
    [SerializeField] private AnimationCurve rollSpeedCurve;
    [SerializeField] private float rollDuration;

    private float moveSpeed;

    public float horizontalMoveAmount { get; private set; }
    public float verticalMoveAmount { get; private set; }

    public bool canMove { get; private set; } = true;
    public bool canRotate { get; private set; } = true;
    public bool canRoll { get; private set; } = true;

    // Built-In Components References
    public CharacterController cc { get; private set; }

    // References
    private PlayerStateMachine stateMachine;
    private PlayerAnimationController animationController;
    private Camera playerCam;


    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        stateMachine = GetComponent<PlayerStateMachine>();
        animationController = GetComponent<PlayerAnimationController>();

        playerCam = Camera.main;
    }

    private void Update()
    {
        ApplyGravity();
    }

    public float GetMoveAmount()
    {
        if (Mathf.Approximately(moveSpeed, walkSpeed))
        {
            return 0.5f;
        }
        else if (Mathf.Approximately(moveSpeed, runSpeed))
        {
            return 1f;
        }
        else
        {
            return 0f;
        }
    }

    public Vector3 GetCameraRelativeMoveDirection(Vector2 moveInput)
    {
        Vector3 moveDirection = moveInput.x * playerCam.transform.right + moveInput.y * playerCam.transform.forward;
        horizontalMoveAmount = moveDirection.x;
        verticalMoveAmount = moveDirection.z;
        moveDirection.y = 0;
        moveDirection.Normalize();
        return moveDirection;
    }

    public void Move(Vector3 moveDirection)
    {
        if (!canMove) return;

        cc.Move(moveDirection * (moveSpeed * Time.deltaTime));
    }

    public void EnableMove() => canMove = true;
    public void DisableMove() => canMove = false;

    public void ChangeSpeedToZero() => moveSpeed = 0;
    public void ChangeSpeedToWalkSpeed() => moveSpeed = walkSpeed;
    public void ChangeSpeedToRunSpeed() => moveSpeed = runSpeed;

    public void ApplyGravity() => cc.Move(Vector3.down * (gravitySpeed * Time.deltaTime));

    public void Rotate(Vector3 moveDirection, bool smooth = true)
    {
        if (!canRotate || moveDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        if (smooth)
        {
            targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
        if (targetRotation != Quaternion.identity) transform.rotation = targetRotation;
    }

    public void StrafeRotate()
    {
        if (!stateMachine.isStrafing) return;

        Vector3 targetDirection = stateMachine.currentTargetEnemy.transform.position - transform.position;
        targetDirection.y = 0;
        targetDirection.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        if (targetRotation != Quaternion.identity) transform.rotation = targetRotation;
    }

    public void EnableRotation() => canRotate = true;
    public void DisableRotation() => canRotate = false;

    public IEnumerator Roll(Vector3 rollDirection)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= rollDuration)
        {
            float eval = elapsedTime / rollDuration;
            cc.Move(rollDirection * (rollSpeed * rollSpeedCurve.Evaluate(eval) * Time.deltaTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }
}
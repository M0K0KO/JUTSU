using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Serialization;

public interface IPlayerModule
{
    Vector3 CalculateVelocity(CharacterMotor motor);
}

public class CharacterMotor : MonoBehaviour
{
    public Rigidbody Rb { get; private set; }
    private IPlayerModule[] _modules;
    private PlayerInput _playerInput;

    public Camera playerCamera;


    [Header("Data Properties")] public MovementData MovementData;
    public JumpData JumpData;
    public GravityData GravityData;
    public DashData DashData;

    [Header("CheckGrounded Properties")]
    [SerializeField]
    private Vector3 sphereCastOffSet;

    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float sphereCastMaxDistance;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Wall Detection Properties")]
    [SerializeField]
    private float playerHeight = 2f;

    [SerializeField] private float playerRadius = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayerMask;

    public RaycastHit GroundHit { get; private set; }
    public RaycastHit WallHit { get; private set; }
    public Vector3 RawMoveDirection { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }
    public bool IsOnValidGround { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsAgainstWall { get; private set; }
    public bool IsDashing { get; set; }
    public bool RetainAirMovement { get; set; }

    [Header("Initialize Options")] public bool MoveComponent;
    public bool RotationComponent;
    public bool GravityComponent;
    public bool JumpComponent;
    public bool SlideComponent;
    public bool DashComponent;

    [HideInInspector] public PlayerMove PlayerMove;
    [HideInInspector] public PlayerRotation PlayerRotation;
    [HideInInspector] public PlayerGravity PlayerGravity;
    [HideInInspector] public PlayerJump PlayerJump;
    [HideInInspector] public PlayerSlide PlayerSlide;
    [HideInInspector] public PlayerDash PlayerDash;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Rb.useGravity = false;

        _playerInput = transform.AddComponent<PlayerInput>();
        if (MoveComponent) PlayerMove = transform.AddComponent<PlayerMove>();
        if (RotationComponent) PlayerRotation = transform.AddComponent<PlayerRotation>();
        if (GravityComponent) PlayerGravity = transform.AddComponent<PlayerGravity>();
        if (JumpComponent) PlayerJump = transform.AddComponent<PlayerJump>();
        if (SlideComponent) PlayerSlide = transform.AddComponent<PlayerSlide>();
        if (DashComponent) PlayerDash = transform.AddComponent<PlayerDash>();

        _modules = GetComponents<IPlayerModule>();
    }


    private void Update()
    {
        CheckLog();
    }

    private void FixedUpdate()
    {
        CalculateRawMoveDirection();
        CheckGrounded();
        CheckWall();

        if (IsOnValidGround && RetainAirMovement == true)
        {
            RetainAirMovement = false;
        }

        Move();
    }

    private void CalculateRawMoveDirection()
    {
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        RawMoveDirection = (forward * _playerInput.MoveInput.y + right * _playerInput.MoveInput.x).normalized;
    }

    private void CheckGrounded()
    {
        IsGrounded = Physics.SphereCast(
            transform.position + sphereCastOffSet,
            sphereCastRadius,
            Vector3.down,
            out RaycastHit hit,
            sphereCastMaxDistance,
            groundLayerMask
        );

        if (IsGrounded)
        {
            GroundHit = hit;
            float slopeAngle = Vector3.Angle(Vector3.up, GroundHit.normal);
            IsOnValidGround = slopeAngle <= MovementData.maxSlopeAngle;
            IsSliding = !IsOnValidGround;
        }
        else
        {
            IsOnValidGround = false;
            IsSliding = false;
        }
    }

    private void CheckWall()
    {
        IsAgainstWall = false;

        if (RawMoveDirection.sqrMagnitude > float.Epsilon)
        {
            if (Physics.CapsuleCast(
                    transform.position,
                    transform.position + (Vector3.up * playerHeight),
                    playerRadius,
                    RawMoveDirection,
                    out RaycastHit hit,
                    wallCheckDistance,
                    wallLayerMask
                ))
            {
                if (Vector3.Angle(Vector3.up, hit.normal) > MovementData.wallAngle)
                {
                    IsAgainstWall = true;
                    WallHit = hit;
                }
            }
        }
    }

    private void Move()
    {
        Vector3 finalVelocity = Vector3.zero;
        foreach (var module in _modules)
        {
            finalVelocity += module.CalculateVelocity(this);
        }

        CurrentVelocity = finalVelocity;

        Vector3 horizontalVelocity = new Vector3(CurrentVelocity.x, 0, CurrentVelocity.z);
        Vector3 verticalVelocity = new Vector3(0, CurrentVelocity.y, 0);
        Rb.linearVelocity = horizontalVelocity + verticalVelocity;
    }


    //------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    private void CheckLog()
    {
        //DebugExtension.ColorLog($"IsGrounded : {IsGrounded}", "cyan");
        //DebugExtension.ColorLog($"IsAgainstWall : {IsAgainstWall}", "red");
    }

    private void OnDrawGizmos()
    {
        // 기존 CheckGrounded Gizmo 코드
        bool isHit = Physics.SphereCast(
            transform.position + sphereCastOffSet,
            sphereCastRadius,
            Vector3.down,
            out RaycastHit hit,
            sphereCastMaxDistance,
            groundLayerMask
        );

        Gizmos.color = isHit ? Color.green : Color.red;
        Vector3 startCenter = transform.position + sphereCastOffSet;
        float distance = isHit ? hit.distance : sphereCastMaxDistance;
        Vector3 endCenter = startCenter + Vector3.down * distance;

        Gizmos.DrawWireSphere(startCenter, sphereCastRadius);
        Gizmos.DrawWireSphere(endCenter, sphereCastRadius);
        Gizmos.DrawLine(startCenter, endCenter);

        if (isHit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hit.point, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.5f);
        }

        // CheckWall Gizmo 코드 추가
        if (RawMoveDirection.sqrMagnitude > float.Epsilon)
        {
            // CapsuleCast 시각화
            Vector3 capsuleStart = transform.position;
            Vector3 capsuleEnd = transform.position + (Vector3.up * playerHeight);
            Vector3 castDirection = RawMoveDirection;

            // 실제 CheckWall과 동일한 CapsuleCast 수행
            bool wallHit = Physics.CapsuleCast(
                capsuleStart,
                capsuleEnd,
                playerRadius,
                castDirection,
                out RaycastHit wallHitInfo,
                wallCheckDistance,
                wallLayerMask
            );

            // 캡슐 색상 설정 (벽 충돌 여부에 따라)
            if (wallHit && Vector3.Angle(Vector3.up, wallHitInfo.normal) > MovementData.wallAngle)
            {
                Gizmos.color = Color.red; // 벽에 충돌
            }
            else if (wallHit)
            {
                Gizmos.color = Color.red + Color.yellow; // 충돌했지만 벽 각도가 아님
            }
            else
            {
                Gizmos.color = Color.cyan; // 충돌 없음
            }

            // 시작 위치의 캡슐 그리기
            DrawCapsule(capsuleStart, capsuleEnd, playerRadius);

            // 이동 방향 표시
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + castDirection * wallCheckDistance);

            // 끝 위치의 캡슐 그리기
            Vector3 endCapsuleStart = capsuleStart + castDirection * wallCheckDistance;
            Vector3 endCapsuleEnd = capsuleEnd + castDirection * wallCheckDistance;

            Gizmos.color = Color.white;
            DrawCapsule(endCapsuleStart, endCapsuleEnd, playerRadius);

            // 벽 충돌 시 충돌 지점과 법선 표시
            if (wallHit)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(wallHitInfo.point, 0.1f);

                // 법선 벡터 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(wallHitInfo.point, wallHitInfo.point + wallHitInfo.normal * 0.5f);

                // 각도 정보 표시용 (벽 각도 체크)
                float wallAngle = Vector3.Angle(Vector3.up, wallHitInfo.normal);
                Gizmos.color = wallAngle > MovementData.wallAngle ? Color.red : Color.green;
                Gizmos.DrawLine(wallHitInfo.point, wallHitInfo.point + Vector3.up * 0.3f);
            }
        }
    }

// 캡슐 그리기 헬퍼 함수
    private void DrawCapsule(Vector3 start, Vector3 end, float radius)
    {
        // 캡슐의 중심축
        Vector3 center = (start + end) / 2f;
        float height = Vector3.Distance(start, end);

        // 상단과 하단 원 그리기
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        // 캡슐의 측면 선 그리기
        Vector3 forward = Vector3.forward * radius;
        Vector3 right = Vector3.right * radius;

        // 4개의 측면 선
        Gizmos.DrawLine(start + forward, end + forward);
        Gizmos.DrawLine(start - forward, end - forward);
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);
    }
#endif
}
using System;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public IdleState idleState { get; private set; }
    public WalkState walkState { get; private set; }
    public RunState runState { get; private set; }
    public RollState rollState { get; private set; }
    public AttackState attackState { get; private set; }

    public BaseState currentState;


    public PlayerInput playerInput { get; private set; }
    public PlayerAnimationController animationController { get; private set; }
    public PlayerMover mover { get; private set; }
    public Camera playerCam { get; private set; }

    [Header("Enemy Detection")]
    [SerializeField]
    private float enemyDetectionRange = 20f;

    [SerializeField] private LayerMask enemyLayerMask = 0;

    public GameObject currentTargetEnemy { get; private set; }


    public bool isStrafing { get; private set; } = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        animationController = GetComponent<PlayerAnimationController>();
        mover = GetComponent<PlayerMover>();
        playerCam = Camera.main;
    }

    private void Start()
    {
        InitializeStateMachine();
    }

    private void Update()
    {
        Debug.Log(currentState);

        currentState.OnUpdateState();

        CheckRollInput();
        CheckAttackInput();
        CheckLockOn();

        ////////////////////////////////////////////////////////////////////////////////////////////
        if (Input.GetMouseButtonDown(1))
        {
            CheckNearbyEnemies(out GameObject target);
            EventManager.TriggerOnCameraStateChange(PlayerCameraState.Skill, target.transform);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdateState();
    }

    public void ChangeState(BaseState newState)
    {
        currentState.OnExitState();
        currentState = newState;
        currentState.OnEnterState();
    }


    #region CheckInput

    private void CheckRollInput()
    {
        if (playerInput.RollInput)
        {
            playerInput.ClearRollInput();

            if (currentState != rollState)
            {
                ChangeState(rollState);
            }
        }
    }

    private void CheckAttackInput()
    {
        if (playerInput.AttackInput)
        {
            playerInput.ClearAttackInput();

            if (currentState != rollState && currentState != attackState)
            {
                ChangeState(attackState);
            }
            else if (currentState == attackState)
            {
                attackState.BufferAttackInput();
            }
        }
    }

    private void CheckLockOn()
    {
        if (playerInput.LockOnInput)
        {
            playerInput.ClearLockOnInput();
            if (!isStrafing)
            {
                if (CheckNearbyEnemies(out GameObject target))
                {
                    currentTargetEnemy = target;
                    EventManager.TriggerOnCameraStateChange(PlayerCameraState.Strafe, target.transform);
                    isStrafing = true;
                }
            }
            else
            {
                currentTargetEnemy = null;
                EventManager.TriggerOnCameraStateChange(PlayerCameraState.Base, null);
                isStrafing = false;
            }
        }
    }

    #endregion

    private void InitializeStateMachine()
    {
        idleState = new IdleState(this);
        walkState = new WalkState(this);
        runState = new RunState(this);
        rollState = new RollState(this);
        attackState = new AttackState(this);

        currentState = idleState;
        currentState.OnEnterState();
    }

    private bool CheckNearbyEnemies(out GameObject target)
    {
        Collider[] nearbyEnemies = new Collider[20];
        int enemyCount = Physics.OverlapSphereNonAlloc(transform.position,
            enemyDetectionRange, nearbyEnemies, enemyLayerMask);

        if (enemyCount > 0)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 targetScreenPosition = playerCam.WorldToScreenPoint(nearbyEnemies[0].transform.position);
            float nearestDistance = Vector2.Distance(screenCenter, targetScreenPosition);
            Collider nearestEnemy = nearbyEnemies[0];

            for (int i = 1; i < enemyCount; i++)
            {
                screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                targetScreenPosition = playerCam.WorldToScreenPoint(nearbyEnemies[i].transform.position);
                float distance = Vector2.Distance(screenCenter, targetScreenPosition);

                if (distance < nearestDistance)
                {
                    nearestEnemy = nearbyEnemies[i];
                    nearestDistance = distance;
                }
            }

            target = nearestEnemy.gameObject;
            return true;
        }

        target = null;
        return false;
    }
}
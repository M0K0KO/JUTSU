using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour, IDamageable
{
    public IdleState idleState { get; private set; }
    public WalkState walkState { get; private set; }
    public RunState runState { get; private set; }
    public RollState rollState { get; private set; }
    public AttackState attackState { get; private set; }
    public HitState hitState { get; private set; }

    
    public BaseState currentState;


    public PlayerManager player { get; private set; }

    [Header("Enemy Detection")]
    [SerializeField]
    private float enemyDetectionRange = 20f;

    [SerializeField] private LayerMask enemyLayerMask = 0;

    public GameObject currentTargetEnemy { get; private set; }
    public Transform currentTargetHitTarget { get; private set; }


    private const int hitAnimationQueueInitialCapacity = 5;
    public Queue<bool> hitAnimationQueue = new Queue<bool>(hitAnimationQueueInitialCapacity); 

    public bool isStrafing { get; private set; } = false;

    public int comboCounter = 0;

    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        InitializeStateMachine();
    }

    private void Update()
    {
        Debug.Log(currentState);
        
        currentState.OnUpdateState();

        if (Input.GetKeyDown(KeyCode.F))
        {
            // TakeDamage();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            player.animator.Play("Player_Attack_03");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            player.animator.Play("Player_Attack_04");
        }
        

        CheckRollInput();
        CheckAttackInput();
        CheckLockOn();
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
        if (currentState == hitState) return;
        
        if (player.playerInput.RollInput)
        {
            player.playerInput.ClearRollInput();

            if (currentState != rollState)
            {
                ChangeState(rollState);
            }
        }
    }

    private void CheckAttackInput()
    {
        if (currentState == hitState) return;
        
        if (player.playerInput.AttackInput)
        {
            player.playerInput.ClearAttackInput();

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
        if (player.playerInput.LockOnInput)
        {
            player.playerInput.ClearLockOnInput();
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
        hitState = new HitState(this);

        currentState = idleState;
        currentState.OnEnterState();
    }

    public bool CheckNearbyEnemies(out GameObject target, bool useDetectionRange = true)
    {
        float detectionRange = useDetectionRange ? enemyDetectionRange : Mathf.Infinity;
        
        Collider[] nearbyEnemies = new Collider[20];
        int enemyCount = Physics.OverlapSphereNonAlloc(transform.position,
            detectionRange, nearbyEnemies, enemyLayerMask);

        if (enemyCount > 0)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 targetScreenPosition = player.playerCam.WorldToScreenPoint(nearbyEnemies[0].transform.position);
            float nearestDistance = Vector2.Distance(screenCenter, targetScreenPosition);
            Collider nearestEnemy = nearbyEnemies[0];

            for (int i = 1; i < enemyCount; i++)
            {
                screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                targetScreenPosition = player.playerCam.WorldToScreenPoint(nearbyEnemies[i].transform.position);
                float distance = Vector2.Distance(screenCenter, targetScreenPosition);

                if (distance < nearestDistance)
                {
                    nearestEnemy = nearbyEnemies[i];
                    nearestDistance = distance;
                }
            }

            Target_Test enemy = nearestEnemy.gameObject.GetComponent<Target_Test>();
            target = enemy.cameraTarget;
            currentTargetHitTarget = enemy.hitTarget;
            return true;
        }

        target = null;
        currentTargetHitTarget = null;
        return false;
    }
    
    
    public void TakeDamage(bool shouldPlayHitReaction, GestureType gestureType, Vector3 damageOrigin)
    {
        if (currentState == rollState) return;
        
        Vector3 damageOriginDir = damageOrigin - transform.position;
        player.mover.Rotate(damageOriginDir, false);
        
        if (currentState != hitState)
        {
            ChangeState(hitState);
        }
        else
        {
            hitAnimationQueue.CapacitySafeEnqueue(true, hitAnimationQueueInitialCapacity);
        }
    }
    

    public void IncreaseComboCounter() => comboCounter++;
    
    public void Attack(Vector3 origin, Vector3 destination)
    {
        bool shouldPlayHitReaction = (comboCounter == 7);
        
        if (shouldPlayHitReaction) Debug.Log("SHOULD PLAY HIT REACTION!!");
        
        Ray ray = new Ray(origin, destination - origin);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out IDamageable damageable))
            {
                //damageable.TakeDamage(origin, shouldPlayHitReaction, GestureType.None);
            }
        }
    }

    public void PlayShootSFX()
    {
        player.audioSourceHolder.sfxDict["Shoot"].PlayAudioClip();
    }
    

    public void PlayRunSFX()
    {
        if (currentState == runState)
            player.audioSourceHolder.sfxDict["Run"].PlayAudioClip();
    }

    public void PlayRollSFX()
    {
        player.audioSourceHolder.sfxDict["Roll"].PlayAudioClip();
    }
}
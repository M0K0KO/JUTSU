using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum BossAttack
{
    ChargeAttack1 = 0,
    ChargeAttack2 = 1,
    ShockwaveAttack = 2
}

public enum BossHit
{
    NormalHit = 0,
    AkaHit = 1,
    KonHit = 2
}

public enum BossAkaHitEnd
{
    Duration = 0,
    Wall = 1
}

public class BossStateMachine : MonoBehaviour
{
    [HideInInspector] public Animator BossAnimator { get; private set; }
    [HideInInspector] public NavMeshAgent BossAgent { get; private set; }
    [HideInInspector] public Rigidbody BossRigidbody { get; private set; }
    [HideInInspector] public Transform BossTransform { get; private set; }
    
    [HideInInspector] public GameObject PlayerGameObject { get; private set; }
    
    [HideInInspector] public BossManager Manager { get; private set; }
    
    public bool IsUnderDomainExpansion { get; set; }
    
    public BossIdleState IdleState { get; private set; }
    public BossChaseState ChaseState { get; private set; }
    public BossChargeAttackState ChargeAttackState { get; private set; }
    public BossShockwaveAttackState ShockwaveAttackState { get; private set; }
    public BossNormalHitState NormalHitState { get; private set; }
    public BossAkaHitState AkaHitState { get; private set; }
    public BossKonHit KonHitState { get; private set; }

    public BossBaseState CurrentState { get; private set; }

    private float _shockwaveAttackCooldown = 10f;
    private float _shockwaveAttackTimer = 10f;
    public bool CanShockwaveAttack { get; set; }
    
    public Vector3 AkaInitialDirection { get; set; }
    public float AkaDuration { get; set; }
    public float AkaSpeed { get; set; }

    private Vector3 _acculmulatedDeltaPos = Vector3.zero;
    
    private int _locomotionId = Animator.StringToHash("Locomotion");

    private void InitStateMachine()
    {
        Manager = GetComponent<BossManager>();
        
        BossAnimator = GetComponent<Animator>();
        BossAnimator.applyRootMotion = true;
        
        BossAgent = GetComponent<NavMeshAgent>();
        BossAgent.updatePosition = false;
        BossAgent.updateRotation = false;
        
        BossRigidbody = GetComponent<Rigidbody>();
        BossRigidbody.isKinematic = true;
        
        BossTransform = transform;
        
        PlayerGameObject = GameObject.FindGameObjectWithTag("Player");
        
        IdleState = new BossIdleState(this);
        ChaseState = new BossChaseState(this);
        ChargeAttackState = new BossChargeAttackState(this);
        ShockwaveAttackState = new BossShockwaveAttackState(this);
        NormalHitState = new BossNormalHitState(this);
        AkaHitState = new BossAkaHitState(this);
        KonHitState = new BossKonHit(this);
        
        
        CurrentState = IdleState;
        CurrentState.OnEnter();
    }

    public void ChangeState(BossBaseState newState)
    {
        CurrentState.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    private void Start()
    {
        InitStateMachine();
    }

    private void OnAnimatorMove()
    {
        
        _acculmulatedDeltaPos += BossAnimator.deltaPosition;
        _acculmulatedDeltaPos.y = 0f;
        
        CurrentState.OnAnimatorMove();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     if (!IsUnderDomainExpansion)
        //     {
        //         IsUnderDomainExpansion = true;
        //         BossAnimator.speed = 0.001f;
        //     }
        //     else
        //     {
        //         IsUnderDomainExpansion = false;
        //         BossAnimator.speed = 1f;
        //     }
        // }
        
        if (!CanShockwaveAttack)
        {
            if (_shockwaveAttackTimer > 0f)
            {
                _shockwaveAttackTimer -= Time.deltaTime * BossAnimator.speed;
            }
            else
            {
                CanShockwaveAttack = true;
                _shockwaveAttackTimer = _shockwaveAttackCooldown;
            }
        }

        if (!IsUnderDomainExpansion)
        {
            CurrentState.OnUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (_acculmulatedDeltaPos != Vector3.zero)
        {
            Vector3 targetPos = BossRigidbody.position + _acculmulatedDeltaPos;
            BossRigidbody.MovePosition(targetPos);
            
            BossAgent.nextPosition = targetPos;
            _acculmulatedDeltaPos = Vector3.zero;
        }
        
        
        CurrentState.OnFixedUpdate();
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.name);
        CurrentState.OnCollisionEnter(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);   
    }

    public void ResetLocomotionValue(float duration)
    {
        StartCoroutine(LocomotionValueResetCoroutine(duration));
    }

    IEnumerator LocomotionValueResetCoroutine(float duration)
    {
        float startValue = BossAnimator.GetFloat(_locomotionId);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float newValue = Mathf.Clamp01(startValue * (1 - Mathf.Clamp01(elapsedTime / duration))); 
            BossAnimator.SetFloat(_locomotionId, newValue);
            elapsedTime += Time.deltaTime * BossAnimator.speed;
            yield return null;
        }
        BossAnimator.SetFloat(_locomotionId, 0f);
    }
}

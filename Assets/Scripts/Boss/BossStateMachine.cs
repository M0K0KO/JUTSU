using System;
using UnityEngine;
using UnityEngine.AI;

public class BossStateMachine : MonoBehaviour, IDamgeable
{
    [HideInInspector] public Animator BossAnimator { get; private set; }
    [HideInInspector] public NavMeshAgent BossAgent { get; private set; }
    [HideInInspector] public Transform BossTransform { get; private set; }
    
    public GameObject PlayerGameObject { get; private set; }
    
    
    public BossIdleState IdleState { get; private set; }
    public BossChaseState ChaseState { get; private set; }
    public BossChargeAttackState ChargeAttackState { get; private set; }

    public BossBaseState CurrentState { get; private set; }

    private Vector2 _velocity;
    private Vector2 _smoothDeltaPosition;

    private void InitStateMachine()
    {
        BossAnimator = GetComponent<Animator>();
        BossAnimator.applyRootMotion = true;
        
        BossAgent = GetComponent<NavMeshAgent>();
        BossAgent.updatePosition = false;
        BossAgent.updateRotation = false;
        
        BossTransform = transform;
        
        PlayerGameObject = GameObject.FindGameObjectWithTag("Player");
        
        IdleState = new BossIdleState(this);
        ChaseState = new BossChaseState(this);
        ChargeAttackState = new BossChargeAttackState(this);
        
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
        Vector3 newPos = transform.position + BossAnimator.deltaPosition;
        newPos.y = BossAgent.nextPosition.y;
        transform.position = newPos;
        BossAgent.nextPosition = newPos;
        
        CurrentState.OnAnimatorMove();
    }

    private void Update()
    {
        CurrentState.OnUpdate();
    }

    private void FixedUpdate()
    {
        CurrentState.OnFixedUpdate();
    }

    private void OnCollisionEnter(Collision other)
    {
        CurrentState.OnCollisionEnter(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);   
    }

    public void TakeDamage(bool shouldPlayHitReaction = false, GestureType gestureType = GestureType.None)
    {
        
    }
}

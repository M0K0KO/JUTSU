using System;
using UnityEngine;
using UnityEngine.AI;

public class BossStateMachine : MonoBehaviour
{
    [HideInInspector] public Animator BossAnimator { get; private set; }
    [HideInInspector] public NavMeshAgent BossAgent { get; private set; }
    [HideInInspector] public Transform BossTransform { get; private set; }
    
    public GameObject PlayerGameObject { get; private set; }
    
    
    public BossIdleState IdleState { get; private set; }
    public BossStrafeState StrafeState { get; private set; }

    public BossBaseState CurrentState { get; private set; }
   

    private void InitStateMachine()
    {
        BossAnimator = GetComponent<Animator>();
        BossAgent = GetComponent<NavMeshAgent>();
        BossTransform = transform;
        PlayerGameObject = GameObject.FindGameObjectWithTag("Player");
        
        IdleState = new BossIdleState(this);
        StrafeState = new BossStrafeState(this);
        
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
}

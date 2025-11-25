using UnityEngine;
using UnityEngine.AI;

public class BossDeathState : BossBaseState
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private BossManager _manager;
    
    private int _deadHash = Animator.StringToHash("DeadTrigger");
    private float _enterTime = 0f;
    private bool _loadingScene = false;
    
    public BossDeathState(BossStateMachine stateMachine) : base(stateMachine)
    {
        _animator = stateMachine.BossAnimator;
        _agent = stateMachine.BossAgent;
        _manager = stateMachine.Manager;
    }

    public override void OnEnter()
    {
        _enterTime = Time.time;
        _agent.ResetPath();
        _animator.SetTrigger(_deadHash);
    }

    public override void OnUpdate()
    {
        if (Time.time - _enterTime > 5f && !_loadingScene)
        {
            _loadingScene = true;
            _manager.LoadMainMenu();
        }
    }
}
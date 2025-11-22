using System;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;

public class BossManager : MonoBehaviour, IDamageable
{
    [HideInInspector] public BossStateMachine StateMachine { get; private set; }
    [HideInInspector] public BossSoundEffect SoundEffect { get; private set; }
    [SerializeField] private BossHand leftHand;
    [SerializeField] private BossHand rightHand;
    [SerializeField] private GameObject shockwaveSphere;
    
    [SerializeField] private CinemachineImpulseSource shockWaveImpulseSource;

    private bool _shockwaveHitPlayer = false;
    private float _shockwaveHitWidth = 0.5f;

    private void Awake()
    {
        StateMachine = GetComponent<BossStateMachine>();
        
        
        leftHand.OnBossHandPlayerTriggerEnter += OnLeftHandPlayerTriggerEnter;
        rightHand.OnBossHandPlayerTriggerEnter += OnRightHandPlayerTriggerEnter;

        EventManager.OnJutsuActivation += OnJutsuActivation;
        EventManager.OnAkaHit += OnAkaHit;
    }

    private void OnDestroy()
    {
        leftHand.OnBossHandPlayerTriggerEnter -= OnLeftHandPlayerTriggerEnter;
        rightHand.OnBossHandPlayerTriggerEnter -= OnRightHandPlayerTriggerEnter;

        EventManager.OnJutsuActivation -= OnJutsuActivation;
        EventManager.OnAkaHit -= OnAkaHit;
    }

    private void Update()
    {
        
#if UNITY_EDITOR
        // if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        //     if (StateMachine.CurrentState != StateMachine.ChargeAttackState &&
        //         StateMachine.CurrentState != StateMachine.ShockwaveAttackState)
        //     {
        //         StateMachine.ChangeState(StateMachine.NormalHitState);
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.U))
        {
            StateMachine.AkaInitialDirection = transform.position - StateMachine.PlayerGameObject.transform.position;
            StateMachine.AkaDuration = 1.5f;
            StateMachine.ChangeState(StateMachine.AkaHitState);
        }
#endif
        
        CheckShockwaveHit();
    }

    private void OnJutsuActivation(GestureType gestureType)
    {
        switch (gestureType)
        {
            //TODO: If gesture type is domain expansion infinite void, change animator speed
        }
    }

    private void OnAkaHit(Vector3 initialDirection, float duration, float projectileSpeed)
    {
        StateMachine.AkaInitialDirection = initialDirection;
        StateMachine.AkaDuration = duration;
        StateMachine.ChangeState(StateMachine.AkaHitState);
    }

    public void StartShockwave()
    {
        if (StateMachine.CurrentState != StateMachine.ShockwaveAttackState) return;
        
        Renderer sphereRenderer = shockwaveSphere.GetComponent<Renderer>();

        _shockwaveHitPlayer = false;
        
        Vector3 currentPosition = transform.position;
        currentPosition.y = 0f;
        sphereRenderer.material.SetFloat("_Opacity", 1f);
        shockwaveSphere.transform.position = currentPosition;
        shockwaveSphere.transform.localScale = Vector3.zero;
        shockwaveSphere.SetActive(true);
        
        Sequence shockwaveSequence = DOTween.Sequence();
        shockwaveSequence.Append(shockwaveSphere.transform.DOScale(60f, 2f).SetEase(Ease.OutSine));
        shockwaveSequence.Insert(1.6f, sphereRenderer.material.DOFloat(0f, "_Opacity", 0.4f).SetEase(Ease.OutExpo));
        shockwaveSequence.OnComplete((() =>
        {
            shockwaveSphere.SetActive(false);
        }));
    }

    private void CheckShockwaveHit()
    {
        if (shockwaveSphere.activeInHierarchy && !_shockwaveHitPlayer)
        {
            float currentRadius = shockwaveSphere.transform.localScale.x / 2f;
            Vector3 shockwavePosition = shockwaveSphere.transform.position;
            Vector3 toPlayer = StateMachine.PlayerGameObject.transform.position - shockwavePosition;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            if (Mathf.Abs(currentRadius - distance) <= _shockwaveHitWidth)
            {
                _shockwaveHitPlayer = true;
                // Debug.Log("Shockwave Hit!");
                if (StateMachine.PlayerGameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.TakeDamage(true, GestureType.None, shockwavePosition);
                }
            }
        }
    }

    private void OnLeftHandPlayerTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(true, GestureType.None, other.ClosestPoint(leftHand.transform.position));
            leftHand.HandCollider.enabled = false;
        }
    }

    private void OnRightHandPlayerTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(true, GestureType.None, other.ClosestPoint(rightHand.transform.position));
            rightHand.HandCollider.enabled = false;
        }
    }

    public void TakeDamage(bool shouldPlayHitReaction, GestureType gestureType, Vector3 hitPoint)
    {
        switch (gestureType)
        {
            case GestureType.None:
            {
                if (StateMachine.CurrentState != StateMachine.ChaseState &&
                    StateMachine.CurrentState != StateMachine.IdleState)
                    return;
                
                if (shouldPlayHitReaction)
                {
                    StateMachine.ChangeState(StateMachine.NormalHitState);
                }
                break;
            }
            
        }
    }

    public void PlayShockwaveImpulse()
    {
        if (StateMachine.CurrentState != StateMachine.ShockwaveAttackState) return;
        shockWaveImpulseSource.GenerateImpulse();
    }

    public void EnableLeftHandTrigger()
    {
        if (StateMachine.CurrentState != StateMachine.ChargeAttackState) return;
        leftHand.HandCollider.enabled = true;
    }

    public void DisableLeftHandTrigger()
    {
        leftHand.HandCollider.enabled = false;
    }

    public void EnableRightHandTrigger()
    {
        if (StateMachine.CurrentState != StateMachine.ChargeAttackState) return;
        rightHand.HandCollider.enabled = true;
    }

    public void DisableRightHandTrigger()
    {
        rightHand.HandCollider.enabled = false;
    }
    
}

using System;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour, IDamageable
{
    [HideInInspector] public BossStateMachine StateMachine { get; private set; }
    [HideInInspector] public BossSoundEffect SoundEffect { get; private set; }
    [SerializeField] private BossHand leftHand;
    [SerializeField] private BossHand rightHand;
    [SerializeField] private GameObject shockwaveSphere;
    [Header("Impulse Sources")]
    public CinemachineImpulseSource shockWaveImpulseSource;
    public CinemachineImpulseSource akaWallEndImpulseSource;
    public CinemachineImpulseSource akaNormalEndImpulseSource;
    public CinemachineImpulseSource konHitImpulseSource;
    public CinemachineImpulseSource muryokushoEndImpulseSource;

    public CinemachineBasicMultiChannelPerlin baseCamPerlin;
    public CinemachineBasicMultiChannelPerlin strafeCamPerlin;
    public CinemachineBasicMultiChannelPerlin skillCamPerlin;

    private bool _shockwaveHitPlayer = false;
    private float _shockwaveHitWidth = 0.5f;
    
    private BlackScreenFade _blackScreenFade;
    
    public bool IsKonJutsuActive { get; private set; } = false;

    public int AkaHitCount { get; set; } = 0;
    public int KonHitCount { get; set; } = 0;
    public int MuryokushoHitCount { get; set; } = 0;


    [HideInInspector] public AkaManager BossHitAkaManager { get; set; }

    private void Awake()
    {
        StateMachine = GetComponent<BossStateMachine>();
        
        _blackScreenFade = FindFirstObjectByType<BlackScreenFade>();
        
        SetPerlinNoiseAmplitude(0f);
        
        leftHand.OnBossHandPlayerTriggerEnter += OnLeftHandPlayerTriggerEnter;
        rightHand.OnBossHandPlayerTriggerEnter += OnRightHandPlayerTriggerEnter;

        EventManager.OnJutsuActivation += OnJutsuActivation;
        EventManager.OnAkaHit += OnAkaHit;
        EventManager.OnMuryokushoStart += OnMuryokushoStart;
        EventManager.OnMuryokushoEnd += OnMuryokushoEnd;
    }

    private void OnDestroy()
    {
        leftHand.OnBossHandPlayerTriggerEnter -= OnLeftHandPlayerTriggerEnter;
        rightHand.OnBossHandPlayerTriggerEnter -= OnRightHandPlayerTriggerEnter;

        EventManager.OnJutsuActivation -= OnJutsuActivation;
        EventManager.OnAkaHit -= OnAkaHit;
        EventManager.OnMuryokushoStart -= OnMuryokushoStart;
        EventManager.OnMuryokushoEnd -= OnMuryokushoEnd;
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

    public void SetPerlinNoiseAmplitude(float amplitude)
    {
        baseCamPerlin.AmplitudeGain = amplitude;
        strafeCamPerlin.AmplitudeGain = amplitude;
        skillCamPerlin.AmplitudeGain = amplitude;
    }

    private void OnMuryokushoStart()
    {
        StateMachine.IsUnderDomainExpansion = true;
        StateMachine.BossAnimator.speed = 0.01f;
        
    }

    private void OnMuryokushoEnd()
    {
        Debug.Log("MuryokushoEnd");
        StateMachine.IsUnderDomainExpansion = false;
        StateMachine.BossAnimator.speed = 1f;
        muryokushoEndImpulseSource.GenerateImpulse();
        StateMachine.ChangeState(StateMachine.MuryokushoEndState);
    }
    
    private void OnJutsuActivation(GestureType gestureType)
    {
        switch (gestureType)
        {
            case GestureType.Aka:
            {
                AkaHitCount++;
                break;
            }
            case GestureType.Kon:
            {
                KonHitCount++;
                IsKonJutsuActive = true;
                break;
            }
            case GestureType.Muryokusho:
            {
                MuryokushoHitCount++;
                break;
            }
        }
    }

    public bool ShouldTransitionToDeathState()
    {
        bool allHitLeastOnce = (AkaHitCount >= 1 && KonHitCount >= 1 && MuryokushoHitCount >= 1);
        
        bool anyHitTwice = (AkaHitCount >= 2 || KonHitCount >= 2 || MuryokushoHitCount >= 2);
        
        return allHitLeastOnce && anyHitTwice;
    }

    private void OnAkaHit(Vector3 initialDirection, float duration, float projectileSpeed)
    {
        StateMachine.AkaInitialDirection = initialDirection;
        StateMachine.AkaDuration = duration;
        StateMachine.AkaSpeed = projectileSpeed * 2f;
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
        shockwaveSequence.Append(shockwaveSphere.transform.DOScale(140f, 2f).SetEase(Ease.OutSine));
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
        Debug.Log("LeftHandPlayerTriggerEnter");
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(true, GestureType.None, transform.position);
            leftHand.HandCollider.enabled = false;
        }
    }

    private void OnRightHandPlayerTriggerEnter(Collider other)
    {
        Debug.Log("RightHandPlayerTriggerEnter");
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(true, GestureType.None, transform.position);
            rightHand.HandCollider.enabled = false;
        }
    }

    public void TakeDamage(bool shouldPlayHitReaction, GestureType gestureType, Vector3 hitPoint)
    {
        // if (StateMachine.IsUnderDomainExpansion) return;
        Debug.Log($"TakeDamage! hit react: {shouldPlayHitReaction}, gesture: {gestureType}");
        switch (gestureType)
        {
            case GestureType.None:
            {
                if (StateMachine.CurrentState != StateMachine.ChaseState &&
                    StateMachine.CurrentState != StateMachine.IdleState)
                {
                    Debug.Log($"Current State: {StateMachine.CurrentState.GetType().Name}");
                    return;
                }
                    
                
                if (shouldPlayHitReaction)
                {
                    StateMachine.ChangeState(StateMachine.NormalHitState);
                }
                break;
            }
            case GestureType.Kon:
            {
                IsKonJutsuActive = false;
                StateMachine.ChangeState(StateMachine.KonHitState);
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

    public void LoadExecutionScene()
    {
        MusicController musicController = FindFirstObjectByType<MusicController>();

        musicController.MusicAudioSource.DOFade(0, 2f);
        _blackScreenFade.FadeOut(2f, () =>
        {
            SceneManager.LoadSceneAsync("Boss Execution Scene", LoadSceneMode.Single);
        });

    }
    
}

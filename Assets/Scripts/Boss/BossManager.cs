using System;
using UnityEngine;

public class BossManager : MonoBehaviour, IDamgeable
{
    [HideInInspector] public BossStateMachine StateMachine { get; private set; }

    private void Awake()
    {
        StateMachine = GetComponent<BossStateMachine>();
    }

    public void TakeDamage(bool shouldPlayHitReaction = false, GestureType gestureType = GestureType.None)
    {
        
    }
}

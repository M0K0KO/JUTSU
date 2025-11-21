using System;
using UnityEngine;

public class BossHand : MonoBehaviour
{
    public event Action<Collider> OnBossHandPlayerTriggerEnter;

    public Collider HandCollider { get; private set; }

    private void Awake()
    {
        HandCollider = GetComponent<Collider>();
        HandCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnBossHandPlayerTriggerEnter?.Invoke(other);
        }
    }
}

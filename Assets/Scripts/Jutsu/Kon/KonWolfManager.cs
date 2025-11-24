using System;
using UnityEngine;

public class KonWolfManager : MonoBehaviour
{
    private Collider _wolfCollider;

    private void Awake()
    {
        _wolfCollider = GetComponentInChildren<Collider>();
        _wolfCollider.enabled = false;
    }

    public void EnableWolfCollider()
    {
        _wolfCollider.enabled = true;
    }

    public void DisableWolfCollider()
    {
        _wolfCollider.enabled = false;
    }

    public void OnWolfTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Boss"))
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(true, GestureType.Kon, other.ClosestPoint(transform.position));
            }
        }
    }
}

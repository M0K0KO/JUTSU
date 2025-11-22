using System;
using UnityEngine;

public class KonWolfManager : MonoBehaviour
{
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

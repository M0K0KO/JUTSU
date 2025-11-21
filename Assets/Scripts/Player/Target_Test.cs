using System;
using System.Collections;
using UnityEngine;

interface IDamageable_TEST
{
    public void OnDamage(float damage);
}

public class Target_Test : MonoBehaviour, IDamageable_TEST
{
    public GameObject cameraTarget;
    public Transform hitTarget;

    public void OnDamage(float damage)
    {
        
    }
}

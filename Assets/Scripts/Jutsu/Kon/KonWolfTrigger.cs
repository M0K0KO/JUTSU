using System;
using UnityEngine;

public class KonWolfTrigger : MonoBehaviour
{
    private KonWolfManager _manager;
    private void Awake()
    {
        _manager = GetComponentInParent<KonWolfManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _manager.OnWolfTriggerEnter(other);
    }
}

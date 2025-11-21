using System;
using UnityEngine;

public class AkaManager : MonoBehaviour
{
    public bool isHit { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss"))
        {
            isHit = true;
            
            transform.SetParent(other.transform);
        }
    }
}

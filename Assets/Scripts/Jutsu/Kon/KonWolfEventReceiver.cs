using UnityEngine;
using Unity.Cinemachine; 

public class KonWolfEventReceiver : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public void KonImpulse() 
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
    }
}
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraImpulseManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource attackImpulse;

    [SerializeField] private CinemachineImpulseSource konRumbleImpulse;

    [SerializeField] private CinemachineImpulseSource konImpulse;

    public void AttackImpulse()
    {
        attackImpulse.GenerateImpulse();
    }

    public void KonImpulse()
    {
        konImpulse.GenerateImpulse();
    }

    public void KonRumbleImpulse()
    {
        konRumbleImpulse.GenerateImpulse();
    }
}

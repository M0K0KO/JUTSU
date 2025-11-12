using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraImpulseManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource attackImpulse;

    public void AttackImpulse()
    {
        attackImpulse.GenerateImpulse();
    }
}

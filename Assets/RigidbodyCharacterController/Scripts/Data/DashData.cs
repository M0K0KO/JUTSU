using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerDashData", menuName = "Scriptable Objects/PlayerDashData")]
public class DashData : ScriptableObject
{
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float dashCoolDown = 1f;
    
    [Header("Multi-Dash Properties")]
    public int MaxDashes = 1;
    public float dashChainWindow = 0.3f;
    public float dashChainCoyoteTime = 0.2f;
}

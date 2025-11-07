using UnityEngine;

[CreateAssetMenu(fileName = "JumpData", menuName = "Scriptable Objects/JumpData")]
public class JumpData : ScriptableObject
{
    [Header("Jump")] 
    public int maxJumps = 1;
    public float CoyoteTime = 0.15f;
    public float JumpBufferTime = 0.15f;
    public float JumpForce = 10f;
}

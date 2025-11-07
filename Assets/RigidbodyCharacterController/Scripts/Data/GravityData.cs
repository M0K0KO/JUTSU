using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GravityData", menuName = "Scriptable Objects/GravityData")]
public class GravityData : ScriptableObject
{
    [Tooltip("Gravity Value")]
    public float Gravity = -30f;

    [Tooltip("종단 속도")]
    public float MaxFallSpeed = -50f;

    [Tooltip("미끄러지는 속도")] 
    public float SlideSpeed = 10f;
}

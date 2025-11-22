using UnityEngine;

[CreateAssetMenu(fileName = "KonSequenceData", menuName = "SO/KonSequenceData", order = 1)]
public class KonSequenceData : ScriptableObject
{
    public Vector3 spawnOffset;
    public float rumbleDuration;
    public AnimationCurve animationPlaybackSpeedCurve;
}

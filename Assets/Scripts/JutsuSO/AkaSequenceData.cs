using UnityEngine;

[CreateAssetMenu(fileName = "AkaSequenceData", menuName = "SO/AkaSequenceData", order = 1)]
public class AkaSequenceData : ScriptableObject
{
    [Header("Aka")] 
    public float waitDuration;
    public float pushDuration;
    public float akaLerpSpeed;
    public float akaSpeed;
}

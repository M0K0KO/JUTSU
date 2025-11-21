using UnityEngine;

[CreateAssetMenu(fileName = "Jutsu", menuName = "SO/Jutsu")]
public class Jutsu : ScriptableObject
{
    public GestureType gestureType;
    public string targetCommand;
}

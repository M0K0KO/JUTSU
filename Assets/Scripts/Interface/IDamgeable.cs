using UnityEngine;

public interface IDamgeable
{
    public void TakeDamage(bool shouldPlayHitReaction = false, GestureType gestureType = GestureType.None);
}

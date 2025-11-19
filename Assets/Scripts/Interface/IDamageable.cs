using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(bool shouldPlayHitReaction = false, GestureType gestureType = GestureType.None);
}

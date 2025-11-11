using UnityEngine;

public class PlayerVFXManager : MonoBehaviour
{
    private PlayerManager player;

    [SerializeField] private Transform muzzleFlashPos;

    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    public void PlayMuzzleFlash()
    {
        GameObject obj = MuzzleFlashObjectPool.instance.pool.Get();
        obj.transform.position = muzzleFlashPos.position;
        obj.transform.rotation = muzzleFlashPos.rotation;
    }

    public void SpawnBulletTrail()
    {
        GameObject obj = BulletTrailObjectPool.instance.pool.Get();
        
        Vector3 origin = muzzleFlashPos.position;

        Vector3 destination = origin;
        if (player.stateMachine.isStrafing)
        {
            destination = player.stateMachine.currentTargetHitTarget.transform.position;
        }
        else
        {
            if (Physics.Raycast(origin, player.transform.forward, out RaycastHit hit, Mathf.Infinity))
            {
                destination = hit.point;
            }
            else
            {
                destination = origin + player.transform.forward * 100f;
            }
        }
        destination += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),Random.Range(-0.1f, 0.1f));
        
        obj.GetComponent<BulletTrailManager>().SetPosition(origin, destination);
    }
}

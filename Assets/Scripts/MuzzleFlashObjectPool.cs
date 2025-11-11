using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class MuzzleFlashObjectPool : MonoBehaviour
{
    public static MuzzleFlashObjectPool instance;
    
    [SerializeField] private GameObject muzzleFlashPrefab;

    public IObjectPool<GameObject> pool { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        pool = new ObjectPool<GameObject>(
            createFunc : CreateItem,
            actionOnGet : OnGet,
            actionOnRelease : OnRelease,
            actionOnDestroy : OnDestroyItem,
            collectionCheck : true,
            defaultCapacity : 10,
            maxSize : 50
        );
    }

    private GameObject CreateItem()
    {
        GameObject obj = Instantiate(muzzleFlashPrefab);
        obj.SetActive(false);
        return obj;
    }
    
    private void OnGet(GameObject obj)
    {
        obj.SetActive(true);
        StartCoroutine(ReturnAfter(obj, 0.1f));
    }
    
    private void OnRelease(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyItem(GameObject obj)
    {
        Destroy(obj);
    }

    private IEnumerator ReturnAfter(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pool.Release(obj);
    }
}

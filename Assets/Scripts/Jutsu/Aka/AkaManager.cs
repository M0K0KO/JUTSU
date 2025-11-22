using System.Collections;
using UnityEngine;

public class AkaManager : MonoBehaviour
{
    [SerializeField] private JutsuAudioSourceHolder jutsuAudioSourceHolder;
    
    public bool isHit { get; private set; } = false;

    [SerializeField] private GameObject akaVFX;
    [SerializeField] private GameObject[] explosionVFXs;
    [SerializeField] private Material sphereMaterial;

    private float originalIntensity = 10.5f;
    private float targetIntensity = 1f;

    private void Start()
    {
        jutsuAudioSourceHolder.sfxDict["Activation"].PlayAudioClip();
        sphereMaterial.SetFloat("_Intensity",originalIntensity);

        StartCoroutine(LerpEmerge());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss")) 
        {
            isHit = true;
            if (other.TryGetComponent(out BossManager bossManager))
            {
                bossManager.BossHitAkaManager = this;
                transform.SetParent(other.transform, true);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Explode();
        }
    }

    public void Explode()
    {
        akaVFX.SetActive(false);

        jutsuAudioSourceHolder.sfxDict["Explosion"].PlayAudioClip();
        
        foreach (var vfx in explosionVFXs)
        {
            vfx.SetActive(true);
            vfx.GetComponent<ParticleSystem>().Play();
        }
    }

    private IEnumerator LerpEmerge()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.unscaledDeltaTime;
            sphereMaterial.SetFloat("_Intensity", 
                Mathf.Lerp(targetIntensity, originalIntensity, elapsedTime / 2f));
            yield return null;
        }

        yield return null;
    }
}

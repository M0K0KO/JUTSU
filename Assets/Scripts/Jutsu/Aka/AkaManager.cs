using System;
using UnityEngine;

public class AkaManager : MonoBehaviour
{
    public bool isHit { get; private set; } = false;


    [SerializeField] private GameObject akaVFX;
    
    [SerializeField] private GameObject[] explosionVFXs;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss")) 
        {
            isHit = true;
        }
    }

    public void Explode()
    {
        akaVFX.SetActive(false);

        foreach (var vfx in explosionVFXs)
        {
            vfx.SetActive(true);
            vfx.GetComponent<ParticleSystem>().Play();
        }
    }
}

using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class EndingBoss : MonoBehaviour
{
    private Animator _executionBossAnimator;
    [SerializeField] private GameObject coreObject;

    private void Start()
    {
        coreObject.transform.localScale = Vector3.zero;
        coreObject.SetActive(false);   
        _executionBossAnimator = GetComponent<Animator>();
    }
    
    private void Update()
    {
#if UNITY_EDITOR        
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartChibakuTensei();
        }
#endif
    }
    


    public void StartChibakuTensei()
    {
        coreObject.SetActive(true);
        coreObject.transform.DOScale(3f, 2f);
        _executionBossAnimator.SetTrigger("Falling");

    }
    
    
}

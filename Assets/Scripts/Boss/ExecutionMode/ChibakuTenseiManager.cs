using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChibakuTenseiManager : MonoBehaviour
{
    [SerializeField] private GameObject coreParticleObject;
    [SerializeField] private Transform targetTransform;

    [SerializeField] private Transform sphereRockParent;
    [SerializeField] private Transform groundRockParent;
    
    private List<GameObject> _sortedSphereRocks = new List<GameObject>();
    private int _currentRockIndex = 0;
    
    private Rigidbody[] _groundRockRigidbodies;

    public bool GravityActive { get; set; } = false;
    
    private void Start()
    {
        InitializeSphereRocks();

        _groundRockRigidbodies = groundRockParent.GetComponentsInChildren<Rigidbody>();
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

    private void FixedUpdate()
    {
        if (GravityActive)
        {
            for (int i = 0; i < _groundRockRigidbodies.Length; i++)
            {
                Rigidbody rb = _groundRockRigidbodies[i];
                if (!rb || !rb.gameObject.activeInHierarchy) continue;
                Vector3 direction = (targetTransform.position - rb.position).normalized;

                rb.AddForce(direction * 4f, ForceMode.Acceleration);
            }
        }
        
    }

    private void InitializeSphereRocks()
    {
        foreach (Transform child in sphereRockParent)
        {
            _sortedSphereRocks.Add(child.gameObject);
        }
        
        _sortedSphereRocks.Sort((a, b) => 
            a.transform.localPosition.sqrMagnitude.CompareTo(b.transform.localPosition.sqrMagnitude));

        foreach (var rock in _sortedSphereRocks)
        {
            rock.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GroundRock"))
        {
            if (_currentRockIndex < _sortedSphereRocks.Count)
            {
                other.gameObject.SetActive(false);
                
                GameObject targetRock = _sortedSphereRocks[_currentRockIndex];
                targetRock.SetActive(true);
                // Vector3 savedScale = targetRock.transform.localScale;
                // targetRock.transform.localScale = Vector3.zero;
                // targetRock.transform.DOScale(savedScale, 0.5f).SetEase(Ease.InExpo);
                
                _currentRockIndex++;
            }
        }
    }

    public void StartChibakuTensei()
    {
        coreParticleObject.SetActive(true);
        coreParticleObject.transform.DOScale(3f, 2f);

        transform.DOMove(targetTransform.position, 3f);
        GravityActive = true;
    }

    
}

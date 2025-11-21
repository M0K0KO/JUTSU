using System;
using System.Collections;
using UnityEngine;

public class BulletTrailManager : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Material lineMaterial;
    private Color originColor;

    [SerializeField] private float initialAlpha = 0.8f;
    [SerializeField] private float fadeDuration = 0.4f;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;
        originColor = lineMaterial.color;
    }

    private void OnEnable()
    {
        lineRenderer.enabled = true;
        lineMaterial.color = new Color(originColor.r, originColor.g, originColor.b, initialAlpha);
        StartCoroutine(Fade());
    }

    public void SetPosition(Vector3 origin, Vector3 destination)
    {
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, destination);
    }

    private IEnumerator Fade()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            lineMaterial.color = new Color(originColor.r, originColor.g, originColor.b, initialAlpha * (1 - (elapsedTime / fadeDuration)));
            yield return null;
        }
        lineRenderer.enabled = false;
    }
}

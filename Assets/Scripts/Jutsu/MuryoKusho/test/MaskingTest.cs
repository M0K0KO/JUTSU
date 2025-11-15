using System.Collections;
using UnityEngine;

public class MaskingTest : MonoBehaviour
{
    [SerializeField] private Vector2 maxXZScale;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private float expansionDuration;

    private Vector3 originalScale;
    private Vector3 scaleVelocity;

    private void Start()
    {
        originalScale = Vector3.zero + Vector3.up * 0.01f;
        transform.localScale = originalScale;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            StartCoroutine(DomainExpansion());
        }
    }

    private IEnumerator DomainExpansion()
    {
        Vector3 targetScale = new Vector3(maxXZScale.x, originalScale.y, maxXZScale.y);

        float elapsedTime = 0f;
        while (elapsedTime < expansionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            Vector3 smoothedScale = Vector3.Lerp(originalScale, targetScale, scaleCurve.Evaluate(elapsedTime / expansionDuration));
            transform.localScale = smoothedScale;

            yield return null;
        }

        yield return null;
    }
}

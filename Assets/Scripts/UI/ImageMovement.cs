using DG.Tweening;
using UnityEngine;

public class ImageMovement : MonoBehaviour
{
    [SerializeField] private float radius = 20f; 
    [SerializeField] private float minTime = 2f;
    [SerializeField] private float maxTime = 4f; 

    private RectTransform _rectTransform;
    private Vector2 _originPos;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originPos = _rectTransform.anchoredPosition;
    }

    private void Start()
    {
        FloatRandomly();
    }

    void FloatRandomly()
    {
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector2 targetPos = _originPos + randomOffset;
        
        float randomDuration = Random.Range(minTime, maxTime);
        
        _rectTransform.DOAnchorPos(targetPos, randomDuration)
            .SetEase(Ease.InOutQuad) 
            .OnComplete(FloatRandomly); 
    }

    private void OnDestroy()
    {
        _rectTransform.DOKill();
    }
}

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreenFade : MonoBehaviour
{
    private Image _blackImage;

    private void Awake()
    {
        _blackImage = GetComponent<Image>();
    }

    private void Start()
    {
        FadeIn();
    }

    public void FadeIn(float duration = 2f, Action onCompleteCallback = null)
    {
        _blackImage.color = new Color(0f, 0f, 0f, 1f);
        _blackImage.enabled = true;
        _blackImage.DOFade(0f, duration).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _blackImage.enabled = false;
            onCompleteCallback?.Invoke();
        });
    }

    public void FadeOut(float duration = 2f, Action onCompleteCallback = null)
    {
        _blackImage.color = new Color(0f, 0f, 0f, 0f);
        _blackImage.enabled = true;
        _blackImage.DOFade(1f, duration).SetEase(Ease.OutSine).OnComplete(() =>
        {
            //_blackImage.enabled = false;
            onCompleteCallback?.Invoke();
        });
    }
}

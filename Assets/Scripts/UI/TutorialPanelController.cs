using System;
using DG.Tweening;
using UnityEngine;

public class TutorialPanelController : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PlayOpenAnimation();
    }

    private void OnDisable()
    {
        DOTween.Kill(_canvasGroup);
        _canvasGroup.DOKill();
    }

    private void PlayOpenAnimation()
    {
        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one * 0.9f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(1f, 0.2f).SetEase(Ease.OutCubic));
        seq.OnComplete(() =>
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        });
    }

    public void ClosePanel()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(0.9f, 0.2f).SetEase(Ease.OutCubic));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

}

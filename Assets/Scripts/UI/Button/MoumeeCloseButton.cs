using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoumeeCloseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    private AudioSource _audioSource;
    private Image _buttonBackground;
    private Image _buttonCross;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _buttonBackground = GetComponent<Image>();
    }

    private void OnEnable()
    {
        _buttonBackground.color = new Color(1f, 1f, 1f, 0f);
        _buttonCross = transform.GetChild(0).GetComponent<Image>();
        _buttonCross.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _audioSource.PlayOneShot(hoverSound);
        _buttonCross.DOColor(Color.black, 0.1f).SetEase(Ease.OutCubic);
        _buttonBackground.DOFade(1, 0.15f).SetEase(Ease.OutCubic);
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonCross.DOColor(Color.white, 0.1f).SetEase(Ease.InCubic);
        _buttonBackground.DOFade(0, 0.15f).SetEase(Ease.InCubic);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _audioSource.PlayOneShot(clickSound);

        Sequence clickSeq = DOTween.Sequence();
        clickSeq.Append(transform.DOScale(0.9f, 0.08f).SetEase(Ease.OutQuad));
        clickSeq.Join(_buttonCross.DOColor(Color.gray, 0.08f));
        clickSeq.Append(transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
        clickSeq.Join(_buttonCross.DOColor(Color.black, 0.15f));
    }
}

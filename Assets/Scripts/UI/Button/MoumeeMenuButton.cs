using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MoumeeMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    
    private Image _buttonBackground;
    private TMP_Text _buttonText;
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        _buttonBackground = GetComponent<Image>();
        _buttonBackground.fillAmount = 0;
        
        _buttonText = GetComponentInChildren<TMP_Text>();
        _buttonText.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _audioSource.PlayOneShot(hoverSound);
        _buttonText.DOColor(Color.black, 0.1f).SetEase(Ease.OutCubic);
        _buttonBackground.DOFillAmount(1f, 0.15f).SetEase(Ease.OutCubic);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // _audioSource.PlayOneShot(hoverSound);
        _buttonText.DOColor(Color.white, 0.1f).SetEase(Ease.InCubic);
        _buttonBackground.DOFillAmount(0f, 0.15f).SetEase(Ease.InCubic);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _audioSource.PlayOneShot(clickSound);
        // _buttonBackground.fillAmount = 1f;
        Color clickedColor = Color.gray;

        Sequence clickSeq = DOTween.Sequence();
        clickSeq.Append(_buttonBackground.rectTransform.DOScale(0.95f, 0.05f).SetEase(Ease.OutCubic))
            .Join(_buttonBackground.DOColor(clickedColor, 0.05f).SetEase(Ease.OutCubic))
            .AppendInterval(0.05f)
            .Append(_buttonBackground.rectTransform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
            .Join(_buttonBackground.DOColor(Color.white, 0.25f).SetEase(Ease.OutCubic))
            .Play();
    }
}

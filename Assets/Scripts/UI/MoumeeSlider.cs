using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MoumeeSlider : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioClip hoverSound;
    private TMP_Text _sliderValueText;
    private AudioSource _audioSource;
    private Slider _slider; 

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _slider = GetComponent<Slider>();
        _sliderValueText = GetComponentInChildren<TMP_Text>();
        
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _sliderValueText.text = _slider.value.ToString("F2");
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _audioSource.PlayOneShot(hoverSound);
    }

    private void OnSliderValueChanged(float value)
    {
        _sliderValueText.text = value.ToString("F2");
    }
}

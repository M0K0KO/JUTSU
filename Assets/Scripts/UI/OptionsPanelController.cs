using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsPanelController : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    
    [SerializeField] private Dropdown windowModeDropdown;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private AudioMixer audioMixer; 
    
    private const string WindowModeKey = "WindowMode";
    private const string MouseSensitivityKey = "MouseSensitivity";
    private const string MasterVolumeKey = "MasterVolume";
    private const string BgmVolumeKey = "BgmVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private Resolution[] _resolutions;
    private int _currentResolutionIndex = 0;

    public void LoadSavedOptions()
    {
        Screen.fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt(WindowModeKey, (int)Screen.fullScreenMode);
        
        // TODO: Handle Mouse Sensitivity
        
        float master = Mathf.Log10(Mathf.Max(PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f), 0.0001f)) * 20f;
        float bgm = Mathf.Log10(Mathf.Max(PlayerPrefs.GetFloat(BgmVolumeKey, 0.5f), 0.0001f)) * 20f;
        float sfx = Mathf.Log10(Mathf.Max(PlayerPrefs.GetFloat(SfxVolumeKey, 0.5f), 0.0001f)) * 20f;
        
        audioMixer.SetFloat("Master", master);
        audioMixer.SetFloat("BGM", bgm);
        audioMixer.SetFloat("SFX", sfx);
    }
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        BuildResolutionOptions();
        
        windowModeDropdown.value = PlayerPrefs.GetInt(WindowModeKey, (int)Screen.fullScreenMode);
        resolutionDropdown.value = _currentResolutionIndex;
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat(MouseSensitivityKey, 0.5f);
        masterVolumeSlider.value = PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f);
        bgmVolumeSlider.value = PlayerPrefs.GetFloat(BgmVolumeKey, 0.5f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SfxVolumeKey, 0.5f);
        
        windowModeDropdown.onValueChanged.AddListener(OnWindowModeDropdownValueChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownValueChanged);
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivitySliderValueChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderValueChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeSliderValueChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeSliderValueChanged);
        
        PlayOpenAnimation();
        
        
    }

    private void OnDisable()
    {
        windowModeDropdown.onValueChanged.RemoveListener(OnWindowModeDropdownValueChanged);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionDropdownValueChanged);
        mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivitySliderValueChanged);
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderValueChanged);
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBgmVolumeSliderValueChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeSliderValueChanged);
        
        DOTween.Kill(_canvasGroup);
        _canvasGroup.DOKill();
    }

    private void PlayOpenAnimation()
    {
        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one * 0.9f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
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

        Sequence seq = DOTween.Sequence();
        seq.Append(_canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(0.9f, 0.2f).SetEase(Ease.OutCubic));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

    private void BuildResolutionOptions()
    {
        _resolutions = Screen.resolutions
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)
            .ThenByDescending(r => (float)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator)
            .ToArray();
        resolutionDropdown.ClearOptions();

        List<string> resolutionStrings = new List<string>();

        foreach (Resolution resolution in _resolutions)
        {
            string resolutionString = $"{resolution.width} x {resolution.height} ({resolution.refreshRateRatio})";
            resolutionStrings.Add(resolutionString); 
        }
        
        resolutionDropdown.AddOptions(resolutionStrings);
    }
    
    private void OnWindowModeDropdownValueChanged(int value)
    {
        windowModeDropdown.value = value;
        PlayerPrefs.SetInt(WindowModeKey, value);
        
        Screen.fullScreenMode = (FullScreenMode)value;
    }

    private void OnResolutionDropdownValueChanged(int value)
    {
        resolutionDropdown.value = value;

        Resolution savedResolution = Screen.resolutions[value];
        Screen.SetResolution(savedResolution.width, savedResolution.height, Screen.fullScreenMode,
            savedResolution.refreshRateRatio);
    }

    private void OnMouseSensitivitySliderValueChanged(float value)
    {
        mouseSensitivitySlider.value = value;
        PlayerPrefs.SetFloat(MouseSensitivityKey, value);
    }
    
    private void OnMasterVolumeSliderValueChanged(float value)
    {
        masterVolumeSlider.value = value;
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("Master", dB);
    }

    private void OnBgmVolumeSliderValueChanged(float value)
    {
        bgmVolumeSlider.value = value;
        PlayerPrefs.SetFloat(BgmVolumeKey, value);
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("BGM", dB);
    }

    private void OnSfxVolumeSliderValueChanged(float value)
    {
        sfxVolumeSlider.value = value;
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("SFX", dB);
    }
}

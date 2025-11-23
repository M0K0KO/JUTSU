using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private AudioSource _mainMenuMusicSource;

    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject optionsPanel;

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        _mainMenuMusicSource = GetComponent<AudioSource>();
        _mainMenuMusicSource.volume = 0f;
        _mainMenuMusicSource.loop = true;
        _mainMenuMusicSource.playOnAwake = true;
        _mainMenuMusicSource.DOFade(1f, 1f).SetUpdate(true);

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.enabled = false;
        
        optionsPanel.SetActive(false);
    }

    public void StartGame()
    {
        _mainMenuMusicSource.DOFade(0f, 0.3f).OnComplete((() => _mainMenuMusicSource.Stop())).SetUpdate(true);
        fadeImage.enabled = true;
        fadeImage.DOFade(1f, 0.5f).OnComplete((() =>
        {
            SceneManager.LoadSceneAsync("Main Combat Scene", LoadSceneMode.Single);
        })).SetUpdate(true);

    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        _mainMenuMusicSource.DOFade(0f, 0.3f).OnComplete((() => _mainMenuMusicSource.Stop())).SetUpdate(true);
        fadeImage.enabled = true;
        fadeImage.DOFade(1f, 0.5f).OnComplete((() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        })).SetUpdate(true);

    }
}

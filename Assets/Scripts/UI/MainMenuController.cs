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
        _mainMenuMusicSource = GetComponent<AudioSource>();
        _mainMenuMusicSource.volume = 0f;
        _mainMenuMusicSource.loop = true;
        _mainMenuMusicSource.playOnAwake = true;
        _mainMenuMusicSource.DOFade(1f, 1f);

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.enabled = false;
        
        optionsPanel.SetActive(false);
    }

    public void StartGame()
    {
        _mainMenuMusicSource.DOFade(0f, 0.3f).OnComplete((() => _mainMenuMusicSource.Stop()));
        fadeImage.enabled = true;
        fadeImage.DOFade(1f, 0.5f).OnComplete((() =>
        {
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        }));

    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        _mainMenuMusicSource.DOFade(0f, 0.3f).OnComplete((() => _mainMenuMusicSource.Stop()));
        fadeImage.enabled = true;
        fadeImage.DOFade(1f, 0.5f).OnComplete((() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }));

    }
}

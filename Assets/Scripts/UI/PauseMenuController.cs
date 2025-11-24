using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public static PauseMenuController Instance { get; private set; }
    public bool IsPaused { get; private set; } = false;
    
    [SerializeField] private Image blackBackground;
    [SerializeField] private GameObject pauseMenuContainer;
    [SerializeField] private OptionsPanelController optionsPanelController;
    [SerializeField] private Image fadeImage;
    
    private CanvasGroup _pauseMenuContainerCanvasGroup;

    private float _savedTimeScale = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _pauseMenuContainerCanvasGroup = pauseMenuContainer.GetComponent<CanvasGroup>();
        
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.enabled = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnPauseInputReceived()
    {
        if (IsPaused)
        {
            if (optionsPanelController.gameObject.activeInHierarchy)
            {
                optionsPanelController.ClosePanel();
            }
            else
            {
                ResumeGame();
            }
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        blackBackground.enabled = true;

        _pauseMenuContainerCanvasGroup.interactable = false;
        _pauseMenuContainerCanvasGroup.blocksRaycasts = false;
        _pauseMenuContainerCanvasGroup.alpha = 0f;
        pauseMenuContainer.SetActive(true);

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_pauseMenuContainerCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));
        seq.OnComplete(() =>
        {
            _pauseMenuContainerCanvasGroup.interactable = true;
            _pauseMenuContainerCanvasGroup.blocksRaycasts = true;
        });

        AudioListener.pause = true;

        _savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        blackBackground.enabled = false;

        _pauseMenuContainerCanvasGroup.interactable = false;
        _pauseMenuContainerCanvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(_pauseMenuContainerCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutCubic));
        seq.OnComplete(() => pauseMenuContainer.SetActive(false));

        AudioListener.pause = false;
        
        Time.timeScale = _savedTimeScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        OnGameResumed?.Invoke();
    }

    public void ExitGame()
    {
        fadeImage.enabled = true;
        fadeImage.DOFade(1f, 0.5f).OnComplete(() =>
        {
            SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        }).SetUpdate(true);
        
    }
    
}


using UnityEngine;

public class GameSceneUIController : MonoBehaviour
{

    [Header("Refs")]
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private ResultScreenView resultScreenView;

    [Header("Screen Roots")]
    [SerializeField]
    private GameObject startScreenRoot;

    [SerializeField]
    private GameObject ruleScreenRoot;

    [SerializeField]
    private GameObject resultScreenRoot;

    [Header("Gameplay Root")]
    [SerializeField]
    private GameObject gameplayRoot;

    private void Awake()
    {

        if (gameManager == null)
        {

            gameManager = FindFirstObjectByType<GameManager>();

        }

    }

    private void OnEnable()
    {

        if (gameManager != null)
        {

            gameManager.OnGameFinished += HandleGameFinished;

        }

    }

    private void OnDisable()
    {

        if (gameManager != null)
        {

            gameManager.OnGameFinished -= HandleGameFinished;

        }

    }

    private void Start()
    {

        ShowStartScreen();

    }

    public void OnClickStartGame()
    {

        ShowGameplay();

        if (gameManager != null)
        {

            gameManager.StartNewSessionFromMainMenu();

        }

        if (SoundManager.Instance != null)
        {

            SoundManager.Instance.LowerBGM();

        }

    }

    public void OnClickStartNextGame()
    {

        ShowGameplay();

        if (gameManager != null)
        {

            gameManager.StartNextGame();

        }

    }

    public void OnClickRule()
    {

        ShowRuleScreen();

    }

    public void OnClickBackToTitle()
    {

        ShowStartScreen();

        if (SoundManager.Instance != null)
        {

            SoundManager.Instance.RestoreBGM();

        }

    }

    public void OnClickCloseRule()
    {

        ShowStartScreen();

    }

    public void OnClickQuitGame()
    {

        Application.Quit();

    }

    private void HandleGameFinished()
    {

        if (resultScreenView != null && gameManager != null)
        {

            resultScreenView.Refresh(gameManager.Players);

        }

        ShowResultScreen();

    }

    private void ShowStartScreen()
    {

        SetActiveSafe(startScreenRoot, true);
        SetActiveSafe(ruleScreenRoot, false);
        SetActiveSafe(resultScreenRoot, false);

        SetGameplayRootActive(false);

    }

    private void ShowRuleScreen()
    {

        SetActiveSafe(startScreenRoot, false);
        SetActiveSafe(ruleScreenRoot, true);
        SetActiveSafe(resultScreenRoot, false);

        SetGameplayRootActive(false);

    }

    private void ShowGameplay()
    {

        SetActiveSafe(startScreenRoot, false);
        SetActiveSafe(ruleScreenRoot, false);
        SetActiveSafe(resultScreenRoot, false);

        SetGameplayRootActive(true);

    }

    private void ShowResultScreen()
    {

        SetActiveSafe(startScreenRoot, false);
        SetActiveSafe(ruleScreenRoot, false);
        SetActiveSafe(resultScreenRoot, true);

        SetGameplayRootActive(false);

    }

    private void SetGameplayRootActive(bool value)
    {

        if (gameplayRoot == null) return;

        gameplayRoot.SetActive(value);

    }

    private void SetActiveSafe(GameObject target, bool value)
    {

        if (target == null) return;

        target.SetActive(value);

    }

}
using UnityEngine;

public class GameKeyboardInput : MonoBehaviour
{

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private PlayerState humanPlayer;

    private bool secretActivated = false;

    private void Awake()
    {

        if (gameManager == null)
        {

            gameManager = FindFirstObjectByType<GameManager>();

        }

    }

    private void Update()
    {

        if (gameManager == null) return;

        //if (Input.GetKeyDown(KeyCode.S))
        //{

        //    gameManager.StartGame();

        //}

        //if (Input.GetKeyDown(KeyCode.R))
        //{

        //    gameManager.StartNextGame();

        //}

        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.H) && Input.GetKeyDown(KeyCode.W))
        {

            secretActivated = !secretActivated;

            gameManager.SetForceHumanStart(secretActivated);

            Debug.Log($"비밀 기능 {(secretActivated ? "ON" : "OFF")}");

        }

        if (Input.GetKeyDown(KeyCode.P))
        {

            PlayerState currentPlayer = gameManager.GetCurrentPlayer();

            if (currentPlayer != null && currentPlayer.isHuman)
            {

                gameManager.PassCurrentPlayer();

            }

        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {

            HandleSubmitInput();

        }

    }

    private void HandleSubmitInput()
    {

        if (gameManager == null) return;

        // 계급 교환 단계 우선 처리
        if (gameManager.IsHierarchyExchangePending())
        {

            if (humanPlayer == null)
            {

                Debug.Log("[GameKeyboardInput] humanPlayer가 비어 있습니다.");
                return;

            }

            if (!gameManager.CanSelectCardsForHierarchyExchange(humanPlayer))
            {

                Debug.Log("[GameKeyboardInput] 지금은 인간 플레이어가 계급 교환 카드를 선택하는 단계가 아닙니다.");
                return;

            }

            gameManager.SubmitSelectedCards(humanPlayer);
            return;

        }

        // 일반 턴 제출 처리
        PlayerState currentPlayer = gameManager.GetCurrentPlayer();

        if (currentPlayer == null) return;

        if (!currentPlayer.isHuman) return;

        gameManager.SubmitSelectedCards(currentPlayer);

    }

}


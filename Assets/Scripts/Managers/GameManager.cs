using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using Unity.Multiplayer.PlayMode;
using System;

// 게임 흐름 관리
// 시작 / 분배 / 턴 순서 결정
// 턴 진행 제어
public class GameManager : MonoBehaviour
{

    public event Action OnGameFinished;

    [SerializeField]
    private PlayerState[] players = new PlayerState[5];
    public PlayerState[] Players => players;

    [SerializeField]
    private TurnDirection turnDirection = TurnDirection.Clockwise;

    [SerializeField]
    private HandView humanHandView;

    [SerializeField]
    private HandView[] aiHandViews = new HandView[4];

    [SerializeField]
    private TableView tableView;

    [SerializeField]
    private PlayerSeatInfoView[] playerSeatInfoViews = new PlayerSeatInfoView[5];

    [SerializeField]
    [Range(0.3f, 1.5f)]
    private float aiTurnDelay = 0.5f;

    [SerializeField]
    [Range(0.3f, 3.0f)]
    private float gameEndDelay = 1.2f;

    private bool isGameEnding = false;

    private float GetPresentationDuration(float ratio)
    {

        return aiTurnDelay * ratio;

    }

    public float AITurnDelay => aiTurnDelay;

    public void SetAITurnDelay(float delay)
    {

        aiTurnDelay = Mathf.Clamp(delay, 0.3f, 1.5f);

    }

    private DeckFactory deckFactory;
    private TableState tableState;
    private RoundState roundState;
    private RuleManager ruleManager;
    private TurnManager turnManager;

    private AIManager aiManager;

    private EffectProcessor effectProcessor;
    private CardSubmissionService submissionService;
    private RoundService roundService;
    private ResultService resultService;

    private PlayerState startingPlayer;

    private FollowUpSelectionController followUpController;
    private RoundConstraintService roundConstraintService;

    private HierarchyExchangeController hierarchyExchangeController;

    [SerializeField]
    private PresentationController presentationController;

    private bool isGameOver = false;

    [SerializeField]
    private bool forceHumanStart = false;
    public void SetForceHumanStart(bool value)
    {

        forceHumanStart = value;

    }

    private void Awake()
    {

        if (deckFactory == null) deckFactory = FindFirstObjectByType<DeckFactory>();
        if (ruleManager == null) ruleManager = FindFirstObjectByType<RuleManager>();

        tableState = new TableState();
        roundState = new RoundState();
        turnManager = new TurnManager();

        aiManager = new AIManager();

        effectProcessor = new EffectProcessor();
        submissionService = new CardSubmissionService();
        roundService = new RoundService();
        resultService = new ResultService();

        followUpController = new FollowUpSelectionController(turnManager, resultService, players);
        roundConstraintService = new RoundConstraintService(ruleManager);

        hierarchyExchangeController = new HierarchyExchangeController(players);

        tableState.tableCardCombination.Type = CardCombinationType.None;

    }

    public void StartGame()
    {

        isGameOver = false;
        isGameEnding = false;

        followUpController.Clear();

        if (hierarchyExchangeController != null)
        {

            hierarchyExchangeController.Clear();

        }

        ResetPlayerForNewGame();

        deckFactory.ResetAndShuffleDeck();

        CardData[] deck = deckFactory.GetDeck();

        DealCards(deck);

        for (int i = 0; i < players.Length; i++)
        {

            players[i].SortHand();

        }

        RefreshGameplayUI();

        bool exchangeStarted = hierarchyExchangeController.Begin();

        RefreshGameplayUI();

        if (exchangeStarted) return;

        StartCoroutine(PlayHierarchyExchangeResultsThenContinueRoutine());

    }

    public void StartNewSessionFromMainMenu()
    {

        StopAllCoroutines();

        isGameOver = false;
        isGameEnding = false;

        roundState = new RoundState();
        tableState = new TableState();

        tableState.tableCardCombination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };

        turnDirection = TurnDirection.Clockwise;
        startingPlayer = null;

        ResetPlayerForNewSession();

        if (tableView != null)
        {

            tableView.ClearTable();

        }

        StartGame();

    }

    private void ContinueGameStartAfterHierarchyExchange()
    {

        DecideStartPlayer();
        DecideOrder();

        tableState.tableCardCombination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };

        if (tableView != null)
        {

            tableView.ClearTable();

        }

        RefreshGameplayUIForCurrentTurn();

        ProcessCurrentTurn();

    }

    private void ResetPlayerForNewGame()
    {

        for (int i = 0; i < players.Length; i++)
        {

            players[i].SetDone(false);
            players[i].SetFinishType(PlayerFinishType.None);
            players[i].ClearSelectedCards();
            players[i].handCards.Clear();

        }

    }

    private void ResetPlayerForNewSession()
    {

        for (int i = 0; i < players.Length; i++)
        {

            players[i].SetDone(false);
            players[i].SetFinishType(PlayerFinishType.None);
            players[i].SetThisGameHierarchy(PlayerHierarchy.None);
            players[i].SetLastGameHierarchy(PlayerHierarchy.None);
            players[i].ClearSelectedCards();
            players[i].handCards.Clear();

        }

    }

    public PlayerState GetCurrentPlayer()
    {

        if (roundState.playerTurnList.Count == 0) return null;

        PlayerState currentPlayer = roundState.playerTurnList[roundState.currentTurnIndex];

        if (currentPlayer == null || currentPlayer.IsDone) return null;

        return currentPlayer;

    }

    public bool IsCurrentTurn(PlayerState player)
    {

        return player == GetCurrentPlayer();

    }

    private void DealCards(CardData[] cards)
    {

        for (int i = 0; i < deckFactory.totalCards.Length; i++)
        {

            Deal1Card(cards[i], players[i % players.Length]);

        }

    }

    private void Deal1Card(CardData card, PlayerState player)
    {

        player.AddCard(card);

    }

    public void ConfirmHierarchyExchangeSelection()
    {

        if (hierarchyExchangeController == null || !hierarchyExchangeController.HasPendingSelection) return;

        hierarchyExchangeController.ConfirmHumanSelection();

        RefreshGameplayUI();

        if (!hierarchyExchangeController.HasPendingSelection)
        {

            StartCoroutine(PlayHierarchyExchangeResultsThenContinueRoutine());

        }

    }

    private void DecideStartPlayer()
    {

        if (players[0].handCards.Count == 0)
        {

            Debug.Log("[GameManager] 손패가 빔");
            return;

        }

        if (forceHumanStart)
        {

            for (int i = 0; i < players.Length; i++)
            {

                if (players[i].isHuman)
                {

                    startingPlayer = players[i];

                    Debug.Log("[GameManager] 비밀 버튼: 인간 플레이어가 시작");

                    return;

                }

            }

        }


        for (int i = 0; i < players.Length; i++)
        {

            for (int j = 0; j < players[i].handCards.Count; j++)
            {

                if (players[i].handCards[j].rank == CardRank.Three && players[i].handCards[j].suit == CardSuit.Diamond)
                {

                    startingPlayer = players[i];

                    Debug.Log($"{players[i]}가 시작합니다.");
                    return;

                }

            }

        }

    }

    private void DecideOrder()
    {

        roundState.playerTurnList.Clear();

        int startIndex = -999;

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] == startingPlayer)
            {

                startIndex = i;
                break;

            }

        }

        if (startIndex == -999)
        {

            Debug.Log("[GameManager] 시작 플레이어를 못 찾음");
            return;

        }

        for (int i = 0; i < players.Length; i++)
        {

            int index = (startIndex + i) % players.Length;
            roundState.playerTurnList.Add(players[index]);

        }

    }

    private IEnumerator ApplyPendingSkipsRoutine(System.Action<bool> onFinished)
    {

        bool roundEnded = false;

        while (roundState.pendingSkipCount > 0)
        {

            PlayerState skippedPlayer = GetCurrentPlayer();

            if (skippedPlayer == null)
            {

                Debug.Log("[GameManager] 스킵 대상 플레이어가 없음");

                roundState.pendingSkipCount = 0;

                onFinished?.Invoke(false);
                yield break;

            }

            List<PresentationEvent> events = BuildSkipPresentation(skippedPlayer);

            yield return StartCoroutine(PlayPresentationSequence(events));

            roundService.RegisterPass(roundState, skippedPlayer);

            turnManager.NextTurn(roundState, turnDirection);

            roundState.pendingSkipCount--;

            if (roundService.IsRoundEnd(roundState))
            {

                EndRound(roundState.lastPlayedPlayer);
                roundEnded = true;
                break;

            }

        }

        onFinished?.Invoke(roundEnded);

    }

    public void SubmitSelectedCards(PlayerState player)
    {

        if (hierarchyExchangeController != null && hierarchyExchangeController.HasPendingSelection)
        {

            ConfirmHierarchyExchangeSelection();
            return;

        }

        if (followUpController.HasPendingFollowUp())
        {

            ConfirmFollowUpSelection();
            return;

        }

        TrySubmitSelectedCards(player);

    }

    private bool TrySubmitSelectedCards(PlayerState player)
    {

        if (!IsCurrentTurn(player))
        {

            Debug.Log("[GameManager] 이 플레이어의 턴이 아님");
            return false;

        }

        CardCombination previousCombination = tableState.tableCardCombination;
        List<CardData> previousCards = new List<CardData>(tableState.tableCards);

        bool reverseOrderBeforeEffects = tableState.IsRevolution != roundState.is11Back;

        bool wasRankTight = roundState.isRankTight;
        bool wasSuitTight = roundState.isSuitTight;

        SubmitResult submitResult = submissionService.TrySubmit(player, tableState, roundState, ruleManager);

        if (!submitResult.succeeded)
        {

            return false;

        }

        if (tableView != null)
        {

            tableView.RefreshTable(tableState.tableCards);

        }

        if (SoundManager.Instance != null)
        {

            SoundManager.Instance.PlayCardPlace();

        }
        else
        {

            Debug.LogError("[GameManager] SoundManager.Instance가 null입니다.");

        }

        RefreshAllHands();

        effectProcessor.ApplyEffects(submitResult.effectResult, player, tableState, roundState, ref turnDirection);

        roundConstraintService.UpdateConstraintsAfterSubmission(
    previousCombination,
    previousCards,
    tableState.tableCardCombination,
    tableState.tableCards,
    tableState,
    roundState,
    reverseOrderBeforeEffects
);

        bool rankTightJustStarted = !wasRankTight && roundState.isRankTight;
        bool suitTightJustStarted = !wasSuitTight && roundState.isSuitTight;

        StartCoroutine(HandlePostSubmitPresentationAndFlow(player, submitResult.effectResult, rankTightJustStarted, suitTightJustStarted));

        return true;

    }

    private void HandleFollowUpResolveResult(FollowUpResolveResult resolveResult)
    {

        StartCoroutine(HandleFollowUpResolveResultRoutine(resolveResult));

    }

    private IEnumerator HandlePostSubmitPresentationAndFlow(PlayerState player, CardEffectResult effectResult, bool rankTightJustStarted, bool suitTightJustStarted)
    {

        if (effectResult.triggerRevolution)
        {

            if (SoundManager.Instance != null)
            {

                SoundManager.Instance.PlayRevolution();

            }

        }

        yield return StartCoroutine(PlayInstantEffectPresentationRoutine(effectResult, rankTightJustStarted, suitTightJustStarted));

        bool hasFollowUp = followUpController.Begin(player, effectResult, roundState, turnDirection);

        if (hasFollowUp)
        {

            RefreshGameplayUI();

            yield return StartCoroutine(PlayFollowUpStartPresentationRoutine(followUpController.CurrentMode));

            if (!player.isHuman)
            {

                FollowUpResolveResult aiResolveResult = followUpController.ResolveForAI(roundState);
                HandleFollowUpResolveResult(aiResolveResult);

            }

            yield break;

        }

        yield return StartCoroutine(FinalizeTurnAfterActionRoutine(player));

    }

    private IEnumerator HandleFollowUpResolveResultRoutine(FollowUpResolveResult resolveResult)
    {

        if (!resolveResult.handled)
        {

            yield break;

        }

        RefreshGameplayUI();

        if (resolveResult.switchedToDiscardMode)
        {

            yield return StartCoroutine(PlayFollowUpStartPresentationRoutine(FollowUpSelectionMode.DiscardCards));

            PlayerState pendingPlayer = followUpController.PendingEffectPlayer;

            if (pendingPlayer != null && !pendingPlayer.isHuman)
            {

                FollowUpResolveResult nextResolveResult = followUpController.ResolveForAI(roundState);
                HandleFollowUpResolveResult(nextResolveResult);

            }

            yield break;

        }

        if (resolveResult.completedAllFollowUps)
        {

            PlayerState finishedPlayer = resolveResult.resolvedPlayer;

            if (finishedPlayer == null)
            {

                yield break;

            }

            yield return StartCoroutine(FinalizeTurnAfterActionRoutine(finishedPlayer));

        }

    }

    private void RefreshGameplayUI(bool refreshSeatInfos = true)
    {

        RefreshAllHands();

        if (refreshSeatInfos)
        {

            RefreshAllPlayerSeatInfos();

        }

    }

    private void RefreshGameplayUIForCurrentTurn()
    {

        UpdateCurrentPlayerSelectableCards();
        RefreshGameplayUI(true);

    }

    private void RecalculateRankTightAfter11Back()
    {

        if (roundState == null || tableState == null || ruleManager == null)
        {

            return;

        }

        if (!roundState.isRankTight)
        {

            return;

        }

        if (tableState.tableCardCombination.Type == CardCombinationType.None)
        {

            roundState.isRankTight = false;
            return;

        }

        CardRank recalculatedNextRank;

        if (ruleManager.TryGetNextTightRank(
            tableState.tableCardCombination.rank,
            tableState,
            roundState,
            out recalculatedNextRank))
        {

            roundState.tightNextRank = recalculatedNextRank;

            Debug.Log($"[GameManager] 11 Back 적용 후 숫자 고정 재계산: next = {recalculatedNextRank}");

        }
        else
        {

            roundState.isRankTight = false;
            Debug.Log("[GameManager] 11 Back 적용 후 숫자 고정 해제");

        }

    }

    private void ApplyPendingEndOfTurnEffects(PlayerState player)
    {

        if (roundState == null)
        {

            return;

        }

        if (roundState.pending11Back)
        {

            roundState.pending11Back = false;
            roundState.is11Back = true;

            Debug.Log("[GameManager] 턴 종료 후 11 백 적용");

            RecalculateRankTightAfter11Back();

        }

    }

    private bool TryResolveRoundEndByEightCut(PlayerState player)
    {

        if (roundState == null)
        {

            return false;

        }

        if (!roundState.pendingEightCut)
        {

            return false;

        }

        roundState.pendingEightCut = false;
        EndRound(player);

        return true;

    }

    private IEnumerator FinalizeTurnAfterActionRoutine(PlayerState player)
    {

        ResolveTurnEndFinish(player);

        if (isGameOver)
        {

            yield break;

        }

        ApplyPendingEndOfTurnEffects(player);

        if (isGameOver)
        {

            yield break;

        }

        if (TryResolveRoundEndByEightCut(player))
        {

            yield break;

        }

        yield return StartCoroutine(ContinueTurnAfterFollowUpRoutine());

    }

    private bool ResolveTurnEndFinish(PlayerState player)
    {

        if (player == null)
        {

            return false;

        }

        if (player.IsDone)
        {

            return false;

        }

        if (player.handCards.Count > 0)
        {

            return false;

        }

        bool playerFinished = resultService.TryHandlePlayerFinish(player, players, roundState);

        if (!playerFinished)
        {

            return false;

        }

        resultService.TryApplyMiyakoOchi(players, roundState);

        RefreshGameplayUI();

        CheckGameEnd();

        return true;

    }

    private void RefreshAllHands()
    {

        if (humanHandView != null)
        {

            humanHandView.RefreshHand();

        }

        for (int i = 0; i < aiHandViews.Length; i++)
        {

            if (aiHandViews[i] != null)
            {

                aiHandViews[i].RefreshHand();

            }

        }

    }

    private void UpdateCurrentPlayerSelectableCards()
    {

        PlayerState currentPlayer = GetCurrentPlayer();

        if (currentPlayer == null)
        {

            Debug.Log("[GameManager] 현재 플레이어가 null");
            return;

        }

        ruleManager.UpdateSelectableCards(currentPlayer, tableState, roundState);

    }

    private void ProcessCurrentTurn()
    {

        if (isGameOver) return;

        PlayerState currentPlayer = GetCurrentPlayer();

        if (currentPlayer == null)
        {

            turnManager.NextTurn(roundState, turnDirection);

            currentPlayer = GetCurrentPlayer();

            if (currentPlayer == null)
            {

                Debug.Log("[GameManager] 다음 턴 플레이어도 없음");
                return;

            }

        }

        if (SoundManager.Instance != null)
        {

            SoundManager.Instance.PlayTurnChange();

        }

        RefreshGameplayUIForCurrentTurn();

        if (!currentPlayer.isHuman)
        {

            StartCoroutine(ProcessAITurn(currentPlayer));

        }

    }

    private IEnumerator ProcessAITurn(PlayerState aiPlayer)
    {

        yield return new WaitForSeconds(aiTurnDelay);

        bool canPlay = aiManager.TrySelectPlay(aiPlayer, tableState, roundState, ruleManager);

        if (!canPlay)
        {

            Debug.Log("[GameManager] AI가 패스함");

            PassCurrentPlayer();
            yield break;

        }

        bool submitSucceeded = TrySubmitSelectedCards(aiPlayer);

        if (!submitSucceeded)
        {

            Debug.Log("[GameManager] AI가 낼 수 있는 카드를 선택했지만 제출 실패함");

            aiPlayer.ClearSelectedCards();

            PassCurrentPlayer();

        }

    }

    public void PassCurrentPlayer()
    {

        if (hierarchyExchangeController != null && hierarchyExchangeController.HasPendingSelection)
        {

            Debug.Log("[GameManager] 계급 교환 중에는 패스할 수 없습니다.");
            return;

        }

        if (followUpController.HasPendingFollowUp())
        {

            Debug.Log("[GameManager] 7/10 후속 선택 중에는 패스할 수 없습니다. Enter로 확정하세요.");
            return;

        }

        PlayerState currentPlayer = GetCurrentPlayer();

        if (currentPlayer == null)
        {

            Debug.Log("[GameManager] 현재 플레이어가 없음");
            return;

        }

        currentPlayer.ClearSelectedCards();
        RefreshAllHands();

        roundService.RegisterPass(roundState, currentPlayer);

        turnManager.NextTurn(roundState, turnDirection);

        if (roundService.IsRoundEnd(roundState))
        {

            EndRound(roundState.lastPlayedPlayer);
            return;

        }

        ProcessCurrentTurn();

    }

    public void ConfirmFollowUpSelection()
    {

        if (!followUpController.HasPendingFollowUp()) return;

        FollowUpResolveResult resolveResult = followUpController.ConfirmSelection(roundState);

        HandleFollowUpResolveResult(resolveResult);

    }

    public void SkipFollowUpSelection()
    {

        if (!followUpController.HasPendingFollowUp()) return;

        FollowUpResolveResult resolveResult = followUpController.SkipSelection();

        HandleFollowUpResolveResult(resolveResult);

    }

    private void EndRound(PlayerState roundEnder)
    {

        roundService.ResetRound(roundState, tableState, roundEnder);

        if (tableView != null)
        {

            tableView.ClearTable();

        }

        Debug.Log("[GameManager] 라운드 종료");

        RefreshGameplayUIForCurrentTurn();
        ProcessCurrentTurn();

    }

    private void CheckGameEnd()
    {

        if (isGameEnding) return;

        bool isEnd = resultService.IsGameEnd(players);

        if (!isEnd)
        {

            return;

        }

        isGameEnding = true;

        resultService.AssignLastRemainingPlayer(players, roundState);

        RefreshGameplayUI();

        StartCoroutine(FinalizeGameResultWithDelayRoutine());

    }

    private IEnumerator FinalizeGameResultWithDelayRoutine()
    {

        yield return new WaitForSeconds(gameEndDelay);

        FinalizeGameResult();

    }

    private void FinalizeGameResult()
    {

        if (isGameOver) return;
        isGameOver = true;

        for (int i = 0; i < players.Length; i++)
        {

            PlayerHierarchy hierarchy = players[i].ThisGameHierarchy;

            switch (hierarchy)
            {

                case PlayerHierarchy.Daifugo:
                    break;
                case PlayerHierarchy.Fugo:
                    break;
                case PlayerHierarchy.Heimin:
                    break;
                case PlayerHierarchy.Hinmin:
                    break;
                case PlayerHierarchy.Daihinmin:
                    break;

            }

        }

        tableState.tableCardCombination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };

        tableState.tableCards.Clear();

        if (tableView != null)
        {

            tableView.ClearTable();

        }

        Debug.Log("[GameManager] 게임 종료");

        RefreshAllPlayerSeatInfos();
        ShowGameResult();

        OnGameFinished?.Invoke();

    }

    private void ShowGameResult()
    {

        List<PlayerState> resultPlayers = resultService.GetPlayersSortedByHierarchy(players);

        for (int i = 0; i < resultPlayers.Count; i++)
        {

            PlayerState player = resultPlayers[i];

            Debug.Log($"순위 {i + 1}: {player.PlayerName} - {player.ThisGameHierarchy}");

        }

    }

    public void StartNextGame()
    {

        PreparePlayersForNextGame();

        roundState = new RoundState();
        tableState = new TableState();
        tableState.tableCardCombination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };

        turnDirection = TurnDirection.Clockwise;
        startingPlayer = null;

        StartGame();

    }

    private void PreparePlayersForNextGame()
    {

        for (int i = 0; i < players.Length; i++)
        {

            players[i].CommitHierarchyForNextGame();
            players[i].SetDone(false);
            players[i].SetFinishType(PlayerFinishType.None);
            players[i].ClearSelectedCards();
            players[i].handCards.Clear();

        }

    }

    private void RefreshAllPlayerSeatInfos()
    {

        PlayerState currentPlayer = GetCurrentPlayer();

        for (int i = 0; i < playerSeatInfoViews.Length; i++)
        {

            if (playerSeatInfoViews[i] == null) continue;

            PlayerState seatPlayer = playerSeatInfoViews[i].PlayerState;

            bool isCurrentTurn = seatPlayer != null && seatPlayer == currentPlayer;

            playerSeatInfoViews[i].Refresh(isCurrentTurn);

        }

    }

    public bool IsHierarchyExchangePending()
    {

        return hierarchyExchangeController != null && hierarchyExchangeController.HasPendingSelection;

    }

    public bool CanSelectCardsForHierarchyExchange(PlayerState player)
    {

        if (player == null)
        {

            return false;

        }

        if (hierarchyExchangeController == null || !hierarchyExchangeController.HasPendingSelection)
        {

            return false;

        }

        return hierarchyExchangeController.PendingHighPlayer == player;

    }

    public int GetHierarchyExchangeSelectionLimit(PlayerState player)
    {

        if (!CanSelectCardsForHierarchyExchange(player))
        {

            return 0;

        }

        return hierarchyExchangeController.PendingExchangeCount;

    }

    public string GetHierarchyExchangeGuideText()
    {

        if (hierarchyExchangeController == null || !hierarchyExchangeController.HasPendingSelection)
        {

            return "";

        }

        PlayerState highPlayer = hierarchyExchangeController.PendingHighPlayer;
        PlayerState lowPlayer = hierarchyExchangeController.PendingLowPlayer;
        int count = hierarchyExchangeController.PendingExchangeCount;

        if (highPlayer == null || lowPlayer == null)
        {

            return "";

        }

        if (highPlayer.LastGameHierarchy == PlayerHierarchy.Daifugo && lowPlayer.LastGameHierarchy == PlayerHierarchy.Daihinmin)
        {

            return "<b>대빈민(Daihinmin)</b>에게 줄 카드 <b>2</b>장을 고르세요.";

        }

        if (highPlayer.LastGameHierarchy == PlayerHierarchy.Fugo && lowPlayer.LastGameHierarchy == PlayerHierarchy.Hinmin)
        {

            return "<b>빈민(Hinmin)</b>에게 줄 카드 <b>1</b>장을 고르세요.";

        }

        return $"<b>{count}</b>장의 카드를 고르세요.";

    }

    private IEnumerator PlayPresentationSequence(List<PresentationEvent> events)
    {

        if (presentationController == null || events == null || events.Count == 0)
        {

            yield break;

        }

        yield return presentationController.PlaySequence(events);

    }

    private List<PresentationEvent> BuildSkipPresentation(PlayerState skippedPlayer)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        events.Add(new PresentationEvent
        {
            EventType = PresentationEventType.Message,
            Message = "5 Skip",
            Duration = GetPresentationDuration(0.6f)
        });

        events.Add(new PresentationEvent
        {
            EventType = PresentationEventType.SkipSeat,
            TargetPlayer = skippedPlayer,
            Duration = GetPresentationDuration(0.7f)
        });

        return events;

    }

    private List<PresentationEvent> BuildInstantEffectPresentation(CardEffectResult effectResult)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        if (effectResult.reversesDirection)
        {

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.ReverseDirection,
                Message = "9 반전",
                Duration = GetPresentationDuration(0.7f)
            });

        }

        if (effectResult.triggerEightCut)
        {

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.EightCut,
                Message = "8 컷",
                Duration = GetPresentationDuration(0.7f)
            });

        }

        if (effectResult.trigger11Back)
        {

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.ElevenBack,
                Message = "11 백",
                Duration = GetPresentationDuration(0.7f)
            });

        }

        return events;

    }

    private IEnumerator PlayInstantEffectPresentationRoutine(CardEffectResult effectResult, bool rankTightJustStarted, bool suitTightJustStarted)
    {

        List<PresentationEvent> events = BuildPostSubmitPresentation(effectResult, rankTightJustStarted, suitTightJustStarted);

        if (events == null || events.Count == 0)
        {

            yield break;

        }

        yield return StartCoroutine(PlayPresentationSequence(events));

    }

    private List<PresentationEvent> BuildRevolutionPresentation(CardEffectResult effectResult)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        if (effectResult.triggerRevolution)
        {

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.Message,
                Message = "Revolution",
                Duration = GetPresentationDuration(0.8f)
            });

        }

        return events;

    }

    private List<PresentationEvent> BuildTightPresentation(bool rankTightJustStarted, bool suitTightJustStarted)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        if (roundState == null)
        {

            return events;

        }

        if (rankTightJustStarted)
        {

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.Message,
                Message = "숫자 고정",
                Duration = GetPresentationDuration(0.6f)
            });

        }

        if (suitTightJustStarted && roundState.tightSuits != null && roundState.tightSuits.Count > 0)
        {

            List<string> suitNames = new List<string>();

            for (int i = 0; i < roundState.tightSuits.Count; i++)
            {

                suitNames.Add(GetSuitEmoji(roundState.tightSuits[i]));

            }

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.Message,
                Message = $"문양 고정 : {string.Join(" ", suitNames)}",
                Duration = GetPresentationDuration(0.3f)
            });

        }

        return events;

    }

    private string GetSuitEmoji(CardSuit suit)
    {

        switch (suit)
        {

            case CardSuit.Spade: return "♠";
            case CardSuit.Heart: return "♥";
            case CardSuit.Diamond: return "♦";
            case CardSuit.Club: return "♣";

            default: return "?";

        }

    }

    private string GetRankText(CardRank rank)
    {

        switch (rank)
        {

            case CardRank.Three: return "3";
            case CardRank.Four: return "4";
            case CardRank.Five: return "5";
            case CardRank.Six: return "6";
            case CardRank.Seven: return "7";
            case CardRank.Eight: return "8";
            case CardRank.Nine: return "9";
            case CardRank.Ten: return "10";
            case CardRank.Jack: return "J";
            case CardRank.Queen: return "Q";
            case CardRank.King: return "K";
            case CardRank.Ace: return "A";
            case CardRank.Two: return "2";

            default: return "?";

        }

    }

    private List<PresentationEvent> BuildPostSubmitPresentation(CardEffectResult effectResult, bool rankTightJustStarted, bool suitTightJustStarted)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        List<PresentationEvent> revolutionEvents = BuildRevolutionPresentation(effectResult);
        List<PresentationEvent> instantEffectEvents = BuildInstantEffectPresentation(effectResult);
        List<PresentationEvent> tightEvents = BuildTightPresentation(rankTightJustStarted, suitTightJustStarted);

        if (revolutionEvents != null && revolutionEvents.Count > 0)
        {

            events.AddRange(revolutionEvents);

        }

        if (instantEffectEvents != null && instantEffectEvents.Count > 0)
        {

            events.AddRange(instantEffectEvents);

        }

        if (tightEvents != null && tightEvents.Count > 0)
        {

            events.AddRange(tightEvents);

        }

        return events;

    }

    private List<PresentationEvent> BuildFollowUpStartPresentation(FollowUpSelectionMode mode)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        switch (mode)
        {

            case FollowUpSelectionMode.GiveCards:
                if (GetCurrentPlayer().isHuman == true)
                {

                    events.Add(new PresentationEvent
                    {
                        EventType = PresentationEventType.Message,
                        Message = "7 Give - 줄 카드를 선택하세요",
                        Duration = GetPresentationDuration(1.2f)
                    });

                }
                else
                {

                    events.Add(new PresentationEvent
                    {
                        EventType = PresentationEventType.Message,
                        Message = "7 Give",
                        Duration = GetPresentationDuration(0.6f)
                    });

                }
                break;

            case FollowUpSelectionMode.DiscardCards:
                if (GetCurrentPlayer().isHuman == true)
                {

                    events.Add(new PresentationEvent
                    {
                        EventType = PresentationEventType.Message,
                        Message = "10 Discard - 버릴 카드를 선택하세요",
                        Duration = GetPresentationDuration(1.2f)
                    });

                }
                else
                {

                    events.Add(new PresentationEvent
                    {
                        EventType = PresentationEventType.Message,
                        Message = "10 Discard",
                        Duration = GetPresentationDuration(0.6f)
                    });

                }
                break;

        }

        return events;

    }

    private IEnumerator PlayFollowUpStartPresentationRoutine(FollowUpSelectionMode mode)
    {

        List<PresentationEvent> events = BuildFollowUpStartPresentation(mode);

        if (events == null || events.Count == 0)
        {

            yield break;

        }

        yield return StartCoroutine(PlayPresentationSequence(events));

    }

    private IEnumerator PlayHierarchyExchangeResultsThenContinueRoutine()
    {

        List<HierarchyExchangeResult> results = hierarchyExchangeController.ConsumeCompletedResults();

        if (results != null && results.Count > 0)
        {

            RefreshGameplayUI();

            for (int i = 0; i < results.Count; i++)
            {

                HierarchyExchangeResult result = results[i];

                bool humanWasAutoAffected =
                    result.lowPlayer != null &&
                    result.lowPlayer.isHuman &&
                    result.highPlayer != null &&
                    !result.highPlayer.isHuman;

                if (humanWasAutoAffected)
                {

                    List<PresentationEvent> events = BuildHierarchyExchangeResultPresentation(result);

                    if (events.Count > 0)
                    {

                        yield return StartCoroutine(PlayPresentationSequence(events));

                    }

                    RefreshGameplayUI();

                    yield return new WaitForSeconds(1.0f);

                }

            }

        }

        ContinueGameStartAfterHierarchyExchange();

    }

    private string GetCardDisplayText(CardData card)
    {

        if (card.IsJoker)
        {

            return "Joker";

        }

        return $"{GetSuitEmoji(card.suit)}{GetRankText(card.rank)}";

    }

    private string GetCardListDisplayText(List<CardData> cards)
    {

        if (cards == null || cards.Count == 0)
        {

            return "(none)";

        }

        List<string> names = new List<string>();

        for (int i = 0; i < cards.Count; i++)
        {

            names.Add(GetCardDisplayText(cards[i]));

        }

        return string.Join(", ", names);

    }

    private string BuildHierarchyExchangeResultMessage(HierarchyExchangeResult result)
    {

        if (result.lowPlayer == null || result.highPlayer == null)
        {

            return "";

        }

        string receivedText = GetCardListDisplayText(result.highCardsToGive);
        string gaveText = GetCardListDisplayText(result.lowCardsToGive);

        if (result.lowPlayer.LastGameHierarchy == PlayerHierarchy.Hinmin)
        {

            return $"<b>부호</b>에게\n<b>{gaveText}</b>를 바치고\n<b>{receivedText}</b>를 얻었습니다.";

        }

        if (result.lowPlayer.LastGameHierarchy == PlayerHierarchy.Daihinmin)
        {

            return $"<b>대부호</b>에게\n<b>{gaveText}</b>를 바치고\n<b>{receivedText}</b>를 얻었습니다.";

        }
        
        return $"<b>{result.lowPlayer.PlayerName}</b>가\n<b>{gaveText}</b>를 주고\n<b>{receivedText}</b>를 얻었습니다.";

    }

    private List<PresentationEvent> BuildHierarchyExchangeResultPresentation(HierarchyExchangeResult result)
    {

        List<PresentationEvent> events = new List<PresentationEvent>();

        string message = BuildHierarchyExchangeResultMessage(result);

        if (!string.IsNullOrEmpty(message))
        {

            events.Add(new PresentationEvent
            {
                EventType = PresentationEventType.Message,
                Message = message,
                Duration = 2.5f
            });

        }

        return events;

    }

    private IEnumerator ContinueTurnAfterFollowUpRoutine()
    {

        turnManager.NextTurn(roundState, turnDirection);

        bool roundEndedBySkip = false;

        if (roundState.pendingSkipCount > 0)
        {

            yield return StartCoroutine(ApplyPendingSkipsRoutine(result => roundEndedBySkip = result));

        }

        if (roundEndedBySkip)
        {

            roundState.pendingSkipCount = 0;
            yield break;

        }

        roundState.pendingSkipCount = 0;

        RefreshGameplayUIForCurrentTurn();
        ProcessCurrentTurn();

    }

    public bool IsReverseDirection()
    {

        return turnDirection == TurnDirection.CounterClockwise;

    }

    public bool Is11BackActive()
    {

        return roundState != null && roundState.is11Back;

    }

    public bool IsRevolutionActive()
    {

        return tableState != null && tableState.IsRevolution;

    }

    public bool IsRankTightActive()
    {

        return roundState != null && roundState.isRankTight;

    }

    public string GetTightRankText()
    {

        if (roundState == null) return "";

        return roundState.tightNextRank.ToString();

    }

    public bool IsSuitTightActive()
    {

        return roundState != null && roundState.isSuitTight;

    }

    public string GetTightSuitText()
    {

        if (roundState == null || roundState.tightSuits == null || roundState.tightSuits.Count == 0)
        {

            return "";

        }

        List<string> suitNames = new List<string>();

        for (int i = 0; i < roundState.tightSuits.Count; i++)
        {

            suitNames.Add(roundState.tightSuits[i].ToString());

        }

        return string.Join(", ", suitNames);

    }

}


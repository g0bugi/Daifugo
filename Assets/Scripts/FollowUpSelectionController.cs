using System.Collections.Generic;
using UnityEngine;

// 7, 10 처리
// 플레이어 및 AI 선택 처리
public class FollowUpSelectionController
{

    private readonly TurnManager turnManager;
    private readonly ResultService resultService;
    private readonly PlayerState[] players;

    private FollowUpSelectionMode followUpSelectionMode = FollowUpSelectionMode.None;

    private PlayerState pendingEffectPlayer;
    private PlayerState pendingGiveTargetPlayer;

    private int pendingGiveCount = 0;
    private int pendingDiscardCount = 0;

    public FollowUpSelectionMode CurrentMode => followUpSelectionMode;

    public PlayerState PendingEffectPlayer => pendingEffectPlayer;

    public PlayerState PendingGiveTargetPlayer => pendingGiveTargetPlayer;

    public int PendingGiveCount => pendingGiveCount;

    public int PendingDiscardCount => pendingDiscardCount;

    public FollowUpSelectionController(TurnManager turnManager, ResultService resultService, PlayerState[] players)
    {

        this.turnManager = turnManager;
        this.resultService = resultService;
        this.players = players;

    }

    public bool HasPendingFollowUp()
    {

        return followUpSelectionMode != FollowUpSelectionMode.None;

    }

    public bool Begin(PlayerState player, CardEffectResult effectResult, RoundState roundState, TurnDirection turnDirection)
    {

        pendingEffectPlayer = player;
        pendingGiveCount = effectResult.giveCount;
        pendingDiscardCount = effectResult.discardCount;
        pendingGiveTargetPlayer = null;

        if (pendingEffectPlayer == null)
        {

            Clear();
            return false;

        }

        // 이미 제출 후 손패가 비었다면
        // 7 주기 / 10 버리기는 모두 "0장 선택"으로 자동 완료 처리한다.
        if (pendingEffectPlayer.handCards.Count == 0)
        {

            Clear();
            return false;

        }

        if (pendingGiveCount > 0)
        {

            int targetIndex = turnManager.PeekNextTurnIndex(roundState, turnDirection);

            if (targetIndex >= 0)
            {

                pendingGiveTargetPlayer = roundState.playerTurnList[targetIndex];
                followUpSelectionMode = FollowUpSelectionMode.GiveCards;

                UpdateFollowUpSelectableCards();

                return true;

            }

        }

        if (pendingDiscardCount > 0)
        {

            followUpSelectionMode = FollowUpSelectionMode.DiscardCards;

            UpdateFollowUpSelectableCards();

            return true;

        }

        Clear();
        return false;

    }

    public FollowUpResolveResult ConfirmSelection(RoundState roundState)
    {

        FollowUpResolveResult result = new FollowUpResolveResult
        {

            handled = false,
            switchedToDiscardMode = false,
            completedAllFollowUps = false,
            resolvedPlayer = null

        };

        if (!HasPendingFollowUp())
        {

            return result;

        }

        switch (followUpSelectionMode)
        {

            case FollowUpSelectionMode.GiveCards:
                return ResolveGiveCards(roundState);

            case FollowUpSelectionMode.DiscardCards:
                return ResolveDiscardCards(roundState);

        }

        return result;

    }

    public FollowUpResolveResult SkipSelection()
    {

        FollowUpResolveResult result = new FollowUpResolveResult
        {

            handled = false,
            switchedToDiscardMode = false,
            completedAllFollowUps = false,
            resolvedPlayer = pendingEffectPlayer

        };

        if (!HasPendingFollowUp())
        {

            return result;

        }

        if (pendingEffectPlayer != null)
        {

            pendingEffectPlayer.ClearSelectedCards();

        }

        switch (followUpSelectionMode)
        {

            case FollowUpSelectionMode.GiveCards:

                result.handled = true;

                if (pendingDiscardCount > 0)
                {

                    followUpSelectionMode = FollowUpSelectionMode.DiscardCards;
                    UpdateFollowUpSelectableCards();

                    result.switchedToDiscardMode = true;

                    return result;

                }

                Clear();

                result.completedAllFollowUps = true;

                return result;

            case FollowUpSelectionMode.DiscardCards:

                result.handled = true;

                Clear();

                result.completedAllFollowUps = true;

                return result;

        }

        return result;

    }

    public FollowUpResolveResult ResolveForAI(RoundState roundState)
    {

        FollowUpResolveResult result = new FollowUpResolveResult
        {

            handled = false,
            switchedToDiscardMode = false,
            completedAllFollowUps = false,
            resolvedPlayer = null

        };

        if (!HasPendingFollowUp() || pendingEffectPlayer == null)
        {

            return result;

        }

        pendingEffectPlayer.ClearSelectedCards();

        if (followUpSelectionMode == FollowUpSelectionMode.GiveCards)
        {

            int count = Mathf.Min(pendingGiveCount, pendingEffectPlayer.handCards.Count);

            for (int i = 0; i < count; i++)
            {

                pendingEffectPlayer.SelectCard(pendingEffectPlayer.handCards[i]);

            }

            return ConfirmSelection(roundState);

        }

        if (followUpSelectionMode == FollowUpSelectionMode.DiscardCards)
        {

            int count = Mathf.Min(pendingDiscardCount, pendingEffectPlayer.handCards.Count);

            for (int i = 0; i < count; i++)
            {

                pendingEffectPlayer.SelectCard(pendingEffectPlayer.handCards[i]);

            }

            return ConfirmSelection(roundState);

        }

        return result;

    }

    public void RefreshSelectableCards()
    {

        UpdateFollowUpSelectableCards();

    }

    public void Clear()
    {

        followUpSelectionMode = FollowUpSelectionMode.None;
        pendingEffectPlayer = null;
        pendingGiveTargetPlayer = null;
        pendingGiveCount = 0;
        pendingDiscardCount = 0;

    }

    private FollowUpResolveResult ResolveGiveCards(RoundState roundState)
    {

        FollowUpResolveResult result = new FollowUpResolveResult
        {

            handled = true,
            switchedToDiscardMode = false,
            completedAllFollowUps = false,
            resolvedPlayer = pendingEffectPlayer

        };

        if (pendingEffectPlayer == null)
        {

            return result;

        }

        List<CardData> selectedCards = new List<CardData>(pendingEffectPlayer.SelectedCards);

        if (selectedCards.Count > pendingGiveCount)
        {

            Debug.Log("[FollowUpSelectionController] 7 주기 선택 수가 초과되었습니다.");
            return result;

        }

        if (pendingGiveTargetPlayer != null)
        {

            RemoveCardsFromHand(pendingEffectPlayer, selectedCards);

            for (int i = 0; i < selectedCards.Count; i++)
            {

                pendingGiveTargetPlayer.AddCard(selectedCards[i]);

            }

            pendingEffectPlayer.SortHand();
            pendingGiveTargetPlayer.SortHand();

        }

        pendingEffectPlayer.ClearSelectedCards();

        if (pendingDiscardCount > 0)
        {

            followUpSelectionMode = FollowUpSelectionMode.DiscardCards;
            UpdateFollowUpSelectableCards();

            result.switchedToDiscardMode = true;

            return result;

        }

        Clear();

        result.completedAllFollowUps = true;

        return result;

    }

    private FollowUpResolveResult ResolveDiscardCards(RoundState roundState)
    {

        FollowUpResolveResult result = new FollowUpResolveResult
        {

            handled = true,
            switchedToDiscardMode = false,
            completedAllFollowUps = false,
            resolvedPlayer = pendingEffectPlayer

        };

        if (pendingEffectPlayer == null)
        {

            return result;

        }

        List<CardData> selectedCards = new List<CardData>(pendingEffectPlayer.SelectedCards);

        if (selectedCards.Count > pendingDiscardCount)
        {

            Debug.Log("[FollowUpSelectionController] 10 버리기 선택 수가 초과되었습니다.");
            return result;

        }

        RemoveCardsFromHand(pendingEffectPlayer, selectedCards);

        pendingEffectPlayer.SortHand();
        pendingEffectPlayer.ClearSelectedCards();

        Clear();

        result.completedAllFollowUps = true;

        return result;

    }

    private void RemoveCardsFromHand(PlayerState player, List<CardData> cardsToRemove)
    {

        if (player == null || cardsToRemove == null) return;

        for (int i = 0; i < cardsToRemove.Count; i++)
        {

            CardData selected = cardsToRemove[i];

            for (int j = 0; j < player.handCards.Count; j++)
            {

                if (player.handCards[j].cardID == selected.cardID)
                {

                    player.handCards.RemoveAt(j);
                    break;

                }

            }

        }

    }

    private void UpdateFollowUpSelectableCards()
    {

        if (pendingEffectPlayer == null) return;

        for (int i = 0; i < pendingEffectPlayer.handCards.Count; i++)
        {

            CardData card = pendingEffectPlayer.handCards[i];

            card.isSelectable = !card.isHidden;
            pendingEffectPlayer.handCards[i] = card;

        }

    }

}


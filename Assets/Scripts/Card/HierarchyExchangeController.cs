using System.Collections.Generic;
using UnityEngine;

public enum HierarchyExchangeMode
{

    None,
    WaitingHumanSelection

}

public struct HierarchyExchangePlan
{

    public PlayerState highPlayer;
    public PlayerState lowPlayer;
    public int exchangeCount;

}

// 계급 교환 관리
// 카드 교환 대상 및 수량 결정
// 교환 선택 및 결과 처리
public class HierarchyExchangeController
{

    private readonly PlayerState[] players;

    private readonly List<HierarchyExchangePlan> exchangePlans = new List<HierarchyExchangePlan>();

    private int currentPlanIndex = 0;

    private HierarchyExchangeMode exchangeMode = HierarchyExchangeMode.None;

    private HierarchyExchangePlan currentPlan;

    public HierarchyExchangeMode ExchangeMode => exchangeMode;

    public bool HasPendingSelection => exchangeMode == HierarchyExchangeMode.WaitingHumanSelection;

    public PlayerState PendingHighPlayer => currentPlan.highPlayer;

    public PlayerState PendingLowPlayer => currentPlan.lowPlayer;

    public int PendingExchangeCount => currentPlan.exchangeCount;

    private readonly List<HierarchyExchangeResult> completedResults = new List<HierarchyExchangeResult>();

    public HierarchyExchangeController(PlayerState[] players)
    {

        this.players = players;

    }

    public void Clear()
    {

        exchangePlans.Clear();
        completedResults.Clear();
        currentPlanIndex = 0;
        exchangeMode = HierarchyExchangeMode.None;
        currentPlan = default;

    }

    public bool Begin()
    {

        Clear();

        if (!HasAnyPreviousHierarchy())
        {

            return false;

        }

        PlayerState daifugo = FindPlayerByLastHierarchy(PlayerHierarchy.Daifugo);
        PlayerState daihinmin = FindPlayerByLastHierarchy(PlayerHierarchy.Daihinmin);
        PlayerState fugo = FindPlayerByLastHierarchy(PlayerHierarchy.Fugo);
        PlayerState hinmin = FindPlayerByLastHierarchy(PlayerHierarchy.Hinmin);

        if (daifugo != null && daihinmin != null)
        {

            HierarchyExchangePlan daifugoPlan = new HierarchyExchangePlan
            {

                highPlayer = daifugo,
                lowPlayer = daihinmin,
                exchangeCount = 2

            };

            exchangePlans.Add(daifugoPlan);

        }

        if (fugo != null && hinmin != null)
        {

            HierarchyExchangePlan fugoPlan = new HierarchyExchangePlan
            {

                highPlayer = fugo,
                lowPlayer = hinmin,
                exchangeCount = 1

            };

            exchangePlans.Add(fugoPlan);

        }

        if (exchangePlans.Count == 0)
        {

            return false;

        }

        return ProcessNextPlan();

    }

    public bool ConfirmHumanSelection()
    {

        if (!HasPendingSelection)
        {

            return false;

        }

        PlayerState highPlayer = currentPlan.highPlayer;

        if (highPlayer == null)
        {

            return false;

        }

        if (highPlayer.SelectedCards.Count != currentPlan.exchangeCount)
        {

            Debug.Log($"[HierarchyExchangeController] 선택한 카드 수가 맞지 않습니다. 필요 수량: {currentPlan.exchangeCount}");
            return false;

        }

        List<CardData> highCardsToGive = new List<CardData>(highPlayer.SelectedCards);
        List<CardData> lowCardsToGive = GetHighestCards(currentPlan.lowPlayer, currentPlan.exchangeCount);

        ExecuteExchange(currentPlan.highPlayer, currentPlan.lowPlayer, highCardsToGive, lowCardsToGive);

        highPlayer.ClearSelectedCards();

        exchangeMode = HierarchyExchangeMode.None;

        currentPlanIndex++;

        return ProcessNextPlan();

    }

    private bool ProcessNextPlan()
    {

        while (currentPlanIndex < exchangePlans.Count)
        {

            currentPlan = exchangePlans[currentPlanIndex];

            if (currentPlan.highPlayer == null || currentPlan.lowPlayer == null)
            {

                currentPlanIndex++;
                continue;

            }

            if (currentPlan.highPlayer.isHuman)
            {

                PrepareHumanSelection(currentPlan);
                return true;

            }

            ApplyAIExchange(currentPlan);
            currentPlanIndex++;

        }

        exchangeMode = HierarchyExchangeMode.None;
        return false;

    }

    private void PrepareHumanSelection(HierarchyExchangePlan plan)
    {

        exchangeMode = HierarchyExchangeMode.WaitingHumanSelection;

        plan.highPlayer.ClearSelectedCards();
        SetOnlyVisibleCardsSelectable(plan.highPlayer);

        Debug.Log($"[HierarchyExchangeController] {plan.highPlayer.PlayerName}가 교환할 카드 {plan.exchangeCount}장을 선택해야 합니다.");

    }

    private void ApplyAIExchange(HierarchyExchangePlan plan)
    {

        List<CardData> highCardsToGive = GetLowestCards(plan.highPlayer, plan.exchangeCount);
        List<CardData> lowCardsToGive = GetHighestCards(plan.lowPlayer, plan.exchangeCount);

        ExecuteExchange(plan.highPlayer, plan.lowPlayer, highCardsToGive, lowCardsToGive);

        Debug.Log($"[HierarchyExchangeController] AI 교환 완료: {plan.highPlayer.PlayerName} ↔ {plan.lowPlayer.PlayerName}");

    }

    private void ExecuteExchange(PlayerState highPlayer, PlayerState lowPlayer, List<CardData> highCardsToGive, List<CardData> lowCardsToGive)
    {

        if (highPlayer == null || lowPlayer == null)
        {

            return;

        }

        string highGiveText = GetCardListText(highCardsToGive);
        string lowGiveText = GetCardListText(lowCardsToGive);

        Debug.Log($"[HierarchyExchangeController] 교환 시작");
        Debug.Log($"[HierarchyExchangeController] {highPlayer.PlayerName} -> {lowPlayer.PlayerName} : {highGiveText}");
        Debug.Log($"[HierarchyExchangeController] {lowPlayer.PlayerName} -> {highPlayer.PlayerName} : {lowGiveText}");

        completedResults.Add(new HierarchyExchangeResult
        {
            highPlayer = highPlayer,
            lowPlayer = lowPlayer,
            highCardsToGive = new List<CardData>(highCardsToGive),
            lowCardsToGive = new List<CardData>(lowCardsToGive)
        });

        RemoveCardsFromHand(highPlayer, highCardsToGive);
        RemoveCardsFromHand(lowPlayer, lowCardsToGive);

        for (int i = 0; i < highCardsToGive.Count; i++)
        {

            lowPlayer.AddCard(highCardsToGive[i]);

        }

        for (int i = 0; i < lowCardsToGive.Count; i++)
        {

            highPlayer.AddCard(lowCardsToGive[i]);

        }

        highPlayer.SortHand();
        lowPlayer.SortHand();

        highPlayer.ClearSelectedCards();
        lowPlayer.ClearSelectedCards();

        Debug.Log($"[HierarchyExchangeController] 교환 완료: {highPlayer.PlayerName} ↔ {lowPlayer.PlayerName}");

    }

    private void RemoveCardsFromHand(PlayerState player, List<CardData> cardsToRemove)
    {

        if (player == null || cardsToRemove == null)
        {

            return;

        }

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

    private List<CardData> GetLowestCards(PlayerState player, int count)
    {

        List<CardData> result = new List<CardData>();

        if (player == null || count <= 0)
        {

            return result;

        }

        player.SortHand();

        for (int i = 0; i < player.handCards.Count && result.Count < count; i++)
        {

            result.Add(player.handCards[i]);

        }

        return result;

    }

    private List<CardData> GetHighestCards(PlayerState player, int count)
    {

        List<CardData> result = new List<CardData>();

        if (player == null || count <= 0)
        {

            return result;

        }

        player.SortHand();

        for (int i = player.handCards.Count - 1; i >= 0 && result.Count < count; i--)
        {

            result.Add(player.handCards[i]);

        }

        return result;

    }

    private void SetOnlyVisibleCardsSelectable(PlayerState player)
    {

        if (player == null)
        {

            return;

        }

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];
            card.isSelectable = !card.isHidden;
            player.handCards[i] = card;

        }

    }

    private bool HasAnyPreviousHierarchy()
    {

        if (players == null || players.Length == 0)
        {

            return false;

        }

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] == null) continue;

            if (players[i].LastGameHierarchy != PlayerHierarchy.None)
            {

                return true;

            }

        }

        return false;

    }

    private PlayerState FindPlayerByLastHierarchy(PlayerHierarchy hierarchy)
    {

        if (players == null)
        {

            return null;

        }

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] == null) continue;

            if (players[i].LastGameHierarchy == hierarchy)
            {

                return players[i];

            }

        }

        return null;

    }

    private string GetCardListText(List<CardData> cards)
    {

        if (cards == null || cards.Count == 0)
        {

            return "(없음)";

        }

        List<string> parts = new List<string>();

        for (int i = 0; i < cards.Count; i++)
        {

            CardData card = cards[i];

            if (card.IsJoker)
            {

                parts.Add("Joker");

            }
            else
            {

                parts.Add($"{card.suit} {card.rank}");

            }

        }

        return string.Join(", ", parts);

    }

    public List<HierarchyExchangeResult> ConsumeCompletedResults()
    {

        List<HierarchyExchangeResult> results = new List<HierarchyExchangeResult>(completedResults);

        completedResults.Clear();

        return results;

    }

}


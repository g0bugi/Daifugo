using System.Collections.Generic;
using UnityEngine;

public class AIManager
{

    // AI 행동 시도. 성공하면 true, 불가능하면 false 반환
    // 현재 테이블 조합 타입에 따라 어떤 방식으로 카드를 고를지 정함
    public bool TrySelectPlay(PlayerState aiPlayer, TableState tableState, RoundState roundState,  RuleManager ruleManager)
    {

        if (aiPlayer == null || tableState == null || ruleManager == null || roundState == null)
        {

            Debug.Log("[AIManager] 참조가 null이라 AI 행동 불가");
            return false;

        }

        aiPlayer.ClearSelectedCards();

        switch (tableState.tableCardCombination.Type)
        {

            case CardCombinationType.None:
                return TrySelectOpeningPlay(aiPlayer, tableState, roundState, ruleManager);

            case CardCombinationType.Single:
                return TrySelectSingle(aiPlayer, tableState, roundState, ruleManager);

            case CardCombinationType.SameRank:
                return TrySelectSameRank(aiPlayer, tableState, roundState, ruleManager);

            case CardCombinationType.Straight:
                return TrySelectStraight(aiPlayer, tableState, roundState, ruleManager);

            default:
                return false;

        }

    }

    // 테이블에 낼 카드가 없는 상태에서의 플레이 선택 시도 (선공 시 AI의 기본 오프닝 선택 로직)
    // SameRank 우선 고려. SameRank 없으면 Single. Straight는 고려하지 않음
    // SameRank는 가장 긴 것부터, 길이가 같다면 더 약한 rank
    private bool TrySelectOpeningPlay(PlayerState aiPlayer, TableState tableState, RoundState roundState, RuleManager ruleManager)
    {

        if (aiPlayer == null || ruleManager == null || tableState == null || roundState == null)
        {

            return false;

        }

        if (aiPlayer.handCards.Count == 0)
        {

            return false;

        }

        // SameRank 고려. length 길게, rank 낮게
        Dictionary<CardRank, List<CardData>> groups = new Dictionary<CardRank, List<CardData>>();
        List<CardData> jokers = new List<CardData>();

        for (int i = 0; i < aiPlayer.handCards.Count; i++)
        {

            CardData card = aiPlayer.handCards[i];

            if (card.IsJoker)
            {

                jokers.Add(card);
                continue;

            }

            if (!groups.ContainsKey(card.rank))
            {

                groups[card.rank] = new List<CardData>();

            }

            groups[card.rank].Add(card);

        }

        int bestLength = 1;
        bool foundSameRank = false;
        CardRank chosenRank = CardRank.Three;

        foreach (KeyValuePair<CardRank, List<CardData>> pair in groups)
        {

            CardRank rank = pair.Key;
            int count = pair.Value.Count;

            if (count < 2)
            {

                continue;

            }

            if (!foundSameRank || count > bestLength)
            {

                bestLength = count;
                chosenRank = rank;
                foundSameRank = true;

            }
            else if (count == bestLength)
            {

                if (ruleManager.GetRankPower(rank, tableState, roundState) < ruleManager.GetRankPower(chosenRank, tableState, roundState))
                {

                    chosenRank = rank;

                }

            }

        }

        if (foundSameRank)
        {

            aiPlayer.ClearSelectedCards();

            List<CardData> chosenGroup = groups[chosenRank];

            for (int i = 0; i < chosenGroup.Count; i++)
            {

                aiPlayer.SelectCard(chosenGroup[i]);

            }

            return true;

        }

        // SameRank 없으면 Single
        CardData lowest = aiPlayer.handCards[0];

        for (int i = 1; i < aiPlayer.handCards.Count; i++)
        {

            if (ComparePlayableSingle(aiPlayer.handCards[i], lowest, tableState, roundState, ruleManager) < 0)
            {

                lowest = aiPlayer.handCards[i];

            }

        }

        aiPlayer.SelectCard(lowest);
        return true;

    }

    // 테이블이 Single일 때 사용
    // 테이블이 조커면 못 냄
    // 고정 있으면 거기 맞는 가장 약한 카드(조커는 고려 일단 X), 없으면 조커 고려해서 냄
    private bool TrySelectSingle(PlayerState aiPlayer, TableState tableState, RoundState roundState, RuleManager ruleManager)
    {

        bool tableIsAllJoker = IsAllJokerPlay(tableState.tableCards);

        if (tableIsAllJoker) return false;

        CardRank targetRank = tableState.tableCardCombination.rank;

        if (roundState.isRankTight)
        {

            bool foundTightRank = false;
            CardData bestTightCard = default;

            for (int i = 0; i < aiPlayer.handCards.Count; i++)
            {

                CardData card = aiPlayer.handCards[i];

                if (card.IsJoker) continue;
                if (card.rank != roundState.tightNextRank) continue;

                if (roundState.isSuitTight)
                {

                    if (!roundState.tightSuits.Contains(card.suit))
                    {

                        continue;

                    }

                }

                if (!foundTightRank || ComparePlayableSingle(card, bestTightCard, tableState, roundState, ruleManager) < 0)
                {

                    bestTightCard = card;
                    foundTightRank = true;

                }

            }

            if (foundTightRank)
            {

                aiPlayer.SelectCard(bestTightCard);
                return true;

            }

            for (int i = 0; i < aiPlayer.handCards.Count; i++)
            {

                CardData card = aiPlayer.handCards[i];

                if (card.IsJoker)
                {

                    aiPlayer.SelectCard(card);
                    return true;

                }

            }

            return false;

        }

        bool found = false;
        CardData best = default;

        for (int i = 0; i < aiPlayer.handCards.Count; i++)
        {

            CardData card = aiPlayer.handCards[i];

            if (card.IsJoker)
            {

                if (!found || ComparePlayableSingle(card, best, tableState, roundState, ruleManager) < 0)
                {

                    best = card;
                    found = true;

                }

                continue;

            }

            if (!ruleManager.IsRankStronger(card.rank, targetRank, tableState, roundState))
            {

                continue;

            }

            if (roundState.isSuitTight)
            {

                if (!roundState.tightSuits.Contains(card.suit))
                {

                    continue;

                }

            }

            if (!found || ComparePlayableSingle(card, best, tableState, roundState, ruleManager) < 0)
            {

                best = card;
                found = true;

            }

        }

        if (!found) return false;

        aiPlayer.SelectCard(best);
        return true;

    }

    // 어떤 Single이 더 약한지 비교. 조커는 가장 강한 쪽으로 취급, 일반은 GetRankPower로 비교(반전 상태도 반영됨), 동률이면 문양으로 비교
    private int ComparePlayableSingle(CardData a, CardData b, TableState tableState, RoundState roundState, RuleManager ruleManager)
    {

        if (a.IsJoker && !b.IsJoker) return 1;
        if (!a.IsJoker && b.IsJoker) return -1;
        if (a.IsJoker && b.IsJoker) return 0;

        int aPower = ruleManager.GetRankPower(a.rank, tableState, roundState);
        int bPower = ruleManager.GetRankPower(b.rank, tableState, roundState);

        if (aPower != bPower)
        {

            return aPower.CompareTo(bPower);

        }

        return a.suit.CompareTo(b.suit);
    }

    // 테이블이 SameRank일 때 사용
    // 손패를 숫자별로 묶고 조커를 따로 모아서 장수, 고정, 조커 보충 가능 여부 검사
    // 최종적으로 선택된 카드들(과 필요한 조커)을 selectedCards에 담고 반환
    private bool TrySelectSameRank(PlayerState aiPlayer, TableState tableState, RoundState roundState, RuleManager ruleManager)
    {

        bool tableIsAllJoker = IsAllJokerPlay(tableState.tableCards);

        if (tableIsAllJoker) return false;

        int targetLength = tableState.tableCardCombination.length;

        Dictionary<CardRank, List<CardData>> groups = new Dictionary<CardRank, List<CardData>>();
        List<CardData> jokers = new List<CardData>();

        for (int i = 0; i < aiPlayer.handCards.Count; i++)
        {

            CardData card = aiPlayer.handCards[i];

            if (card.IsJoker)
            {

                jokers.Add(card);
                continue;

            }

            if (!groups.ContainsKey(card.rank))
            {

                groups[card.rank] = new List<CardData>();

            }

            groups[card.rank].Add(card);

        }

        bool found = false;
        CardRank chosenRank = CardRank.Three;

        if (roundState.isRankTight)
        {

            foreach (KeyValuePair<CardRank, List<CardData>> pair in groups)
            {

                CardRank rank = pair.Key;
                List<CardData> cardsOfRank = pair.Value;

                if (rank != roundState.tightNextRank)
                {

                    continue;

                }

                int neededJoker = targetLength - cardsOfRank.Count;

                if (neededJoker < 0)
                {

                    neededJoker = 0;

                }

                if (neededJoker > jokers.Count)
                {

                    continue;

                }

                if (roundState.isSuitTight)
                {

                    List<CardSuit> suitsOfRank = ExtractNonJokerSuits(cardsOfRank);

                    bool satisfiesSuitTight = true;

                    for (int i = 0; i < roundState.tightSuits.Count; i++)
                    {

                        if (!suitsOfRank.Contains(roundState.tightSuits[i]))
                        {

                            satisfiesSuitTight = false;
                            break;

                        }

                    }

                    if (!satisfiesSuitTight)
                    {

                        continue;

                    }

                }

                chosenRank = rank;
                found = true;
                break;

            }

            aiPlayer.ClearSelectedCards();

            if (found)
            {

                List<CardData> chosenGroup = groups[chosenRank];

                for (int i = 0; i < chosenGroup.Count && i < targetLength; i++)
                {

                    aiPlayer.SelectCard(chosenGroup[i]);

                }

                int selectedCount = aiPlayer.SelectedCards.Count;

                for (int i = 0; i < jokers.Count && selectedCount < targetLength; i++)
                {

                    aiPlayer.SelectCard(jokers[i]);
                    selectedCount++;

                }

                return aiPlayer.SelectedCards.Count == targetLength;

            }

            if (jokers.Count >= targetLength)
            {

                if (!roundState.isSuitTight || roundState.tightSuits.Count == 0)
                {

                    aiPlayer.ClearSelectedCards();

                    for (int i = 0; i < targetLength; i++)
                    {

                        aiPlayer.SelectCard(jokers[i]);

                    }

                    return true;

                }

            }

            return false;

        }

        foreach (KeyValuePair<CardRank, List<CardData>> pair in groups)
        {

            CardRank rank = pair.Key;
            List<CardData> cardsOfRank = pair.Value;

            if (!ruleManager.IsRankStronger(rank, tableState.tableCardCombination.rank, tableState, roundState))
            {

                continue;

            }

            int neededJoker = targetLength - cardsOfRank.Count;

            if (neededJoker < 0)
            {

                neededJoker = 0;

            }

            if (neededJoker > jokers.Count)
            {

                continue;

            }

            if (roundState.isSuitTight)
            {

                List<CardSuit> suitsOfRank = ExtractNonJokerSuits(cardsOfRank);

                bool satisfiesSuitTight = true;

                for (int i = 0; i < roundState.tightSuits.Count; i++)
                {

                    if (!suitsOfRank.Contains(roundState.tightSuits[i]))
                    {

                        satisfiesSuitTight = false;
                        break;

                    }

                }

                if (!satisfiesSuitTight)
                {

                    continue;

                }

            }

            if (!found || ruleManager.GetRankPower(rank, tableState, roundState) < ruleManager.GetRankPower(chosenRank, tableState, roundState))
            {

                chosenRank = rank;
                found = true;

            }

        }

        if (!found)
        {

            return false;

        }

        aiPlayer.ClearSelectedCards();

        List<CardData> normalChosenGroup = groups[chosenRank];

        for (int i = 0; i < normalChosenGroup.Count && i < targetLength; i++)
        {

            aiPlayer.SelectCard(normalChosenGroup[i]);

        }

        int normalSelectedCount = aiPlayer.SelectedCards.Count;

        for (int i = 0; i < jokers.Count && normalSelectedCount < targetLength; i++)
        {

            aiPlayer.SelectCard(jokers[i]);
            normalSelectedCount++;

        }

        return aiPlayer.SelectedCards.Count == targetLength;

    }

    // 테이블이 Straight일 때 사용
    // 손패를 문양별로 묶고 각 그룹 안에서 목표 길이만큼 연속 숫자 있는지 탐색
    private bool TrySelectStraight(PlayerState aiPlayer, TableState tableState, RoundState roundState, RuleManager ruleManager)
    {

        int targetLength = tableState.tableCardCombination.length;
        CardRank targetRank = tableState.tableCardCombination.rank;

        Dictionary<CardSuit, List<CardData>> suitGroups = new Dictionary<CardSuit, List<CardData>>();

        for (int i = 0; i < aiPlayer.handCards.Count; i++)
        {

            CardData card = aiPlayer.handCards[i];

            if (card.IsJoker)
            {

                continue;

            }

            if (!suitGroups.ContainsKey(card.suit))
            {

                suitGroups[card.suit] = new List<CardData>();

            }

            suitGroups[card.suit].Add(card);

        }

        bool found = false;
        List<CardData> bestStraight = null;
        CardRank bestTopRank = CardRank.Two;

        foreach (KeyValuePair<CardSuit, List<CardData>> pair in suitGroups)
        {

            List<CardData> cards = pair.Value;
            ruleManager.SortCardsByRank(cards);

            for (int start = 0; start <= cards.Count - targetLength; start++)
            {

                bool isStraight = true;

                for (int offset = 1; offset < targetLength; offset++)
                {

                    int prev = (int)cards[start + offset - 1].rank;
                    int current = (int)cards[start + offset].rank;

                    if (current != prev + 1)
                    {

                        isStraight = false;
                        break;

                    }

                }

                if (!isStraight)
                {

                    continue;

                }

                CardRank topRank = cards[start + targetLength - 1].rank;

                if (!ruleManager.IsRankStronger(topRank, targetRank, tableState, roundState))
                {

                    continue;

                }

                if (!found || ruleManager.GetRankPower(topRank, tableState, roundState) < ruleManager.GetRankPower(bestTopRank, tableState, roundState))
                {

                    bestStraight = new List<CardData>();
                    bestTopRank = topRank;
                    found = true;

                    for (int i = 0; i < targetLength; i++)
                    {

                        bestStraight.Add(cards[start + i]);

                    }

                }

            }

        }

        if (!found || bestStraight == null)
        {

            return false;

        }

        aiPlayer.ClearSelectedCards();

        for (int i = 0; i < bestStraight.Count; i++)
        {

            aiPlayer.SelectCard(bestStraight[i]);

        }

        return true;

    }

    // 조커 제외한 문양만 중복 없이 추출 후 반환
    private List<CardSuit> ExtractNonJokerSuits(List<CardData> cards)
    {

        List<CardSuit> suits = new List<CardSuit>();

        for (int i = 0; i < cards.Count; i++)
        {

            if (cards[i].IsJoker) continue;

            if (!suits.Contains(cards[i].suit))
            {

                suits.Add(cards[i].suit);

            }

        }

        return suits;

    }

    // 모두 조커인지 판정하는 불리언 함수
    private bool IsAllJokerPlay(List<CardData> cards)
    {

        if (cards == null || cards.Count == 0) return false;

        for (int i = 0; i < cards.Count; i++)
        {

            if (!cards[i].IsJoker)
            {

                return false;

            }

        }

        return true;

    }

}


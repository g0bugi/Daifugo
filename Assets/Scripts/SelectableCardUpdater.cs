using System.Collections.Generic;
using UnityEngine;

public class SelectableCardUpdater
{

    // 카드 강약 비교를 해야 하니까 RankComparer가 필요한데 내부에서 new RankComparer()를 또 해버리면 의존성이 숨겨지고 테스트나 교체가 어려워진다고 함
    // 필요한 도구를 밖에서 주입받는 구조
    private readonly RankComparer rankComparer;

    public SelectableCardUpdater(RankComparer rankComparer)
    {

        this.rankComparer = rankComparer;

    }

    // 손패 전체를 선택 불가 상태로 초기화한 뒤, 
    // 현재 테이블 조합 타입에 따라 Single, SameRank, Straight, None 중 어떤 규칙으로 선택 가능 카드를 표시할 지 결정
    public void UpdateSelectableCards(PlayerState player, TableState table, RoundState roundState)
    {

        if (player == null) return;
        if (table == null) return;

        ResetSelectableCards(player);

        //if (player.SelectedCards != null && player.SelectedCards.Count > 0)
        //{

        //    DetectSelectableCardsWithCurrentSelection(player, table, roundState);
        //    return;

        //}

        switch (table.tableCardCombination.Type)
        {

            case CardCombinationType.Single:
                DetectSingleSelectableCards(player, table, roundState);
                return;

            case CardCombinationType.SameRank:
                DetectSameRankSelectableCards(player, table, roundState);
                return;

            case CardCombinationType.Straight:
                DetectStraightSelectableCards(player, table, roundState);
                return;

            case CardCombinationType.None:
                ActivateAllVisibleCards(player);
                return;

        }

    }
    //private void DetectSelectableCardsWithCurrentSelection(PlayerState player, TableState table, RoundState roundState)
    //{

    //    switch (table.tableCardCombination.Type)
    //    {

    //        case CardCombinationType.Single:
    //            DetectSingleSelectableCardsWithCurrentSelection(player, table, roundState);
    //            return;

    //        case CardCombinationType.SameRank:
    //            DetectSameRankSelectableCardsWithCurrentSelection(player, table, roundState);
    //            return;

    //        case CardCombinationType.Straight:
    //            DetectStraightSelectableCardsWithCurrentSelection(player, table, roundState);
    //            return;

    //        case CardCombinationType.None:
    //            DetectOpeningSelectableCardsWithCurrentSelection(player);
    //            return;

    //    }

    //}

    // 손패의 모든 카드들을 선택 불가능 상태로 초기화, 이후 다시 조건에 맞는 카드만 true로 켜기 위한 준비 단계
    private void ResetSelectableCards(PlayerState player)
    {

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];
            card.isSelectable = false;
            player.handCards[i] = card;

        }

    }

    // 테이블이 None일 때 사용. 숨겨지지 않은 카드들을 모두 선택 가능 상태로 바꿈 (아무 카드나 낼 수 있는 상태)
    private void ActivateAllVisibleCards(PlayerState player)
    {

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (!card.isHidden)
            {

                card.isSelectable = true;
                player.handCards[i] = card;

            }

        }

    }

    // 테이블이 Single일 때 사용.
    // 테이블이 조커이면 아무것도 못 냄
    // 내 카드가 조커면 선택 가능
    // 숫자 고정이면 거기 따라야 함
    // 아니면 서열에 맞아야 함
    // 문양 고정이면 그 문양이어야 함
    private void DetectSingleSelectableCards(PlayerState player, TableState table, RoundState roundState)
    {

        bool tableIsAllJoker = IsAllJokerPlay(table.tableCards);
        CardRank targetRank = table.tableCardCombination.rank;

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (tableIsAllJoker)
            {

                card.isSelectable = false;
                player.handCards[i] = card;
                continue;

            }

            if (card.IsJoker)
            {

                card.isSelectable = true;
                player.handCards[i] = card;
                continue;

            }

            if (roundState.isRankTight)
            {

                if (card.rank != roundState.tightNextRank)
                {

                    card.isSelectable = false;
                    player.handCards[i] = card;
                    continue;

                }

            }
            else
            {

                if (!rankComparer.IsRankStronger(card.rank, targetRank, table, roundState))
                {

                    card.isSelectable = false;
                    player.handCards[i] = card;
                    continue;

                }

            }

            if (roundState.isSuitTight)
            {

                if (!roundState.tightSuits.Contains(card.suit))
                {

                    card.isSelectable = false;
                    player.handCards[i] = card;
                    continue;

                }

            }

            card.isSelectable = true;
            player.handCards[i] = card;

        }

    }

    // 테이블이 SameRank일 때 사용.
    // 대응 가능한 카드 후보의 isSelectable을 켬.
    // 손패를 Rank로 묶어 딕셔너리로 관리
    // 묶인 각 그룹이, length가 맞는지, 아니어도 조커로 보충 가능한지, 숫자/문양 고정 만족하는지 확인 후 조건 만족하는 카드들만 선택 가능하게 켬
    private void DetectSameRankSelectableCards(PlayerState player, TableState table, RoundState roundState)
    {

        int targetLength = table.tableCardCombination.length;
        CardRank targetRank = table.tableCardCombination.rank;

        Dictionary<CardRank, List<CardData>> rankCardDictionary = new Dictionary<CardRank, List<CardData>>();

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (card.IsJoker) continue;

            if (!rankCardDictionary.ContainsKey(card.rank))
            {

                rankCardDictionary[card.rank] = new List<CardData>();

            }

            rankCardDictionary[card.rank].Add(card);

        }

        int playerJokerCount = player.GetJokerCount();

        // 조커만으로 테이블 조합 길이를 맞출 수 있다면,
        // 2, 2 위나 반전 상태의 3, 3 위에도 조커, 조커를 낼 수 있으므로
        // 조커를 선택 가능하게 켠다.
        if (playerJokerCount >= targetLength)
        {

            bool canUseOnlyJokers = true;

            if (roundState.isSuitTight && roundState.tightSuits != null && roundState.tightSuits.Count > 0)
            {

                // 문양 고정이 걸려 있으면, 필요한 문양 수만큼 조커가 있어야 함
                canUseOnlyJokers = roundState.tightSuits.Count <= playerJokerCount;

            }

            if (canUseOnlyJokers)
            {

                ActivateOnlyJokers(player, targetLength);

            }

        }

        foreach (KeyValuePair<CardRank, List<CardData>> pair in rankCardDictionary)
        {

            CardRank rank = pair.Key;
            List<CardData> cardsOfRank = pair.Value;

            if (roundState.isRankTight)
            {

                if (rank != roundState.tightNextRank)
                {

                    continue;

                }

            }
            else
            {

                if (!rankComparer.IsRankStronger(rank, targetRank, table, roundState))
                {

                    continue;

                }

            }

            int neededJoker = targetLength - cardsOfRank.Count;

            if (neededJoker < 0)
            {

                neededJoker = 0;

            }

            if (neededJoker > playerJokerCount)
            {

                continue;

            }

            if (roundState.isSuitTight)
            {

                if (!SatisfiesSuitTightWithJoker(cardsOfRank, roundState, playerJokerCount))
                {

                    continue;

                }

            }

            if (roundState.isSuitTight)
            {

                ActivateSameRankCardForSuitTight(player, cardsOfRank, roundState, targetLength, playerJokerCount);

            }
            else
            {

                ActivateSameRankCard(player, rank, targetLength);

            }

        }

    }
    private bool SatisfiesSuitTightWithJoker(List<CardData> cardsOfRank, RoundState roundState, int jokerCount)
    {

        if (roundState == null) return false;

        if (!roundState.isSuitTight)
        {

            return true;

        }

        List<CardSuit> suitsOfRank = ExtractNonJokerSuits(cardsOfRank);

        int missingSuitCount = 0;

        for (int i = 0; i < roundState.tightSuits.Count; i++)
        {

            CardSuit requiredSuit = roundState.tightSuits[i];

            if (!suitsOfRank.Contains(requiredSuit))
            {

                missingSuitCount++;

            }

        }

        return missingSuitCount <= jokerCount;

    }
    private void ActivateSameRankCardForSuitTight(PlayerState player, List<CardData> cardsOfRank, RoundState roundState, int targetLength, int jokerCount)
    {

        if (player == null || cardsOfRank == null || roundState == null)
        {

            return;

        }

        for (int i = 0; i < cardsOfRank.Count; i++)
        {

            CardData candidate = cardsOfRank[i];

            if (CanUseNonJokerCardInSuitTightSameRank(candidate, cardsOfRank, roundState, targetLength, jokerCount))
            {

                ActivateCardById(player, candidate.cardID);

            }

        }

        if (CanUseJokerInSuitTightSameRank(cardsOfRank, roundState, targetLength, jokerCount))
        {

            ActivateAllJokers(player);

        }

    }
    private bool CanUseNonJokerCardInSuitTightSameRank(CardData candidate, List<CardData> cardsOfRank, RoundState roundState, int targetLength, int jokerCount)
    {

        if (roundState.tightSuits == null || roundState.tightSuits.Count == 0)
        {

            return true;

        }

        int usedCardCount = 1;
        int usedJokerCount = 0;

        List<CardSuit> coveredSuits = new List<CardSuit>();

        if (roundState.tightSuits.Contains(candidate.suit))
        {

            coveredSuits.Add(candidate.suit);

        }

        for (int i = 0; i < roundState.tightSuits.Count; i++)
        {

            CardSuit requiredSuit = roundState.tightSuits[i];

            if (coveredSuits.Contains(requiredSuit))
            {

                continue;

            }

            CardData foundCard;
            bool hasRealCard = TryFindSameRankCardWithSuit(cardsOfRank, requiredSuit, candidate.cardID, out foundCard);

            if (hasRealCard)
            {

                coveredSuits.Add(requiredSuit);
                usedCardCount++;

            }
            else
            {

                usedJokerCount++;
                usedCardCount++;

                if (usedJokerCount > jokerCount)
                {

                    return false;

                }

            }

        }

        if (usedCardCount > targetLength)
        {

            return false;

        }

        int totalAvailableCount = cardsOfRank.Count + jokerCount;

        return totalAvailableCount >= targetLength;

    }
    private bool CanUseJokerInSuitTightSameRank(List<CardData> cardsOfRank, RoundState roundState, int targetLength, int jokerCount)
    {

        if (jokerCount <= 0)
        {

            return false;

        }

        if (roundState.tightSuits == null || roundState.tightSuits.Count == 0)
        {

            return true;

        }

        int usedCardCount = 1;
        int remainingJokerCount = jokerCount - 1;

        List<CardSuit> coveredSuits = new List<CardSuit>();

        // 조커 1장은 필요한 문양 하나를 대신할 수 있음
        if (roundState.tightSuits.Count > 0)
        {

            coveredSuits.Add(roundState.tightSuits[0]);

        }

        for (int i = 0; i < roundState.tightSuits.Count; i++)
        {

            CardSuit requiredSuit = roundState.tightSuits[i];

            if (coveredSuits.Contains(requiredSuit))
            {

                continue;

            }

            CardData foundCard;
            bool hasRealCard = TryFindSameRankCardWithSuit(cardsOfRank, requiredSuit, -1, out foundCard);

            if (hasRealCard)
            {

                coveredSuits.Add(requiredSuit);
                usedCardCount++;

            }
            else
            {

                if (remainingJokerCount <= 0)
                {

                    return false;

                }

                remainingJokerCount--;
                usedCardCount++;

            }

        }

        if (usedCardCount > targetLength)
        {

            return false;

        }

        int totalAvailableCount = cardsOfRank.Count + jokerCount;

        return totalAvailableCount >= targetLength;

    }

    private bool TryFindSameRankCardWithSuit(List<CardData> cardsOfRank, CardSuit suit, int exceptCardID, out CardData foundCard)
    {

        for (int i = 0; i < cardsOfRank.Count; i++)
        {

            CardData card = cardsOfRank[i];

            if (card.cardID == exceptCardID)
            {

                continue;

            }

            if (card.suit == suit)
            {

                foundCard = card;
                return true;

            }

        }

        foundCard = default;
        return false;

    }

    private void ActivateAllJokers(PlayerState player)
    {

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (card.IsJoker)
            {

                card.isSelectable = true;
                player.handCards[i] = card;

            }

        }

    }

    // 조커만으로 낼 수 있는 예외상황에서 조커 카드들만 선택 가능하게 켜는 함수
    private void ActivateOnlyJokers(PlayerState player, int count)
    {

        int activatedCount = 0;

        for (int i = 0; i < player.handCards.Count; i++)
        {

            if (activatedCount >= count) break;

            CardData card = player.handCards[i];

            if (card.IsJoker)
            {

                card.isSelectable = true;
                player.handCards[i] = card;
                activatedCount++;

            }

        }

    }

    // 특정 랭크 카드와 조커를 조합해서 낼 수 있는 상황에서 해당 랭크 카드들과 필요한 수의 조커를 선택 가능하게 켜는 함수
    private void ActivateSameRankCard(PlayerState player, CardRank rank, int targetLength)
    {

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (!card.IsJoker && card.rank == rank)
            {

                card.isSelectable = true;
                player.handCards[i] = card;

            }

        }

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (card.IsJoker)
            {

                card.isSelectable = true;
                player.handCards[i] = card;

            }

        }

    }

    // 테이블이 Straight일 때 사용.
    // 손패를 문양별로 묶고, 각 문양 그룹(4개)에 대해 가능한 연속 숫자 구간을 탐색.
    // 부족한 숫자를 조커로 보충할 수 있는지 확인
    // 최종 최고 숫자가 현재 테이블보다 강한지, 숫자 고정에 맞는지 검사
    // 이후 조건 만족하는 카드들만 선택 가능하게 켬
    private void DetectStraightSelectableCards(PlayerState player, TableState table, RoundState roundState)
    {

        int targetLength = table.tableCardCombination.length;
        CardRank targetRank = table.tableCardCombination.rank;

        Dictionary<CardSuit, List<CardData>> suitGroups = new Dictionary<CardSuit, List<CardData>>();
        List<CardData> jokers = new List<CardData>();

        for (int i = 0; i < player.handCards.Count; i++)
        {

            CardData card = player.handCards[i];

            if (card.IsJoker)
            {

                jokers.Add(card);
                continue;

            }

            if (!suitGroups.ContainsKey(card.suit))
            {

                suitGroups[card.suit] = new List<CardData>();

            }

            suitGroups[card.suit].Add(card);

        }

        foreach (KeyValuePair<CardSuit, List<CardData>> pair in suitGroups)
        {

            List<CardData> suitCards = pair.Value;
            suitCards.Sort(CompareCardRankOnly);

            for (int start = 0; start <= 12 - targetLength + 1; start++)
            {

                List<CardRank> neededRanks = new List<CardRank>();

                for (int offset = 0; offset < targetLength; offset++)
                {

                    neededRanks.Add((CardRank)(start + offset));

                }

                List<CardData> matchedCards = new List<CardData>();
                int missingCount = 0;

                for (int r = 0; r < neededRanks.Count; r++)
                {

                    bool found = false;

                    for (int c = 0; c < suitCards.Count; c++)
                    {

                        if (suitCards[c].rank == neededRanks[r])
                        {

                            matchedCards.Add(suitCards[c]);
                            found = true;
                            break;

                        }

                    }

                    if (!found)
                    {

                        missingCount++;

                    }

                }

                if (missingCount > jokers.Count)
                {

                    continue;

                }

                CardRank topRank = neededRanks[neededRanks.Count - 1];

                if (!IsSelectableRank(topRank, targetRank, table, roundState))
                {

                    continue;

                }

                for (int i = 0; i < matchedCards.Count; i++)
                {

                    ActivateCardById(player, matchedCards[i].cardID);

                }

                for (int i = 0; i < missingCount && i < jokers.Count; i++)
                {

                    ActivateCardById(player, jokers[i].cardID);

                }

            }

        }

    }

    // 손패에서 특정 ID 가진 카드 찾아 isSelectable 켜는 함수
    // 스트레이트에서 씀
    private void ActivateCardById(PlayerState player, int cardID)
    {

        for (int i = 0; i < player.handCards.Count; i++)
        {

            if (player.handCards[i].cardID == cardID)
            {

                CardData card = player.handCards[i];
                card.isSelectable = true;
                player.handCards[i] = card;

                return;

            }

        }

    }

    // 카드 두 장을 숫자 순서대로 비교 정렬
    private int CompareCardRankOnly(CardData a, CardData b)
    {

        return a.rank.CompareTo(b.rank);

    }

    // 현재 규칙 기준으로 선택 가능한 숫자인지 판정
    // 숫자 고정(tight) 상황에서는 단순히 다음에 나와야 하는 랭크인지 여부로 선택 가능 여부가 결정되고, 그렇지 않은 상황에서는 기존의 강약 비교 로직이 적용됨
    private bool IsSelectableRank(CardRank candidateRank, CardRank targetRank, TableState table, RoundState roundState)
    {

        if (roundState != null && roundState.isRankTight)
        {

            return candidateRank == roundState.tightNextRank;

        }

        return rankComparer.IsRankStronger(candidateRank, targetRank, table, roundState);

    }

    // 카드 목록에서 조커가 아닌 카드들의 문양을 추출해서 리스트로 반환하는 함수
    // 문양 고정 조건 검사할 때 씀
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

    // 전달받은 카드 목록이 전부 조커인지 확인하는 함수
    // 테이블이 조커 봉쇄 상태인지 확인, 선택 후보가 조커만으로 구성되는 예외상황인지 확인
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


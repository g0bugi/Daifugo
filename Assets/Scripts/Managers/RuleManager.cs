using System.Collections.Generic;
using UnityEngine;

// 낼 수 있는 카드 판단
// 조합 판정
public class RuleManager : MonoBehaviour
{

    // Refactoring
    private RankComparer rankComparer;
    private CardEffectAnalyzer cardEffectAnalyzer;
    private CombinationResolver combinationResolver;
    private SelectableCardUpdater selectableCardUpdater;

    private void Awake()
    {

        rankComparer = new RankComparer();
        cardEffectAnalyzer = new CardEffectAnalyzer();
        combinationResolver = new CombinationResolver();
        selectableCardUpdater = new SelectableCardUpdater(rankComparer);

    }

    // 테이블에 놓인 현재 카드 조합 보고 손패에서 가능한 족보를 판정해주는 함수. 래퍼로 교체됨
    public void UpdateSelectableCards(PlayerState player, TableState table, RoundState roundState)
    {

        selectableCardUpdater.UpdateSelectableCards(player, table, roundState);

    }

    // 선택한 카드들을 이 함수에 넣으면, 카드 분석의 결과(CardEffectResult)가 나옴. 
    // 즉 적용해야 할 특수 효과들이 어떤 게 몇 개가 있다는 게 struct CardEffectResult로 나온다는 것.
    public CardEffectResult AnalyzeCardEffects(List<CardData> selectedCards, CombinationResolution resolution)
    {

        return cardEffectAnalyzer.Analyze(selectedCards, resolution);

    }

    // 조합을 만들어줌. 래퍼로 바뀜
    public CardCombination MakeCombination(List<CardData> selectedCards)
    {

        return combinationResolver.MakeCombination(selectedCards);

    }

    // 래퍼로 바꿈
    public CombinationResolution ResolveCombination(List<CardData> selectedCards)
    {

        return combinationResolver.ResolveCombination(selectedCards);

    }

    public bool CanPlayCombination(CardCombination selectedCombination, TableState tableState, RoundState roundState, List<CardData> selectedCards)
    {

        CardCombination tableCombination = tableState.tableCardCombination;

        bool tableIsAllJoker = IsAllJokerPlay(tableState.tableCards);
        bool selectedIsAllJoker = IsAllJokerPlay(selectedCards);

        if (tableIsAllJoker) return false;

        if (selectedCombination.Type == CardCombinationType.None) return false;

        if (tableCombination.Type == CardCombinationType.None) return true;

        if (selectedCombination.Type != tableCombination.Type) return false;

        switch (selectedCombination.Type)
        {

            case CardCombinationType.Single:
                {

                    bool tableIsJoker = tableState.tableCards.Count == 1 && tableState.tableCards[0].IsJoker;
                    bool selectedIsJoker = selectedCards.Count == 1 && selectedCards[0].IsJoker;

                    if (tableIsJoker) return false;

                    if (selectedIsJoker)
                    {

                        return true;

                    }

                    if (roundState.isRankTight)
                    {

                        return selectedCombination.rank == roundState.tightNextRank;

                    }

                    if (roundState.isSuitTight)
                    {

                        if (!selectedIsJoker && !roundState.tightSuits.Contains(selectedCards[0].suit))
                        {

                            return false;

                        }

                    }

                    return rankComparer.IsRankStronger(selectedCombination.rank, tableCombination.rank, tableState, roundState);

                }
            case CardCombinationType.SameRank:
                {

                    if (selectedCombination.length != tableCombination.length) return false;

                    if (selectedIsAllJoker) return true;

                    if (roundState.isRankTight)
                    {

                        if (selectedIsAllJoker)
                        {

                            return true;

                        }

                        return selectedCombination.rank == roundState.tightNextRank;

                    }

                    if (roundState.isSuitTight)
                    {

                        if (!SatisfiesSuitTightWithJoker(selectedCards, roundState))
                        {

                            return false;

                        }

                    }

                }
                return rankComparer.IsRankStronger(selectedCombination.rank, tableCombination.rank, tableState, roundState);
            case CardCombinationType.Straight:
                {

                    if (selectedCombination.length != tableCombination.length) return false;

                    if (roundState.isRankTight)
                    {

                        return selectedCombination.rank == roundState.tightNextRank;

                    }

                    return rankComparer.IsRankStronger(selectedCombination.rank, tableCombination.rank, tableState, roundState);

                }
            default:
                return false;

        }

    }
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
    private bool SatisfiesSuitTightWithJoker(List<CardData> selectedCards, RoundState roundState)
    {

        if (selectedCards == null || roundState == null) return false;

        if (!roundState.isSuitTight)
        {

            return true;

        }

        List<CardSuit> selectedSuits = ExtractNonJokerSuits(selectedCards);

        int jokerCount = 0;

        for (int i = 0; i < selectedCards.Count; i++)
        {

            if (selectedCards[i].IsJoker)
            {

                jokerCount++;

            }

        }

        int missingSuitCount = 0;

        for (int i = 0; i < roundState.tightSuits.Count; i++)
        {

            CardSuit requiredSuit = roundState.tightSuits[i];

            if (!selectedSuits.Contains(requiredSuit))
            {

                missingSuitCount++;

            }

        }

        return missingSuitCount <= jokerCount;

    }

    // 현재 적용 중인 서열 기준으로, current가 previous보다 정확히 1단계 강한지 검사
    public bool IsNextRankTightTrigger(CardCombination previous, CardCombination current, TableState tableState, RoundState roundState)
    {

        if (previous.Type == CardCombinationType.None) return false;
        if (previous.Type != current.Type) return false;
        if (previous.length != current.length) return false;

        int previousPower = rankComparer.GetRankPower(previous.rank, tableState, roundState);
        int currentPower = rankComparer.GetRankPower(current.rank, tableState, roundState);

        return currentPower == previousPower + 1;

    }

    // 현재 적용 중인 서열 기준으로, current 다음에 와야 하는 tight rank를 구함
    public bool TryGetNextTightRank(CardRank currentRank, TableState tableState, RoundState roundState, out CardRank nextRank)
    {

        int currentPower = rankComparer.GetRankPower(currentRank, tableState, roundState);
        int targetPower = currentPower + 1;

        for (int i = 0; i <= (int)CardRank.Two; i++)
        {

            CardRank candidate = (CardRank)i;

            if (rankComparer.GetRankPower(candidate, tableState, roundState) == targetPower)
            {

                nextRank = candidate;
                return true;

            }

        }

        nextRank = CardRank.Three;
        return false;

    }

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

    // 리팩토링 과정에서 남겨둔 래퍼들 (AIManager에서 쓰고 있음)
    public int GetRankPower(CardRank rank, TableState tableState, RoundState roundState)
    {

        return rankComparer.GetRankPower(rank, tableState, roundState);

    }
    public bool IsRankStronger(CardRank a, CardRank b, TableState tableState, RoundState roundState)
    {

        return rankComparer.IsRankStronger(a, b, tableState, roundState);

    }
    public void SortCardsByRank(List<CardData> cards)
    {

        cards.Sort(CompareCardRankOnly);

    }
    public int CompareCardRankOnly(CardData a, CardData b)
    {

        return a.rank.CompareTo(b.rank);

    }

}


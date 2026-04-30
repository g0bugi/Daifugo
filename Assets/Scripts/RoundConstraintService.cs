using System.Collections.Generic;
using UnityEngine;

// 라운드 제한 규칙 관리
// 제출이 끝난 후 roundState의 고정(tight) 관련 제약 갱신을 담당
public class RoundConstraintService
{

    private readonly RuleManager ruleManager;

    // 숫자 고정에서 다음에 와야 할 랭크를 계산할 때 RuleManager의 룰이 필요해서 의존성 주입
    public RoundConstraintService(RuleManager ruleManager)
    {

        this.ruleManager = ruleManager;

    }

    // 카드 제출 직후, 두 고정 상태 각각 다시 계산해 roundState에 반영하는 메서드
    public void UpdateConstraintsAfterSubmission(CardCombination previousCombination, List<CardData> previousCards, CardCombination currentCombination, List<CardData> currentCards, TableState tableState, RoundState roundState, bool reverseOrderBeforeEffects)
    {

        UpdateRankTightAfterEffects(previousCombination, currentCombination, tableState, roundState, reverseOrderBeforeEffects);
        UpdateSuitTightAfterEffects(previousCards, currentCards, currentCombination.Type, roundState);

    }

    // 제출 결과 바탕으로 이번 제출이 숫자고정 발동 조건인지 확인, 맞으면 TryGetNextTightRank()로 다음에 와야 할 랭크 계산해서 tightNextRank에 저장
    private void UpdateRankTightAfterEffects(CardCombination previousCombination, CardCombination currentCombination, TableState tableState, RoundState roundState, bool reverseOrderBeforeEffects)
    {

        if (!IsRankTightTriggerBeforeEffects(previousCombination, currentCombination, reverseOrderBeforeEffects))
        {

            roundState.isRankTight = false;
            return;

        }

        CardRank nextTightRank;

        if (ruleManager.TryGetNextTightRank(currentCombination.rank, tableState, roundState, out nextTightRank))
        {

            roundState.isRankTight = true;
            roundState.tightNextRank = nextTightRank;

        }
        else
        {

            roundState.isRankTight = false;

        }

    }

    // 숫자 고정이 발동해야 하는 상황인지 판정
    private bool IsRankTightTriggerBeforeEffects(CardCombination previousCombination, CardCombination currentCombination, bool reverseOrderBeforeEffects)
    {

        if (previousCombination.Type == CardCombinationType.None) return false;
        if (previousCombination.Type != currentCombination.Type) return false;
        if (previousCombination.length != currentCombination.length) return false;

        int previousPower = GetRankPowerWithReverseFlag(previousCombination.rank, reverseOrderBeforeEffects);
        int currentPower = GetRankPowerWithReverseFlag(currentCombination.rank, reverseOrderBeforeEffects);

        return currentPower == previousPower + 1;

    }

    // 반전 상태가 아니면 enum 그대로, 반전이라면 12에서 빼서 숫자 서열 뒤집음
    private int GetRankPowerWithReverseFlag(CardRank rank, bool isReverseOrder)
    {

        int normalPower = (int)rank;

        if (!isReverseOrder)
        {

            return normalPower;

        }

        return 12 - normalPower;

    }

    // 문양 고정 상태 갱신.
    // Single이라면 이전과 현재의 문양이 같을 때 발동
    // SameRank라면 이전과 현재의 카드들에서 겹치는 문양이 있을 때 발동, 그 문양들을 tightSuits에 저장 (하나라도 겹치면 isSuitTight 참)
    private void UpdateSuitTightAfterEffects(List<CardData> previousCards, List<CardData> currentCards, CardCombinationType currentType, RoundState roundState)
    {

        roundState.isSuitTight = false;
        roundState.tightSuits.Clear();

        if (currentType == CardCombinationType.Straight)
        {

            return;

        }

        if (previousCards == null || previousCards.Count == 0)
        {

            return;

        }

        if (currentType == CardCombinationType.Single)
        {

            if (currentCards == null || currentCards.Count == 0)
            {

                return;

            }

            CardData prev = previousCards[0];
            CardData curr = currentCards[0];

            if (!prev.IsJoker && !curr.IsJoker && prev.suit == curr.suit)
            {

                roundState.isSuitTight = true;
                roundState.tightSuits.Add(curr.suit);

            }

            return;

        }

        if (currentType == CardCombinationType.SameRank)
        {

            List<CardSuit> previousSuits = ExtractNonJokerSuits(previousCards);
            List<CardSuit> currentSuits = ExtractNonJokerSuits(currentCards);

            for (int i = 0; i < currentSuits.Count; i++)
            {

                if (previousSuits.Contains(currentSuits[i]) && !roundState.tightSuits.Contains(currentSuits[i]))
                {

                    roundState.tightSuits.Add(currentSuits[i]);

                }

            }

            if (roundState.tightSuits.Count > 0)
            {

                roundState.isSuitTight = true;

            }

        }

    }

    // 카드 리스트에서 조커가 아닌 카드들의 문양만 추출해서 리스트로 반환하는 헬퍼 메서드
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

}


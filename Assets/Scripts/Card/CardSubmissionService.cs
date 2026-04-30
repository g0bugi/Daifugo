using System.Collections.Generic;
using UnityEngine;

// 카드 제출 처리
// 선택 카드 조합 판정 요청, 제출 가능 여부 확인 및 결과 반환
public class CardSubmissionService
{

    public SubmitResult TrySubmit(PlayerState player, TableState tableState, RoundState roundState, RuleManager ruleManager)
    {

        CardCombination previousCombination = tableState.tableCardCombination;

        SubmitResult result = new SubmitResult
        {

            succeeded = false,
            resolution = default,
            effectResult = default,
            playerHandBecameEmpty = false

        };

        if (player == null)
        {

            Debug.LogError("[CardSubmissionService] player가 null");
            return result;

        }

        List<CardData> selectedCards = player.SelectedCards;

        // 조합 해석
        CombinationResolution resolution = ruleManager.ResolveCombination(selectedCards);

        if (!resolution.isValid)
        {

            Debug.Log("[CardSubmissionService] 유효하지 않은 조합");
            return result;

        }

        CardCombination combination = resolution.combination;

        // 제출 가능 여부 검사
        if (!ruleManager.CanPlayCombination(combination, tableState, roundState, selectedCards))
        {

            Debug.Log("[CardSubmissionService] 낼 수 없는 조합");
            return result;

        }

        // 효과 분석
        CardEffectResult effectResult = ruleManager.AnalyzeCardEffects(selectedCards, resolution);

        // 테이블 반영
        tableState.tableCardCombination = combination;

        tableState.tableCards.Clear();

        for (int i = 0; i < selectedCards.Count; i++)
        {

            CardData card = selectedCards[i];
            card.isHidden = false;
            tableState.tableCards.Add(card);

        }

        // 숫자 고정(tight) 계산은 이 함수에서 하지 않음
        // 혁명 / 11 back 등으로 서열이 바뀔 수 있으므로,
        // 효과 적용이 끝난 뒤의 상태를 기준으로 별도 계산해야 함

        // lastPlayedPlayer 갱신
        roundState.lastPlayedPlayer = player;

        // 패스 초기화
        roundState.passedPlayers.Clear();

        // 손패에서 제거
        RemoveSelectedCardsFromHand(player);

        // 손패 비었는지 체크
        if (player.handCards.Count == 0)
        {

            result.playerHandBecameEmpty = true;

        }

        // 선택 초기화
        player.ClearSelectedCards();

        // 결과 세팅
        result.succeeded = true;
        result.resolution = resolution;
        result.effectResult = effectResult;

        return result;

    }

    private void RemoveSelectedCardsFromHand(PlayerState player)
    {

        List<CardData> selectedCards = player.SelectedCards;

        for (int i = 0; i < selectedCards.Count; i++)
        {

            CardData selected = selectedCards[i];

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

}


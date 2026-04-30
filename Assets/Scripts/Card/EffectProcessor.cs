using UnityEngine;

// 카드 효과 적용
// 5, 8, 9, 10, 11, 혁명 처리
// 효과 결과를 상태에 반영
public class EffectProcessor
{

    public void ApplyEffects(CardEffectResult effectResult, PlayerState player, TableState tableState, RoundState roundState, ref TurnDirection turnDirection)
    {

        if (tableState == null)
        {

            Debug.LogError("[EffectProcessor] tableState가 null입니다.");
            return;

        }

        if (roundState == null)
        {

            Debug.LogError("[EffectProcessor] roundState가 null입니다.");
            return;

        }

        if (effectResult.triggerRevolution)
        {

            tableState.IsRevolution = !tableState.IsRevolution;
            Debug.Log("[EffectProcessor] 혁명 발생");

        }

        if (effectResult.reversesDirection)
        {

            Debug.Log("[EffectProcessor] 9 반전");

            switch (turnDirection)
            {

                case TurnDirection.Clockwise:
                    turnDirection = TurnDirection.CounterClockwise;
                    break;

                case TurnDirection.CounterClockwise:
                    turnDirection = TurnDirection.Clockwise;
                    break;

                default:
                    Debug.LogWarning("[EffectProcessor] TurnDirection이 None입니다.");
                    break;

            }

        }

        if (effectResult.giveCount > 0)
        {

            Debug.Log($"[EffectProcessor] 7 주기 {effectResult.giveCount}회 예정");

        }

        if (effectResult.discardCount > 0)
        {

            Debug.Log($"[EffectProcessor] 10 버리기 {effectResult.discardCount}회 예정");

        }

        if (effectResult.skipCount > 0)
        {

            Debug.Log($"[EffectProcessor] 5 스킵 {effectResult.skipCount}회");
            roundState.pendingSkipCount += effectResult.skipCount;

        }

        if (effectResult.triggerEightCut)
        {

            Debug.Log("[EffectProcessor] 8 컷 예약");
            roundState.pendingEightCut = true;

        }

        if (effectResult.trigger11Back)
        {

            Debug.Log("[EffectProcessor] 11 백 예약");
            roundState.pending11Back = true;

        }

    }

}


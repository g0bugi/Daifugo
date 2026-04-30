using UnityEngine;
using System.Collections.Generic;

// 턴 진행 로직 분리
public class TurnManager
{

    public void ProceedTurn(RoundState roundState, PlayerState[] players)
    {

        // CurrentPlayerIndex 변경
        // TurnDirection 반영
        // 탈락자 건너뛰기

    }

    public void NextTurn(RoundState roundState, TurnDirection direction)
    {

        if (roundState == null || roundState.playerTurnList == null || roundState.playerTurnList.Count == 0)
        {

            Debug.LogError("[TurnManager] roundState나 playerTurnList이 null이거나 비어있습니다.");

            return;

        }

        int dir = GetDirectionValue(direction);

        if (dir == 0)
        {

            Debug.LogWarning("[TurnManager] TurnDirection이 None으로 설정되어 있습니다. 방향이 변경되지 않습니다.");

            return;

        }

        int playerCount = roundState.playerTurnList.Count;
        int nextIndex = roundState.currentTurnIndex;

        for (int i = 0; i < playerCount; i++)
        {

            nextIndex = (nextIndex + dir + playerCount) % playerCount;

            PlayerState nextPlayer = roundState.playerTurnList[nextIndex];

            if (nextPlayer == null || nextPlayer.IsDone) continue;

            roundState.currentTurnIndex = nextIndex;

            return;

        }

    }

    // 다음 플레이어를 미리 보기. 실제 턴은 안 움직이고 다음 활성 플레이어가 누구인지만 알려줌
    public int PeekNextTurnIndex(RoundState roundState, TurnDirection direction)
    {

        if (roundState == null || roundState.playerTurnList == null || roundState.playerTurnList.Count == 0)
        {

            Debug.LogError("[TurnManager] roundState나 playerTurnList가 null이거나 비어 있습니다.");
            return -1;

        }

        int dir = GetDirectionValue(direction);

        if (dir == 0)
        {

            Debug.LogWarning("[TurnManager] TurnDirection이 None입니다.");
            return -1;

        }

        int playerCount = roundState.playerTurnList.Count;
        int nextIndex = roundState.currentTurnIndex;

        for (int i = 0; i < playerCount; i++)
        {

            nextIndex = (nextIndex + dir + playerCount) % playerCount;

            PlayerState nextPlayer = roundState.playerTurnList[nextIndex];

            if (nextPlayer == null || nextPlayer.IsDone) continue;

            return nextIndex;

        }

        return -1;

    }

    private int GetDirectionValue(TurnDirection direction)
    {

        switch (direction)
        {

            case TurnDirection.Clockwise:
                return 1;

            case TurnDirection.CounterClockwise:
                return -1;

            default:
                return 0;

        }

    }

}


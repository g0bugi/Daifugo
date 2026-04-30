using UnityEngine;

// 라운드 상태 관리
// 패스 처리, 라운드 종료, 초기화 등 판정
public class RoundService
{

    public void RegisterPass(RoundState roundState, PlayerState player)
    {

        if (roundState == null)
        {

            Debug.LogError("[RoundService] roundState가 null입니다.");
            return;

        }

        if (player == null)
        {

            Debug.LogWarning("[RoundService] pass 등록 대상 player가 null입니다.");
            return;

        }

        if (!roundState.passedPlayers.Contains(player))
        {

            roundState.passedPlayers.Add(player);

        }

    }

    public bool IsRoundEnd(RoundState roundState)
    {

        if (roundState == null)
        {

            Debug.LogError("[RoundService] roundState가 null입니다.");
            return false;

        }

        if (roundState.lastPlayedPlayer == null)
        {

            return false;

        }

        for (int i = 0; i < roundState.playerTurnList.Count; i++)
        {

            PlayerState player = roundState.playerTurnList[i];

            if (player == null || player.IsDone || player == roundState.lastPlayedPlayer)
            {

                continue;

            }

            if (!roundState.passedPlayers.Contains(player))
            {

                return false;

            }

        }

        return true;

    }

    public void ResetRound(RoundState roundState, TableState tableState, PlayerState roundEnder)
    {

        if (roundState == null)
        {

            Debug.LogError("[RoundService] roundState가 null입니다.");
            return;

        }

        if (tableState == null)
        {

            Debug.LogError("[RoundService] tableState가 null입니다.");
            return;

        }

        roundState.isRoundOver = true;

        roundState.passedPlayers.Clear();
        roundState.is11Back = false;
        roundState.lastPlayedPlayer = null;
        roundState.pendingSkipCount = 0;

        roundState.pendingEightCut = false;
        roundState.pending11Back = false;

        tableState.tableCardCombination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };

        tableState.tableCards.Clear();

        if (roundEnder != null)
        {

            for (int i = 0; i < roundState.playerTurnList.Count; i++)
            {

                if (roundState.playerTurnList[i] == roundEnder)
                {

                    roundState.currentTurnIndex = i;
                    break;

                }

            }

        }

        roundState.isRankTight = false;
        roundState.tightNextRank = CardRank.Three;

        roundState.isSuitTight = false;
        roundState.tightSuits.Clear();

        roundState.isRoundOver = false;

    }

}


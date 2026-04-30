using System.Collections.Generic;
using UnityEngine;

// 게임 결과 처리
// 플레이어 순위 및 계급 확정
// 게임 종료 조건 판정
public class ResultService
{

    private static readonly PlayerHierarchy[] highToLowOrder =
    {
        PlayerHierarchy.Daifugo,
        PlayerHierarchy.Fugo,
        PlayerHierarchy.Heimin,
        PlayerHierarchy.Hinmin,
        PlayerHierarchy.Daihinmin
    };

    private static readonly PlayerHierarchy[] lowToHighOrder =
    {
        PlayerHierarchy.Daihinmin,
        PlayerHierarchy.Hinmin,
        PlayerHierarchy.Heimin,
        PlayerHierarchy.Fugo,
        PlayerHierarchy.Daifugo
    };

    // 정상 종료: 남아 있는 최고 계급 슬롯 부여
    public bool TryHandlePlayerFinish(PlayerState player, PlayerState[] players, RoundState roundState)
    {

        if (player == null)
        {

            Debug.LogWarning("[ResultService] player가 null입니다.");
            return false;

        }

        if (players == null || players.Length == 0)
        {

            Debug.LogError("[ResultService] players가 null이거나 비어 있습니다.");
            return false;

        }

        if (roundState == null)
        {

            Debug.LogError("[ResultService] roundState가 null입니다.");
            return false;

        }

        if (player.handCards.Count > 0)
        {

            return false;

        }

        if (player.IsDone)
        {

            return false;

        }

        PlayerHierarchy hierarchyToAssign = GetHighestAvailableHierarchy(players);

        if (hierarchyToAssign == PlayerHierarchy.None)
        {

            Debug.LogError("[ResultService] 배정 가능한 상위 계급 슬롯이 없습니다.");
            return false;

        }

        player.SetDone(true);
        player.SetThisGameHierarchy(hierarchyToAssign);
        player.SetFinishType(PlayerFinishType.Normal);

        if (SoundManager.Instance != null)
        {

            SoundManager.Instance.PlayHierarchyConfirm();

        }

        Debug.Log($"[ResultService] {player.PlayerName}는 이번 게임에서 제외됨");
        Debug.Log($"[ResultService] {player.PlayerName} 플레이어의 이번 게임 계급: {hierarchyToAssign}");

        RegisterPlayerFinish(player, roundState);

        return true;

    }

    public void RegisterPlayerFinish(PlayerState player, RoundState roundState)
    {

        if (player == null || roundState == null)
        {

            return;

        }

        if (roundState.finishedPlayers.Contains(player))
        {

            return;

        }

        roundState.finishedPlayers.Add(player);

    }

    // 이전 판 대부호 찾기
    public PlayerState FindPreviousDaifugo(PlayerState[] players)
    {

        if (players == null) return null;

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] == null) continue;

            if (players[i].LastGameHierarchy == PlayerHierarchy.Daifugo)
            {

                return players[i];

            }

        }

        return null;

    }

    // 남아 있는 최고 계급 슬롯 찾기
    public PlayerHierarchy GetHighestAvailableHierarchy(PlayerState[] players)
    {

        if (players == null) return PlayerHierarchy.None;

        for (int i = 0; i < highToLowOrder.Length; i++)
        {

            PlayerHierarchy candidate = highToLowOrder[i];

            if (!IsHierarchyTaken(players, candidate))
            {

                return candidate;

            }

        }

        return PlayerHierarchy.None;

    }

    // 남아 있는 최하위 계급 슬롯 찾기
    public PlayerHierarchy GetLowestAvailableHierarchy(PlayerState[] players)
    {

        if (players == null) return PlayerHierarchy.None;

        for (int i = 0; i < lowToHighOrder.Length; i++)
        {

            PlayerHierarchy candidate = lowToHighOrder[i];

            if (!IsHierarchyTaken(players, candidate))
            {

                return candidate;

            }

        }

        return PlayerHierarchy.None;

    }

    private bool IsHierarchyTaken(PlayerState[] players, PlayerHierarchy hierarchy)
    {

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] == null) continue;

            if (players[i].ThisGameHierarchy == hierarchy)
            {

                return true;

            }

        }

        return false;

    }

    // 강제 탈락 공통 처리
    public bool ForceEliminatePlayer(PlayerState target, PlayerHierarchy hierarchy, RoundState roundState, PlayerFinishType finishType)
    {

        if (target == null)
        {

            Debug.LogWarning("[ResultService] 강제 탈락 대상이 null입니다.");
            return false;

        }

        if (roundState == null)
        {

            Debug.LogError("[ResultService] roundState가 null입니다.");
            return false;

        }

        if (target.IsDone)
        {

            return false;

        }

        target.handCards.Clear();
        target.ClearSelectedCards();
        target.SetDone(true);
        target.SetThisGameHierarchy(hierarchy);
        target.SetFinishType(finishType);

        RegisterPlayerFinish(target, roundState);

        Debug.Log($"[ResultService] {target.PlayerName} 강제 탈락, 계급: {hierarchy}");

        return true;

    }

    // Miyako Ochi
    public bool TryApplyMiyakoOchi(PlayerState[] players, RoundState roundState)
    {

        if (players == null || roundState == null)
        {

            return false;

        }

        PlayerState currentDaifugo = GetPlayerByHierarchy(players, PlayerHierarchy.Daifugo);

        if (currentDaifugo == null)
        {

            return false;

        }

        PlayerState previousDaifugo = FindPreviousDaifugo(players);

        if (previousDaifugo == null)
        {

            return false;

        }

        // 연임 성공이면 추락 없음
        if (previousDaifugo == currentDaifugo)
        {

            return false;

        }

        if (previousDaifugo.IsDone)
        {

            return false;

        }

        PlayerHierarchy lowestAvailable = GetLowestAvailableHierarchy(players);

        if (lowestAvailable == PlayerHierarchy.None)
        {

            Debug.LogWarning("[ResultService] Miyako Ochi에 배정할 하위 계급 슬롯이 없습니다.");
            return false;

        }

        bool applied = ForceEliminatePlayer(previousDaifugo, lowestAvailable, roundState, PlayerFinishType.MiyakoOchi);

        if (applied)
        {

            if (SoundManager.Instance != null)
            {

                SoundManager.Instance.PlayMiyakoOchi();

            }


            Debug.Log($"[ResultService] Miyako Ochi 발생: {previousDaifugo.PlayerName} -> {lowestAvailable}");

        }

        return applied;

    }

    public bool IsGameEnd(PlayerState[] players)
    {

        if (players == null || players.Length == 0)
        {

            return false;

        }

        int doneCount = 0;

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] != null && players[i].IsDone)
            {

                doneCount++;

            }

        }

        return doneCount >= players.Length - 1;

    }

    // 마지막 생존자도 "남아 있는 최고 계급 슬롯" 받게 수정
    public PlayerState AssignLastRemainingPlayer(PlayerState[] players, RoundState roundState)
    {

        if (players == null || roundState == null)
        {

            return null;

        }

        PlayerState lastPlayer = null;

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] != null && !players[i].IsDone)
            {

                lastPlayer = players[i];
                break;

            }

        }

        if (lastPlayer == null)
        {

            return null;

        }

        PlayerHierarchy hierarchyToAssign = GetHighestAvailableHierarchy(players);

        if (hierarchyToAssign == PlayerHierarchy.None)
        {

            Debug.LogError("[ResultService] 마지막 플레이어에게 배정할 계급 슬롯이 없습니다.");
            return null;

        }

        lastPlayer.SetDone(true);
        lastPlayer.SetThisGameHierarchy(hierarchyToAssign);
        lastPlayer.SetFinishType(PlayerFinishType.Normal);

        //if (SoundManager.Instance != null)
        //{

        //    SoundManager.Instance.PlayHierarchyConfirm();

        //}

        RegisterPlayerFinish(lastPlayer, roundState);

        Debug.Log($"[ResultService] 마지막 남은 플레이어 {lastPlayer.PlayerName} 계급: {hierarchyToAssign}");

        return lastPlayer;

    }

    public PlayerState GetPlayerByHierarchy(PlayerState[] players, PlayerHierarchy hierarchy)
    {

        if (players == null) return null;

        for (int i = 0; i < players.Length; i++)
        {

            if (players[i] == null) continue;

            if (players[i].ThisGameHierarchy == hierarchy)
            {

                return players[i];

            }

        }

        return null;

    }

    // 최종 결과 출력/표시용 정렬 리스트
    public List<PlayerState> GetPlayersSortedByHierarchy(PlayerState[] players)
    {

        List<PlayerState> result = new List<PlayerState>();

        if (players == null) return result;

        for (int i = 0; i < highToLowOrder.Length; i++)
        {

            PlayerHierarchy hierarchy = highToLowOrder[i];
            PlayerState player = GetPlayerByHierarchy(players, hierarchy);

            if (player != null)
            {

                result.Add(player);

            }

        }

        return result;

    }

}


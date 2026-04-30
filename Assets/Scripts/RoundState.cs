using UnityEngine;
using System.Collections.Generic;

// 현재 라운드의 진행 상태
// 턴 순서 리스트, 현재 턴 인덱스 등
public class RoundState
{

    public List<PlayerState> playerTurnList = new List<PlayerState>();

    public int currentTurnIndex = 0;

    public PlayerState lastPlayedPlayer;

    public List<PlayerState> passedPlayers = new List<PlayerState>();
    public List<PlayerState> finishedPlayers = new List<PlayerState>();

    public int pendingSkipCount = 0;        // 5 스킵

    public bool pendingEightCut = false;
    public bool pending11Back = false;

    public bool is11Back = false;
    public bool isRoundOver = false;

    public bool isRankTight = false;        // false면 일반 규칙대로, true면 다음 플레이어는 무조건 tightNextRank여야 함
    public CardRank tightNextRank = CardRank.Three;

    public bool isSuitTight = false;      
    public List<CardSuit> tightSuits = new List<CardSuit>();   

}


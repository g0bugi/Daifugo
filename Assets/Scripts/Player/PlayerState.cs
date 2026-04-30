using UnityEngine;
using System.Collections.Generic;

// 플레이어 손패 관리
// 손패에 카드 추가
// 손패 정렬
// 손패 조커 개수 관리
public class PlayerState : MonoBehaviour
{

    [SerializeField]
    private string playerName;
    public string PlayerName => playerName;

    [SerializeField]
    public bool isHuman;

    [SerializeField]
    private PlayerHierarchy lastGameHierarchy = PlayerHierarchy.None;
    public PlayerHierarchy LastGameHierarchy => lastGameHierarchy;
    public void SetLastGameHierarchy(PlayerHierarchy hierarchy) { lastGameHierarchy = hierarchy; }

    [SerializeField]
    private PlayerHierarchy thisGameHierarchy = PlayerHierarchy.None;
    public PlayerHierarchy ThisGameHierarchy => thisGameHierarchy;
    public void SetThisGameHierarchy(PlayerHierarchy hierarchy) { thisGameHierarchy = hierarchy; }

    [SerializeField]
    private int hierarchyStreakCount = 0;

    public int HierarchyStreakCount => hierarchyStreakCount;

    // 카드를 다 썼거나 Miyako Ochi 돼서 이 플레이어가 현재 게임에 참여하지 않는 상태 = isDone
    // 마지막까지 카드를 못 냈을 떄에도 모든 플레이어의 계급이 정해졌는지 확인 후 정해졌다면 활성화한 후,
    // 모든 플레이어가 isDone이 되면 게임 종료 판정을 내리도록 해도 괜찮을 것 같다
    [SerializeField]
    private bool isDone = false;
    public bool IsDone => isDone;
    public void SetDone(bool value) { isDone = value;}

    [SerializeField]
    private PlayerFinishType finishType = PlayerFinishType.None;
    public PlayerFinishType FinishType => finishType;

    public void SetFinishType(PlayerFinishType value)
    {

        finishType = value;

    }

    [SerializeField]
    private int score = 0;
    public int Score => score;

    // 손패
    public List<CardData> handCards = new List<CardData>();

    // 손패 중 낼 카드 후보 (조합 적용 전)
    [SerializeField]
    private List<CardData> selectedCards = new List<CardData> ();

    public List<CardData> SelectedCards => selectedCards;

    // 다음 게임 시작 직전, 이번 판 계급을 지난 판 계급으로 넘기기 위한 함수
    public void CommitHierarchyForNextGame()
    {

        UpdateHierarchyStreak();

        lastGameHierarchy = thisGameHierarchy;
        thisGameHierarchy = PlayerHierarchy.None;

    }

    // 손패의 조커 개수 반환
    public int GetJokerCount()
    {

        int count = 0;

        for (int i = 0; i < handCards.Count; i++)
        {

            if (handCards[i].IsJoker)
            {

                count++;

            }

        }

        return count;

    }

    // 카드 한 장 손패에 추가
    public void AddCard(CardData card)
    {

        card.isHidden = !isHuman;
        handCards.Add(card);

    }

    // 손패 안 비어있으면 손패 정렬
    public void SortHand()
    {

        if (handCards.Count == 0)
        {

            Debug.Log("[PlayerState] 손패가 비어서 정렬을 할 수 없습니다");

            return;

        }

        handCards.Sort(CompareCard);

    }
    private int CompareCard(CardData a, CardData b)
    {

        if (a.IsJoker && !b.IsJoker) return 1;
        if (!a.IsJoker && b.IsJoker) return -1;
        if (a.IsJoker && b.IsJoker) return 0;

        int rankCompare = (a.rank.CompareTo(b.rank));

        if (rankCompare != 0) return rankCompare;

        return a.suit.CompareTo(b.suit);

    }

    // 내기 전에 활성화 카드 선택 (조합 만들어지기 전)
    public void SelectCard(CardData card)
    {

        if (IsSelected(card)) return;

        // Debug.Log("[PlayerState] SelectCard 함수 호출됨");

        selectedCards.Add(card);

    }

    public bool IsSelected(CardData card)
    {

        for (int i = 0; i < selectedCards.Count; i++)
        {

            if (selectedCards[i].cardID == card.cardID) return true;

        }

        return false;

    }

    // 내기 전에 활성화 카드 취소
    public void UnselectCard(CardData card)
    {

        for (int i = 0; i < selectedCards.Count; i++)
        {

            if (selectedCards[i].cardID == card.cardID)
            {

                selectedCards.RemoveAt(i);

                return;

            }

        }

    }

    // 내기 전에 활성화 카드 다 없애기
    public void ClearSelectedCards()
    {

        selectedCards.Clear();

    }

    public void UpdateHierarchyStreak()
    {

        if (thisGameHierarchy == PlayerHierarchy.None)
        {

            hierarchyStreakCount = 0;
            return;

        }

        if (lastGameHierarchy == thisGameHierarchy)
        {

            hierarchyStreakCount++;

        }
        else
        {

            hierarchyStreakCount = 1;

        }

    }

}


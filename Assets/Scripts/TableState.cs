using UnityEngine;
using System.Collections.Generic;

// 테이블 위 카드 상태 관리
// 현재 제출된 조합 확인
public class TableState
{

    public CardCombination tableCardCombination;

    public List<CardData> tableCards = new List<CardData>();

    public bool IsRevolution = false;

}


using UnityEngine;

public class RankComparer
{

    public int GetRankPower(CardRank rank, TableState tableState, RoundState roundState)
    {

        int normalPower = (int)rank;

        if (!IsReverseOrder(tableState, roundState))
        {

            return normalPower;

        }

        // 반전 시 3이 가장 강하고 2가 가장 약함
        return 12 - normalPower;

    }

    public bool IsRankStronger(CardRank a, CardRank b, TableState tableState, RoundState roundState)
    {

        return GetRankPower(a, tableState, roundState) > GetRankPower(b, tableState, roundState);

    }

    private bool IsReverseOrder(TableState tableState, RoundState roundState)
    {

        bool revolution = tableState != null && tableState.IsRevolution;
        bool elevenBack = roundState != null && roundState.is11Back;

        // 둘 중 하나만 true면 반전
        return revolution != elevenBack;

    }

}


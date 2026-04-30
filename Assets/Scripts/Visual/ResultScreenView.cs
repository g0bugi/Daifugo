using System.Collections.Generic;
using UnityEngine;

public class ResultScreenView : MonoBehaviour
{

    [SerializeField]
    private Transform rowParent;

    [SerializeField]
    private ResultRowView rowPrefab;

    public void Refresh(PlayerState[] players)
    {

        ClearRows();

        if (players == null || rowPrefab == null || rowParent == null)
        {

            return;

        }

        List<PlayerState> sortedPlayers = new List<PlayerState>(players);

        sortedPlayers.Sort(CompareByCurrentHierarchy);

        for (int i = 0; i < sortedPlayers.Count; i++)
        {

            ResultRowView row = Instantiate(rowPrefab, rowParent);

            row.Bind(i + 1, sortedPlayers[i]);

        }

    }

    private void ClearRows()
    {

        if (rowParent == null) return;

        for (int i = rowParent.childCount - 1; i >= 0; i--)
        {

            Destroy(rowParent.GetChild(i).gameObject);

        }

    }

    private int CompareByCurrentHierarchy(PlayerState a, PlayerState b)
    {

        int aRank = GetHierarchyOrder(a.ThisGameHierarchy);
        int bRank = GetHierarchyOrder(b.ThisGameHierarchy);

        return aRank.CompareTo(bRank);

    }

    private int GetHierarchyOrder(PlayerHierarchy hierarchy)
    {

        switch (hierarchy)
        {

            case PlayerHierarchy.Daifugo:
                return 0;

            case PlayerHierarchy.Fugo:
                return 1;

            case PlayerHierarchy.Heimin:
                return 2;

            case PlayerHierarchy.Hinmin:
                return 3;

            case PlayerHierarchy.Daihinmin:
                return 4;

            default:
                return 999;

        }

    }

}


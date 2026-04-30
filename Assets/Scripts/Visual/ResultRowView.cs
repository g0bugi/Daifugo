using TMPro;
using UnityEngine;

public class ResultRowView : MonoBehaviour
{

    [SerializeField]
    private TMP_Text rankText;

    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private TMP_Text lastHierarchyText;

    [SerializeField]
    private TMP_Text currentHierarchyText;

    public void Bind(int rank, PlayerState player)
    {

        if (player == null)
        {

            return;

        }

        if (rankText != null)
        {

            rankText.text = rank.ToString();

        }

        if (playerNameText != null)
        {

            playerNameText.text = player.PlayerName;

        }

        if (lastHierarchyText != null)
        {

            lastHierarchyText.text = GetHierarchyLabel(player.LastGameHierarchy);

        }

        if (currentHierarchyText != null)
        {

            currentHierarchyText.text = GetHierarchyLabelWithStreak(player);

        }

    }

    private string GetHierarchyLabel(PlayerHierarchy hierarchy)
    {

        switch (hierarchy)
        {

            case PlayerHierarchy.Daifugo:
                return "Daifugo";

            case PlayerHierarchy.Fugo:
                return "Fugo";

            case PlayerHierarchy.Heimin:
                return "Heimin";

            case PlayerHierarchy.Hinmin:
                return "Hinmin";

            case PlayerHierarchy.Daihinmin:
                return "Daihinmin";

            default:
                return "-";

        }

    }

    private string GetHierarchyLabelWithStreak(PlayerState player)
    {

        if (player == null)
        {

            return "-";

        }

        string label = GetHierarchyLabel(player.ThisGameHierarchy);

        if (player.LastGameHierarchy == player.ThisGameHierarchy && player.HierarchyStreakCount >= 1)
        {

            return $"{label} X {player.HierarchyStreakCount + 1}";

        }

        return label;

    }

}


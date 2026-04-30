using UnityEngine;
using UnityEngine.UI;

public class GameStatusView : MonoBehaviour
{

    [SerializeField]
    private GameManager gameManager;

    [Header("Persistent Effect Icons")]
    [SerializeField]
    private GameObject persistentEffectsRoot;

    [SerializeField]
    private Image reverseIcon;

    [SerializeField]
    private Image elevenBackIcon;

    [SerializeField]
    private Image revolutionIcon;

    [Header("Tight Icons")]
    [SerializeField]
    private GameObject tightIconsRoot;

    [SerializeField]
    private Image numIcon;

    [SerializeField]
    private Image spadeIcon;

    [SerializeField]
    private Image heartIcon;

    [SerializeField]
    private Image diamondIcon;

    [SerializeField]
    private Image clubIcon;

    private void Awake()
    {

        if (gameManager == null)
        {

            gameManager = FindFirstObjectByType<GameManager>();

        }

    }

    private void Update()
    {

        RefreshStatus();

    }

    public void RefreshStatus()
    {

        if (gameManager == null)
        {

            return;

        }

        RefreshPersistentEffectIcons();
        RefreshTightIcons();

    }

    private void RefreshPersistentEffectIcons()
    {

        bool isReverse = gameManager.IsReverseDirection();
        bool is11Back = gameManager.Is11BackActive();
        bool isRevolution = gameManager.IsRevolutionActive();

        bool hasAnyPersistentEffect = isReverse || is11Back || isRevolution;

        if (persistentEffectsRoot != null)
        {

            persistentEffectsRoot.SetActive(hasAnyPersistentEffect);

        }

        SetIcon(reverseIcon, isReverse);
        SetIcon(elevenBackIcon, is11Back);
        SetIcon(revolutionIcon, isRevolution);

    }

    private void RefreshTightIcons()
    {

        bool isRankTight = gameManager.IsRankTightActive();
        bool isSuitTight = gameManager.IsSuitTightActive();

        bool hasAnyTight = isRankTight || isSuitTight;

        if (tightIconsRoot != null)
        {

            tightIconsRoot.SetActive(hasAnyTight);

        }

        SetIcon(numIcon, isRankTight);

        SetIcon(spadeIcon, false);
        SetIcon(heartIcon, false);
        SetIcon(diamondIcon, false);
        SetIcon(clubIcon, false);

        if (!isSuitTight)
        {

            return;

        }

        string tightSuitText = gameManager.GetTightSuitText();

        if (string.IsNullOrEmpty(tightSuitText))
        {

            return;

        }

        string[] split = tightSuitText.Split(',');

        for (int i = 0; i < split.Length; i++)
        {

            string suitName = split[i].Trim();

            switch (suitName)
            {

                case "Spade":
                    SetIcon(spadeIcon, true);
                    break;

                case "Heart":
                    SetIcon(heartIcon, true);
                    break;

                case "Diamond":
                    SetIcon(diamondIcon, true);
                    break;

                case "Club":
                    SetIcon(clubIcon, true);
                    break;

            }

        }

    }

    private void SetIcon(Image targetImage, bool isActive)
    {

        if (targetImage == null)
        {

            return;

        }

        targetImage.gameObject.SetActive(isActive);

    }

}


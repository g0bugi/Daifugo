using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class HandView : MonoBehaviour
{

    [SerializeField]
    private PlayerState playerState;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private Transform cardParent;

    [SerializeField]
    private CardSpriteLibrary cardSpriteLibrary;

    [SerializeField]
    private float cardSpacing = 60f;

    [SerializeField]
    private TMP_Text guideText;

    private List<CardView> spawnedCards = new List<CardView>();

    private GameManager gameManager;

    private void Awake()
    {

        if (gameManager == null)
        {

            gameManager = FindFirstObjectByType<GameManager>();

        }

    }

    public void RefreshHand()
    {

        if (playerState == null)
        {

            Debug.Log("[HandView] playerState 빔");
            return;

        }

        if (cardPrefab == null)
        {

            Debug.Log("[HandView] cardPrefab 빔");
            return;

        }

        if (cardParent == null)
        {

            Debug.Log("[HandView] cardParent 빔");
            return;

        }

        if (cardSpriteLibrary == null)
        {

            Debug.Log("[HandView] cardSpriteLibrary 빔");
            return;

        }

        ClearHand();

        for (int i = 0; i < playerState.handCards.Count; i++)
        {

            GameObject obj = Instantiate(cardPrefab, cardParent);

            RectTransform rect = obj.GetComponent<RectTransform>();

            CardData cardData = playerState.handCards[i];

            float yOffset = 0f;

            if (playerState.IsSelected(cardData)) yOffset = 30f;

            if (rect != null) rect.anchoredPosition = new Vector2(i * cardSpacing - playerState.handCards.Count * cardSpacing * 0.5f, yOffset); // 나중에 부채꼴 모양으로

            CardView cardView = obj.GetComponent<CardView>();

            if (cardView == null)
            {

                Debug.LogWarning("[HandView] cardPrefab에 CardView가 없음");
                continue;

            }

            Sprite sprite = cardSpriteLibrary.GetSprite(cardData);

            cardView.SetCard(cardData, sprite, this);

            spawnedCards.Add(cardView);

        }

        RefreshGuideText();

    }

    public void OnCardClicked(CardData card)
    {

        if (playerState == null)
        {

            Debug.Log("[HandView] playerState가 null");

            return;

        }

        if (!playerState.isHuman) return;

        bool canUseNormalTurnSelection = gameManager != null && gameManager.IsCurrentTurn(playerState);
        bool canUseHierarchyExchangeSelection = gameManager != null && gameManager.CanSelectCardsForHierarchyExchange(playerState);

        if (!canUseNormalTurnSelection && !canUseHierarchyExchangeSelection)
        {

            Debug.Log("[HandView] 지금은 이 플레이어가 카드를 선택할 수 있는 상태가 아님");

            return;

        }

        if (card.isHidden)
        {

            Debug.Log("[HandView] isHidden이라 선택할 수 없는 카드");

            return;

        }

        if (!card.isSelectable)
        {

            Debug.Log("[HandView] isSelectable이 false라 선택할 수 없는 카드");

            return;

        }

        if (playerState.IsSelected(card))
        {

            playerState.UnselectCard(card);

        }
        else
        {

            if (canUseHierarchyExchangeSelection)
            {

                int selectionLimit = gameManager.GetHierarchyExchangeSelectionLimit(playerState);

                if (playerState.SelectedCards.Count >= selectionLimit)
                {

                    Debug.Log($"[HandView] 최대 {selectionLimit}장까지만 선택할 수 있습니다.");
                    return;

                }

            }

            playerState.SelectCard(card);

        }

        RefreshHand();

    }

    private void RefreshGuideText()
    {

        if (guideText == null)
        {

            return;

        }

        if (playerState == null || !playerState.isHuman || gameManager == null)
        {

            guideText.text = "";
            return;

        }

        if (gameManager.IsHierarchyExchangePending())
        {

            guideText.text = gameManager.GetHierarchyExchangeGuideText();

        }
        else
        {

            guideText.text = "";

        }

    }

    private void ClearHand()
    {

        for (int i = 0; i < spawnedCards.Count; i++)
        {

            Destroy(spawnedCards[i].gameObject);

        }

        spawnedCards.Clear();

    }

    public bool ShouldUseSelectableDimming()
    {

        if (playerState == null) return false;
        if (!playerState.isHuman) return false;
        if (gameManager == null) return false;

        if (gameManager.IsCurrentTurn(playerState))
        {

            return true;

        }

        if (gameManager.CanSelectCardsForHierarchyExchange(playerState))
        {

            return true;

        }

        return false;

    }

    public bool IsHumanHand()
    {

        if (playerState == null) return false;

        return playerState.isHuman;

    }

}


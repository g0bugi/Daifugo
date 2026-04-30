using System.Collections.Generic;
using UnityEngine;

// 제출한 카드 필드에 놓기
public class TableView : MonoBehaviour
{

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private Transform cardParent;

    [SerializeField]
    private CardSpriteLibrary cardSpriteLibrary;

    [SerializeField]
    private float cardSpacing = 60f;

    [SerializeField]
    private float randomRange = 70f;

    private List<CardView> spawnedCards = new List<CardView>();

    public void RefreshTable(List<CardData> tableCards)
    {

        // ClearTable();

        if (tableCards == null)
        {

            Debug.Log("[TableView] tableCards가 null");
            return;

        }

        float randomXOffset = Random.Range(-randomRange, randomRange);
        float randomYOffset = Random.Range(-randomRange, randomRange);

        for (int i = 0; i < tableCards.Count; i++)
        {

            GameObject obj = Instantiate(cardPrefab, cardParent);

            RectTransform rect = obj.GetComponent<RectTransform>();

            if (rect != null)
            {

                rect.anchoredPosition = new Vector2((i * cardSpacing - tableCards.Count * cardSpacing * 0.5f) + randomXOffset, randomYOffset);

            }

            CardView cardView = obj.GetComponent<CardView>();

            if (cardView == null)
            {

                Debug.LogWarning("[TableView] cardPrefab에 CardView가 없음");
                continue;

            }

            CardData cardData = tableCards[i];

            // 필드 위 카드는 앞면으로 보여야 하니까 강제로 공개
            cardData.isHidden = false;

            Sprite sprite = cardSpriteLibrary.GetSprite(cardData);

            cardView.SetCard(cardData, sprite, null);

            spawnedCards.Add(cardView);

        }

    }

    public void ClearTable()
    {

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            
            if (spawnedCards[i] != null)
            {

                Destroy(spawnedCards[i].gameObject);

            }

        }

        spawnedCards.Clear();

    }

}


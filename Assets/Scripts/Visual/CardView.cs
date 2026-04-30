using System;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{

    [SerializeField]
    private Image cardImage;

    private CardData cardData;
    public CardData CardData => cardData;

    private HandView handView;

    public void SetCard(CardData data, Sprite sprite, HandView owner)
    {

        cardData = data;
        cardImage.sprite = sprite;
        handView = owner;

        // 필드 위 카드들은 항상 원래 컬러
        if (handView == null)
        {

            cardImage.color = Color.white;
            return;

        }

        // 내 턴이 아닐 때는 손패도 모두 원래 컬러
        if (!handView.ShouldUseSelectableDimming())
        {

            cardImage.color = Color.white;
            return;

        }

        // 내 턴일 때만 현재 낼 수 없는 카드들을 어둡게 표시
        if (cardData.isSelectable)
        {

            cardImage.color = Color.white;

        }
        else
        {

            cardImage.color = new Color(0.55f, 0.55f, 0.55f, 1f);

        }

    }

    public void OnClickCard()
    {

        if (handView == null) return;

        handView.OnCardClicked(cardData);

    }

}

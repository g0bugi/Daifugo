using UnityEngine;

// 덱 생성기
public class DeckFactory : MonoBehaviour
{

    public CardData[] totalCards = new CardData[54];

    private void Start()
    {

        for (int i = 0; i < totalCards.Length - 2; i++)
        {

            totalCards[i] = new CardData((CardRank)(i % 13), (CardSuit)(i / 13), i, true, false);

        }

        totalCards[52] = new CardData(CardRank.Ace, CardSuit.Joker, 52, true, false);
        totalCards[53] = new CardData(CardRank.Ace, CardSuit.Joker, 53, true, false);

        ShuffleDeck();

    }

    private void ShuffleDeck()      // Fisher-Yates
    {

        for (int i = totalCards.Length - 1; i > 0; i--)
        {

            int randomIndex = Random.Range(0, i + 1);

            CardData temp = totalCards[i];

            totalCards[i] = totalCards[randomIndex];
            totalCards[randomIndex] = temp;

        }

    }

    public void ResetAndShuffleDeck()
    {

        for (int i = 0; i < totalCards.Length - 2; i++)
        {

            totalCards[i] = new CardData((CardRank)(i % 13), (CardSuit)(i / 13), i, true, false);

        }

        totalCards[52] = new CardData(CardRank.Ace, CardSuit.Joker, 52, true, false);
        totalCards[53] = new CardData(CardRank.Ace, CardSuit.Joker, 53, true, false);

        ShuffleDeck();

    }       // 덱 초기화 후 섞기. 이렇게 하는 이유는, 그냥 다시 섞기만 해도 되긴 하지만,나중에 카드 데이터의 isHidden, isSelectable 같은 값이 남아 꼬일 가능성까지 같이 없애기 위해서. 지금 CardData가 struct라 값복사되긴 하지만, 덱 원본을 매 판 새로 초기화해두는 게 더 안전하다.

    public CardData[] GetDeck()
    {

        return totalCards;

    }

}


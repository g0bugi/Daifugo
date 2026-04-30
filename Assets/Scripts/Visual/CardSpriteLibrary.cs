using UnityEngine;

public class CardSpriteLibrary : MonoBehaviour
{

    [SerializeField]
    private Sprite[] cardSprites = new Sprite[54];      // 스페이드 3 ~ 스페이드 2 ~ 다이아몬드 3 ~ ... ~ 클로버 3 ~ 클로버 2 + 조커 + 조커 이렇게 넣기

    [SerializeField]
    private Sprite backSprite;

    public Sprite GetSprite(CardData card)
    {

        if (card.isHidden) return backSprite;

        return GetFrontSprite(card);

    }

    public Sprite GetFrontSprite(CardData card)
    {

        if (card.cardID < 0 || card.cardID >= cardSprites.Length)
        {

            Debug.LogWarning($"[CardSpriteLibrary] 잘못된 cardID: {card.cardID}");

            return null;

        }

        Sprite sprite = cardSprites[card.cardID];

        if (sprite == null) Debug.LogWarning($"[CardSpriteLibrary] {card.cardID}번 스프라이트가 비어 있음");

        return sprite;

    }

    public Sprite GetBackSprite()
    {

        if (backSprite == null) Debug.LogWarning("[CardSpriteLibrary] backSprite가 비어 있음");

        return backSprite;

    }

    public int GetSpriteCount()
    {

        return cardSprites.Length;

    }

    public bool HasValidSprite(CardData card)
    {

        if (card.cardID < 0 || card.cardID >= cardSprites.Length) return false;

        return cardSprites[card.cardID] != null;

    }

}


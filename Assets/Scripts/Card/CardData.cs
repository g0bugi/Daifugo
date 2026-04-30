
// 카드 한 장의 정보 (데이터 단위)
[System.Serializable]
public struct CardData
{

    public int cardID;

    public CardRank rank;
    public CardSuit suit;

    public bool IsJoker => suit == CardSuit.Joker;

    public bool isHidden;
    public bool isSelectable;

    public CardData(CardRank rank, CardSuit suit, int cardID, bool isHidden, bool isSelectable)
    {

        this.rank = rank;
        this.suit = suit;
        this.cardID = cardID;
        this.isHidden = isHidden;
        this.isSelectable = isSelectable;

    }

}


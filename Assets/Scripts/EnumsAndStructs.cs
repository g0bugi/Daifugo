using System.Collections.Generic;
using UnityEngine;

public enum CardSuit
{

    Spade,
    Diamond,
    Heart,
    Club,
    Joker,
    None

}

public enum CardRank
{

    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace,
    Two

}

public enum TurnDirection
{

    Clockwise,
    CounterClockwise,
    None

}

public enum PlayerHierarchy
{

    Daifugo,
    Fugo,
    Heimin,
    Hinmin,
    Daihinmin,
    None

}

public struct CardEffectResult
{

    public bool triggerRevolution;       // 혁명
    public bool reversesDirection;        // 9

    public int skipCount;       // 5
    public int giveCount;       // 7
    public int discardCount;        // 10

    public bool triggerEightCut;     // 8
    public bool trigger11Back;       // J

}

public enum FollowUpSelectionMode
{

    None,
    GiveCards,
    DiscardCards

}

public struct SubmitResult
{

    public bool succeeded;

    public CombinationResolution resolution;
    public CardEffectResult effectResult;

    public bool playerHandBecameEmpty;

}

public enum CardCombinationType
{

    None,
    Single,
    SameRank,
    Straight

}

public struct CardCombination
{

    public CardCombinationType Type;

    public int length;
    public CardRank rank;

}

public struct CombinationResolution
{

    public bool isValid;

    public CardCombination combination;

    public bool usesJoker;

    public CardRank resolvedRank;
    public CardSuit resolvedSuit;

    public List<CardRank> resolvedRanks;        // 조합 전체가 최종적으로 어떤 숫자들로 해석됐는지 담음 (7 조커 9 -> 7 8 9, 조커 조커 8 8 -> 8 8 8 8)
    public CardSuit straightSuit;

}

public enum PresentationEventType
{

    Message,
    HighlightSeat,
    MoveCard,
    DiscardCard,
    SkipSeat,
    ReverseDirection,
    EightCut,
    ElevenBack,
    HierarchyExchangeStart,
    HierarchyExchangeGive,
    GiveCard,
    PlayerFinish

}

public struct HierarchyExchangeResult
{

    public PlayerState highPlayer;
    public PlayerState lowPlayer;

    public List<CardData> highCardsToGive;
    public List<CardData> lowCardsToGive;

}

public enum PlayerFinishType
{

    None,
    Normal,
    MiyakoOchi,
    ForbiddenFinish

}
using System.Collections.Generic;
using UnityEngine;

public struct FollowUpResolveResult
{

    public bool handled;
    public bool switchedToDiscardMode;
    public bool completedAllFollowUps;
    public PlayerState resolvedPlayer;

    public List<CardData> resolvedCards;
    public PlayerState sourcePlayer;
    public PlayerState targetPlayer;
    public bool isGiveAction;
    public bool isDiscardAction;

}


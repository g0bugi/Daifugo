using System.Collections.Generic;

public class PresentationEvent
{

    private GameManager gameManager;

    public PresentationEventType EventType;

    public string Message;

    public PlayerState SourcePlayer;
    public PlayerState TargetPlayer;

    public List<CardData> Cards = new List<CardData>();

    public float Duration = 0.6f;

}


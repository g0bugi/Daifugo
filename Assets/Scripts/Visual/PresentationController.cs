using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresentationController : MonoBehaviour
{

    [SerializeField]
    private CenterMessageView centerMessageView;

    [SerializeField]
    private PlayerSeatInfoView[] playerSeatInfoViews;

    [SerializeField]
    private CardMovePresenter cardMovePresenter;

    public IEnumerator PlaySequence(List<PresentationEvent> events)
    {

        if (events == null || events.Count == 0)
        {

            yield break;

        }

        Queue<PresentationEvent> eventQueue = new Queue<PresentationEvent>(events);

        while (eventQueue.Count > 0)
        {

            PresentationEvent currentEvent = eventQueue.Dequeue();

            yield return PlayEvent(currentEvent);

        }

    }

    public IEnumerator PlayEvent(PresentationEvent presentationEvent)
    {

        if (presentationEvent == null)
        {

            yield break;

        }

        switch (presentationEvent.EventType)
        {

            case PresentationEventType.Message:
                yield return PlayMessage(presentationEvent);
                break;

            case PresentationEventType.HighlightSeat:
                yield return PlayHighlightSeat(presentationEvent);
                break;

            case PresentationEventType.MoveCard:
            case PresentationEventType.GiveCard:
            case PresentationEventType.HierarchyExchangeGive:
                yield return PlayMoveCard(presentationEvent);
                break;

            case PresentationEventType.DiscardCard:
                yield return PlayDiscardCard(presentationEvent);
                break;

            case PresentationEventType.SkipSeat:
                yield return PlaySkipSeat(presentationEvent);
                break;

            case PresentationEventType.ReverseDirection:
                yield return PlayReverseDirection(presentationEvent);
                break;

            case PresentationEventType.EightCut:
                yield return PlayEightCut(presentationEvent);
                break;

            case PresentationEventType.ElevenBack:
                yield return PlayElevenBack(presentationEvent);
                break;

            case PresentationEventType.PlayerFinish:
                yield return PlayPlayerFinish(presentationEvent);
                break;

            default:
                if (presentationEvent.Duration > 0f)
                {

                    yield return new WaitForSeconds(presentationEvent.Duration);

                }
                break;

        }

    }

    private IEnumerator PlayMessage(PresentationEvent presentationEvent)
    {

        if (centerMessageView != null)
        {

            centerMessageView.ShowMessage(presentationEvent.Message);
        }

        yield return new WaitForSeconds(presentationEvent.Duration);

        if (centerMessageView != null)
        {

            centerMessageView.HideMessage();
        }

    }

    private IEnumerator PlayHighlightSeat(PresentationEvent presentationEvent)
    {

        if (presentationEvent.TargetPlayer != null)
        {

            PlayerSeatInfoView seatView = FindSeatView(presentationEvent.TargetPlayer);

            if (seatView != null)
            {

                seatView.PlayHighlightEffect();
            }

        }

        yield return new WaitForSeconds(presentationEvent.Duration);

    }

    private IEnumerator PlayMoveCard(PresentationEvent presentationEvent)
    {

        if (cardMovePresenter != null)
        {

            yield return cardMovePresenter.PlayMoveCards(
                presentationEvent.SourcePlayer,
                presentationEvent.TargetPlayer,
                presentationEvent.Cards,
                presentationEvent.Duration
            );
        }
        else
        {

            yield return new WaitForSeconds(presentationEvent.Duration);

        }

    }

    private IEnumerator PlayDiscardCard(PresentationEvent presentationEvent)
    {

        if (cardMovePresenter != null)
        {

            yield return cardMovePresenter.PlayDiscardCards(
                presentationEvent.SourcePlayer,
                presentationEvent.Cards,
                presentationEvent.Duration
            );
        }
        else
        {

            yield return new WaitForSeconds(presentationEvent.Duration);

        }

    }

    private IEnumerator PlaySkipSeat(PresentationEvent presentationEvent)
    {

        PlayerSeatInfoView seatView = FindSeatView(presentationEvent.TargetPlayer);

        if (seatView != null)
        {

            seatView.PlaySkipEffect();
        }

        yield return new WaitForSeconds(presentationEvent.Duration);

    }

    private IEnumerator PlayReverseDirection(PresentationEvent presentationEvent)
    {

        if (centerMessageView != null)
        {

            centerMessageView.ShowMessage("9 Opposite");
        }

        yield return new WaitForSeconds(presentationEvent.Duration);

        if (centerMessageView != null)
        {

            centerMessageView.HideMessage();
        }

    }

    private IEnumerator PlayEightCut(PresentationEvent presentationEvent)
    {

        if (centerMessageView != null)
        {

            centerMessageView.ShowMessage("8 Cut");
        }

        yield return new WaitForSeconds(presentationEvent.Duration);

        if (centerMessageView != null)
        {

            centerMessageView.HideMessage();
        }

    }

    private IEnumerator PlayElevenBack(PresentationEvent presentationEvent)
    {

        if (centerMessageView != null)
        {

            centerMessageView.ShowMessage("11 Back");
        }

        yield return new WaitForSeconds(presentationEvent.Duration);

        if (centerMessageView != null)
        {

            centerMessageView.HideMessage();
        }

    }

    private IEnumerator PlayPlayerFinish(PresentationEvent presentationEvent)
    {

        if (centerMessageView != null && presentationEvent.TargetPlayer != null)
        {

            centerMessageView.ShowMessage($"{presentationEvent.TargetPlayer.PlayerName} 순위 확정");
        }

        yield return new WaitForSeconds(presentationEvent.Duration);

        if (centerMessageView != null)
        {

            centerMessageView.HideMessage();
        }

    }

    private PlayerSeatInfoView FindSeatView(PlayerState player)
    {

        if (player == null || playerSeatInfoViews == null)
        {

            return null;

        }

        for (int i = 0; i < playerSeatInfoViews.Length; i++)
        {

            if (playerSeatInfoViews[i] == null) continue;

            if (playerSeatInfoViews[i].PlayerState == player)
            {

                return playerSeatInfoViews[i];

            }

        }

        return null;

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMovePresenter : MonoBehaviour
{

    public IEnumerator PlayMoveCards(PlayerState fromPlayer, PlayerState toPlayer, List<CardData> cards, float duration)
    {

        yield return new WaitForSeconds(duration);

    }

    public IEnumerator PlayDiscardCards(PlayerState fromPlayer, List<CardData> cards, float duration)
    {

        yield return new WaitForSeconds(duration);

    }

}


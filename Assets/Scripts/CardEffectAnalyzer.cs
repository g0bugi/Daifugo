using System.Collections.Generic;

public class CardEffectAnalyzer
{

    public CardEffectResult Analyze(List<CardData> selectedCards, CombinationResolution resolution)
    {

        CardEffectResult result = new CardEffectResult();

        if (!resolution.isValid)
        {

            return result;

        }

        int fiveCount = 0;
        int sevenCount = 0;
        int eightCount = 0;
        int nineCount = 0;
        int tenCount = 0;
        int jCount = 0;

        for (int i = 0; i < selectedCards.Count; i++)
        {

            CardData card = selectedCards[i];

            if (card.IsJoker)
            {

                continue;

            }

            switch (card.rank)
            {

                case CardRank.Five:
                    fiveCount++;
                    break;

                case CardRank.Seven:
                    sevenCount++;
                    break;

                case CardRank.Eight:
                    eightCount++;
                    break;

                case CardRank.Nine:
                    nineCount++;
                    break;

                case CardRank.Ten:
                    tenCount++;
                    break;

                case CardRank.Jack:
                    jCount++;
                    break;

                default:
                    break;

            }

        }

        if (resolution.isValid && (resolution.combination.Type == CardCombinationType.SameRank || resolution.combination.Type == CardCombinationType.Straight) && resolution.combination.length >= 4)
        {

            result.triggerRevolution = true;

        }

        result.skipCount = fiveCount;
        result.giveCount = sevenCount;
        result.discardCount = tenCount;
        result.triggerEightCut = eightCount > 0;
        result.reversesDirection = nineCount > 0;
        result.trigger11Back = jCount > 0;

        return result;

    }

}


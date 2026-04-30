using System.Collections.Generic;
using UnityEngine;

public class CombinationResolver
{

    public CombinationResolution ResolveCombination(List<CardData> selectedCards)
    {

        CombinationResolution result = new CombinationResolution();

        result.isValid = false;
        result.combination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };
        result.usesJoker = false;
        result.resolvedRank = CardRank.Three;
        result.resolvedSuit = CardSuit.None;

        if (selectedCards == null || selectedCards.Count == 0)
        {

            return result;

        }

        result.combination.length = selectedCards.Count;

        // Single
        if (selectedCards.Count == 1)
        {

            CardData card = selectedCards[0];

            result.isValid = true;
            result.combination.Type = CardCombinationType.Single;

            if (card.IsJoker)
            {

                result.usesJoker = true;
                result.combination.rank = CardRank.Two;
                result.resolvedRank = CardRank.Two;
                result.resolvedSuit = CardSuit.Joker;

            }
            else
            {

                result.combination.rank = card.rank;
                result.resolvedRank = card.rank;
                result.resolvedSuit = card.suit;

            }

            result.resolvedRanks = new List<CardRank> { result.resolvedRank };

            return result;

        }

        int jokerCount = 0;
        List<CardData> normalCards = new List<CardData>();

        for (int i = 0; i < selectedCards.Count; i++)
        {

            if (selectedCards[i].IsJoker)
            {

                jokerCount++;

            }
            else
            {

                normalCards.Add(selectedCards[i]);

            }

        }

        if (normalCards.Count == 0 && jokerCount >= 2)
        {

            result.isValid = true;
            result.combination.Type = CardCombinationType.SameRank;
            result.combination.length = selectedCards.Count;
            result.combination.rank = CardRank.Two;

            result.usesJoker = true;
            result.resolvedRank = CardRank.Two;
            result.resolvedSuit = CardSuit.Joker;

            result.resolvedRanks = new List<CardRank>();

            for (int i = 0; i < selectedCards.Count; i++)
            {

                result.resolvedRanks.Add(CardRank.Two);

            }

            return result;

        }

        // SameRank
        if (normalCards.Count > 0)
        {

            CardRank firstRank = normalCards[0].rank;
            bool allSameRank = true;

            for (int i = 1; i < normalCards.Count; i++)
            {

                if (normalCards[i].rank != firstRank)
                {

                    allSameRank = false;
                    break;

                }

            }

            if (allSameRank)
            {

                result.isValid = true;
                result.combination.Type = CardCombinationType.SameRank;
                result.combination.rank = firstRank;
                result.usesJoker = jokerCount > 0;
                result.resolvedRank = firstRank;
                result.resolvedSuit = CardSuit.None;

                result.resolvedRanks = new List<CardRank>();

                for (int i = 0; i < selectedCards.Count; i++)
                {

                    result.resolvedRanks.Add(firstRank);

                }

                return result;

            }

        }

        // Straight
        CombinationResolution straightResult;

        if (TryResolveStraight(selectedCards, out straightResult))
        {

            return straightResult;

        }

        return result;

    }

    public CardCombination MakeCombination(List<CardData> selectedCards)
    {

        CardCombination combination = new CardCombination
        {

            Type = CardCombinationType.None,
            length = 0,
            rank = CardRank.Three

        };

        if (selectedCards == null || selectedCards.Count == 0)
        {

            Debug.Log("[CombinationResolver] selectedCards가 없어서 MakeCombination 함수가 안 됩니다");
            return combination;

        }

        combination.length = selectedCards.Count;

        if (selectedCards.Count == 1)
        {

            combination.Type = CardCombinationType.Single;
            combination.rank = selectedCards[0].rank;

            return combination;

        }

        if (IsSameRank(selectedCards))
        {

            combination.Type = CardCombinationType.SameRank;
            combination.rank = selectedCards[0].rank;

            return combination;

        }

        if (IsStraight(selectedCards))
        {

            List<CardData> copiedCards = new List<CardData>(selectedCards);
            SortCardsByRank(copiedCards);

            combination.Type = CardCombinationType.Straight;
            combination.rank = copiedCards[copiedCards.Count - 1].rank;

            return combination;

        }

        Debug.Log("[CombinationResolver] 아무 조합도 찾지 못했습니다");

        return combination;

    }

    private bool TryResolveStraight(List<CardData> selectedCards, out CombinationResolution result)
    {

        result = new CombinationResolution
        {

            isValid = false,
            combination = new CardCombination
            {

                Type = CardCombinationType.None,
                length = 0,
                rank = CardRank.Three

            },
            usesJoker = false,
            resolvedRank = CardRank.Three,
            resolvedSuit = CardSuit.None,
            resolvedRanks = new List<CardRank>(),
            straightSuit = CardSuit.None

        };

        if (selectedCards == null || selectedCards.Count < 3)
        {

            return false;

        }

        List<CardData> normalCards = new List<CardData>();
        int jokerCount = 0;

        for (int i = 0; i < selectedCards.Count; i++)
        {

            if (selectedCards[i].IsJoker)
            {

                jokerCount++;

            }
            else
            {

                normalCards.Add(selectedCards[i]);

            }

        }

        if (normalCards.Count == 0)
        {

            return false;

        }

        CardSuit suit = normalCards[0].suit;

        for (int i = 1; i < normalCards.Count; i++)
        {

            if (normalCards[i].suit != suit)
            {

                return false;

            }

        }

        SortCardsByRank(normalCards);

        for (int i = 1; i < normalCards.Count; i++)
        {

            if (normalCards[i].rank == normalCards[i - 1].rank)
            {

                return false;

            }

        }

        int totalLength = selectedCards.Count;

        int minStart = 0;
        int maxStart = 12 - totalLength + 1;

        for (int start = minStart; start <= maxStart; start++)
        {

            int neededJoker = 0;
            List<CardRank> resolved = new List<CardRank>();

            for (int offset = 0; offset < totalLength; offset++)
            {

                CardRank targetRank = (CardRank)(start + offset);
                bool found = false;

                for (int j = 0; j < normalCards.Count; j++)
                {

                    if (normalCards[j].rank == targetRank)
                    {

                        found = true;
                        break;

                    }

                }

                if (!found)
                {

                    neededJoker++;

                }

                resolved.Add(targetRank);

            }

            if (neededJoker != jokerCount)
            {

                continue;

            }

            bool allIncluded = true;

            for (int i = 0; i < normalCards.Count; i++)
            {

                bool exists = false;

                for (int j = 0; j < resolved.Count; j++)
                {

                    if (resolved[j] == normalCards[i].rank)
                    {

                        exists = true;
                        break;

                    }

                }

                if (!exists)
                {

                    allIncluded = false;
                    break;

                }

            }

            if (!allIncluded)
            {

                continue;

            }

            result.isValid = true;
            result.combination.Type = CardCombinationType.Straight;
            result.combination.length = totalLength;
            result.combination.rank = resolved[resolved.Count - 1];
            result.usesJoker = jokerCount > 0;
            result.resolvedRank = resolved[resolved.Count - 1];
            result.resolvedSuit = suit;
            result.resolvedRanks = resolved;
            result.straightSuit = suit;

            return true;

        }

        return false;

    }

    private bool IsSameRank(List<CardData> selectedCards)
    {

        CardRank firstRank = selectedCards[0].rank;

        for (int i = 1; i < selectedCards.Count; i++)
        {

            if (selectedCards[i].rank != firstRank)
            {

                return false;

            }

        }

        return true;

    }

    private bool IsStraight(List<CardData> selectedCards)
    {

        if (selectedCards.Count < 3)
        {

            return false;

        }

        List<CardData> sortedCards = new List<CardData>(selectedCards);
        SortCardsByRank(sortedCards);

        CardSuit suit = sortedCards[0].suit;

        for (int i = 1; i < sortedCards.Count; i++)
        {

            if (sortedCards[i].suit != suit)
            {

                return false;

            }

        }

        for (int i = 1; i < sortedCards.Count; i++)
        {

            int prev = (int)sortedCards[i - 1].rank;
            int current = (int)sortedCards[i].rank;

            if (current != prev + 1)
            {

                return false;

            }

        }

        return true;

    }

    private void SortCardsByRank(List<CardData> cards)
    {

        cards.Sort(CompareCardRankOnly);

    }

    private int CompareCardRankOnly(CardData a, CardData b)
    {

        return a.rank.CompareTo(b.rank);

    }

}


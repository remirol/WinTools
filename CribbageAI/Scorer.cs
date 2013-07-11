using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CribbageAI
{
    public class Score
    {
        public Score()
        {
            fifteens = pairs = 0;
            runs = new Dictionary<int, int>();
        }

        public int fifteens;
        public int pairs;
        public int flushCount;
        public Dictionary<int, int> runs;
    }

    public class Scorer
    {
        private List<String> seenCombinations;

        public Scorer()
        {
            seenCombinations = new List<string>();
        }

        /// <summary>
        /// Scores a given hand appropriately
        /// </summary>
        /// <param name="cards">A hand of cards (any size)</param>
        /// <returns>The scoring breakdown for that hand</returns>
        public Score ScoreHand(List<Card> cards)
        {
            seenCombinations.Clear();
            return ScoreUnit(cards);
        }

        private Score ScoreUnit(List<Card> cards)
        {
            Score partialScore = new Score();
            cards.Sort();

            for (int i = 0; i <= cards.Count; i++)
            {
                partialScore.runs[i] = 0;
            }

            // nothing further to score, so just return our zeroes
            if (cards == null || cards.Count < 2)
            {
                return partialScore;
            }

            // don't parse the same set of cards more than once
            StringBuilder sb = new StringBuilder();
            foreach (Card card in cards)
            {
                sb.Append(card.ToShortString());
            }
            if (seenCombinations.Contains(sb.ToString()))
            {
                return partialScore;
            }
            seenCombinations.Add(sb.ToString());

            // do all these cards add up to 15?
            int temp15 = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                temp15 += cards[i].Value;
            }
            if (temp15 == 15)
            {
                partialScore.fifteens += 1;
            }

            // handle checking for pairs
            if (cards.Count == 2 && cards[0].Rank == cards[1].Rank)
            {
                partialScore.pairs = 1;
            }

            // all more-than-two cases can be handled by the sliding gap
            if (cards.Count > 2)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    List<Card> tempCards = new List<Card>(cards);
                    tempCards.RemoveAt(i);
                    Score tempScore = ScoreUnit(tempCards);

                    // these just accumulate
                    partialScore.pairs += tempScore.pairs;
                    partialScore.fifteens += tempScore.fifteens;

                    // shovel over all the smaller iterations
                    for (int j = 1; j <= tempCards.Count; j++)
                    {
                        partialScore.runs[j] += tempScore.runs[j];
                    }
                }

                // again, the sorting lets us just walk up in order
                bool bLongRun = true;
                bool bFlush = true;
                for (int i = 0; i < cards.Count - 1; i++)
                {
                    // don't forget that aces can be high or low
                    if (cards[i + 1].Rank != cards[i].Rank + 1 || (cards[i].Rank == 13 && cards[i + 1].Rank == 1))
                    {
                        bLongRun = false;
                    }

                    // might as well check flushes while we're here
                    if (cards[i + 1].Suit != cards[i].Suit)
                    {
                        bFlush = false;
                    }
                }

                if (bLongRun)
                {
                    partialScore.runs[cards.Count] += 1;
                }

                // only 4s and 5s count
                if (bFlush && (cards.Count == 4 || cards.Count == 5))
                {
                    partialScore.flushCount = cards.Count;
                }
            }
            
            return partialScore;
        }
    }
}

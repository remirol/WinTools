using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CribbageAI
{
    public class EvalScore : IComparable
    {
        public int[] Scores;

        public List<Card> scoredHand;

        public int High
        {
            get { return Scores.ToList().Max(); }
        }

        public int HighIndex
        {
            get { return Scores.ToList().FindIndex(tst => { return this.High == tst; }); }
        }

        public int Low
        {
            get { return Scores.ToList().Min(); }
        }

        public int LowIndex
        {
            get { return Scores.ToList().FindIndex(tst => { return this.Low == tst; }); }
        }

        public double Average
        {
            get { return Scores.ToList().Average(); }
        }

        public double PctOfHigh
        {
            get { return High > 0 ? Average/High : 0.0; }
        }

        public EvalScore()
        {
            Scores = new int[13];
            scoredHand = new List<Card>();
        }

        #region IComparable Members

        // Uses average score for comparison purposes
        public int CompareTo(object obj)
        {
            EvalScore other = obj as EvalScore;
            return (this.Average.CompareTo(other.Average));
        }

        #endregion
    }
}

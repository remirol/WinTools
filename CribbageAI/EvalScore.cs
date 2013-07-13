using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Controls;

namespace CribbageAI
{
    public class EvalScore : IComparable
    {
        public int[] Scores;

        #region Properties

        public ObservableCollection<Card> scoredHand { get; set; }

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

        public double PotentialGain
        {
            get { return Average - Low; }
        }

        #endregion

        public EvalScore()
        {
            Scores = new int[13];
            scoredHand = new ObservableCollection<Card>();
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

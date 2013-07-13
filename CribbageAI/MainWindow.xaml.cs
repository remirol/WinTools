using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing.Imaging;

namespace CribbageAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(String propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        #region Variables

        private Deck deck;
        public Random generator;   // it stays here so everything uses the same one

        private ObservableCollection<Card> _yourHand;
        public ObservableCollection<Card> yourHand
        {
            get { return _yourHand; }
            set { _yourHand = value; Notify("yourHand"); }
        }

        private ObservableCollection<Card> _theirHand;
        public ObservableCollection<Card> theirHand
        {
            get { return _theirHand; }
            set { _theirHand = value; Notify("theirHand"); }
        }

        private Card _upcard;
        public Card upcard
        {
            get { return _upcard; }
            set { _upcard = value; Notify("upcard"); }
        }

        private ObservableCollection<EvalScore> _evaluations;
        public ObservableCollection<EvalScore> evaluations
        {
            get { return _evaluations; }
            set { _evaluations = value; Notify("evaluations"); }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            evaluations = new ObservableCollection<EvalScore>();
        }

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            generator = new Random((int)DateTime.Now.Ticks);
            deck = new Deck(generator);
            DataContext = this;
            eval.IsEnabled = false;
            upcardFront.Visibility = Visibility.Hidden;
            upcardBack.Visibility = Visibility.Hidden;
        }

        private void deal_Click(object sender, RoutedEventArgs e)
        {
            deck.Shuffle();
            playerScore.Items.Clear();
            opponentScore.Items.Clear();
            evaluations.Clear();
            upcardFront.Visibility = Visibility.Hidden;
            upcardBack.Visibility = Visibility.Visible;
            yourHand = new ObservableCollection<Card>(deck.Deal(6));
            theirHand = new ObservableCollection<Card>(deck.Deal(6));
            upcard = deck.Deal();
            eval.IsEnabled = true;
        }

        private void eval_Click(object sender, RoutedEventArgs e)
        {
            List<String> seenCombinations = new List<String>();
            Dictionary<String, EvalScore> handRanks = new Dictionary<String, EvalScore>();
            Scorer scorer = new Scorer();

            // we need to remove two cards each time, so...
            for (int i = 0; i < yourHand.Count; i++)
            {
                List<Card> tempFive = new List<Card>(yourHand);
                tempFive.RemoveAt(i);
                for (int j = 0; j < tempFive.Count; j++)
                {
                    List<Card> tempFour = new List<Card>(tempFive);
                    tempFour.RemoveAt(j);

                    // make sure we aren't doing one we've already done
                    StringBuilder sb = new StringBuilder();
                    tempFour.Sort();
                    foreach (Card card in tempFour)
                    {
                        sb.Append(card.ToShortString() + " ");
                    }
                    String handOption = sb.ToString();
                    if (!seenCombinations.Contains(handOption))
                    {
                        seenCombinations.Add(handOption);

                        // now, we need to evaluate this combination for all 13 possible upcard ranks
                        // and store the average score improvement in our ranking list
                        handRanks[handOption] = new EvalScore();
                        handRanks[handOption].scoredHand = new ObservableCollection<Card>(tempFour);
                        for (int k = 0; k < 13; k++)
                        {
                            // EXCEPT in one case: if we have 12 pairs, that means we have quads
                            // and it's not possible to get a fifth of that rank, so mark it zero
                            Score quads = scorer.ScoreHand(tempFour);
                            if (quads.pairs != 6)
                            {
                                List<Card> tempHand = new List<Card>(tempFour);
                                tempHand.Add(new Card(k + 1, 5));
                                Score tempScore = scorer.ScoreHand(tempHand);
                                handRanks[handOption].Scores[k] = tempScore.totalScore;
                            }
                            else
                            {
                                // it can't get any better so treat it as if we got a card that added nothing
                                // so we don't vaporize the average
                                handRanks[handOption].Scores[k] = quads.totalScore;
                            }
                        }
                    }
                }
            }

            // show us all our evaluations
            List<EvalScore> tempList = new List<EvalScore>();
            foreach (String cards in handRanks.Keys)
            {
                List<Card> finalHand = new List<Card>(handRanks[cards].scoredHand);
                finalHand.Add(upcard);
                tempList.Add(handRanks[cards]);
            }
            tempList.Sort();
            tempList.Reverse();
            evaluations = new ObservableCollection<EvalScore>(tempList);
            upcardFront.Visibility = Visibility.Visible;
            eval.IsEnabled = false;
        }

        #endregion

    }
}

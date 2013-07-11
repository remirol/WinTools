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

        private FontFamily labelFont = new FontFamily("Miriam Fixed");

        private List<Card> yourHand;
        private List<Card> theirHand;
        private Card upcard;

        private ObservableCollection<String> _messages;
        public ObservableCollection<String> logMessages
        {
            get { return _messages; }
            set { _messages = value; Notify("logMessages"); }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _messages = new ObservableCollection<string>();
        }

        private Label MakeLabelFromCard(Card card)
        {
            Label lbl = new Label();
            lbl.FontFamily = labelFont;
            lbl.FontSize = 24;
            lbl.FontWeight = FontWeights.Bold;
            lbl.Content = card.ToShortString();
            return lbl;
        }

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            generator = new Random((int)DateTime.Now.Ticks);
            deck = new Deck(generator);
            DataContext = this;
        }

        private void deal_Click(object sender, RoutedEventArgs e)
        {
            deck.Shuffle();
            playerScore.Items.Clear();
            opponentScore.Items.Clear();
            playerHand.Children.Clear();
            opposingHand.Children.Clear();
            logMessages.Clear();
            if (upcardPanel.Children.Count > 2)
            {
                upcardPanel.Children.RemoveAt(0);
            }

            yourHand = deck.Deal(6);
            foreach (Card card in yourHand)
            {
                playerHand.Children.Add(MakeLabelFromCard(card));
            }

            theirHand = deck.Deal(6);
            foreach (Card card in theirHand)
            {
                opposingHand.Children.Add(MakeLabelFromCard(card));
            }

            upcard = deck.Deal();
        }

        private void eval_Click(object sender, RoutedEventArgs e)
        {
            List<String> seenCombinations = new List<String>();
            Dictionary<String, double> handRanks = new Dictionary<String, double>();
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
                        sb.Append(card.ToShortString());
                    }
                    String handOption = sb.ToString();
                    if (!seenCombinations.Contains(handOption))
                    {
                        seenCombinations.Add(handOption);

                        // now, we need to evaluate this combination for all 13 possible upcard ranks
                        // and store the average score improvement in our ranking list
                        handRanks[handOption] = 0.0;
                        int[] tempScores = new int[13];
                        for (int k = 0; k < 13; k++)
                        {
                            // EXCEPT in one case: if we have 12 pairs, that means we have quads
                            // and it's not possible to get a fifth of that rank, so mark it zero
                            Score quads = scorer.ScoreHand(tempFour);
                            if (quads.pairs != 12)
                            {
                                List<Card> tempHand = new List<Card>(tempFour);
                                tempHand.Add(new Card(k + 1, 1));
                                Score tempScore = scorer.ScoreHand(tempHand);
                                tempScores[k] = tempScore.totalScore;
                            }
                            else
                            {
                                // it can't get any better so treat it as if we got a card that added nothing
                                // so we don't vaporize the average
                                tempScores[k] = quads.totalScore;
                            }
                        }
                        // record it and move on
                        handRanks[handOption] = tempScores.ToList().Average();

                        // show us where we'd have been compared to the actual upcard
                        List<Card> finalHand = new List<Card>(tempFour);
                        finalHand.Add(upcard);
                        Score finalScore = scorer.ScoreHand(finalHand);
                        logMessages.Add(String.Format("Hand: {0}  Average EV: {1}  Actual: {2}",
                            handOption, handRanks[handOption].ToString("F2"), finalScore.totalScore));
                    }
                }
            }

            // and actually show the card here
            upcardPanel.Children.Insert(0, MakeLabelFromCard(upcard));
        }

        #endregion

    }
}

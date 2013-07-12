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


        private Image MakeImageFromCard(Card card, bool faceUp = true)
        {
            Image img = new Image();
            if (faceUp)
            {
                img.Source = card.FrontPicture;
            }
            else
            {
                img.Source = deck.BackPicture;
            }
            img.Width = card.FrontPicture.PixelWidth;
            img.Height = card.FrontPicture.PixelHeight;
            img.Margin = new Thickness(5);
            return img;
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
                playerHand.Children.Add(MakeImageFromCard(card));
            }

            theirHand = deck.Deal(6);
            foreach (Card card in theirHand)
            {
                opposingHand.Children.Add(MakeImageFromCard(card, false));
            }
            upcard = deck.Deal();
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
                        handRanks[handOption].scoredHand = new List<Card>(tempFour);
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

            // show us where we'd have been compared to the actual upcard
            foreach (String cards in handRanks.Keys)
            {
                List<Card> finalHand = new List<Card>(handRanks[cards].scoredHand);
                finalHand.Add(upcard);
                logMessages.Add(String.Format("Hand: {0}  High: {1} ({2})  Low: {3} ({4})  Expected: {5}  Potential: {6}  Actual: {7}  ",
                    cards, handRanks[cards].High, new Card(handRanks[cards].HighIndex + 1, 5).ToShortString(), handRanks[cards].Low, 
                    new Card(handRanks[cards].LowIndex + 1, 5).ToShortString(), handRanks[cards].Average.ToString("F2"), 
                    handRanks[cards].PctOfHigh.ToString("P"), scorer.ScoreHand(finalHand).totalScore));
            }

            // and actually show the card here
            upcardPanel.Children.Insert(0, MakeImageFromCard(upcard));
        }

        #endregion

    }
}

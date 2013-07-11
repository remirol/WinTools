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

        private Deck deck;
        public Random generator;   // it stays here so everything uses the same one

        private FontFamily labelFont = new FontFamily("Miriam Fixed");

        private List<Card> yourHand;
        private List<Card> theirHand;

        private Score yourScore;
        private Score theirScore;

        private ObservableCollection<String> _messages;
        public ObservableCollection<String> logMessages
        {
            get { return _messages; }
            set { _messages = value; Notify("logMessages"); }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            _messages = new ObservableCollection<string>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            generator = new Random((int)DateTime.Now.Ticks);
            deck = new Deck(generator);
            DataContext = this;
        }

        private void GenerateScore(List<Card> hand, ListBox box, ref Score score)
        {
            int points = 0;
            Scorer scorer = new Scorer();

            score = scorer.ScoreHand(hand);
            box.Items.Add(String.Format("fifteens: {0}", score.fifteens));
            box.Items.Add(String.Format("pairs: {0}", score.pairs));
            foreach (int key in score.runs.Keys)
            {
                if (score.runs[key] > 0)
                {
                    box.Items.Add(String.Format("runs of {0}: {1}", key, score.runs[key]));
                    points += score.runs[key] * key;
                }
            }
            points += (score.fifteens + score.pairs) * 2;
            box.Items.Add(String.Format("total score: {0}", points));
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

        private void deal_Click(object sender, RoutedEventArgs e)
        {
            deck.Shuffle();
            playerScore.Items.Clear();
            opponentScore.Items.Clear();
            playerHand.Children.Clear();
            opposingHand.Children.Clear();

            yourHand = deck.Deal(4);
            foreach (Card card in yourHand)
            {
                playerHand.Children.Add(MakeLabelFromCard(card));
            }
            GenerateScore(yourHand, playerScore, ref yourScore);

            theirHand = deck.Deal(4);
            foreach (Card card in theirHand)
            {
                opposingHand.Children.Add(MakeLabelFromCard(card));
            }
            GenerateScore(theirHand, opponentScore, ref theirScore);
        }

    }
}

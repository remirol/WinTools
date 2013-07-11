using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace CribbageAI
{
    public class Deck : INotifyPropertyChanged
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

        private Image _backPicture;
        public Image BackPicture
        {
            get { return _backPicture; }
            set { _backPicture = value; Notify("BackPicture"); }
        }

        private List<Card> _cards;
        public List<Card> Cards
        {
            get { return _cards; }
        }

        private List<Card> orderedList;
        private Random generator;

        private void FillDeck()
        {
            // fill the base deck we have to work with
            orderedList.Clear();
            for (int suit = 1; suit < 5; suit++)
            {
                for (int rank = 1; rank < 14; rank++)
                {
                    orderedList.Add(new Card(rank, suit));
                }
            }
        }

        /// <summary>
        /// Create a new deck with an internal RNG
        /// </summary>
        public Deck() : this(new Random((int)DateTime.Now.Ticks))
        {
        }

        /// <summary>
        /// Create a new deck (fixed at 52 for now)
        /// </summary>
        /// <param name="gen">An existing random number generator to use</param>
        public Deck(Random gen)
        {
            generator = gen;

            _cards = new List<Card>();
            orderedList = new List<Card>();
            FillDeck();
        }

        /// <summary>
        /// Shuffles the deck
        /// </summary>
        public void Shuffle()
        {
            _cards.Clear();
            FillDeck();

            for (int i = 0; i < 52; i++)
            {
                int x = generator.Next(orderedList.Count);
                _cards.Add(orderedList[x]);
                orderedList.RemoveAt(x);
            }
        }

        /// <summary>
        /// Deals the top card.
        /// </summary>
        /// <returns>The top card in the pile</returns>
        public Card Deal()
        {
            Card card = _cards.First();
            _cards.Remove(_cards.First());
            return card;
        }

        /// <summary>
        /// Deals a number of cards from the top of the deck
        /// </summary>
        /// <param name="count">The number of cards to deal</param>
        /// <returns>A list of the cards that were dealt</returns>
        public List<Card> Deal(int count)
        {
            List<Card> pile = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                pile.Add(_cards.First());
                _cards.Remove(_cards.First());
            }
            return pile;
        }
    }
}

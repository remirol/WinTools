﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Diagnostics;

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

        #region properties

        private List<Card> _cards;
        /// <summary>
        /// All cards currently in the working deck
        /// </summary>
        public List<Card> Cards
        {
            get { return _cards; }
        }

        /// <summary>
        /// A stock 52-card deck that never changes; used for performance reasons
        /// to avoid constantly redoing the card picture transforms
        /// </summary>
        private List<Card> stockDeck;

        /// <summary>
        /// Temporary list of cards for shuffling
        /// </summary>
        private List<Card> orderedList;

        /// <summary>
        /// Deck-private RNG, so that even if someone doesn't pass us one in, we
        /// won't constantly reseed ourselves as long as we're using the same deck
        /// </summary>
        private Random generator;

        /// <summary>
        /// Bitmap with all cards on it; expected to be four rows of Ace through King in CDHS order
        /// and then a fifth with two jokers and the card back
        /// </summary>
        private BitmapSource allCards;

        #endregion

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
            stockDeck = new List<Card>();
            allCards = new BitmapImage(new Uri(@"pack://application:,,,/Images/cards.png"));
            BuildDeck(true);
        }

        private void BuildDeck(bool useImages)
        {
            // fill the base deck we have to work with
            stockDeck.Clear();
            for (int suit = 1; suit < 5; suit++)
            {
                for (int rank = 1; rank < 14; rank++)
                {
                    if (useImages)
                    {
                        int cardWidth = allCards.PixelWidth / 13;
                        int cardHeight = allCards.PixelHeight / 5;
                        int cardX = cardWidth * (rank - 1);
                        int cardY = cardHeight * (suit - 1);
                        System.Windows.Int32Rect rect = new System.Windows.Int32Rect(cardX, cardY, cardWidth, cardHeight);
                        CroppedBitmap subCard = new CroppedBitmap(allCards, rect);

                        // we know where our 'back' card is
                        rect.X = cardWidth * 2;
                        rect.Y = cardHeight * 4;
                        CroppedBitmap cardBack = new CroppedBitmap(allCards, rect);

                        stockDeck.Add(new Card(rank, suit, subCard, cardBack));
                    }
                    else
                    {
                        stockDeck.Add(new Card(rank, suit));
                    }

                }
            }
        }

        /// <summary>
        /// Shuffles the deck
        /// </summary>
        public void Shuffle()
        {
            _cards.Clear();
            orderedList = new List<Card>(stockDeck);

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
            pile.Sort();
            return pile;
        }
    }
}

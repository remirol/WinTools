using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace CribbageAI
{
    public class Card : INotifyPropertyChanged, IComparable, IEquatable<Card>
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

        #region IComparable Members

        public int CompareTo(object obj)
        {
            Card target = obj as Card;
            if (this._rank != target.Rank)
            {
                // if the ranks are different it's easy enough
                return this._rank.CompareTo(target.Rank);
            }

            // we need to make sure that the 4H and 4D are not considered the same card though
            return this._suit.CompareTo(target.Suit);
        }

        #endregion

        #region IEquatable<Card> Members

        public bool Equals(Card other)
        {
            return (this.Rank == other.Rank && this.Suit == other.Suit);
        }

        #endregion

        #region Properties

        public String[] CardNames = { "ERROR", "Ace", "Two", "Three", "Four", 
                                        "Five", "Six", "Seven", "Eight", "Nine", 
                                        "Ten", "Jack", "Queen", "King" };
        public String[] CardSuits = { "ERROR", "Clubs", "Diamonds", "Hearts", "Spades" };

        private int _rank;
        public int Rank
        {
            get { return _rank; }
        }

        private int _suit;
        public int Suit
        {
            get { return _suit; }
        }

        public int Value
        {
            get
            {
                if (_rank < 11)
                {
                    return _rank;
                }
                return 10;  // TJQK 
            }
        }

        private Image _frontPicture;
        public Image FrontPicture
        {
            get { return _frontPicture; }
            set { _frontPicture = value; Notify("FrontPicture"); }
        }

        #endregion

        public Card(int rank, int suit, Image image) : this(rank, suit)
        {
            _frontPicture = image;
        }

        public Card(int rank, int suit)
        {
            _rank = rank;
            _suit = suit;
        }

        /// <summary>
        /// Returns the full rank and suit of the card
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} of {1}", CardNames[_rank], CardSuits[_suit]);
        }

        /// <summary>
        /// Returns the two-character rank and suit
        /// </summary>
        public string ToShortString()
        {
            return String.Format("{0}{1}", "0A23456789TJQK".Substring(_rank, 1), "0CDHS".Substring(_suit, 1));
        }

    }
}

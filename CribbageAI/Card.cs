using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

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
        public String[] CardSuits = { "ERROR", "Clubs", "Diamonds", "Hearts", "Spades", "Any" };

        private int _rank;
        /// <summary>
        /// Numeric rank of this card
        /// </summary>
        public int Rank
        {
            get { return _rank; }
        }

        private int _suit;
        /// <summary>
        /// Numeric suit of this card
        /// </summary>
        public int Suit
        {
            get { return _suit; }
        }

        /// <summary>
        /// Numeric value of this card; aces are worth 1 and face cards are worth 10
        /// </summary>
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

        /// <summary>
        /// The image for the front of this card, with rank and suit
        /// </summary>
        private BitmapSource _frontPicture;
        public BitmapSource FrontPicture
        {
            get { return _frontPicture; }
            set { _frontPicture = value; Notify("FrontPicture"); }
        }

        /// <summary>
        /// A smaller image suitable for thumbnails
        /// </summary>
        public BitmapSource SmallPicture
        {
            get
            {
                return new TransformedBitmap(FrontPicture.Clone(), new ScaleTransform(.5, .5));
            }
        }

        /// <summary>
        /// The image for the back of this card
        /// </summary>
        private BitmapSource _backPicture;
        public BitmapSource BackPicture
        {
            get { return _backPicture; }
            set { _backPicture = value; Notify("BackPicture"); }
        }

        #endregion

        /// <summary>
        /// Creates a new card object
        /// </summary>
        /// <param name="rank">Card rank (1-13 corresponding to Ace through King)</param>
        /// <param name="suit">Card suit (1-4 corresponding to Clubs/Diamonds/Hearts/Spades)</param>
        /// <param name="frontImage">Image to use for the front of this card</param>
        /// <param name="backImage">Image to use for the back of this card</param>
        public Card(int rank, int suit, BitmapSource frontImage, BitmapSource backImage)
            : this(rank, suit)
        {
            _frontPicture = frontImage;
            _backPicture = backImage;
        }

        /// <summary>
        /// Creates a new card object
        /// </summary>
        /// <param name="rank">Card rank (1-13 corresponding to Ace through King)</param>
        /// <param name="suit">Card suit (1-4 corresponding to Clubs/Diamonds/Hearts/Spades)</param>
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
            return String.Format("{0}{1}", "0A23456789TJQK".Substring(_rank, 1), "0CDHSx".Substring(_suit, 1));
        }

    }
}

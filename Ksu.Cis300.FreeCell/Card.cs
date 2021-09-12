/* Card.cs
 * Author: Rod Howell
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Ksu.Cis300.FreeCell
{
    /// <summary>
    /// An immutable representation of a single card.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// The image file name prefixes of each of the four suits.
        /// </summary>
        private string[] _filePrefixes = { "clubs_", "diamonds_", "hearts_", "spades_" };

        /// <summary>
        /// The name of the directory containing the images.
        /// </summary>
        private const string _directory = "../../images/";

        /// <summary>
        /// The image file name suffix.
        /// </summary>
        private const string _fileSuffix = ".png";

        /// <summary>
        /// Gets the rank of the card.
        /// </summary>
        public int Rank { get; }

        /// <summary>
        /// Gets the suit of the card.
        /// </summary>
        public Suits Suit { get; }

        /// <summary>
        /// Indicates whether this card is red.
        /// </summary>
        public bool IsRed { get; }

        /// <summary>
        /// The image of this card.
        /// </summary>
        public Image Picture { get; }

        /// <summary>
        /// Constructs a new card representing the given rank and suit.
        /// </summary>
        /// <param name="rank">The rank of the card.</param>
        /// <param name="suit">The suit of the card.</param>
        public Card(int rank, Suits suit)
        {
            if (rank < 1 || rank > 13 || suit < 0 || (int)suit >= 4)
            {
                throw new ArgumentException();
            }
            Rank = rank;
            Suit = suit;
            if (suit == Suits.Diamonds || suit == Suits.Hearts)
            {
                IsRed = true;
            }
            Picture = Image.FromFile(_directory + _filePrefixes[(int)suit] + rank + _fileSuffix);
        }
    }
}

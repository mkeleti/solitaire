/* Subproblem.cs
 * Author: Rod Howell
 * Modified By: Michael Keleti
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ksu.Cis300.FreeCell
{
    /// <summary>
    /// Instances represent subproblems of a complex move of a stack.
    /// </summary>
    public class Subproblem
    {
        /// <summary>
        /// Gets the source stack.
        /// </summary>
        public Stack<Card> Source { get; }

        /// <summary>
        /// Gets the destination stack.
        /// </summary>
        public Stack<Card> Destination { get; }

        /// <summary>
        /// Gets the number of cards to be moved.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the number of available empty tableau cells.
        /// </summary>
        public int AvailableTableauCells { get; }

        /// <summary>
        /// Constructs a new subproblem with the given information.
        /// </summary>
        /// <param name="source">The source stack.</param>
        /// <param name="dest">The destination stack.</param>
        /// <param name="len">The number of cards to be moved.</param>
        /// <param name="avail">The number of available tableau cells.</param>
        public Subproblem(Stack<Card> source, Stack<Card> dest, int len, int avail)
        {
            Source = source;
            Destination = dest;
            Length = len;
            AvailableTableauCells = avail;
        }
    }
}

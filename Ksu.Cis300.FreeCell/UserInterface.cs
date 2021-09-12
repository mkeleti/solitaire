/* UserInterface.cs
 * Author: Rod Howell
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Ksu.Cis300.FreeCell
{
    /// <summary>
    /// A GUI for a FreeCell Solitaire game.
    /// </summary>
    public partial class UserInterface : Form
    {
        #region Constants and fields used for graphics

        /// <summary>
        /// The padding between free cells and between home cells.
        /// </summary>
        private const int _upperPadding = 5;

        /// <summary>
        /// The padding between columns in the tableau.
        /// </summary>
        private const int _lowerPadding = 8;

        /// <summary>
        /// The padding between the free cells and the home cells.
        /// </summary>
        private const int _innerPadding = 7 * _lowerPadding - 6 * _upperPadding;

        /// <summary>
        /// The padding between the upper and lower portions of the board.
        /// </summary>
        private const int _verticalPadding = 20;

        /// <summary>
        /// The top, left, and right margins on the board.
        /// </summary>
        private const int _margins = 5;

        /// <summary>
        /// The total amount of horizontal spacing needed.
        /// </summary>
        private const int _totalHorizontalSpacing = 2 * _margins + 7 * _lowerPadding;

        /// <summary>
        /// The total amount of vertical spacing needed.
        /// </summary>
        private const int _totalVerticalSpacing = _margins + _verticalPadding;

        /// <summary>
        /// The height of a card image.
        /// </summary>
        private const int _cardImageHeight = 333;

        /// <summary>
        /// The width of a card image.
        /// </summary>
        private const int _cardImageWidth = 234;

        /// <summary>
        /// The portion of a card that must be visible in the tableau.
        /// </summary>
        private const float _minimumVisibleCardPortion = 0.2f;

        /// <summary>
        /// The number of milliseconds to pause each time the board is redrawn.
        /// </summary>
        private const int _delay = 35;

        /// <summary>
        /// The pen with which to draw cells.
        /// </summary>
        private Pen _cellPen = new Pen(Color.White, 1);

        /// <summary>
        /// The pen with which to highlight cells or columns.
        /// </summary>
        private Pen _highlightPen = new Pen(Color.Magenta, 2);

        /// <summary>
        /// The height of a card on the board.
        /// </summary>
        private int _cardHeight;

        /// <summary>
        /// The width of a card on the board.
        /// </summary>
        private int _cardWidth;

        /// <summary>
        /// The offset needed to center horizontally.
        /// </summary>
        private int _horizontalOffset;

        /// <summary>
        /// The clickable regions on the board. The first index gives the cell type. The second
        /// gives the cell within that type. The third is always 0 for FreeCell and HomeCell types,
        /// but for the TableauCell type, gives the number of cards outside the selection.
        /// </summary>
        private Rectangle[][][] _regions = new Rectangle[3][][];

        #endregion

        /// <summary>
        /// The stacks of cards on the board. Element 0 is the array of free cells,
        /// element 1 is the array of home cells, and element 2 is the array of tableau columns.
        /// </summary>
        private Stack<Card>[][] _board = new Stack<Card>[3][];

        /// <summary>
        /// Gives the number of cards currently selected.
        /// </summary>
        private int _selectionCount = 0;

        /// <summary>
        /// The type of the currently selected cell, if there is one.
        /// </summary>
        private CellType _selectedCellType;

        /// <summary>
        /// The index of the selected cell, if there is one.
        /// </summary>
        private int _selectedCell;

        #region Methods for initialization and graphics

        /// <summary>
        /// Constructs the GUI.
        /// </summary>
        public UserInterface()
        {
            InitializeComponent();
            ComputeCardSize();
        }

        /// <summary>
        /// Computes the height and width of a card on the board.
        /// </summary>
        private void ComputeCardSize()
        {
            // First find the vertical space available.
            int cardHeightAvail = uxBoard.Height - _totalVerticalSpacing;

            // Find the maximum scale factor that keeps enough of the cards visible vertically.
            // A maximum of 19 cards can be on a tableau cell, and the top portion of each needs to
            // be visible. We also need enough vertical space for an entire card in the upper part
            // of the board.
            float maxVScale = (cardHeightAvail / (1 + _minimumVisibleCardPortion * 19)) / _cardImageHeight;

            // Now find the horizontal space available.
            int cardWidthAvail = uxBoard.Width - _totalHorizontalSpacing;

            // Now find the maximum scale factor that will allow 8 cards to be shown horizontally.
            float maxHScale = (cardWidthAvail / 8.0f) / _cardImageWidth;

            // Compute the height and width of a card using the minimum of the above scale factors.
            float scale = Math.Min(maxVScale, maxHScale); // The scale factor to use.
            _cardHeight = (int)(scale * _cardImageHeight);
            _cardWidth = (int)(scale * _cardImageWidth);

            // Center the board horizontally. It will be fixed at the top if there is more vertical space.
            _horizontalOffset = (uxBoard.Width - 8 * _cardWidth - 7 * _lowerPadding) / 2;
        }

        /// <summary>
        /// Handles a Resize event on the board.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxBoard_Resize(object sender, EventArgs e)
        {
            ComputeCardSize(); // Adjust the scaling to the new size.
            RedrawBoard();
        }

        /// <summary>
        /// Draws one of the three sets of empty cells.
        /// </summary>
        /// <param name="g">The graphics context on which to draw.</param>
        /// <param name="hStart">The horizontal compoent of the starting point.</param>
        /// <param name="vStart">The vertical component of the starting point.</param>
        /// <param name="padding">The amount of padding to use between cells.</param>
        /// <param name="n">The number of cells to draw.</param>
        /// <returns>The rectangles drawn.</returns>
        private Rectangle[][] DrawCells(Graphics g, int hStart, int vStart, int padding, int n)
        {
            Rectangle[][] regions = new Rectangle[n][];
            for (int i = 0; i < n; i++)
            {
                Rectangle r = new Rectangle(hStart, vStart, _cardWidth, _cardHeight);
                g.DrawRectangle(_cellPen, r);
                regions[i] = new Rectangle[1];
                regions[i][0] = r;
                hStart += _cardWidth + padding;
            }
            return regions;
        }

        /// <summary>
        /// Draws the cards for one of the three areas of the board.
        /// </summary>
        /// <param name="g">The graphics context on which to draw.</param>
        /// <param name="stacks">The stacks of cards to draw.</param>
        /// <param name="fullStack">Whether to draw all the cards in each stack.</param>
        /// <param name="hStart">The horizontal component of the starting point.</param>
        /// <param name="vStart">The vertical component of the starting point.</param>
        /// <param name="padding">The padding between cells.</param>
        private void DrawCards(Graphics g, Stack<Card>[] stacks, bool fullStack, int hStart, int vStart, int padding)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                float nextVertical = vStart;
                if (fullStack && stacks[i].Count > 0)
                {
                    _regions[(int)CellType.TableauCell][i] = new Rectangle[stacks[i].Count];
                    DrawCardStack(g, stacks[i], hStart, vStart, i);
                }
                else if (stacks[i].Count > 0)
                {
                    g.DrawImage(stacks[i].Peek().Picture, hStart, nextVertical, _cardWidth, _cardHeight);
                }
                hStart += _cardWidth + padding;
            }
        }

        /// <summary>
        /// Draws a stack of cards, showing a portion of each card.
        /// </summary>
        /// <param name="g">The graphics context on which to draw.</param>
        /// <param name="s">The stack to draw.</param>
        /// <param name="x">The horizontal component of the upper-left corner of the drawing.</param>
        /// <param name="y">The vertical component of the upper-left corner of the drawing.</param>
        /// <param name="i">Which tableau cell is being drawn.</param>
        private void DrawCardStack(Graphics g, Stack<Card> s, int x, float y, int i)
        {

            // We need to start drawing from the bottom of the stack, so we use a second stack to reverse the order
            Stack<Card> temp = new Stack<Card>();
            foreach (Card c in s)
            {
                temp.Push(c);
            }
            while (temp.Count > 0)
            {
                _regions[(int)CellType.TableauCell][i][s.Count - temp.Count] = 
                    new Rectangle(x, (int)y, _cardWidth,
                    (int)((1 + (temp.Count - 1) * _minimumVisibleCardPortion) * _cardHeight));
                g.DrawImage(temp.Pop().Picture, x, y, _cardWidth, _cardHeight);
                y += _minimumVisibleCardPortion * _cardHeight;
            }
        }

        /// <summary>
        /// Handles a Paint event on the board.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; // The graphics context on which to draw.
            _regions[(int)CellType.FreeCell] = DrawCells(g, _horizontalOffset, _margins, _upperPadding, 4);
            _regions[(int)CellType.HomeCell] = DrawCells(g, _horizontalOffset + 4 * _cardWidth + 3 * _upperPadding + _innerPadding, 
                _margins, _upperPadding, 4);
            _regions[(int)CellType.TableauCell] = DrawCells(g, _horizontalOffset, _margins + _cardHeight + _verticalPadding, 
                _lowerPadding, 8);
            if (uxBoard.Enabled)
            {
                DrawCards(g, _board[(int)CellType.FreeCell], false, _horizontalOffset, _margins, _upperPadding);
                DrawCards(g, _board[(int)CellType.HomeCell], false, _horizontalOffset + 4 * _cardWidth + 3 * _upperPadding + _innerPadding,
                    _margins, _upperPadding);
                DrawCards(g, _board[(int)CellType.TableauCell], true, _horizontalOffset, _margins + _cardHeight + _verticalPadding, _lowerPadding);
                DrawSelection(g);
            }
        }

        /// <summary>
        /// Draws the selection rectangle if one is needed.
        /// <param name="g">The graphics context on which to draw.</param>
        /// </summary>
        private void DrawSelection(Graphics g)
        {
            if (_selectionCount > 0) // If there is a selection
            {
                if (_selectedCellType == CellType.TableauCell)
                {
                    g.DrawRectangle(_highlightPen,
                        _regions[(int)CellType.TableauCell][_selectedCell][_board[(int)CellType.TableauCell][_selectedCell].Count - _selectionCount]);
                }
                else
                {
                    g.DrawRectangle(_highlightPen, _regions[(int)_selectedCellType][_selectedCell][0]);
                }
            }
        }

        /// <summary>
        /// Initializes the board stacks.
        /// </summary>
        private void InitializeStacks()
        {
            for (int i = 0; i < _board.Length; i++)
            {
                if (i == (int)CellType.TableauCell)
                {
                    _board[i] = new Stack<Card>[8];
                }
                else
                {
                    _board[i] = new Stack<Card>[4];
                }
                for (int j = 0; j < _board[i].Length; j++)
                {
                    _board[i][j] = new Stack<Card>();
                }
            }
        }

        /// <summary>
        /// Gets a new unshuffled deck of cards.
        /// </summary>
        /// <returns>The deck of cards.</returns>
        private Card[] GetDeck()
        {
            Card[] deck = new Card[52];
            int i = 0;
            for (Suits suit = 0; (int)suit < 4; suit++)
            {
                for (int rank = 1; rank <= 13; rank++)
                {
                    deck[i] = new Card(rank, suit);
                    i++;
                }
            }
            return deck;
        }

        /// <summary>
        /// Shuffles the given deck of cards.
        /// </summary>
        /// <param name="deck">The deck to shuffle.</param>
        private void Shuffle(Card[] deck)
        {
            // Using the game number as the seed for a random number generator guarantees that the same
            // sequence of random numbers will be generated each time this game number is used. This
            // guarantees that the deck is shuffled the same way each time the same game number is used.
            Random r = new Random((int)uxGameNumber.Value);
            for (int i = deck.Length - 1; i >= 0; i--)
            {
                int j = r.Next(i + 1); // A random value from 0 to i

                // Swap the cards at locations i and j
                Card temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        /// <summary>
        /// Deals the given deck to the tableau.
        /// </summary>
        /// <param name="deck">The deck of cards.</param>
        private void Deal(Card[] deck)
        {
            int stack = 0;
            for (int i = 0; i < deck.Length; i++)
            {
                _board[(int)CellType.TableauCell][stack].Push(deck[i]);
                RedrawBoard(); // Redraw after each card is dealt.
                stack++;
                if (stack == 8)
                {
                    stack = 0; // Return to the beginning after we've dealt a card to each tableau cell.
                }
            }
        }

        /// <summary>
        /// Handles a Click event on the "New Game" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxNewGame_Click(object sender, EventArgs e)
        {
            InitializeStacks();
            uxBoard.Enabled = true;
            uxMoveHome.Enabled = true;
            Card[] deck = GetDeck();
            Shuffle(deck);
            Deal(deck);
        }

        /// <summary>
        /// Handles a MouseClick event on the board.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxBoard_MouseClick(object sender, MouseEventArgs e)
        {
            Point loc = e.Location;
            for (CellType i = 0; (int)i < _regions.Length; i++)
            {
                for (int j = 0; j < _regions[(int)i].Length; j++)
                {
                    // Because the regions in a tableau cell overlap, we need to start with the
                    // smallest.
                    for (int k = 1; k <= _regions[(int)i][j].Length; k++)
                    {
                        if (_regions[(int)i][j][_regions[(int)i][j].Length - k].Contains(loc))
                        {
                            SelectCell(i, j, k);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Redraws the board.
        /// </summary>
        private void RedrawBoard()
        {
            // A Control's Refresh method causes the Control to be redrawn immediately by signalling a
            // Paint event. The uxBoard_Paint method above handles this event for the DrawingBoard.
            uxBoard.Refresh();

            // Wait long enough for the user to get a glimpse of the changes before it is redrawn again.
            Thread.Sleep(_delay);
        }

        /// <summary>
        /// Selects, deselects, or attempts to make a move to the given cell, depending on the
        /// current selection status.
        /// </summary>
        /// <param name="type">The type of cell selected.</param>
        /// <param name="cell">The index of the selected cell.</param>
        /// <param name="count">The number of cards to select.</param>
        private void SelectCell(CellType type, int cell, int count)
        {
            // Insert code here
        }
    }
}

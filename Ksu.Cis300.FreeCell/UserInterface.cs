/* UserInterface.cs
 * Author: Rod Howell
 * Modified: Michael Keleti
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
            if (_selectionCount > 0)
            {
                Console.WriteLine("Destination Selected");
                SelectDestination(type, cell, count);
            }
            else if (type != CellType.HomeCell && _board[Convert.ToInt32(type)][cell].Count > 0)
            {
                Console.WriteLine("Cell Selected");
                _selectionCount = count;
                _selectedCell = cell;
                _selectedCellType = type;
            }
            RedrawBoard();

        }
        /// <summary>
        /// Moves a single card.
        /// </summary>
        /// <param name="Source">Stack of Cards to Move</param>
        /// <param name="Destination">Stack of Cards to Move to</param>
        private void MoveOneCard(Stack<Card> Source, Stack<Card> Destination)
        {
            Console.WriteLine("Card Moved");
            Destination.Push(Source.Pop());
            RedrawBoard();
        }
        /// <summary>
        /// Moves a card to a free cell
        /// </summary>
        /// <param name="Source">Stack of Cards to Move</param>
        /// <param name="Destination">Stack of Cards to Move to</param>
        /// <param name="CardsToMove">Number of cards to move</param>
        /// <returns>Wether or not it is a legal move</returns>
        private bool MoveToFreeCell(Stack<Card> Source, Stack<Card> Destination, int CardsToMove)
        {
            if (Destination.Count == 0 && CardsToMove == 1)
            {
                MoveOneCard(Source, Destination);
                return true;
            }
            else
            {
                Console.WriteLine("Move to Free Cell False");
                return false;
            }
        }
        /// <summary>
        /// Moves a single card to a home cell.
        /// </summary>
        /// <param name="Source">Stack of Cards to Move</param>
        /// <param name="Destination">Stack of Cards to Move to</param>
        /// <param name="CardsToMove">Number of cards to move</param>
        /// <returns>Wether or not it is a legal move</returns>
        private bool MoveToHomeCell(Stack<Card> Source, Stack<Card> Destination, int CardsToMove)
        {
            Card SourceCard = Source.Peek();
            if (CardsToMove == 1)
            {
                if (Destination.Count != 0)
                {
                    Console.WriteLine("Move to Home Cell Condition 1.1 met");
                    bool SameSuit = (Source.Peek().Suit == Destination.Peek().Suit);
                    bool CorrectRank = (Source.Peek().Rank == Destination.Peek().Rank + 1);

                    if (CorrectRank && SameSuit)
                    {
                        MoveOneCard(Source, Destination);
                        Console.WriteLine("Move to Home Cell Condition 1.2 met");
                        return true;
                    }
                }
                else if (SourceCard.Rank == 1)
                {
                    MoveOneCard(Source, Destination);
                    Console.WriteLine("Move to Home Cell Condition 2 met");
                    return true;
                }
            }
            Console.WriteLine("Move to Home Cell False");
            return false;
        }
        /// <summary>
        /// Determines if you can move a single card to a Tableua Cell
        /// </summary>
        /// <param name="SourceCard">Card to Move</param>
        /// <param name="Target">Card to Move to</param>
        /// <returns></returns>
        private bool CanMoveToTableua(Card SourceCard, Card Target)
        {

            bool OppositeColor = (Target.IsRed ^ SourceCard.IsRed);
            bool CorrectRank = (SourceCard.Rank == Target.Rank - 1);

            if (OppositeColor && CorrectRank)
            {
                Console.WriteLine("Can move Tableau True");
                return true;
            }
            else
            {
                Console.WriteLine("Can move Tableau False");
                Console.WriteLine(OppositeColor);
                Console.WriteLine(SourceCard.Rank);
                Console.WriteLine(Target.Rank);
                Console.WriteLine(CorrectRank);
                return false;
            }
        }
        /// <summary>
        /// Determines if you can move a Stack of Cards to a destination cell in a certain ammount of moves
        /// </summary>
        /// <param name="Source">Stack of Cards to Move</param>
        /// <param name="Destination">Stack of Cards to Move to</param>
        /// <param name="CardsToMove">Number of cards to move</param>
        /// <returns>A bool determining if it is a legal move or not</returns>
        private bool CanAddTableua(Stack<Card> Source, Stack<Card> Destination, int CardsToMove)
        {
            int Count = 0;
            Card PreviousCard = null;
            bool CanMove;
            Console.WriteLine(CardsToMove);
            foreach (Card SourceCard in Source)
            {
                Count++;
                if (Count < CardsToMove && PreviousCard != null)
                {
                    Console.WriteLine("Can add Tableau Condition 1.1 met");
                    CanMove = CanMoveToTableua(PreviousCard, SourceCard);
                    if (!CanMove)
                    {
                        Console.WriteLine("Can add Tableau Condition 1.2 met");
                        return false;
                    }
                }
                else if (CardsToMove == Count && (Destination.Count == 0 || CanMoveToTableua(SourceCard, Destination.Peek())))
                {
                    Console.WriteLine("Can add Tableau Condition 2 met");
                    return true;
                }
                PreviousCard = SourceCard;
            }
            Console.WriteLine("Can Add Tableua False");
            Console.WriteLine("Cards to move:" + CardsToMove + " Count: " + Count);
            
            return false;
            
        }
        /// <summary>
        /// Counts the amount of empty cells of a given type
        /// </summary>
        /// <param name="Type">The type of the selected cell</param>
        /// <returns>return the counted Empty Cells</returns>
        private int CountEmptyCells(CellType Type)
        {
            int Count = 0;
            for (int i = 0; i < _board[Convert.ToInt32(Type)].Length; i++)
            {
               if (_board[Convert.ToInt32(Type)][i].Count == 0)
                {
                    Count++;
                }
            }
            Console.WriteLine("Empty Cells: " + Count);
            return Count;
        }
        /// <summary>
        /// Gets empty cells of a given type returns them.
        /// </summary>
        /// <param name="Type">The selected cell type</param>
        /// <param name="Exclude">What cells to exclude</param>
        /// <returns>Returns the empty cell</returns>
        private Stack<Card> GetEmptyCell(CellType Type, Stack<Card> Exclude)
        {
            bool NotExclude;
            bool IsEmpty;

            if (CountEmptyCells(Type) != 0)
            {
                for (int i = 0; i < _board[Convert.ToInt32(Type)].Length; i++)
                {
                    NotExclude = (_board[Convert.ToInt32(Type)][i] != Exclude);
                    IsEmpty = (_board[Convert.ToInt32(Type)][i].Count == 0);
                    if (NotExclude && IsEmpty)
                    {

                        return _board[Convert.ToInt32(Type)][i];
                    }
                }
            }
            Console.WriteLine("Get Empty Cells returned null");
            return null;
        }
        /// <summary>
        /// Moves a sequence of free cells
        /// </summary>
        /// <param name="subproblem">Selected subproblem to use for sequence</param>
        private void MoveSequenceFreeCells(Subproblem subproblem)
        {

            
            Stack<Card> Source = subproblem.Source;
            Stack<Card> Destination = subproblem.Destination;
            int Length = subproblem.Length;
            int AvailableTableauCells = subproblem.AvailableTableauCells;
            Stack<Stack<Card>> MovedToFree = new Stack<Stack<Card>>(Length);

            for (int i = 2; i <= Length; i++)
            {
                MovedToFree.Push(GetEmptyCell(0, null));
                MoveToFreeCell(Source, GetEmptyCell(0, null), 1);
            }
            MoveOneCard(Source, Destination);
            
            foreach(Stack<Card> stack in MovedToFree)
            {
                MoveOneCard(stack, Destination);
            }

        }
        /// <summary>
        /// Move a Sequence to Tableau Cells
        /// </summary>
        /// <param name="Source">The Source cell to transfer from</param>
        /// <param name="Destination">Cell to transfer to</param>
        /// <param name="EmptyTableau">Number of empty tableau's</param>
        /// <param name="EmptyFree">How many empty free cells</param>
        /// <param name="CardsToMove">How many cards to move</param>
        private void MoveSequenceTableauCell(Stack<Card> Source, Stack<Card> Destination, int EmptyTableau, int EmptyFree, int CardsToMove)
        {
            Stack<Subproblem> SubproblemTracker = new Stack<Subproblem>(CardsToMove);
            Subproblem FirstProblem = new Subproblem(Source, Destination, CardsToMove, EmptyTableau);
            SubproblemTracker.Push(FirstProblem);
     
            while (SubproblemTracker.Count() > 0)
            {
                
                FirstProblem = SubproblemTracker.Pop();
                if (FirstProblem.Length <= EmptyFree + 1)
                {
                    Console.WriteLine("Tableau Cell Sequence condition 1");
                    MoveSequenceFreeCells(FirstProblem);
                }
                else
                {
                    Console.WriteLine("Tableau Cell Sequence condition 2");
                    Stack<Card> TempStorage = GetEmptyCell(CellType.TableauCell, FirstProblem.Destination);
                    Subproblem HalfProblem = new Subproblem(Source, TempStorage, FirstProblem.Length / 2, EmptyTableau);
                    Subproblem RestProblem = new Subproblem(Source, FirstProblem.Destination, FirstProblem.Length / 2, EmptyTableau);
                    Subproblem EmptyTemp = new Subproblem(TempStorage, FirstProblem.Destination, FirstProblem.Length /2, EmptyTableau);
                    SubproblemTracker.Push(EmptyTemp);
                    SubproblemTracker.Push(RestProblem);
                    SubproblemTracker.Push(HalfProblem);
                   
                }
            }
        }
        /// <summary>
        /// Moves a sequence to the Tableau while checking that the move is legal.
        /// </summary>
        /// <param name="Source">Source cell to transfer from</param>
        /// <param name="Destination">Destination cell to transfer to</param>
        /// <param name="CardsToMove">How many cards to move</param>
        /// <returns></returns>
        private bool MoveToTableau(Stack<Card> Source, Stack<Card> Destination, int CardsToMove)
        {
            int EmptyFreeCells = CountEmptyCells(CellType.FreeCell);
            int EmptyTableauCells = CountEmptyCells(CellType.TableauCell);

            if (Destination.Count() == 0)
            {
                EmptyTableauCells -= 1;
            }
            int value = (1 << EmptyTableauCells) * (EmptyFreeCells + 1);
            if (CardsToMove > value)
            {
                Console.Write("Move To Tableau Calculation Exception");
                return false;
            }
            else if (!CanAddTableua(Source, Destination, CardsToMove))
            {
                Console.Write("Move To Tableau Can Add Tableau Exception");
                return false;
            }
            else
            {
                MoveSequenceTableauCell(Source, Destination, EmptyTableauCells, EmptyFreeCells, CardsToMove);
                return true;
            }
        }
        /// <summary>
        /// Makes a play to move selected cards to their destination
        /// </summary>
        /// <param name="Source">Source of cards</param>
        /// <param name="Destination">Destination for cards</param>
        /// <param name="TargetType">Type of cell for Destination</param>
        /// <param name="CardsToMove">How many Cards to move</param>
        /// <returns></returns>
        private bool MakePlay(Stack<Card> Source, Stack<Card> Destination, CellType TargetType, int CardsToMove)
        {
            if (TargetType == CellType.FreeCell)
            {
                return MoveToFreeCell(Source, Destination, CardsToMove);
            }
            else if (TargetType == CellType.HomeCell)
            {
                return MoveToHomeCell(Source, Destination, CardsToMove);
            }
            else if (TargetType == CellType.TableauCell)
            {
                return MoveToTableau(Source, Destination, CardsToMove);
            }
            else
            {
                Console.WriteLine("No Type");
                return false;
            }
        }
        /// <summary>
        /// Checks to see if the player has won the game
        /// </summary>
        private void CheckWin()
        {
            foreach (Stack<Card> card in _board[Convert.ToInt32(CellType.HomeCell)])
            {
                if (card.Count != 13)
                {
                    return;
                }
            }
            uxMoveHome.Enabled = false;
            MessageBox.Show("You win!");
        }
        /// <summary>
        /// Selects a destination cell and moves the selected cards to that cell
        /// </summary>
        /// <param name="Type">Type of cell being moved to</param>
        /// <param name="CellIndex">Index of destination cell</param>
        /// <param name="CardsSelected">How many cards are selected</param>
        private void SelectDestination(CellType Type, int CellIndex, int CardsSelected)
        {
            bool TypeCheck = (Type == _selectedCellType);
            bool IndexCheck = (CellIndex == _selectedCell);
            bool CountCheck = (CardsSelected == _selectionCount);

            if (TypeCheck && IndexCheck && CountCheck)
            {
                _selectionCount = 0;
            }
            else
            {
                int LastCount = _selectionCount;
                _selectionCount = 0;
                bool Checksum = MakePlay(_board[Convert.ToInt32(_selectedCellType)][_selectedCell], _board[Convert.ToInt32(Type)][CellIndex], Type, LastCount);
                if (Checksum == true)
                {
                    CheckWin();
                }
                else if (Checksum == false) {
                    MessageBox.Show("Invalid play");
                }
            }
        }
        /// <summary>
        /// Checks to see if the source cell isnt empty and then moves the rest to the home cells
        /// </summary>
        /// <param name="Source">The source cells we are transfering from</param>
        /// <param name="Destination">The destination cells we are transfering to</param>
        /// <returns></returns>
        private bool MoveToHome(Stack<Card>[] Source, Stack<Card> Destination)
        {
            bool returnValue;
            foreach (Stack<Card> stacks in Source)
            {
                if (stacks.Count > 0)
                {
                    returnValue = MoveToHomeCell(stacks, Destination, 1);
                    if (returnValue)
                    {
                        return true;
                    }
                }
            }
            Console.WriteLine("Move to Home Exception");
            return false;
        }
        /// <summary>
        /// Triggers the event for the "Move all Home" button and moves any possible cardws that can go into home into home
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxMoveHome_Click(object sender, EventArgs e)
        {
            _selectionCount = 0;
            bool CardMove = true;
            while (CardMove)
            {
                CardMove = false;
                bool checksum;
                foreach (Stack<Card> homecell in _board[Convert.ToInt32(CellType.HomeCell)])
                {
                    checksum = MoveToHome(_board[Convert.ToInt32(CellType.FreeCell)], homecell);
                    if (checksum)
                    {
                        CardMove = true;
                    }
                    else
                    {
                        checksum = MoveToHome(_board[Convert.ToInt32(CellType.TableauCell)], homecell);
                        if (checksum)
                        {
                            CardMove = true;
                        }
                    }
                }
            }
        }
    }
    }

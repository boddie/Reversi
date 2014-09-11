using System;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

// Main minimax class used by the AI.
public class Minimax
{
    // The depth the minimax should go in the trees
    private int _difficulty;
    private const int SWITCH_TIME = 58;
    private int[] ordered;
    private int[] reverseOrdered;

    public Minimax(int difficulty)
    {
        _difficulty = difficulty;
        buildOrder();
    }

    private void buildOrder()
    {
        ordered = new int[] 
            { 0, 56, 7, 63, 16, 2, 5, 23, 
              40, 58, 47, 61, 18, 42, 21, 45, 
              24, 32, 3, 4, 59, 60, 31, 39, 
              26, 34, 19, 20, 29, 37, 43, 44, 
              25, 33, 30, 38, 51, 52, 11, 12, 
              10, 17, 41, 50, 13, 22, 46, 53, 
              1, 8, 6, 15, 55, 62, 48, 57, 
              9, 14, 49, 54 };
        reverseOrdered = new int[ordered.Length];
        for (int i = 0; i < ordered.Length; i++)
            reverseOrdered[i] = ordered.Length - 1 - i;
    }

    // The method called to get the move computed by the minimax algorithm
    public int GetMove(Dictionary<int, Piece> pieces, int turn, int moveCount)
    {
        // Copy the reference dictionary to a local changeable copy for calculations
        Dictionary<int, PieceState> board = new Dictionary<int, PieceState>();
        foreach(KeyValuePair<int, Piece> p in pieces)
        {
            board.Add(p.Key, p.Value.State);
        }
        return minimax(board, turn, _difficulty, 0, int.MaxValue, moveCount).Move;
    }

    // Used to store the score for a move
    private class ScoreMovePair
    {
        public int Move { get; set; }
        public int Score { get; set; }
        public ScoreMovePair(int move, int score)
        {
            Move = move;
            Score = score;
        }
    }

    // The minimax algorithm itself
    private ScoreMovePair minimax(Dictionary<int, PieceState> board, int turn, int maxDepth, int currDepth, int prune, int moveCount)
    {
        // Checks if either at end of game state or as far down in the tree we are suppose to be. If so uses the 
        // static evaluation method appropriate for diffulty settings
        if (isGameOver(board) || currDepth == maxDepth)
        {
            if (GameOptions.Instance.SMART)
            {
                if(currDepth % 2 == 0)
                {
                    if (moveCount >= SWITCH_TIME) // If at the end of game try to maximize the number of pieces instead of playing position
                        return new ScoreMovePair(-1, unweightedEvaluation(board, (turn)));
                    else
                        return new ScoreMovePair(-1, weightedEvaluation(board, (turn)));
                }
                else
                {
                    if (moveCount >= SWITCH_TIME) // If at the end of game try to maximize the number of pieces instead of playing position
                        return new ScoreMovePair(-1, unweightedEvaluation(board, (turn == 0) ? 1 : 0));
                    else
                        return new ScoreMovePair(-1, weightedEvaluation(board, (turn == 0) ? 1 : 0)); // uses positional strategy
                }
            }
            else
            {
                if(maxDepth % 2 == 0)
                    return new ScoreMovePair(-1, unweightedEvaluation(board, (turn)));
                else
                    return new ScoreMovePair(-1, unweightedEvaluation(board, (turn == 0) ? 1 : 0));
            }
        }
        
        ScoreMovePair best = new ScoreMovePair(-1, 0); // Initialize best move to none
        if (turn == 0) // Turn is AI's
            best.Score = int.MinValue;
        else // Turn is player's
            best.Score = int.MaxValue;
        for (int i = 0; i < 60; i++) // Iterate through board positions
        {
            int id = (turn == 0) ? ordered[i] : reverseOrdered[i];
            // If move not available skip
            if (board[id] != PieceState.NONE || !isViable(board, id, turn))
                continue;

            // Create copy of board for new board
            Dictionary<int, PieceState> newBoard = new Dictionary<int, PieceState>();
            foreach (KeyValuePair<int, PieceState> b in board)
                newBoard.Add(b.Key, b.Value);
            // Attempt a move and evaluate it
            if (MakeMove(newBoard, id, turn, false) == 0)
                continue;
            newBoard[id] = (turn == 0) ? PieceState.BLACK : PieceState.WHITE;
            ScoreMovePair current = minimax(newBoard, (turn == 0) ? 1 : 0, maxDepth, currDepth + 1, best.Score, moveCount + 1);
            if (turn == 0) // Maximize
            {
                if (current.Score > prune)
                {
                    return current;
                }
                if (current.Score > best.Score)
                {
                    best.Score = current.Score;
                    best.Move = id;
                }
            }
            else // Minimize
            {
                if (current.Score < prune)
                {
                    return current;
                }
                if (current.Score < best.Score)
                {
                    best.Score = current.Score;
                    best.Move = id;
                }
            }
        }
        return best;
    }

    private bool isGameOver(Dictionary<int, PieceState> board)
    {
        for (int i = 0; i < 64; i++)
        {
            if(isViable(board, i, 0))
            {
                Dictionary<int, PieceState> testBoard = new Dictionary<int, PieceState>();
                foreach (KeyValuePair<int, PieceState> b in board)
                    testBoard.Add(b.Key, b.Value);
                if (MakeMove(testBoard, i, 0, true) != 0)
                    return false;
            }

            if (isViable(board, i, 1))
            {
                Dictionary<int, PieceState> testBoard = new Dictionary<int, PieceState>();
                foreach (KeyValuePair<int, PieceState> b in board)
                    testBoard.Add(b.Key, b.Value);
                if (MakeMove(testBoard, i, 1, true) != 0)
                    return false;
            }
        }
        return true;
    }

    // This is a dumb evaluation method that simply weights every spot on the board the same
    private int unweightedEvaluation(Dictionary<int, PieceState> board, int turn)
    {
        int score = 0;
        for (int i = 0; i < 64; i++)
        {
            if (board[i] == PieceState.WHITE)
                score += (turn == 0) ? -1 : 1;
            else if (board[i] == PieceState.BLACK)
                score += (turn == 0) ? 1 : -1;
        }
        return score;
    }

    // This evaluation method is based off of the one used by Microsoft's Reversi game.
    // I changed it so that after a corner has been reached it measures everything in that quadrant of the
    // board as a positive value since at that point they are all beneificial.
    private int weightedEvaluation(Dictionary<int, PieceState> board, int turn)
    {
        int score = 0;
        for (int i = 0; i < 64; i++)
        {
            if (board[i] == PieceState.NONE)
                continue;

            if (i == 0 || i == 7 || i == 56 || i == 63)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -99 : 99;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 99 : -99;
            }
            else if (i == 1 || i == 8)
            {
                if (board[0] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 8 : -8;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -8 : 8;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 6 || i == 15)
            {
                if (board[7] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 8 : -8;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -8 : 8;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 48 || i == 57)
            {
                if (board[56] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 8 : -8;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -8 : 8;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 62 || i == 55)
            {
                if (board[63] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 8 : -8;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -8 : 8;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 9)
            {
                if (board[0] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 24 : -24;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -24 : 24;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 49)
            {
                if (board[56] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 24 : -24;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -24 : 24;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 14)
            {
                if (board[7] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 24 : -24;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -24 : 24;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 54)
            {
                if (board[63] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 24 : -24;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -24 : 24;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 2 || i == 16 || i == 5 || i == 23 || i == 40 || i == 58 || i == 47 || i == 61)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -8 : 8;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 8 : -8;
            }
            else if (i == 3 || i == 4 || i == 24 || i == 32 || i == 31 || i == 39 || i == 59 || i == 60)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -6 : 6;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 6 : -6;
            }
            else if (i == 18 || i == 21 || i == 42 || i == 45)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -7 : 7;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 7 : -7;
            }
            else if (i == 19 || i == 20 || i == 26 || i == 34 || i == 29 || i == 37 || i == 43 || i == 44)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -4 : 4;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 4 : -4;
            }
            else if (i == 11 || i == 25)
            {
                if (board[0] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 3 : -3;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -3 : 3;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if(i == 33 || i == 51)
            {
                if (board[56] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 3 : -3;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -3 : 3;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 38 || i == 52)
            {
                if (board[63] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 3 : -3;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -3 : 3;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 12 || i == 30)
            {
                if (board[7] == PieceState.NONE)
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? 3 : -3;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? -3 : 3;
                }
                else
                {
                    if (board[i] == PieceState.WHITE)
                        score += (turn == 0) ? -2 : 2;
                    else if (board[i] == PieceState.BLACK)
                        score += (turn == 0) ? 2 : -2;
                }
            }
            else if (i == 27 || i == 28 || i == 35 || i == 36)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -3 : 3;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 3 : -3;
            }
            else
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? 4 : -4;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? -4 : 4;
            }
        }
        return score;
    }

    private bool isViable(Dictionary<int, PieceState> board, int id, int turn)
    {
        if (id % 8 != 0 && board[id - 1] == GetOpposite(turn)) // Above
            return true;
        if ((id - 7) % 8 != 0 && board[id + 1] == GetOpposite(turn)) // Below
            return true;
        if (id > 7 && board[id - 8] == GetOpposite(turn)) // Left
            return true;
        if (id < 56 && board[id + 8] == GetOpposite(turn)) // right
            return true;
        if (id % 8 != 0 && id > 7 && board[id - 9] == GetOpposite(turn)) // top-left
            return true;
        if ((id - 7) % 8 != 0 && id < 56 && board[id + 9] == GetOpposite(turn)) // bottom-right
            return true;
        if (id % 8 != 0 && id < 56 && board[id + 7] == GetOpposite(turn)) // top-right
            return true;
        if ((id - 7) % 8 != 0 && id > 7 && board[id - 7] == GetOpposite(turn)) // bottom-left
            return true;
        return false;
    }

    // Makes a move on the board and flips the pieces if the move was an actual move. Adds them to a queue
    // if a piece needs flipped and pops them off flipping them at the end. It returns the number of pieces
    // that were successfully flipped
    private int MakeMove(Dictionary<int, PieceState> board, int id, int turn, bool gameCheck)
    {
        if (board[id] != PieceState.NONE)
            return 0;
        Queue<int> needsFlipped = new Queue<int>();

        // Check above
        if (id % 8 != 0 && board[id - 1] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 1;
            while (next % 8 != 0)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next - 1] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next--;
            }
        }

        // Check below
        if ((id - 7) % 8 != 0 && board[id + 1] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 1;
            while ((next - 7) % 8 != 0)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next + 1] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next++;
            }
        }

        // Check left
        if (id > 7 && board[id - 8] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 8;
            while (next > 7)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next - 8] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next -= 8;
            }
        }

        // Check right
        if (id < 56 && board[id + 8] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 8;
            while (next < 56)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next + 8] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next += 8; ;
            }
        }

        // Check top-left
        if (id % 8 != 0 && id > 7 && board[id - 9] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 9;
            while (next % 8 != 0 && next > 7)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next - 9] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next -= 9;
            }
        }

        // Check bottom-right
        if ((id - 7) % 8 != 0 && id < 56 && board[id + 9] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 9;
            while ((next - 7) % 8 != 0 && next < 56)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next + 9] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next += 9;
            }
        }

        // Check top-right
        if (id % 8 != 0 && id < 56 && board[id + 7] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 7;
            while (next % 8 != 0 && next < 56)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next + 7] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next += 7;
            }
        }

        // Check bottom-left
        if ((id - 7) % 8 != 0 && id > 7 && board[id - 7] == GetOpposite(turn))
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 7;
            while ((next - 7) % 8 != 0 && next > 7)
            {
                if (board[next] == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (board[next - 7] == GetSame(turn))
                {
                    while (flipChecks.Count != 0)
                    {
                        if (gameCheck) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next -= 7;
            }
        }

        if (needsFlipped.Count == 0)
            return 0;

        int flippedPieces = needsFlipped.Count;

        while (needsFlipped.Count != 0)
        {
            if (turn == 0)
                board[needsFlipped.Dequeue()] = PieceState.BLACK;
            else
                board[needsFlipped.Dequeue()] = PieceState.WHITE;
        }

        return flippedPieces;
    }

    private PieceState GetOpposite(int turn)
    {
        return (turn != 0) ? PieceState.BLACK : PieceState.WHITE;
    }

    private PieceState GetSame(int turn)
    {
        return (turn != 0) ? PieceState.WHITE : PieceState.BLACK;
    }
}

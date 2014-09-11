using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Minimax
{
    private int _difficulty;

    public Minimax(int difficulty)
    {
        _difficulty = difficulty;
    }

    public int GetMove(Dictionary<int, Piece> pieces, int turn)
    {
        Dictionary<int, PieceState> board = new Dictionary<int, PieceState>();
        foreach(KeyValuePair<int, Piece> p in pieces)
        {
            board.Add(p.Key, p.Value.State);
        }
        int move = minimax(board, turn, _difficulty, 0, int.MaxValue).Move;
        return move;
    }

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

    private ScoreMovePair minimax(Dictionary<int, PieceState> board, int turn, int maxDepth, int currDepth, int prune)
    {
        if (isGameOver(board) || currDepth == maxDepth)
        {
            if(GameOptions.Instance.SMART)
                return new ScoreMovePair(-1, weightedEvaluation(board, (turn == 0) ? 1 : 0));
            else
                return new ScoreMovePair(-1, unweightedEvaluation(board, (turn == 0) ? 1 : 0));
        }
        ScoreMovePair best = new ScoreMovePair(-1, 0); // Initialize best move to none
        if (turn == 0) // Turn is AI's
            best.Score = int.MinValue;
        else // Turn is player's
            best.Score = int.MaxValue;
        for (int i = 0; i < 64; i++) // Iterate through board positions
        {
            // If move not available skip
            if (board[i] != PieceState.NONE)
                continue;

            // Create copy of board for new board
            Dictionary<int, PieceState> newBoard = new Dictionary<int, PieceState>();
            foreach (KeyValuePair<int, PieceState> b in board)
                newBoard.Add(b.Key, b.Value);
            // Attempt a move and evaluate it
            if (MakeMove(newBoard, i, turn, false) == 0)
                continue;
            newBoard[i] = (turn == 0) ? PieceState.BLACK : PieceState.WHITE;
            ScoreMovePair current = minimax(newBoard, (turn == 0) ? 1 : 0, maxDepth, currDepth + 1, best.Score);
            if (turn == 0) // Maximize
            {
                if (current.Score > prune)
                {
                    return current;
                }
                if (current.Score > best.Score)
                {
                    best.Score = current.Score;
                    best.Move = i;
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
                    best.Move = i;
                }
            }
        }

        return best;
    }

    private bool isGameOver(Dictionary<int, PieceState> board)
    {
        // Check if black can move
        for (int i = 0; i < 64; i++)
        {
            Dictionary<int, PieceState> testBoard = new Dictionary<int, PieceState>();
            foreach (KeyValuePair<int, PieceState> b in board)
                testBoard.Add(b.Key, b.Value);
            if (MakeMove(testBoard, i, 0, true) != 0)
                return false;

            testBoard = new Dictionary<int, PieceState>();
            foreach (KeyValuePair<int, PieceState> b in board)
                testBoard.Add(b.Key, b.Value);
            if (MakeMove(testBoard, i, 1, true) != 0)
                return false;
        }
        return true;
    }

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

    // This evaluation method is based off of the one used by Microsoft's Reversi game
    private int weightedEvaluation(Dictionary<int, PieceState> board, int turn)
    {
        int score = 0;
        for (int i = 0; i < 64; i++)
        {
            if (i == 0 || i == 7 || i == 56 || i == 63)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? -99 : 99;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? 99 : -99;
            }
            else if(i == 1 || i == 8 || i == 6 || i == 15 || i == 48 || i == 57 || i == 62 || i == 55)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? 8 : -8;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? -8 : 8;
            }
            else if(i == 9 || i == 49 || i == 14 || i == 54)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? 24 : -24;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? -24 : 24;
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
            else if (i == 11 || i == 12 || i == 25 || i == 33 || i == 30 || i == 38 || i == 51 || i == 52)
            {
                if (board[i] == PieceState.WHITE)
                    score += (turn == 0) ? 3 : -3;
                else if (board[i] == PieceState.BLACK)
                    score += (turn == 0) ? -3 : 3;
            }
            else if (i == 27 || i == 28 || i == 35 || i == 36)
            {
                if (board[i] == PieceState.WHITE)
                    score += 0;
                else if (board[i] == PieceState.BLACK)
                    score += 0;
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

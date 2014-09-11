using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Main controller for the game. This is used for a multiplayer and 
/// single player game.
/// </summary>
public class GameController : MonoBehaviour
{
    #region Class Member Variables

    private const int PIECE_COUNT = 64; // This should not be changed
    private const float PIECE_OFFSET = 0.5f; // Offset distance of each piece on the physical board

    // Lookup tables for the pieces
    private Dictionary<int, Piece> _pieceLookup; // This represents the board in the game
    private Dictionary<int, GameObject> _pieceLocations; // This is used by the AI so that it knows what location to instantiate the game piece at

    // Basic trackers of the game
    private bool _whitesMove = true;
    public GameObject PiecePrefab;
    public GameObject Parent;
    public int WhiteScore { get; private set; }
    public int BlackScore { get; private set; }
    public bool WhitesMove { get { return _whitesMove; } }
    public bool BlacksMove { get { return !_whitesMove; } }
    public bool GameOver { get; private set; }

    // used for the threading
    private bool _beganCalculations = false;
    private volatile bool _setMoveAI = false;
    private int _moveAI;

    // Property to tell the UI when to let the user know that a thread is computing the AI's turn
    public bool Calculating { get { return _beganCalculations; } }

    #endregion

    #region Singleton Contruction and Initialization

    // Static instance of itself used by the game screen ui to get the properties of 
    // the game state to notify the user.
    public static GameController Instance;

    private void Awake()
    {
        if (Instance != null)
            GameObject.Destroy(Instance);
        else
            Instance = this;
    }

    // Basic initialization
    private void Start()
    {
        _pieceLookup = new Dictionary<int, Piece>();
        _pieceLocations = new Dictionary<int, GameObject>();
        Initialize();
        WhiteScore = 2;
        BlackScore = 2;
        GameOver = false;
    }

    // Iterates through all the pieces to see if there is a move that can be made by
    // anyone on the board
    private bool canMove()
    {
        // Check if black can move
        for (int i = 0; i < 64; i++)
        {
            if (CheckAndMove(i, false) != 0)
                return true;
        }
        return false;
    }

    private void Update()
    {
        // If in singleplayer and its the AIs turn the thread needs to be started to compute its move
        if (!GameOver && GameOptions.Instance.Options == Options.SINGLEPLAYER && !_whitesMove && !_beganCalculations)
        {
            _beganCalculations = true;
            Thread compute = new Thread(Calculate);
            compute.Start();
        }
        if (_setMoveAI)
        {
            _setMoveAI = false;
            _beganCalculations = false;
            if (_moveAI != -1) // A move of -1 means it could not find one (This is a bug if it occurs)
            {
                SetPiece(_moveAI, _pieceLocations[_moveAI].transform.position); // Sets the calculated move on the board
            }
            else
            {
                // If there was a bug in the minimax than just place the piece in an available position
                for (int i = 0; i < 64; i++)
                {
                    if (CheckAndMove(i, false) != 0)
                        SetPiece(i, _pieceLocations[i].transform.position);
                }
            }
        }
    }

    // Threaded method that calls minimax to compute the move of the AI
    private void Calculate()
    {
        Minimax mm = new Minimax(GameOptions.Instance.Difficulty);
        _moveAI = mm.GetMove(_pieceLookup, 0, WhiteScore + BlackScore);
        _setMoveAI = true;
    }

    #endregion

    #region Public Methods

    // Sets the piece onto the board  tracking whose turn it is and only doing it if it should
    public void SetPiece(int id, Vector3 location)
    {
        if(CheckAndMove(id, true) == 0)
            return;
        _pieceLookup[id].PlacePiece((_whitesMove) ? PieceState.WHITE : PieceState.BLACK, location);
        if (_whitesMove)
            WhiteScore++;
        else
            BlackScore++;
        _whitesMove = (_whitesMove) ? false : true;
        if (!canMove())
        {
            _whitesMove = (_whitesMove) ? false : true;
            if (!canMove())
            {
                GameOver = true;
            }
        }
    }

    #endregion

    #region Private Helper Methods

    // Initializes the board state for the beginning of the game
    private void Initialize()
    {
        for (int i = 0; i < PIECE_COUNT; i++)
        {
            Piece piece = new Piece();
            // These four pieces are the initial four pieces on the board
            switch (i)
            {
                case 27:
                    piece.PlacePiece(PieceState.WHITE, new Vector3(-PIECE_OFFSET, 1, PIECE_OFFSET));
                    break;
                case 28:
                    piece.PlacePiece(PieceState.BLACK, new Vector3(-PIECE_OFFSET, 1, -PIECE_OFFSET));
                    break;
                case 35:
                    piece.PlacePiece(PieceState.BLACK, new Vector3(PIECE_OFFSET, 1, PIECE_OFFSET));
                    break;
                case 36:
                    piece.PlacePiece(PieceState.WHITE, new Vector3(PIECE_OFFSET, 1, -PIECE_OFFSET));
                    break;
            }
            _pieceLookup.Add(i, piece);
            _pieceLocations.Add(i, GameObject.Find(i.ToString()));
        }
    }

    // This checks if a person can move and if they can it adds the flips to a queue
    // and at the end pops them all off and flipping the pieces on the board. Returns the
    // number of pieces successfully flipped.
    private int CheckAndMove(int id, bool move)
    {
        if (_pieceLookup[id].State != PieceState.NONE)
            return 0;
        Queue<int> needsFlipped = new Queue<int>();

        // Check above
        if (id % 8 != 0 && _pieceLookup[id - 1].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 1;
            while (next % 8 != 0)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next - 1].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next--;
            }
        }

        // Check below
        if ((id - 7) % 8 != 0 && _pieceLookup[id + 1].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 1;
            while ((next - 7) % 8 != 0)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next + 1].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next++;
            }
        }

        // Check left
        if (id > 7 && _pieceLookup[id - 8].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 8;
            while (next > 7)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next - 8].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next -= 8;
            }
        }

        // Check right
        if (id < 56 && _pieceLookup[id + 8].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 8;
            while (next < 56)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next + 8].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next += 8;
            }
        }

        // Check top-left
        if (id % 8 != 0 && id > 7 && _pieceLookup[id - 9].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 9;
            while (next % 8 != 0 && next > 7)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next - 9].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next -= 9;
            }
        }

        // Check bottom-right
        if ((id - 7) % 8 != 0 && id < 56 && _pieceLookup[id + 9].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 9;
            while ((next - 7) % 8 != 0 && next < 56)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next + 9].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next += 9;
            }
        }

        // Check top-right
        if (id % 8 != 0 && id < 56 && _pieceLookup[id + 7].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id + 7;
            while (next % 8 != 0 && next < 56)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next + 7].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next += 7;
            }
        }

        // Check bottom-left
        if ((id - 7) % 8 != 0 && id > 7 && _pieceLookup[id - 7].State == GetOpposite())
        {
            Queue<int> flipChecks = new Queue<int>();
            int next = id - 7;
            while ((next - 7) % 8 != 0 && next > 7)
            {
                if (_pieceLookup[next].State == PieceState.NONE)
                    break;
                flipChecks.Enqueue(next);
                if (_pieceLookup[next - 7].State == GetSame())
                {
                    while (flipChecks.Count != 0)
                    {
                        if (!move) return 1;
                        needsFlipped.Enqueue(flipChecks.Dequeue());
                    }
                    break;
                }
                next -= 7;
            }
        }

        if(needsFlipped.Count == 0)
            return 0;

        int flippedPieces = needsFlipped.Count;
        if (move)
        {
            if (_whitesMove)
            {
                WhiteScore += needsFlipped.Count;
                BlackScore -= needsFlipped.Count;
            }
            else
            {
                BlackScore += needsFlipped.Count;
                WhiteScore -= needsFlipped.Count;
            }
            while (needsFlipped.Count != 0)
            {
                _pieceLookup[needsFlipped.Dequeue()].Flip();
            }
        }
        return flippedPieces;
    }

    private PieceState GetOpposite()
    {
        return (_whitesMove) ? PieceState.BLACK : PieceState.WHITE;
    }

    private PieceState GetSame()
    {
        return (_whitesMove) ? PieceState.WHITE : PieceState.BLACK;
    }

    #endregion
}

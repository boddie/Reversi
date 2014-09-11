using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class GameController : MonoBehaviour
{
    #region Class Member Variables

    private const int PIECE_COUNT = 64;
    private const float PIECE_OFFSET = 0.5f;

    private Dictionary<int, Piece> _pieceLookup;
    private Dictionary<int, GameObject> _pieceLocations;
    private bool _whitesMove = true;

    public GameObject PiecePrefab;
    public GameObject Parent;
    public int WhiteScore { get; private set; }
    public int BlackScore { get; private set; }
    public bool WhitesMove { get { return _whitesMove; } }
    public bool BlacksMove { get { return !_whitesMove; } }
    public bool GameOver { get; private set; }

    private bool _beganCalculations = false;
    private volatile bool _setMoveAI = false;
    private int _moveAI;

    public bool Calculating { get { return _beganCalculations; } }

    #endregion

    #region Singleton Contruction and Initialization

    public static GameController Instance;

    private void Awake()
    {
        if (Instance != null)
            GameObject.Destroy(Instance);
        else
            Instance = this;
    }

    private void Start()
    {
        _pieceLookup = new Dictionary<int, Piece>();
        _pieceLocations = new Dictionary<int, GameObject>();
        Initialize();
        WhiteScore = 2;
        BlackScore = 2;
        GameOver = false;
    }

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
            if (_moveAI != -1)
            {
                SetPiece(_moveAI, _pieceLocations[_moveAI].transform.position);
            }
            else
            {
                for (int i = 0; i < 64; i++)
                {
                    if (CheckAndMove(i, false) != 0)
                        SetPiece(i, _pieceLocations[i].transform.position);
                }
            }
        }
    }

    private void Calculate()
    {
        Minimax mm = new Minimax(GameOptions.Instance.Difficulty);
        _moveAI = mm.GetMove(_pieceLookup, 0);
        _setMoveAI = true;
    }

    #endregion

    #region Public Methods

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

    private void Initialize()
    {
        for (int i = 0; i < PIECE_COUNT; i++)
        {
            Piece piece = new Piece();
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

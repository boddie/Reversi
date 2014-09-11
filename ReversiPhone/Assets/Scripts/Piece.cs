using UnityEngine;
using System.Collections;

/// <summary>
/// Represents the state of a game piece
/// </summary>
public enum PieceState
{
    NONE,
    WHITE,
    BLACK
}

/// <summary>
/// This is on every piece in the game. This plays the sound for placing a 
/// piece and puts it literally in the game scene.
/// </summary>
public class Piece 
{
    // Used to track what state the piece is in
    public PieceState State { get; private set; }

    // The model of the piece
    private GameObject _prefab;

    // Initializes the piece
    public Piece()
    {
        this.State = PieceState.NONE;
    }

    // Called when a piece is initially put on the board.
    public void PlacePiece(PieceState State, Vector3 Location)
    {
        this.State = State;
        _prefab = (GameObject)GameObject.Instantiate(GameController.Instance.PiecePrefab, Location, Quaternion.identity);
        _prefab.transform.parent = GameController.Instance.Parent.transform;
        Quaternion parentRotation = GameController.Instance.Parent.transform.rotation;
        _prefab.transform.Rotate(parentRotation.eulerAngles, Space.Self);
        if (State == PieceState.BLACK)
        {
            _prefab.transform.Rotate(180, 0, 0, Space.Self);
        }
        _prefab.audio.Play();
    }

    // Called by Game controller when user or AI moves and has to flip in between pieces.
    public bool Flip()
    {
        if (State == PieceState.NONE)
            return false;
        if (State == PieceState.WHITE)
            State = PieceState.BLACK;
        else if (State == PieceState.BLACK)
            State = PieceState.WHITE;
        _prefab.transform.Rotate(180, 0, 0, Space.Self);
        _prefab.audio.Play();
        return true;
    }
}

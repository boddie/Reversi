using UnityEngine;
using System.Collections;

public enum PieceState
{
    NONE,
    WHITE,
    BLACK
}

public class Piece 
{
    public PieceState State { get; private set; }

    private GameObject _prefab;

    public Piece()
    {
        this.State = PieceState.NONE;
    }

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

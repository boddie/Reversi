using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used for placing a piece at its location on the board
/// </summary>
public class PositionScript : MonoBehaviour 
{
    private void OnMouseDown()
    {
        // This can only be done when it is the person's turn. If playing an AI you cannot move for the AI
        if (GameOptions.Instance.Options == Options.SINGLEPLAYER && GameController.Instance.BlacksMove)
            return;
        // Each piece is named its location in the dictionary, so I convert it to the index and instantiate it at its position
        GameController.Instance.SetPiece(Convert.ToInt32(this.transform.name), this.transform.position);
    }
}

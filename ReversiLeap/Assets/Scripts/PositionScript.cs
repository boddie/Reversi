using System;
using UnityEngine;
using System.Collections;

public class PositionScript : MonoBehaviour 
{
    private void OnMouseDown()
    {
        if (GameOptions.Instance.Options == Options.SINGLEPLAYER && GameController.Instance.BlacksMove)
            return;
        GameController.Instance.SetPiece(Convert.ToInt32(this.transform.name), this.transform.position);
    }

    private void OnCollisionEnter(Collision c)
    {
        if (GameOptions.Instance.Options == Options.SINGLEPLAYER && GameController.Instance.BlacksMove)
            return;
        GameController.Instance.SetPiece(Convert.ToInt32(this.transform.name), this.transform.position);
    }
}

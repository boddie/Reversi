using UnityEngine;
using System.Collections;

/// <summary>
/// Script to rotate a game object a small amount each frame
/// </summary>
public class CameraRotate : MonoBehaviour 
{
	void Update () 
    {
        this.transform.Rotate(new Vector3(0, 0, 1), 0.01f, Space.World);
	}
}

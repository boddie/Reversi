using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates the object about its center. I use this to create the moving in 
/// space backgroun in the game
/// </summary>
public class SceneRotate : MonoBehaviour {

    void Update()
    {
        this.transform.Rotate(new Vector3(0, 0, 1), -.01f, Space.Self);
        this.transform.Rotate(new Vector3(1, 0, 1), -.01f, Space.Self);
    }
}

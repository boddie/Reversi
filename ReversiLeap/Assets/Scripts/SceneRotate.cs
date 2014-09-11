using UnityEngine;
using System.Collections;

public class SceneRotate : MonoBehaviour {

    void Update()
    {
        this.transform.Rotate(new Vector3(0, 0, 1), -.01f, Space.Self);
        this.transform.Rotate(new Vector3(1, 0, 1), -.01f, Space.Self);
    }
}

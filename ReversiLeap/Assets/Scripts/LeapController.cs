using UnityEngine;
using System.Collections;
using Leap;

public class LeapController : MonoBehaviour 
{
    Controller controller;
    float baseZ;

    void Start()
    {
        controller = new Controller();

        // Gets Z as reference
        baseZ = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
    }

    void Update()
    {
        Frame frame = controller.Frame();

        // Get frontmost pointable
        Pointable pointable = frame.Pointables.Frontmost;
        
        // Get distance (1 to -1)
        float distance = pointable.TouchDistance;

        // Get position
        Vector stabilizedPosition = pointable.StabilizedTipPosition;

        // Make it useable
        InteractionBox iBox = controller.Frame().InteractionBox;
        Vector normalizedPosition = iBox.NormalizePoint(stabilizedPosition);
        float x = normalizedPosition.x * UnityEngine.Screen.width;
        float y = UnityEngine.Screen.height - normalizedPosition.y * UnityEngine.Screen.height;
        float z = baseZ - distance * 6;

        // Move it
        this.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, UnityEngine.Screen.height - y, z));
    }
}

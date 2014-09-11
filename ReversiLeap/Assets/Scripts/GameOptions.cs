using UnityEngine;
using System.Collections;

public enum Options
{
    SINGLEPLAYER,
    TWOPLAYER
}

public class GameOptions : MonoBehaviour 
{
    public static GameOptions Instance;

    public int Difficulty { get; set; }
    public bool SMART { get; set; }
    public Options Options { get; set; }

    private void Awake()
    {
        if (Instance != null)
            GameObject.Destroy(Instance);
        else
            Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        SMART = false;
    }
}

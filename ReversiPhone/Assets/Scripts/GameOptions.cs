using UnityEngine;
using System.Collections;

// Used to denote a singleplayer or two player game
public enum Options
{
    SINGLEPLAYER,
    TWOPLAYER
}

/// <summary>
/// Singleton to pass game information about the AI to the game itself
/// from the start screen
/// </summary>
public class GameOptions : MonoBehaviour 
{
    public static GameOptions Instance;

    // Dificulty of game (depth of minimax)
    public int Difficulty { get; set; }
    // Sets if it uses a good evaluation method or a bad one
    public bool SMART { get; set; }
    // Tells game controller if single player mode or multiplayer
    public Options Options { get; set; }

    // Creates an instance of itself
    private void Awake()
    {
        if (Instance != null)
            GameObject.Destroy(Instance);
        else
            Instance = this;
        DontDestroyOnLoad(this);
    }

    // Initialization
    private void Start()
    {
        SMART = false;
    }
}

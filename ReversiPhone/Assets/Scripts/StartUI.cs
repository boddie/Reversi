using UnityEngine;
using System.Collections;

/// <summary>
/// This is the class that draws the UI on the initial start screen of the game
/// </summary>
public class StartUI : MonoBehaviour 
{
    // Max depth of the minimax algorithm
    private const int MAX_DEPTH = 5;

    #region Class member variables

    private Texture2D _reversi;
    private float _baseWidth;
    private float _baseHeight;

    // Rects for drawing UI elements
    private Rect _titleRect;
    private Rect _singlePlayer;
    private Rect _multiPlayer;
    private Rect _quit;
    private Rect _moves;
    private Rect _thinking;

    private int _prevScreenWidth;
    private int _prevScreenHeight;
    private int _difficulty = 1;

    private Color _uiColor;

    // used to scale and adjsut fonts
    public GUISkin ReversiSkin;
    public GameObject SoundObject;

    // Sets game options such as single player or multiplayer
    private GameOptions _options;

    // Used for changing the color when the mouse is over the menu item.
    private Vector2 _mousePosition;
    private bool _inRect1 = false;
    private bool _inRect2 = false;
    private bool _inRect3 = false;
    private bool _inRect4 = false;
    private bool _inRect5 = false;

    // Sets the evaluation function to be used in the game
    private bool _smart = false;

    #endregion

    // initializes rects and loads textures and colors
    private void Start () 
    {
        _reversi = Resources.Load("Reversi") as Texture2D;

        _baseWidth = Screen.width / 20;
        _baseHeight = Screen.height / 20;

        _prevScreenWidth = Screen.width;
        _prevScreenHeight = Screen.height;

        _titleRect = new Rect(_baseWidth / 4, _baseHeight / 2, _baseWidth * 10, _baseHeight * 5);
        _thinking = new Rect(_baseWidth / 2, _baseHeight * 10, 350, 35);
        _moves = new Rect(_baseWidth / 2, _baseHeight * 12, 325, 35);
        _singlePlayer = new Rect(_baseWidth / 2, _baseHeight * 14, 250, 35);
        _multiPlayer = new Rect(_baseWidth / 2, _baseHeight * 16, 225, 35);
        _quit = new Rect(_baseWidth / 2, _baseHeight * 18, 100, 35);

        _uiColor = new Color(0, 1, 0, 1);
	}

    private void Update()
    {
        // Redraws the scene dynamically (I may forget to do this in the game scene)
        if (_prevScreenHeight != Screen.height || _prevScreenWidth != Screen.width)
        {
            _baseWidth = Screen.width / 20;
            _baseHeight = Screen.height / 20;

            _titleRect = new Rect(_baseWidth / 4, _baseHeight / 2, _baseWidth * 10, _baseHeight * 5);
            _singlePlayer = new Rect(_baseWidth / 2, _baseHeight * 14, 250, 35);
            _multiPlayer = new Rect(_baseWidth / 2, _baseHeight * 16, 225, 35);
            _quit = new Rect(_baseWidth / 2, _baseHeight * 18, 100, 35);
        }
        _prevScreenWidth = Screen.width;
        _prevScreenHeight = Screen.height;
    }

    // Most the code here should be pretty self-explanatory. Draws the UI elements and sets the 
    // properties based on the user's selection
    private void OnGUI()
    {
        GUISkin current = GUI.skin;
        GUI.skin = ReversiSkin;

        _mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        GUI.DrawTexture(_titleRect, _reversi, ScaleMode.StretchToFill);

        if (_thinking.Contains(_mousePosition))
        {
            if (!SoundObject.audio.isPlaying && !_inRect4)
            {
                SoundObject.audio.Play();
                _inRect4 = true;
            }
            Color prev = GUI.color;
            GUI.color = _uiColor;
            if(_smart)
                GUI.Label(_thinking, "INTELLIGENCE: SMART");
            else
                GUI.Label(_thinking, "INTELLIGENCE: DUMB");
            GUI.color = prev;
        }
        else
        {
            if (_smart)
                GUI.Label(_thinking, "INTELLIGENCE: SMART");
            else
                GUI.Label(_thinking, "INTELLIGENCE: DUMB");
            _inRect4 = false;
        }

        if (GUI.Button(_thinking, "", "Label"))
        {
            _smart = (_smart) ? false : true;
            GameOptions.Instance.SMART = _smart;
        }

        if (_moves.Contains(_mousePosition))
        {
            if (!SoundObject.audio.isPlaying && !_inRect5)
            {
                SoundObject.audio.Play();
                _inRect5 = true;
            }
            Color prev = GUI.color;
            GUI.color = _uiColor;
            GUI.Label(_moves, "THOUGHT DEPTH: " + _difficulty);
            GUI.color = prev;
        }
        else
        {
            GUI.Label(_moves, "THOUGHT DEPTH: " + _difficulty);
            _inRect5 = false;
        }

        if (GUI.Button(_moves, "", "Label"))
        {
            _difficulty++;
            if (_difficulty > MAX_DEPTH)
                _difficulty = 1;
        }

        if (_singlePlayer.Contains(_mousePosition))
        {
            if (!SoundObject.audio.isPlaying && !_inRect1)
            {
                SoundObject.audio.Play();
                _inRect1 = true;
            }
            Color prev = GUI.color;
            GUI.color = _uiColor;
            GUI.Label(_singlePlayer, "SINGLE PLAYER");
            GUI.color = prev;
        }
        else
        {
            GUI.Label(_singlePlayer, "SINGLE PLAYER");
            _inRect1 = false;
        }

        if (_multiPlayer.Contains(_mousePosition))
        {
            if (!SoundObject.audio.isPlaying && !_inRect2)
            {
                SoundObject.audio.Play();
                _inRect2 = true;
            }
            Color prev = GUI.color;
            GUI.color = _uiColor;
            GUI.Label(_multiPlayer, "MULTIPLAYER");
            GUI.color = prev;
        }
        else
        {
            GUI.Label(_multiPlayer, "MULTIPLAYER");
            _inRect2 = false;
        }

        if (_quit.Contains(_mousePosition))
        {
            if (!SoundObject.audio.isPlaying && !_inRect3)
            {
                SoundObject.audio.Play();
                _inRect3 = true;
            }
            Color prev = GUI.color;
            GUI.color = _uiColor;
            GUI.Label(_quit, "QUIT");
            GUI.color = prev;
        }
        else
        {
            GUI.Label(_quit, "QUIT");
            _inRect3 = false;
        }

        if (GUI.Button(_singlePlayer, string.Empty, "Label"))
        {
            GameOptions.Instance.Options = Options.SINGLEPLAYER;
            GameOptions.Instance.Difficulty = _difficulty;
            Application.LoadLevel("scene_singleplayer");
        }
        if (GUI.Button(_multiPlayer, string.Empty, "Label"))
        {
            GameOptions.Instance.Options = Options.TWOPLAYER;
            Application.LoadLevel("scene_twoplayer");
        }
        if (GUI.Button(_quit, string.Empty, "Label"))
        {
            Application.Quit();
        }

        GUI.skin = current;
    }
}

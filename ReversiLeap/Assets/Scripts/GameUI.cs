using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour 
{
    private float _standardHeight;
    private float _standardWidth;

    private Rect _gameOverRect;
    private Rect _playerOneRect;
    private Rect _playerTwoRect;
    private Rect _pOneScoreRect;
    private Rect _pTwoScoreRect;
    private Rect _pOneTurnRect;
    private Rect _pTwoTurnRect;
    private Rect _quitRect;

    private Texture _gameOverTexture;

    public GUISkin GameSkin;

	void Start () 
    {
        _standardHeight = Screen.height / 8;
        _standardWidth = Screen.width / 16;

        _gameOverRect = new Rect(_standardWidth * 3, _standardHeight * 3, _standardWidth * 10, _standardHeight * 2);
        _playerOneRect = new Rect(_standardWidth * 0.5f, _standardHeight * 2, 0, 0);
        _playerTwoRect = new Rect(_standardWidth * 12.5f, _standardHeight * 2, 0, 0);
        _pOneScoreRect = new Rect(_standardWidth * 0.5f, _standardHeight * 3, 0, 0);
        _pTwoScoreRect = new Rect(_standardWidth * 12.5f, _standardHeight * 3, 0, 0);
        _pOneTurnRect = new Rect(_standardWidth * 0.5f, _standardHeight * 4, 0, 0);
        _pTwoTurnRect = new Rect(_standardWidth * 12.5f, _standardHeight * 4, 0, 0);
        _quitRect = new Rect(10, 0, 0, 0);

        _gameOverTexture = Resources.Load("GameOver") as Texture;
	}
	
	void OnGUI () 
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        if (GameController.Instance.Calculating)
            GUILayout.Label("Calculating...");

        GUISkin current = GUI.skin;
        GUI.skin = GameSkin;

        Vector2 size;

        Color currentColor = GUI.color;
        GUI.color = Color.green;

        size = GUI.skin.label.CalcSize(new GUIContent("WHITE'S TURN!"));
        _pOneTurnRect.width = size.x;
        _pOneTurnRect.height = size.y;
        if (GameController.Instance.WhitesMove)
        {
            GUI.Label(_pOneTurnRect, "WHITE'S TURN!");
        }

        size = GUI.skin.label.CalcSize(new GUIContent("BLACK'S TURN!"));
        _pTwoTurnRect.width = size.x;
        _pTwoTurnRect.height = size.y;
        if (GameController.Instance.BlacksMove)
        {
            GUI.Label(_pTwoTurnRect, "BLACK'S TURN!");
        }

        size = GUI.skin.label.CalcSize(new GUIContent("WHITE"));
        _playerOneRect.width = _pOneTurnRect.width;
        _playerOneRect.height = size.y;
        GUI.Label(_playerOneRect, "WHITE");

        size = GUI.skin.label.CalcSize(new GUIContent("BLACK"));
        _playerTwoRect.width = _pTwoTurnRect.width;
        _playerTwoRect.height = size.y;
        GUI.Label(_playerTwoRect, "BLACK");

        GUI.color = currentColor;

        size = GUI.skin.label.CalcSize(new GUIContent(GameController.Instance.WhiteScore.ToString()));
        _pOneScoreRect.width = _pOneTurnRect.width;
        _pOneScoreRect.height = size.y;
        GUI.Label(_pOneScoreRect, GameController.Instance.WhiteScore.ToString());

        size = GUI.skin.label.CalcSize(new GUIContent(GameController.Instance.BlackScore.ToString()));
        _pTwoScoreRect.width = _pTwoTurnRect.width;
        _pTwoScoreRect.height = size.y;
        GUI.Label(_pTwoScoreRect, GameController.Instance.BlackScore.ToString());

        size = GUI.skin.label.CalcSize(new GUIContent("BACK TO MENU"));
        _quitRect.width = size.x;
        _quitRect.height = size.y;
        _quitRect.y = Screen.height - 10 - size.y;

        if(GameController.Instance.Calculating)
        {
            GUI.color = new Color(0.25f, 0.25f, 0.25f, 1);
            GUI.Label(_quitRect, "BACK TO MENU");
        }
        else if (_quitRect.Contains(mousePosition))
        {
            GUI.color = Color.green;
            GUI.Label(_quitRect, "BACK TO MENU");
            if (GUI.Button(_quitRect, "", "Label"))
            {
                Application.LoadLevel("scene_start");
            }
        }
        else
        {
            GUI.Label(_quitRect, "BACK TO MENU");
        }

        GUI.color = currentColor;

        if (GameController.Instance.GameOver)
        {
            GUI.DrawTexture(_gameOverRect, _gameOverTexture);
        }

        GUI.skin = current;
	}
}

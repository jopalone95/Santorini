using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum States
{
    StartupFirstFirst,
    StartupFirstSecond,
    StartupSecondFirst,
    StartupSecondSecond,
    MoveFirst,
    BuildFirst,
    MoveSecond,
    BuildSecond
}

public enum Turn
{
    Player1,
    Player2
}

public class GameController : MonoBehaviour
{
    //pamtiću string koji će mi govoriti da li sam izabrao da igram kao Player1 ili Player2

    public static GameObject currentPlayer;
    public static States     currentState;
    public static Turn       onMove;
    public static string     lastMovedFigure;
    public static Text       onTheMove;
    public static bool       gameOver;

    void Awake()
    {
        currentState  = States.StartupFirstFirst;
        onMove        = Turn.Player1;
        gameOver      = false;

        onTheMove = GameObject.Find("OnTheMove").GetComponent<Text>();
    }

    public void NextMove()
    {
        if (IsSettingUp() == true)
        {
            AIController.Startup();
        }
        else if (IsMovingAfterSetup() == true)
        {
            AIController.Move();
        }
    }

    public static bool IsGameOver()
    {
        //proverava da li je kraj igre nastupio blokiranjem, ukoliko jeste ispisuje ime pobednika u konzoli

        string winningPlayer = GetWinningPlayer();

        if (winningPlayer != null)
        {

            Debug.Log(winningPlayer + " has won!");

            GameModeController.winner = winningPlayer;

            return true;
        }
        else
        {
            return false;
        }
    }


    public static string GetWinningPlayer()
    {
        //vraća string sa imenom igrača koji je pobedio (Player1 ili Player2) ukoliko ima pobednika u datom trenutku,
        //ukoliko pobednika nema vraća null

        if (IsSettingUp() == true)
        {
            //do kraja igre ne može doći u delu igre kada se figure tek postavljaju na tablu

            return null;
        }
 
        string[] players = { "Player1", "Player2" };

        for (int k = 0; k < players.Length; k++)
        {
            string player     = players[k];
            bool   isGameOver = true;

            GameObject[] playerFigures = GameObject.FindGameObjectsWithTag(player.ToString());

            for (int i = 0; i < playerFigures.Length; i++)
            {
                GameObject figure = playerFigures[i];
                PlayerController figureController = figure.GetComponent<PlayerController>();

                GameObject currentTile = figureController.currentTile;
                TileController currentTileController = currentTile.GetComponent<TileController>();

                int currentLevel = currentTileController.currentLevel;
                GameObject[] neighbouringTiles = currentTileController.neighbouringTiles;

                for (int j = 0; j < neighbouringTiles.Length; j++)
                {
                    GameObject potentialTileToMoveOn = neighbouringTiles[j];
                    TileController potentialTileController = potentialTileToMoveOn.GetComponent<TileController>();

                    int levelToMoveOn = potentialTileController.currentLevel;
                    bool isPotentialTileOccupied = potentialTileController.isOccupied;

                    if ((isPotentialTileOccupied == true || levelToMoveOn - currentLevel > 1) == false)
                        isGameOver = false;
                }
            }

            if (isGameOver == true)
                return players[1 - k]; //vraća ime igrača koji je pobedio
        }

        return null; //znači da nijedan igrač još nije izgubio
    }


    static public void UpdateGameState()
    {
        GameController.ChangeWhoIsOnMove();
        GameController.ChangeState();       
        GameController.SetLastMovedFigure();
    }


    static public string GetFigureName()
    {
        int whereToSplit = currentPlayer.name.IndexOf('-') + 1;
        return currentPlayer.name.Substring(whereToSplit);
    }


    static public void SetLastMovedFigure()
    {
        lastMovedFigure = GetFigureName();

        Debug.Log("Last moved is " + lastMovedFigure + "!");
    }


    static public void ChangeWhoIsOnMove()
    {
        if (IsTimeToChangeTurn() == true)
        {
            onMove = (Turn)(1 - (int)onMove);
        }

        onTheMove.text = "Now is " + onMove.ToString() + " on the move!";
    }


    static public void ChangeState()
    {
        if (currentState == States.BuildSecond)
        {
            currentState = States.MoveFirst;
        }
        else
        {
            currentState = (States)((int)currentState + 1);
        }

        Debug.Log("New state is " + currentState.ToString() + "!");
    }


    static public bool IsBuilding()
    {
        return currentState == States.BuildFirst || currentState == States.BuildSecond;
    }


    static public bool IsMoving()
    {
        return currentState == States.StartupFirstFirst  || currentState == States.StartupFirstSecond  ||
               currentState == States.StartupSecondFirst || currentState == States.StartupSecondSecond ||
               currentState == States.MoveFirst          || currentState == States.MoveSecond;
    }


    static public bool IsSettingUp()
    {
        return currentState == States.StartupFirstFirst || currentState == States.StartupFirstSecond ||
               currentState == States.StartupSecondFirst || currentState == States.StartupSecondSecond;
    }


    static public bool IsSettingUpSecond()
    {
        return currentState == States.StartupFirstSecond || currentState == States.StartupSecondSecond;
    }


    static public bool IsMovingAfterSetup()
    {
        return currentState == States.MoveFirst || currentState == States.MoveSecond;
    }


    static public bool IsTimeToChangeTurn()
    {
        return currentState == States.StartupFirstSecond || currentState == States.StartupSecondSecond ||
               currentState == States.BuildFirst         || currentState == States.BuildSecond;
    }
}
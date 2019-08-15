using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    const int   MAX_LEVEL     = 4;
    const int   WINNING_LEVEL = 3;
    const float HEIGHT        = 0.1f;

    public GameObject Plank;
    public GameObject Roof;
    public GameObject currentTile;
    public int        currentHeight;

    void Start()
    {
        currentHeight = 0;
    }


    public void OnMouseUp()
    {
        //ne treba da reaguje na klik ukoliko igraju AIVsAI ili ukoliko igraju HumanVsAI, a human nije na potezu
        if (GameModeController.IsAIVsAI() == true)
        {
            return;
        }

        GameController.currentPlayer = gameObject;
    }


    public void Move(GameObject tileToMoveOn)
    {
        if (currentTile != null)
        {
            TileController currentTileController = currentTile.GetComponent<TileController>();
            currentTileController.isOccupied = false;
        }
 
        currentTile = tileToMoveOn;
        TileController nextTileController = currentTile.GetComponent<TileController>();
        nextTileController.isOccupied = true;

        int levelToMoveOn = nextTileController.currentLevel;

        Vector3 nextPosition = tileToMoveOn.transform.position;
        nextPosition.y = HEIGHT * levelToMoveOn;

        gameObject.transform.position = nextPosition;

        //upisivanje u fajl treba da se vrši ovde


        //nakon pomeranja na plank koji se nalazi na trećem nivou igrač je odneo pobedu
        if (levelToMoveOn == WINNING_LEVEL)
            Debug.Log(gameObject.tag + " has won!");

        //proverava da li je igra završena tako što su obe figure jednog od igrača blokirane ili tako što se figura igrača popela na treći nivo
        if (GameController.IsGameOver() == true || levelToMoveOn == WINNING_LEVEL)
        {
            if (levelToMoveOn == WINNING_LEVEL)
                GameModeController.winner = gameObject.tag;

            SceneManager.LoadScene("_GAME_OVER_");
        }


        States prevState = GameController.currentState;

        GameController.UpdateGameState();

        bool calledFromMove = false;
        AIController.CheckIfOnMove(calledFromMove, prevState);
    }


    public void Build(GameObject tileToBuildOn)
    {
        TileController tileController = tileToBuildOn.GetComponent<TileController>();
        int levelToBuildOn = tileController.currentLevel;

        if (levelToBuildOn < MAX_LEVEL)
            tileController.currentLevel++;

        Vector3 nextPosition = tileToBuildOn.transform.position;
        nextPosition.y = HEIGHT * levelToBuildOn;

        GameObject toBuild;
        if (levelToBuildOn < MAX_LEVEL - 1)
            toBuild = Plank;
        else
            toBuild = Roof;

        Instantiate(toBuild, nextPosition, Quaternion.identity, tileToBuildOn.transform);

        //upisivanje u fajl treba da se vrši ovde


        //provera da li je igra završena tako što su obe figure jednog od igrača blokirane
        if (GameController.IsGameOver() == true)
        {
            SceneManager.LoadScene("_GAME_OVER_");
        }


        States prevState = GameController.currentState;

        GameController.UpdateGameState();

        bool calledFromBuild = true;
        AIController.CheckIfOnMove(calledFromBuild, prevState);
    }
}

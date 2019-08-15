using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{

    const int MAX_LEVEL = 4;

    public GameObject[] neighbouringTiles; //initialized in editor
    public bool         isOccupied;        //default value for bool is false
    public int          currentLevel;      //set in start

    void Start()
    {
        currentLevel = 0;
    }


    public void OnMouseUp()
    {
        //ne treba da reaguje na klik ukoliko igraju AIVsAI ili ukoliko igraju HumanVsAI, a human nije na potezu
        if (GameModeController.IsAIVsAI())
        {
            return;
        }
        

        GameObject currentPlayer = GameController.currentPlayer;

        if (currentPlayer == null)
        {
            //do ove situacije dolazi kada nijedna od figura nije izabrana

            return;
        }


        PlayerController playerController = currentPlayer.GetComponent<PlayerController>();


        if (GameController.IsMoving())
        {
            if (IsMovingPossible() == true)
                playerController.Move(gameObject);
            else
                return;
        }
        else if (GameController.IsBuilding())
        {
            if (IsBuildingPossible() == true)
                playerController.Build(gameObject);
            else
                return;
        }
    }


    public bool IsBuildingPossible()
    {
        GameObject currentPlayer = GameController.currentPlayer;

        if (currentPlayer == null)
        {
            //do ove situacije dolazi kada nijedna od figura nije izabrana

            return false;
        }

        if (GameController.onMove.ToString() != currentPlayer.tag)
        {
            //izabrali smo figuru igrača koji nije na potezu i stoga ne vršimo nikakvu akciju

            Debug.Log(currentPlayer.tag + " is not on the move!");

            return false;
        }

        if (GameController.GetFigureName() != GameController.lastMovedFigure)
        {
            Debug.Log("You must proceed with picking a tile next to the figure you moved last!");

            return false;
        }

        if (isOccupied == true)
        {
            //ukoliko se na polju na koje želimo da premestimo figuru već nalazi figura ovaj potez je nemoguće odigrati

            Debug.Log("The tile you want to build on is occupied!");

            return false;
        }

        if (IsOnNeighbouringTile(currentPlayer) == false)
        {
            Debug.Log("The tile you want to build on is not a neighbouring tile!");

            return false;
        }

        if (IsRoofBuilt() == true)
        {
            //ne možemo da gradimo ništa na polju na kom je izgrađen krov

            Debug.Log("The tile you want to build on has a roof!");

            return false;
        }

        return true;
    }


    public bool IsMovingPossible()
    {
        GameObject       currentPlayer    = GameController.currentPlayer;

        if (currentPlayer == null)
        {
            //do ove situacije dolazi kada nijedna od figura nije izabrana

            return false;
        }

        PlayerController playerController = currentPlayer.GetComponent<PlayerController>();

        if (GameController.onMove.ToString() != currentPlayer.tag)
        {
            //izabrali smo figuru igrača koji nije na potezu i stoga ne vršimo nikakvu akciju

            Debug.Log(currentPlayer.tag + " is not on the move!");

            return false;
        }

        if (GameController.IsSettingUpSecond() == true)
        {
            //u ovim stanjima ne možemo da pomerimo istu figuru dva puta za redom, već je nužno da obe postavimo na tablu

            if (GameController.GetFigureName() == GameController.lastMovedFigure)
            {
                Debug.Log("You must set the other figure as well!");

                return false;
            }
        }

        if (GameController.IsSettingUp() == false)
        {
            //u trenucima kada tek postavljamo figure na tablu, možemo ih postaviti na bilo koje polje koje nije zauzeto,
            //tako da nema potrebe da proveravamo da li je polje na kojoj se nalazi figura susedno sa poljem na koje želimo
            //da premestimo figuru u toj situaciji

            //ukoliko se nalazimo u ostalim stanjima
            //treba da proverimo da li je polje na kojoj se player nalazi u neighbouring tiles

            if (IsOnNeighbouringTile(currentPlayer) == false)
            {
                Debug.Log("The tile you want to move your figure to is not a neighbouring tile!");

                return false;
            }
        }

        if (isOccupied == true)
        {
            //ukoliko se na polju na koje želimo da premestimo figuru već nalazi figura ovaj potez je nemoguće odigrati

            Debug.Log("The tile you want to move your figure to is occupied!");

            return false;
        }

        //uslov koji se nalazi u ovom if-u je potrebno proveravati samo nakon setup-a, pre setup-a figure nemaju dodeljena polja,
        //jer se ne nalaze ni na jednom
        if (GameController.IsMovingAfterSetup() == true)
        {
            //currentLevel predstavlja nivo polja koje smo kliknuli s namerom da se na njega pomerimo, pošto se u ovom trenutku nalazimo
            //u kontroleru tog polja
            int currentTileLevel = playerController.currentTile.GetComponent<TileController>().currentLevel;

            if (IsRoofBuilt() == true || currentLevel - currentTileLevel > 1)
            {
                //može da se pomeri na polje koje je najviše za jedan nivo niže ili više od polja na kojem se figura trenutno nalazi,
                //takođe ne može da se pomeri na polje na kom se nalazi kupola (krov)

                Debug.Log("The tile you want to move on is too steep!");

                return false;
            }
        }

        return true;
    }


    bool IsOnNeighbouringTile(GameObject currentPlayer)
    {
        PlayerController playerController = currentPlayer.GetComponent<PlayerController>();

        for (int i = 0; i < neighbouringTiles.Length; i++)
        {
            if (neighbouringTiles[i] == playerController.currentTile)
            {
                return true;
            }
        }

        return false;
    }


    bool IsRoofBuilt()
    {
        if (currentLevel >= MAX_LEVEL)
            return true;
        else
            return false;
    }
}

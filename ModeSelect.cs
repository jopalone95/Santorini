using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModeSelect : MonoBehaviour
{
    void Start()
    {
        GameObject winnerDisplayGO = GameObject.Find("Winner");

        if (winnerDisplayGO != null)
        {
            Text winnerDisplay = winnerDisplayGO.GetComponent<Text>();

            winnerDisplay.text = GameModeController.winner + " has won!";
        }
    }

    public void ButtonClicked(string buttonText)
    {
        string sceneToLoad = "_SCENE_";

        if (buttonText == "Human vs Human")
        {
            GameModeController.gameMode = GameMode.HumanVsHuman;

            Debug.Log("Set to HumanVsHuman!");
        }
        else if (buttonText == "Human vs AI")
        {
            GameModeController.gameMode = GameMode.HumanVsAI;

            sceneToLoad = "_PLAYER_PICKER_";

            Debug.Log("Set to HumanVsAI!");
        }
        else if (buttonText == "AI vs AI")
        {
            GameModeController.gameMode = GameMode.AIVsAI;

            sceneToLoad = "_AIvsAI_";

            Debug.Log("Set to AIVsAI!");
        }
        else if (buttonText == "Restart")
        {
            sceneToLoad = "_MODE_SELECT_";
        }
        else if (buttonText == "Quit")
        {
            //u editoru se ništa ne dešava, u aplikaciji bi trebalo

            Application.Quit();
        }

        StartCoroutine(LoadYourAsyncScene(sceneToLoad));
    }

    IEnumerator LoadYourAsyncScene(string sceneToLoad)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}

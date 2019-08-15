using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSelect : MonoBehaviour
{
    public void ButtonClick(string huPlayer)
    {
        GameModeController.huPlayer  = huPlayer;
        GameModeController.aiPlayer  = (huPlayer == "Player1" ? "Player2" : "Player1");

        StartCoroutine(LoadYourAsyncScene("_DIF_PICKER_"));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelect : MonoBehaviour
{
    public void SelectDifficulty(string difficulty)
    {
        if (difficulty == "Easy")
        {
            GameModeController.difficulty = Difficulty.Easy;
        }
        else if (difficulty == "Medium")
        {
            GameModeController.difficulty = Difficulty.Medium;
        }
        else if (difficulty == "Hard")
        {
            GameModeController.difficulty = Difficulty.Hard;
        }

        StartCoroutine(LoadYourAsyncScene("_SCENE_"));
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

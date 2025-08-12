using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.HDROutputUtils;

public class loadingScreen : MonoBehaviour
{

    public Text progressText; // Text showing loading percentage (optional)
    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PlayTest");

        asyncLoad.allowSceneActivation = false; // stop the level from activating 

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            // Calculate the progress (ranges from 0.0 to 0.9, so divide by 0.9 for a percentage)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update the progress text (if any)
            if (progressText != null)
            {
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
            }

            yield return null;
            yield return new WaitForSeconds(3);
            asyncLoad.allowSceneActivation = true;
        }
    }
}

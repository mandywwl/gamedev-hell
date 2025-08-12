using UnityEngine;
using UnityEngine.SceneManagement;

public class menuSelection : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        Debug.Log("Start game");
        SceneManager.LoadScene("LoadingScrn");
    }

    public void SettingsGame()
    {
        Debug.Log("Settings");
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
    }
}

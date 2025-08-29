using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class endGame : MonoBehaviour
{
    public TMP_Text winLoseText;

    public Button exitButton;
    public Button restartButton;

    private void Start()
    {
        string result = PlayerPrefs.GetString("BattleResult");
        winLoseText.text = (result == "WON") ? "You won the game!" : "You have died.";
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("LoadingScrn");
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class menuSelection : MonoBehaviour
{
    //Menu States hi
    public enum MenuStates { Main, Settings};
    public MenuStates currentstate;

    //Menu Panel Objects
    public GameObject mainMenu;
    public GameObject settingsMenu;

    //When scipt first starts
    void Awake()
    {
        //Always set first menu to main menu
        currentstate =  MenuStates.Main;
    }

    void Update()
    {
        //checks current menu states
        switch (currentstate)
        {
            case MenuStates.Main:

                //sets active gameobject for main menu
                mainMenu.SetActive(true);
                settingsMenu.SetActive(false);
                break;
            case MenuStates.Settings:

                //sets active gameobject for main menu
                mainMenu.SetActive(false);
                settingsMenu.SetActive(true);
                break;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        Debug.Log("Start game");
        SceneManager.LoadScene("LoadingScrn");
    }

    public void SettingsGame()
    {
        Debug.Log("Settings");

        //Change menu state
        currentstate = MenuStates.Settings;
    }

    public void MainMenu()
    {
        Debug.Log("Back to Main Menu");

        //Change menu state
        currentstate = MenuStates.Main;
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
        Application.Quit();
    }
}

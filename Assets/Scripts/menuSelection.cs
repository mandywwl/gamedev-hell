using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class menuSelection : MonoBehaviour
{
    //Menu States
    public enum MenuStates { Main, Settings, Credits};
    public MenuStates currentstate;

    //Menu Panel Objects
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;

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
                mainMenu.SetActive(true);
                settingsMenu.SetActive(false);
                creditsMenu.SetActive(false);
                break;

            case MenuStates.Settings:
                mainMenu.SetActive(false);
                settingsMenu.SetActive(true);
                creditsMenu.SetActive(false);
                break;

            case MenuStates.Credits:
                mainMenu.SetActive(false);
                settingsMenu.SetActive(false);
                creditsMenu.SetActive(true);
                break;

        }
    }
    
    public void StartGame()
    {
        Debug.Log("Start game");
        SceneManager.LoadScene("LoadingScrn");
    }

    public void SettingsGame()
    {
        Debug.Log("Settings");

        currentstate = MenuStates.Settings;
    }

    public void MainMenu()
    {
        Debug.Log("Back to Main Menu");

        currentstate = MenuStates.Main;
    }

    public void CreditsMenu()
    {
        Debug.Log("Credits");

        currentstate = MenuStates.Credits;
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public bool isPaused; //track pause state

    [SerializeField] public GameObject pausePanel;    
    [SerializeField] public GameObject settingsMenu; 

    void Awake()
    {
        //clean startup
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle pause state on Escape key
            isPaused = !isPaused;
            if (isPaused)
            {
                PauseGame();
            }
            else
            {
                if (settingsMenu.activeSelf)
                    CloseSettings();
                else
                    ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        // Set Time.timeScale to 0 to pause gameplay
        Time.timeScale = 0;
        // Make PauseMenu panel visible (activate its gameObject)
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        // Set Time.timeScale back to 1 to resume gameplay
        Time.timeScale = 1;
        // Hide PauseMenu panel (deactivate its gameObject)
        pausePanel.SetActive(false);
    }

    // Called by the Pause menu “Settings” button
    public void OpenSettings()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(true);
    }

    // Called by the Settings menu “Back” button OR ESC while in settings
    public void CloseSettings()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (settingsMenu) settingsMenu.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
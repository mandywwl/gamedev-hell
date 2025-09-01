using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // --- PUBLIC REFERENCES TO UI PANELS ---
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsMenu;

    public static bool isPaused = false;

    void Awake()
    {
        // Ensure UI is hidden and game is running at the start
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the settings menu is open, Esc should close it. Otherwise, toggle the pause state.
            if (settingsMenu != null && settingsMenu.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePause();
            }
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        
        // Unlock and show the cursor to interact with the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        isPaused = false;

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called by the Pause menu “Settings” button
    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsMenu.SetActive(true);
    }

    // Called by the Settings menu “Back” button
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game..."); // Good for testing in the editor
        Application.Quit();
    }
}
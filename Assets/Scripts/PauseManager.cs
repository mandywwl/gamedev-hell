using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // --- PUBLIC REFERENCES TO UI PANELS ---
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsMenu;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pausePanel   = FindTaggedInScene(scene, "PausePanel");
        settingsMenu = FindTaggedInScene(scene, "SettingsMenu");

        // reset state each time
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel)   pausePanel.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
    }

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
            // Inventory takes priority over Pause on the same keypress - close it instead
            // of also opening Pause on top, and let a later press open Pause on its own.
            if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.IsOpen)
            {
                InventoryUIManager.Instance.CloseInventory();
                return;
            }

            if (GameState.I != null)
            {
                if (GameState.I.CurrentMode == GameState.PlayMode.Exploration)
                {
                    if (settingsMenu != null && settingsMenu.activeSelf)
                        CloseSettings();
                    else
                        TogglePause();
                }
                else if (GameState.I.CurrentMode == GameState.PlayMode.Combat)
                {
                    // In combat: don’t freeze Time.timeScale, just toggle UI
                    if (settingsMenu != null && settingsMenu.activeSelf)
                    {
                        CloseSettings();
                    }
                    else if (pausePanel != null)
                    {
                        bool newState = !pausePanel.activeSelf;
                        pausePanel.SetActive(newState);
                        isPaused = newState;
                    }

                    // Keep cursor visible in combat
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
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
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Unlock and show the cursor to interact with the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        if (pausePanel != null)
            pausePanel.SetActive(false);


        isPaused = false;

        if (GameState.I != null && GameState.I.CurrentMode == GameState.PlayMode.Exploration)
        {
            // Exploration: lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Combat: keep cursor visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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

    // Helper that can find inactive tagged objects inside scene
    private GameObject FindTaggedInScene(Scene scene, string tag)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var trs = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in trs)
                if (t.CompareTag(tag)) return t.gameObject;
        }
        return null;
    }
    
    public void BindUIPanels(GameObject pause, GameObject settings)
    {
        pausePanel = pause;
        settingsMenu = settings;

        if (pausePanel)   pausePanel.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
        isPaused = false;
    }



}
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuBinder : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton; // from Settings menu

    void Awake()
    {
        var pm = FindFirstObjectByType<PauseManager>(); // Unity 6 API
        if (!pm) return;

        if (resumeButton)  { resumeButton.onClick.RemoveAllListeners();  resumeButton.onClick.AddListener(pm.ResumeGame); }
        if (settingsButton){ settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(pm.OpenSettings); }
        if (quitButton)    { quitButton.onClick.RemoveAllListeners();     quitButton.onClick.AddListener(pm.QuitGame); }
        if (backButton)    { backButton.onClick.RemoveAllListeners();     backButton.onClick.AddListener(pm.CloseSettings); }
    }
}

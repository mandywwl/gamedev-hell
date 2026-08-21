using UnityEngine;

public class ScenePlayModeSet : MonoBehaviour
{
    public GameState.PlayMode mode = GameState.PlayMode.Exploration;

    void Start()
    {
        if (GameState.I != null)
            GameState.I.CurrentMode = mode;

        // Optional: cursor defaults
        if (mode == GameState.PlayMode.Exploration)
        { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        else
        { Cursor.lockState = CursorLockMode.None;  Cursor.visible = true; }
    }
}

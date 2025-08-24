using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "NextScene"; // change this in Inspector

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered door trigger!");
            SceneManager.LoadScene(sceneToLoad); // will change scene
        }
    }
}

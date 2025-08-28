using System.Collections;
using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] private AudioClip openChest;

    [Header("Chest Settings")]
    [Tooltip("Whether this chest has already been opened")]
    public bool isOpened = false;

    [Header("Loot Settings")]
    [Tooltip("Loot table name to use when opened")]
    public string lootTableName = "ChestDrop";

    [Header("UI Feedback")]
    [Tooltip("Text to show when player is near")]
    public string interactionPrompt = "Press E to open chest";

    // Components
    private Animator animator;
    private AudioSource audioSource;
    private bool playerInRange = false;

    // Events for UI updates
    public static System.Action<string> OnShowPrompt;
    public static System.Action OnHidePrompt;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Add AudioSource if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Make sure the chest has a trigger collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"Chest {gameObject.name} needs a Collider2D with IsTrigger enabled!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            playerInRange = true;
            OnShowPrompt?.Invoke(interactionPrompt);
            Debug.Log($"Near chest: {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            OnHidePrompt?.Invoke();
        }
    }

    public bool CanInteract()
    {
        return playerInRange && !isOpened;
    }

    public void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;

        // Play chest opening sound
        if (audioSource != null && openChest != null)
        {
            audioSource.PlayOneShot(openChest);
        }

        // Play opening animation
        if (animator != null)
        {
            animator.SetTrigger("Open");
            animator.SetBool("IsOpen", true);
        }

        // Give loot directly to player inventory (NOT drop on ground)
        if (LootSystem.Instance != null)
        {
            LootSystem.Instance.GiveLootToPlayer(lootTableName);
            Debug.Log($"Opened {gameObject.name} and gave loot from {lootTableName} to player inventory");
        }
        else
        {
            Debug.LogWarning("LootSystem.Instance not found! Make sure LootSystem is in the scene.");
        }

        // Hide interaction prompt
        OnHidePrompt?.Invoke();
    }
}
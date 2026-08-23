using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] private AudioClip openChest;

    [Header("Chest Settings")]
    public bool isOpened = false;

    // internal
    private Animator animator;
    private bool playerInRange = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (GetComponent<Collider2D>() == null)
            Debug.LogWarning($"Chest {name} needs a Collider2D set as Trigger.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    public bool CanInteract() => playerInRange && !isOpened;

    public void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;

        // sound
        if (openChest != null) AudioManager.I.PlaySFX(openChest);

        // anim
        if (animator != null)
        {
            animator.SetTrigger("Open");
            animator.SetBool("IsOpen", true);
        }

        // give a random item from the loot table and show what the player got
        if (LootSystem.Instance != null)
        {
            var (item, quantity) = LootSystem.Instance.GiveRandomLoot();
            if (LootPopupUI.Instance != null) LootPopupUI.Instance.ShowLoot(item, quantity);
        }
        else Debug.LogWarning("LootSystem not found in scene — cannot give loot.");
    }
}

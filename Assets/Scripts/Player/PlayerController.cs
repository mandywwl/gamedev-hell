using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    [Tooltip("Units per second. Retro games often feel good between 2.2 and 3.2 at PPU=64.")]
    [SerializeField] private float moveSpeed = 2.6f;

    [Tooltip("Snap WASD to 8 directions (no analog drift). Keep ON for retro feel.")]
    [SerializeField] private bool snapToEightDirections = true;

    [Tooltip("Input below this magnitude is treated as 0 (prevents accidental drift).")]
    [SerializeField, Range(0f, 0.4f)] private float inputDeadzone = 0.15f;
    
    [Header("Optional Softness")]
    [Tooltip("If > 0, applies a tiny bit of smoothing (0 = instant).")]
    [SerializeField, Range(0f, 20f)] private float soften = 0f;

    [Header("Animator / Isometric")]
    [Tooltip("Rotate Animator inputs by -45° if sprites are authored to iso tile axes.")]
    [SerializeField] private bool rotateAnimatorInputBy45 = false;

    // [Header("Rendering")]
    // [SerializeField] private int sortingMultiplier = 1; // TODO: tweak

    [Header("Interaction")]
    [Tooltip("Range to detect interactable objects like chests")]
    [SerializeField] private float interactionRange = 1.5f;
    [Tooltip("Layer mask for interactable objects")]
    [SerializeField] private LayerMask interactableLayer = -1;

    [Header("Inventory")]
    [Tooltip("Reference to the Inventory UI Manager (optional - will auto-find if not assigned)")]
    [SerializeField] private InventoryUIManager inventoryUIManager;
    

    // cached
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    // state
    private Vector2 rawInput;
    private Vector2 snappedInput;     // 8-dir snapped input
    private Vector2 desiredVelocity;  // moveSpeed * snappedInput
    private Vector2 smoothVelocity;   // used only if soften > 0
    private Vector2 lastLookDir = Vector2.down;

    // sanity movement scaling (1 = normal speed)
    [SerializeField, Range(0.25f, 1f)] private float sanitySpeedFactor = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // 2D top-down default
        rb.gravityScale   = 0f;
        rb.interpolation  = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true; // avoid accidental torque/rotation
    }

    // Interactions
    private ChestInteraction nearbyChest;

    void HandleMovement()
    {
        // Read input (legacy Input Manager)
        rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Deadzone + normalize to avoid diagonal speed boost
        if (rawInput.magnitude < inputDeadzone) rawInput = Vector2.zero;
        else rawInput = rawInput.normalized;

        // Snap to 8 directions
        snappedInput = snapToEightDirections ? SnapTo8(rawInput) : rawInput;

        // Compute desired velocity
        desiredVelocity = snappedInput * moveSpeed * sanitySpeedFactor;

        // Softness (optional); 0 = instant
        if (soften > 0f)
            smoothVelocity = Vector2.MoveTowards(smoothVelocity, desiredVelocity, soften * Time.deltaTime);
        else
            smoothVelocity = desiredVelocity;

        // Drive Animator
        Vector2 dirForAnim = snappedInput.sqrMagnitude > 0f ? snappedInput : lastLookDir;
        if (dirForAnim.sqrMagnitude > 0.0001f) lastLookDir = dirForAnim;

        if (rotateAnimatorInputBy45 && dirForAnim.sqrMagnitude > 0f)
            dirForAnim = Rotate(dirForAnim, -45f * Mathf.Deg2Rad);

        anim.SetFloat("MoveX", dirForAnim.x);
        anim.SetFloat("MoveY", dirForAnim.y);
        anim.SetFloat("Speed", snappedInput.sqrMagnitude > 0f ? 1f : 0f); 
        
    }
    
    void FixedUpdate()
    {
        // MovePosition = deterministic
        Vector2 next = rb.position + smoothVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    void Start()
    {
        // Auto-find inventory UI manager if not assigned
        if (inventoryUIManager == null)
        {
            inventoryUIManager = FindFirstObjectByType<InventoryUIManager>(); // unity 6 update
            if (inventoryUIManager == null)
            {
                Debug.LogWarning("InventoryUIManager not found in scene. Inventory functionality will not work.");
            }
        }
    }
    
    void Update()
    {
        HandleMovement();
        HandleInteraction();
        HandleInventoryInput();
    }

    // --- Helpers ---
    private static Vector2 SnapTo8(Vector2 v)
    {
        if (v.sqrMagnitude == 0f) return Vector2.zero;

        // angle in radians and snap to 45° steps
        float a = Mathf.Atan2(v.y, v.x);
        float step = Mathf.PI / 4f;                    // 45°
        float snapped = Mathf.Round(a / step) * step;  // nearest 45°
        return new Vector2(Mathf.Cos(snapped), Mathf.Sin(snapped));
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithChest();
        }

        // Update nearby chest detection
        UpdateNearbyChest();
    }

    private void HandleInventoryInput()
    {
        // Handle inventory toggle with "I" key
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryUIManager != null)
        {
            inventoryUIManager.ToggleInventory();
        }
        else
        {
            Debug.LogWarning("Cannot open inventory - InventoryUIManager not found!");
        }
    }

    // Public method to open inventory (can be called from other scripts)
    public void OpenInventory()
    {
        if (inventoryUIManager != null)
        {
            inventoryUIManager.OpenInventory();
        }
    }

    // Public method to close inventory (can be called from other scripts)
    public void CloseInventory()
    {
        if (inventoryUIManager != null)
        {
            inventoryUIManager.CloseInventory();
        }
    }

    private void TryInteractWithChest()
    {
        if (nearbyChest != null && nearbyChest.CanInteract())
        {
            nearbyChest.OpenChest();
            Debug.Log($"Interacted with chest: {nearbyChest.gameObject.name}");
        }
        else
        {
            Debug.Log("No chest to interact with nearby");
        }
    }

    private void UpdateNearbyChest()
    {
        // Find the closest chest within interaction range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

        ChestInteraction closestChest = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            ChestInteraction chest = collider.GetComponent<ChestInteraction>();
            if (chest != null && chest.CanInteract())
            {
                float distance = Vector2.Distance(transform.position, chest.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestChest = chest;
                }
            }
        }

        nearbyChest = closestChest;
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    public void SetSanitySpeedFactor(float factor)
    {
        sanitySpeedFactor = Mathf.Clamp(factor, 0.25f, 1f);
    }
}

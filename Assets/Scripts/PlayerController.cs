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

    [Header("Rendering")]
    [SerializeField] private int sortingMultiplier = 1; // TODO: tweak
    
    // cached
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    // state
     private Vector2 rawInput;
    private Vector2 snappedInput;     // 8-dir (or raw if snapping off)
    private Vector2 desiredVelocity;  // moveSpeed * snappedInput
    private Vector2 smoothVelocity;   // used only if soften > 0
    private Vector2 lastLookDir = Vector2.down;

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

    void Update()
    {
        // Read input (legacy Input Manager)
        rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Deadzone + normalize to avoid diagonal speed boost
        if (rawInput.magnitude < inputDeadzone) rawInput = Vector2.zero;
        else rawInput = rawInput.normalized;

        // Snap to 8 directions
        snappedInput = snapToEightDirections ? SnapTo8(rawInput) : rawInput;

        // Compute desired velocity
        desiredVelocity = snappedInput * moveSpeed;

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

        // Y-sorting by feet ---
        sr.sortingOrder = -(int)(transform.position.y * sortingMultiplier);
        
    }
    
    void FixedUpdate()
    {
        // MovePosition = deterministic
        Vector2 next = rb.position + smoothVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
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

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }


}

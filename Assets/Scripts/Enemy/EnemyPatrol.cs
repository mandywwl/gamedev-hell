using UnityEngine;

// Walks the enemy back and forth along a patrol route, and starts combat (via BossEncounter)
// if the player enters its vision cone - on top of the existing touch trigger on BossEncounter's
// own collider. Put this on the same GameObject as BossEncounter.
[RequireComponent(typeof(BossEncounter))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [Tooltip("Points to walk between, in order, looping back to the first. Leave empty to auto-patrol left/right of the starting position.")]
    public Transform[] patrolPoints;
    [Tooltip("Only used when patrolPoints is empty: how far to walk from the starting position before turning around.")]
    public float autoPatrolDistance = 3f;
    public float moveSpeed = 1.5f;
    [Tooltip("Seconds to pause at each patrol point before moving to the next.")]
    public float waitTimeAtPoint = 1f;

    [Header("Vision Detection")]
    [Tooltip("How far the enemy can spot the player while facing them.")]
    public float visionRange = 4f;
    [Tooltip("Full width of the vision cone, in degrees.")]
    public float visionAngle = 60f;
    [Tooltip("Layers that block line of sight (walls, obstacles). Leave empty if you don't have any yet.")]
    public LayerMask obstructionMask;

    [Header("References")]
    [Tooltip("Auto-found via the 'Player' tag if left empty.")]
    [SerializeField] private Transform player;

    [Header("Sprite Flipping")]
    [Tooltip("Flips the sprite horizontally to face its movement direction. Turn off if your sprite art doesn't work with a simple mirror flip.")]
    public bool flipSpriteToFaceMovement = true;
    [Tooltip("Check this if your sprite's art faces left by default (flips the flip).")]
    public bool spriteFacesLeftByDefault = false;

    private BossEncounter bossEncounter;
    private SpriteRenderer spriteRenderer;
    private Vector2[] waypoints;
    private int currentPointIndex;
    private float waitTimer;
    private Vector2 facingDirection = Vector2.down;

    void Awake()
    {
        bossEncounter = GetComponent<BossEncounter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (player == null)
        {
            // Prefer the object that actually carries a PlayerController: a scene can hold more
            // than one object tagged "Player" (a mis-tagged trigger volume, say), and
            // FindGameObjectWithTag would then hand back an arbitrary one of them.
            var controller = FindFirstObjectByType<PlayerController>();
            if (controller != null)
            {
                player = controller.transform;
            }
            else
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
            }
        }

        BuildWaypoints();
    }

    private void BuildWaypoints()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            waypoints = new Vector2[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
                waypoints[i] = patrolPoints[i].position;
            return;
        }

        Vector2 start = transform.position;
        waypoints = new Vector2[]
        {
            start - Vector2.right * autoPatrolDistance,
            start + Vector2.right * autoPatrolDistance,
        };
    }

    void Update()
    {
        Patrol();

        if (player != null && CanSeePlayer())
            bossEncounter.StartCombat(player.position);
    }

    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector2 target = waypoints[currentPointIndex];
        Vector2 toTarget = target - (Vector2)transform.position;

        if (toTarget.magnitude <= 0.05f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                waitTimer = 0f;
                currentPointIndex = (currentPointIndex + 1) % waypoints.Length;
            }
            return;
        }

        facingDirection = toTarget.normalized;
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        UpdateSpriteFlip();
    }

    private void UpdateSpriteFlip()
    {
        if (!flipSpriteToFaceMovement || spriteRenderer == null) return;
        if (Mathf.Abs(facingDirection.x) < 0.01f) return; // moving straight up/down - keep current facing

        bool movingLeft = facingDirection.x < 0f;
        spriteRenderer.flipX = movingLeft != spriteFacesLeftByDefault;
    }

    private bool CanSeePlayer()
    {
        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;
        if (distance > visionRange) return false;

        float angle = Vector2.Angle(facingDirection, toPlayer);
        if (angle > visionAngle * 0.5f) return false;

        // A wall/obstacle between the enemy and the player blocks the sighting - but a hit on
        // the player themselves (the ray's own target, if their layer is in obstructionMask
        // too) must not count as an obstruction, or vision would never succeed.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, obstructionMask);
        if (hit.collider != null && hit.transform != player) return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 forward = facingDirection.sqrMagnitude > 0f ? (Vector3)facingDirection : Vector3.down;
        Quaternion leftRot = Quaternion.Euler(0, 0, visionAngle * 0.5f);
        Quaternion rightRot = Quaternion.Euler(0, 0, -visionAngle * 0.5f);
        Gizmos.DrawRay(transform.position, leftRot * forward * visionRange);
        Gizmos.DrawRay(transform.position, rightRot * forward * visionRange);
    }
}

using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public StateMachine StateMachine { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public AIAnimationController aiAnimationController { get; private set; }
    // public Animator Animator { get; private set; } // Not needed since we're not using animations
    public Transform[] Waypoints;
    public Transform Player;

    public float AttackRange = 2f; // New attack range variable
    public LayerMask PlayerLayer;
    public StateType currentState;


    [Header("Vision Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public float eyeHeight = 1.6f; // where the AI "looks" from
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Vision Stability")]
    public float visionPersistence = 0.5f; // seconds to keep seeing after losing sight
    private float lastSeenTime = -999f;

    private bool debugVision = true; // toggle to enable/disable debug

    public bool CanSeePlayer()
{
    if (Player == null)
    {
        if (debugVision)
            Debug.LogWarning($"{name}: ❌ Player reference is missing!");
        return false;
    }

    Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
    Vector3 targetPosition = Player.position + Vector3.up * 0.5f;
    Vector3 directionToPlayer = (targetPosition - eyePosition).normalized;
    float distanceToPlayer = Vector3.Distance(eyePosition, targetPosition);
    float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

    // 🟡 Draw vision ray
    if (debugVision)
    {
        Color rayColor = Color.red;
        if (angleToPlayer <= viewAngle / 2f && distanceToPlayer <= viewDistance)
            rayColor = Color.yellow; // in FOV range
        if (Time.time - lastSeenTime < visionPersistence)
            rayColor = Color.green; // recently seen
        Debug.DrawLine(eyePosition, targetPosition, rayColor, 0.1f);
    }

    // Check field of view
    if (angleToPlayer > viewAngle / 2f)
    {
        if (debugVision)
            Debug.Log($"{name}: 🚫 Player out of view angle ({angleToPlayer:F1}° > {viewAngle / 2f}°)");
        return Time.time - lastSeenTime < visionPersistence;
    }

    // Check distance
    if (distanceToPlayer > viewDistance)
    {
        if (debugVision)
            Debug.Log($"{name}: 🚫 Player too far ({distanceToPlayer:F2}m > {viewDistance:F2}m)");
        return Time.time - lastSeenTime < visionPersistence;
    }

    // Perform raycast
    if (Physics.Raycast(eyePosition, directionToPlayer, out RaycastHit hit, viewDistance))
    {
        // 🧠 Log detailed hit info
        if (debugVision)
        {
            string hitInfo = hit.transform != null
                ? $"{name}: 🧱 Hit '{hit.transform.name}' at distance {hit.distance:F2}m"
                : $"{name}: Raycast hit NOTHING";

            Debug.Log(hitInfo);

            // Color the ray based on what it hit
            Color hitColor = hit.transform == Player ? Color.green : Color.magenta;
            Debug.DrawLine(eyePosition, hit.point, hitColor, 0.1f);
        }

        // If hit the player
        if (hit.transform == Player)
        {
            lastSeenTime = Time.time;
            if (debugVision)
                Debug.Log($"{name}: ✅ Player visible! (direct line of sight)");
            return true;
        }
        else
        {
            if (debugVision)
                Debug.Log($"{name}: ⛔ Vision blocked by {hit.transform.name}");
        }
    }
    else if (debugVision)
    {
        Debug.Log($"{name}: 🕳️ No hit detected — ray reached max distance ({viewDistance:F2}m)");
    }

    // If recently seen, still count as visible
    bool recentlySeen = Time.time - lastSeenTime < visionPersistence;
    if (recentlySeen && debugVision)
        Debug.Log($"{name}: 👁️ Remembering player (persistence active)");

    return recentlySeen;
}

    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        aiAnimationController = GetComponent<AIAnimationController>();
        // Animator = GetComponent<Animator>(); // Commented out since we're not using animations

        StateMachine = new StateMachine();
        StateMachine.AddState(new IdleState(this));
        StateMachine.AddState(new PatrolState(this));
        StateMachine.AddState(new ChaseState(this));
        StateMachine.AddState(new AttackState(this)); // Add the new AttackState

        StateMachine.TransitionToState(StateType.Idle);
    }

    void Update()
    {
        StateMachine.Update();
        currentState = StateMachine.GetCurrentStateType();
    }


    // New method to check if the AI is within attack range
    public bool IsPlayerInAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        return distanceToPlayer <= AttackRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
}

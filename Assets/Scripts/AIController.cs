using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }
    public AIAnimationController aiAnimationController { get; private set; }
    // public Animator Animator { get; private set; } // Not needed since we're not using animations
    public Transform[] Waypoints;
    public Transform Player;

    public float AttackRange = 2f; // New attack range variable
    public LayerMask PlayerLayer;
    public StateType currentState;

    [Header("Attack Settings")]
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    [Header("Vision Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public float eyeHeight = 1.6f; // where the AI "looks" from
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Vision Stability")]
    public float visionPersistence = 0.5f; // seconds to keep seeing after losing sight
    private float lastSeenTime = -999f;


    // Add State Machine code Here


    
    // 
    public bool CanSeePlayer()
    {
        if (Player == null)
        {
            return false;
        }

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = Player.position + Vector3.up * 0.5f;
        Vector3 directionToPlayer = (targetPosition - eyePosition).normalized;
        float distanceToPlayer = Vector3.Distance(eyePosition, targetPosition);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check field of view
        if (angleToPlayer > viewAngle / 2f)
        {
            return Time.time - lastSeenTime < visionPersistence;
        }

        // Check distance
        if (distanceToPlayer > viewDistance)
        {
            return Time.time - lastSeenTime < visionPersistence;
        }

        // Perform raycast
        if (Physics.Raycast(eyePosition, directionToPlayer, out RaycastHit hit, viewDistance))
        {
            // If hit the player
            if (hit.transform == Player)
            {
                lastSeenTime = Time.time;
                return true;
            }
        }
    

        // If recently seen, still count as visible
        bool recentlySeen = Time.time - lastSeenTime < visionPersistence;

        return recentlySeen;
    }

    public bool CheckHandsCollision(out GameObject collidedObject, string Tag)
    {
        // You can define these in AIController (leftHandPoint, rightHandPoint)
        Transform[] handTransforms = { leftHandTransform, rightHandTransform };

        foreach (Transform hand in handTransforms)
        {
            // Overlap check — sphere or capsule works well for melee hitboxes
            Collider[] hits = Physics.OverlapSphere(hand.position, 0.5f, PlayerLayer);

            foreach (var hit in hits)
            {
                if (hit.CompareTag(Tag))
                {
                    collidedObject = hit.gameObject;
                    return true;
                }
            }
        }
        collidedObject = null;
        return false;
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

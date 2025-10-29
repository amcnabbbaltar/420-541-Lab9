using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class AIAnimationController : MonoBehaviour
{
    public  Animator animator { get; private set; }
    private NavMeshAgent agent;


    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Update animation parameters
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        float speed = agent != null ? agent.velocity.magnitude : 0f;
        animator.SetFloat("CharacterSpeed", speed);
    }

}

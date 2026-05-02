using UnityEngine;
using UnityEngine.AI;

public class DogAnimation : MonoBehaviour
{
    public Animator dogAnimator;
    public DogAI dogAI;
    public NavMeshAgent agent;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Use actual movement speed instead of state for smoother animation
        float speed = agent != null ? agent.velocity.magnitude : 0f;

        bool isCaught = dogAI.state == DogAI.dogState.playerCaught;
        bool isMovingFast = speed > dogAI.chaseSpeed * 0.5f;
        bool isMovingSlow = speed > 0.3f;

        if (isCaught)
        {
            dogAnimator.SetBool("IsIdle", false);
            dogAnimator.SetBool("IsWalking", false);
            dogAnimator.SetBool("IsRunning", false);
            dogAnimator.SetBool("CaughtPlayer", true);
        }
        else if (isMovingFast)
        {
            dogAnimator.SetBool("IsIdle", false);
            dogAnimator.SetBool("IsWalking", false);
            dogAnimator.SetBool("IsRunning", true);
            dogAnimator.SetBool("CaughtPlayer", false);
        }
        else if (isMovingSlow)
        {
            dogAnimator.SetBool("IsIdle", false);
            dogAnimator.SetBool("IsWalking", true);
            dogAnimator.SetBool("IsRunning", false);
            dogAnimator.SetBool("CaughtPlayer", false);
        }
        else
        {
            dogAnimator.SetBool("IsIdle", true);
            dogAnimator.SetBool("IsWalking", false);
            dogAnimator.SetBool("IsRunning", false);
            dogAnimator.SetBool("CaughtPlayer", false);
        }
    }
}
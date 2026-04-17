using UnityEngine;
[RequireComponent(typeof(Animator))]

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private bool wasGrounded;
    private float idleTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();

        // PlayerController is on the parent (Player object)
        playerController = GetComponentInParent<PlayerController>();

        if(playerController != null)
        {
            wasGrounded = playerController.isGrounded;
        }
    }

    void Update()
    {
        if (playerController == null || animator == null) return;

        // Speed: 0 when idle, varies when moving
        float speed = playerController.isMoving ? (playerController.isRunning ? 1f : 0.5f) : 0f;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsRunning", playerController.isRunning);
        animator.SetBool("IsGrounded", playerController.isGrounded);

        if (speed <= 0.01f && playerController.isGrounded)
        {
            idleTimer += Time.deltaTime;
        }
        else
        {
            idleTimer = 0f;
        }
        animator.SetFloat("IdleTimer", idleTimer);

        if (wasGrounded && !playerController.isGrounded)
        {
            animator.SetTrigger("Jump");
            idleTimer = 0f;
            animator.SetFloat("IdleTimer", idleTimer);
        }

        wasGrounded = playerController.isGrounded;
    }
}

using UnityEngine;
[RequireComponent(typeof(Animator))]

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private bool wasGrounded;
    private bool wasJumping;
    private float idleTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();

        // PlayerController is on the parent (Player object)
        playerController = GetComponentInParent<PlayerController>();

        if(playerController != null)
        {
            wasGrounded = playerController.isGrounded;
            wasJumping = playerController.isJumping;
        }
    }

    public void TriggerDeath()
    {
        animator.SetTrigger("Death");
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

        if (!wasJumping && playerController.isJumping)
        {
            animator.SetTrigger("Jump");
            idleTimer = 0f;
            animator.SetFloat("IdleTimer", idleTimer);
        }

        wasGrounded = playerController.isGrounded;
        wasJumping = playerController.isJumping;
    }

    public void TriggerRespawn()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Death");
            animator.SetTrigger("Respawn");
            idleTimer = 0f;
            animator.SetFloat("IdleTimer", idleTimer);
        }
    }

    public void TriggerSearch()
    {
        if (animator != null)
        {
            animator.SetTrigger("Search");
            idleTimer = 0f;
            animator.SetFloat("IdleTimer", idleTimer);
        }
    }
}

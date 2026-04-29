using UnityEngine;
[RequireComponent(typeof(Animator))]

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private bool wasGrounded;
    private bool wasJumping;
    private float idleTimer = 0f;
    private float sprintTimer = 0f;
    [SerializeField] private float gallopDelay = 0.5f;

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
        float speed = 0f;
        float moveBlend = 0f;

        if (!playerController.isMoving)
        {
            sprintTimer = 0f;
            speed = 0f;
            moveBlend = 0f;
        }
        else if (!playerController.isRunning)
        {
            sprintTimer = 0f;
            speed = 0.5f;
            moveBlend = 0.5f;
        }
        else
        {
            sprintTimer += Time.deltaTime;

            speed = 1f;

            if (sprintTimer < gallopDelay)
                moveBlend = 1f;     // trot
            else
                moveBlend = 1.5f;   // gallop
        }

        animator.SetFloat("Speed", speed);
        animator.SetFloat("MoveBlend", moveBlend);
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

    public void TriggerPickup()
    {
        if (animator != null)
        {
            animator.SetTrigger("KeyPickup");
            idleTimer = 0f;
            animator.SetFloat("IdleTimer", idleTimer);
        }
    }
}

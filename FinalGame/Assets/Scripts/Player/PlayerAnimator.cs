using UnityEngine;
[RequireComponent(typeof(Animator))]

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    void Start()
    {
        animator = GetComponent<Animator>();

        // PlayerController is on the parent (Player object)
        playerController = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (playerController == null || animator == null) return;

        // Speed: 0 when idle, varies when moving
        float speed = playerController.isMoving ? (playerController.isRunning ? 1f : 0.5f) : 0f;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsRunning", playerController.isRunning);
        animator.SetBool("IsGrounded", playerController.isGrounded);

        if (playerController.isJumping)
        {
            animator.SetTrigger("Jump");
        }
    }
}

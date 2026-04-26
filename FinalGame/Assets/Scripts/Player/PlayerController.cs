using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f;
    public float rotationSpeed = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    // State — readable by other scripts
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isJumping;

    private CharacterController controller;
    private Vector3 velocity;
    private Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Cache the main camera transform for movement direction
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Raw input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        isMoving = inputDir.magnitude >= 0.1f;

        if (isMoving && cameraTransform != null)
        {
            float speed = isRunning ? runSpeed : walkSpeed;

            // Get the camera's forward and right directions, flattened to the XZ plane
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            // Build movement direction relative to where the camera is looking
            Vector3 moveDir = camForward * v + camRight * h;
            moveDir.Normalize();

            // Rotate the cat to face movement direction
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
        else if (isGrounded)
        {
            isJumping = false;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
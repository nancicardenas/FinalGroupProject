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

    [Header("Jump Timing")]
    public float jumpStartDelay = 0.25f;
    private bool jumpQueued;
    private float jumpQueueTimer;

    // State — readable by other scripts
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isJumping;
    [HideInInspector] public bool movementLocked = false;
    [HideInInspector] public bool isDiving;
    [HideInInspector] public bool isRecovering;

    //external tunables used for mips
    public float m_fDiveTime = 0.3f;
    public float m_fDiveRecoveryTime = 0.5f;
    public float m_fDiveDistance = 3.0f;

    //internal variables used for mips
    public Vector3 m_vDiveStartPos;
    public Vector3 m_vDiveEndPos;
    private Vector3 diveDir;
    private Vector3 lastMoveDir;
    public float m_fDiveStartTime;

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

    public void CheckForDive()
    {
        if(Input.GetKeyDown(KeyCode.X) && !isDiving && !isRecovering)
        {
            //Start dive operation
            isDiving = true;

            //Store starting parameters
            m_vDiveStartPos = transform.position;
            m_vDiveEndPos = m_vDiveStartPos + transform.forward * m_fDiveDistance;
            m_fDiveStartTime = Time.time;
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
        
        if (movementLocked)
        {
            isMoving = false;
            isRunning = false;
            isJumping = false;

            // Still apply gravity so the character does not freeze unnaturally
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        // Raw input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        isMoving = inputDir.magnitude >= 0.1f;

        CheckForDive();

        if(isDiving)
        {
            diveDir = lastMoveDir;

            float t = (Time.time - m_fDiveStartTime) / m_fDiveTime;

            float diveSpeed = m_fDiveDistance / m_fDiveTime;

            controller.Move(diveDir * diveSpeed * Time.deltaTime);

            if(t >= 1.0f)
            {
                isRecovering = true;
                isDiving = false;
                m_fDiveStartTime = Time.time;
            }
        }

        //checks for slow or fast state
        if (isMoving && cameraTransform != null && !isDiving && !isRecovering)
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

            lastMoveDir = moveDir;

            // Rotate the cat to face movement direction
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !jumpQueued)
        {
            jumpQueued = true;
            jumpQueueTimer = jumpStartDelay;
            isJumping = true;
        }

        if (jumpQueued)
        {
            jumpQueueTimer -= Time.deltaTime;

            if (jumpQueueTimer <= 0f)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpQueued = false;
            }
        }
        else if (isGrounded)
        {
            isJumping = false;
        }

        if(isRecovering)
        {
            float t = (Time.time - m_fDiveStartTime) / m_fDiveRecoveryTime;

            if(t >= 1f)
            {
                isRecovering = false;
            }
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
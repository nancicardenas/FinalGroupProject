using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HumanAI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public enum humanState : int
    {
        idle,
        walking,
        search,
        alert,
        distracted,
        playerCaught
    }

    public humanState state = humanState.idle;
    public NavMeshAgent humanAgent;
    
    private float idleTimer = 0f;
    private float idleDuration = 1f;
    
    //Change this to align with floor in scene
    private float humanYPosition = 0.6f;

    private Vector3 destinationPos;

    public Transform target;
    
    //Change These when resizing cat/human objects
    private float humanHeight = 2.3f;
    private float catHeight = 0.5f;
    private float ghostHeight = 0.3f;
    private float targetHeight;

    private float viewDistance = 15f;
    private float viewAngle = 100f;
    private float catchRadius = 3f;

    private float idleScanAngle = 90f;
    private float scanSpeed = 2f;
    private float baseRotationY;

    private float walkSpeed = 2f;
    private float runSpeed = 6f;

    private Vector3 lastKnownPlayerPosition;
    private bool hasReachedLastKnownPlayerPosition;
    private float searchDuration = 7f;
    private float searchTimer = 0f;
    private float searchRadius = 5f;

    private float distractionTimer;
    private bool distractedOnce = false;
    private int lastNumGhosts = 0;
    
    public bool isTargetPlayer = true;

    //TODO: delete later (for testing in AI scene)
    private CameraFollow cameraFollow;
    
    private GhostManager ghostManager;
    
    private PlayerLife playerLife;
    
    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        ghostManager = GameObject.FindGameObjectWithTag("GhostManager").GetComponent<GhostManager>();
        playerLife = target.root.GetComponent<PlayerLife>();
        
        //TODO: delete later (for testing in AI scene)
        if (SceneManager.GetActiveScene().name == "Human AI Test")
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>().target = target.transform;
        }
        baseRotationY = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case humanState.idle:
                UpdateIdle();
                break;
            case humanState.walking:
                UpdateWalking();
                break;
            case humanState.search:
                UpdateSearch();
                break;
            case humanState.alert:
                UpdateAlert();
                break;
            case humanState.distracted:
                UpdateDistracted();
                break;
            case humanState.playerCaught:
                UpdatePlayerCaught();
                break;
        }
    }

    private void EnterIdle()
    {
        state = humanState.idle;
        humanAgent.isStopped = true;
        idleTimer = 0f;
        baseRotationY = transform.eulerAngles.y;
        idleDuration = Random.Range(1f, 3f); //Wait for a random amount of time from 1 to 3 seconds
    }

    private void UpdateIdle()
    {
        idleTimer += Time.deltaTime;
        
        //Turn left and right to scan for player
        float angle = Mathf.Sin(Time.time * scanSpeed) * idleScanAngle;
        float newYAngle = baseRotationY + angle;
        transform.rotation = Quaternion.Euler(0f, newYAngle, 0f);
        
        //After waiting, pick new patrolling point
        if (idleTimer >= idleDuration)
        {
            EnterWalking();
        }
        
        //Try to enter distracted, will depend on number of ghosts in scene
        TryEnterDistracted();
    }
    
    //Enter the walking state
    private void EnterWalking()
    {
        humanAgent.isStopped = false;
        humanAgent.speed = 2f;
        
        //Pick random points in a certain area, TODO can change later to predetermined points like the dog
        destinationPos = new Vector3(Random.Range(-9, 9), humanYPosition, Random.Range(-9, 9));
        humanAgent.SetDestination(destinationPos);
        state = humanState.walking;
    }
    
    //Helper used to check if the AI has reached its navigation point
    bool HasReachedDestination()
    {
        return !humanAgent.pathPending &&
               humanAgent.remainingDistance <= humanAgent.stoppingDistance &&
               (!humanAgent.hasPath || humanAgent.velocity.sqrMagnitude < 0.01f);
    }

    private void UpdateWalking()
    {
        //Try to enter distracted, will depend on number of ghosts in scene
        TryEnterDistracted();
        
        //If player has been scene start running after them
        if (CanSeePlayer())
        {
            EnterAlert();
        }
        
        //Enter idle scanning when reaching the walking point
        if (HasReachedDestination())
        {
            EnterIdle();
        }
    }

    //Enter the search state
    private void EnterSearch()
    {
        state = humanState.search;
        humanAgent.isStopped = false;
        searchTimer = searchDuration;

        humanAgent.speed = walkSpeed;
        hasReachedLastKnownPlayerPosition = false;
        humanAgent.SetDestination(lastKnownPlayerPosition);
    }

    private void UpdateSearch()
    {
        TryEnterDistracted();
        
        //If player is seen during search, enter alert state
        if (CanSeePlayer())
        {
            EnterAlert();
            return;
        }

        //Only start search timer when the AI has reached the last known player location
        if (!hasReachedLastKnownPlayerPosition)
        {
            //Check if AI reached last known player location then set 
            if (HasReachedDestination())
            {
                hasReachedLastKnownPlayerPosition = true;
                ChooseNextSearchPoint();
            }
            return;
        }
        
        //Decrement search timer until it times out, go back to walking state
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            EnterWalking();
        }
        
        if (HasReachedDestination())
        {
            ChooseNextSearchPoint();
        }
    }
    
    //Choose the next search point
    void ChooseNextSearchPoint()
    {
        Vector3 offset = Random.insideUnitSphere * searchRadius;
        offset.y = 0;
        humanAgent.SetDestination(lastKnownPlayerPosition + offset);
    }
    
    private void EnterAlert()
    {
        print("alert");
        humanAgent.isStopped = false;
        humanAgent.speed = runSpeed;
        state = humanState.alert;
    }

    private void UpdateAlert()
    {
        print("update");
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = target.position;
            humanAgent.SetDestination(target.position);
        }
        else
        {
            EnterSearch();
            print("search");
            return;
        }

        if (ReachedTarget())
        {
            EnterPlayerCaught();
        }
        /*humanAgent.ResetPath();
        EnterIdle();*/
    }

    bool ReachedTarget()
    {
        return Vector3.Distance(target.position, transform.position) <= catchRadius;
    }

    //Try to enter the distracted state by checking how many ghosts are in the scene
    //5 ghosts = 3 seconds of distraction
    //6 ghosts = 4 seconds
    //7 ghosts = 5 seconds
    //and so on...
    private void TryEnterDistracted()
    {
        int numGhosts = ghostManager.activeGhosts.Count;
        
        if (numGhosts < 5)
        {
            lastNumGhosts = numGhosts;
            return;
        }
        
        //If on a new ghost run set distractedOnce to false
        if (lastNumGhosts < numGhosts)
        {
            distractedOnce = false;
        }
        
        //Don't enter distracted state if already distracted once on the current ghost run
        if (distractedOnce)
        {
            lastNumGhosts = numGhosts;
            return;
        }
        
        lastNumGhosts = numGhosts;
        distractionTimer = numGhosts - 2;
        EnterDistracted();
    }

    //Enter the distracted state, stop the agent
    void EnterDistracted()
    {
        state = humanState.distracted;
        humanAgent.ResetPath();
        humanAgent.isStopped = true;
        distractedOnce = true;
    }

    //Stay distracted for the specified amount of time in distractionTimer, rotate to show confusion
    //Enter the search state if the timer is over
    void UpdateDistracted()
    {
        distractionTimer -= Time.deltaTime;
        baseRotationY = transform.eulerAngles.y;
        float angle = Mathf.LerpAngle(baseRotationY, -target.eulerAngles.y, Time.deltaTime * scanSpeed);
        
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        if (distractionTimer <= 0f)
        {
            EnterSearch();
        }
    }
    
    private void EnterPlayerCaught()
    {
        print("playerCaught");
        //state = humanState.playerCaught;
    }

    private void UpdatePlayerCaught()
    {
        print("done");
    }
    
    bool CanSeePlayer()
    {
        targetHeight = isTargetPlayer ? catHeight : ghostHeight;
        
        //Ray setup
        Vector3 origin = transform.position + Vector3.up * humanHeight;
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 dir = (targetPosition - origin).normalized;
        
        float dist = Vector3.Distance(origin, targetPosition);

        //The distance between the player and the human has to be less than the viewDistance to return true
        if (dist > viewDistance) 
            return false;

        float angleToTarget = Vector3.Angle(transform.forward, dir);
        if (angleToTarget > viewAngle * 0.5f) 
            return false;

        //Cast a ray toward the player to see if there is an obstacle in the way
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            return hit.transform == target;
        }

        return false;
    }
    
    //Helper used to draw gizmos in the editor for detection
    void OnDrawGizmosSelected()
    {
        if (this.target == null) return;

        Vector3 origin = transform.position + Vector3.up * humanHeight;

        // --- Draw detection radius ---
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, viewDistance);

        // --- Draw FOV cone ---
        Gizmos.color = Color.blue;

        int segments = 30; // More = smoother arc
        float halfFOV = viewAngle * 0.5f;

        Vector3 previousPoint = origin;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -halfFOV + (viewAngle / segments) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 point = origin + direction * viewDistance;

            // Draw lines forming the arc
            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            // Draw lines from origin to edge (cone sides)
            if (i == 0 || i == segments)
            {
                Gizmos.DrawLine(origin, point);
            }

            previousPoint = point;
        }

        // --- Existing player line + raycast ---
        Vector3 targetPos = this.target.position + Vector3.up * targetHeight;
        Vector3 dir = (targetPos - origin).normalized;
        float dist = Vector3.Distance(origin, targetPos);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(origin, targetPos);

        if (dist <= viewDistance)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                Gizmos.color = CanSeePlayer() ? Color.green : Color.red;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }
    }
}

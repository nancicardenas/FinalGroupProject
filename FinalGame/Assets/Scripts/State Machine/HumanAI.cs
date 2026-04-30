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
        alert,
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
    private float catchRadius = 1f;
    
    public bool isTargetPlayer = true;

    //TODO: delete later (for testing in AI scene)
    private CameraFollow cameraFollow;
    
    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        
        //TODO: delete later (for testing in AI scene)
        if (SceneManager.GetActiveScene().name == "Human AI Test")
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>().target = target.transform;
        }
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
            case humanState.alert:
                UpdateAlert();
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
        idleDuration = Random.Range(1f, 3f); //Wait for a random amount of time from 1 to 3 seconds
    }

    private void UpdateIdle()
    {
        idleTimer += Time.deltaTime;

        //After waiting, pick new patrolling point
        if (idleTimer >= idleDuration)
        {
            EnterWalking();
        }
    }
    
    private void EnterWalking()
    {
        humanAgent.isStopped = false;
        humanAgent.speed = 2f;
        destinationPos = new Vector3(Random.Range(-9, 9), humanYPosition, Random.Range(-9, 9));
        humanAgent.SetDestination(destinationPos);
        state = humanState.walking;
    }
    
    bool HasReachedDestination()
    {
        return !humanAgent.pathPending &&
               humanAgent.remainingDistance <= humanAgent.stoppingDistance &&
               (!humanAgent.hasPath || humanAgent.velocity.sqrMagnitude < 0.01f);
    }

    private void UpdateWalking()
    {
        if (CanSeePlayer())
        {
            EnterAlert();
        }
        if (HasReachedDestination())
        {
            EnterIdle();
        }
    }
    
    private void EnterAlert()
    {
        humanAgent.isStopped = false;
        humanAgent.speed = 6f;
        state = humanState.alert;
    }

    private void UpdateAlert()
    {
        if (CanSeePlayer())
        {
            humanAgent.SetDestination(target.position);
            return;
        }

        if (ReachedTarget())
        {
            EnterPlayerCaught();
            return;
        }
        humanAgent.ResetPath();
        EnterIdle();
    }

    bool ReachedTarget()
    {
        return Vector3.Distance(target.position, transform.position) <= catchRadius;
    }
    private void EnterPlayerCaught()
    {
        print("playerCaught");
        state = humanState.playerCaught;
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

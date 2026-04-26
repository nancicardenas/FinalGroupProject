using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class DogAI : MonoBehaviour
{

    public enum dogState : int
    {
        idle,
        patrol,
        chase,
        playerCaught,
        numStates
    }

    public dogState state = dogState.patrol;
    
    public Transform target;
    public NavMeshAgent dogAgent;

    public Transform[] destinationPoints;
    public Vector3 destinationPos;
    private float detectionRadius = 10f;
    private float idleTimer = 0f;
    private float idleDuration = 1f;

    private float catchRadius = 2f;
    private float caughtTimer = 0f;
    private float caughtDuration = 4f;
    
    //Change These when resizing cat/dog objects
    private float dogHeight = 0.75f;
    private float catHeight = 0.5f;

    private float patrolSpeed = 2f;

    private float chaseSpeed = 5f;

    public PlayerLife playerLife;
    private void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case dogState.idle:
                UpdateIdle();
                break;
            case dogState.patrol:
                UpdatePatrol();
                break;
            case dogState.chase:
                UpdateChase();
                break;
            case dogState.playerCaught:
                UpdatePlayerCaught();
                break;
        }
    }

    //Enter the Idle state, stop the agent's movement
    void EnterIdle()
    {
        state = dogState.idle;
        dogAgent.isStopped = true;
        idleTimer = 0f;
    }

    void UpdateIdle()
    {
        //If player is in range and visible start chasing
        if (CanSeePlayer())
        {
            EnterChase();
            return;
        }

        idleTimer += Time.deltaTime;

        //After waiting, pick new patrolling point
        if (idleTimer >= idleDuration)
        {
            EnterPatrol();
        }
    }

    //Helper for readability
    bool HasReachedDestination()
    {
        return !dogAgent.pathPending &&
               dogAgent.remainingDistance <= dogAgent.stoppingDistance &&
               (!dogAgent.hasPath || dogAgent.velocity.sqrMagnitude < 0.01f);
    }

    void EnterPatrol()
    {
        dogAgent.isStopped = false;
        dogAgent.speed = patrolSpeed;
        
        destinationPos = destinationPoints[Random.Range(0, destinationPoints.Length)].position;
        dogAgent.SetDestination(destinationPos);

        state = dogState.patrol;
    }
    void UpdatePatrol()
    {
        //If the player is within distance and is visible set state to chase
        if (CanSeePlayer())
        {
            EnterChase();
            return;
        }
        
        //Once dog reaches destination enter the idle state
        if (HasReachedDestination())
        {
            EnterIdle();
        }
    }

    void EnterChase()
    {
        dogAgent.isStopped = false;
        dogAgent.speed = chaseSpeed;
        state = dogState.chase;
    }

    void UpdateChase()
    {
        //If the player has been caught enter player caught state, don't enter if going after ghost
        if (ReachedPlayer())
        {
            EnterPlayerCaught();
            return;
        }
        
        //Move towards the player if the player is in sight and is within distance
        if (CanSeePlayer())
        {
            dogAgent.SetDestination(target.position);
            return;
        }
        
        dogAgent.ResetPath();
        EnterIdle();
    }

    //If the player is within the catchRadius return true
    bool ReachedPlayer()
    {
        return Vector3.Distance(target.position, transform.position) <= catchRadius;
    }
    
    bool CanSeePlayer()
    {
        //Ray setup
        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 targetPosition = this.target.position + Vector3.up * catHeight;
        Vector3 dir = (targetPosition - origin).normalized;
        
        float dist = Vector3.Distance(origin, targetPosition);

        //The distance between the player and the dog has to be less than the detection radius to return true
        if (dist > detectionRadius) return false;

        //Cast a ray toward the player to see if there is an obstacle in the way
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            print("chase");
            return hit.transform == this.target;
        }

        return false;
    }

    //Enter the playerCaught state
    void EnterPlayerCaught()
    {
        dogAgent.isStopped = true;
        dogAgent.ResetPath();
        caughtTimer = 0f;
        caughtDuration =  target.root.CompareTag("Player") ? 1f : 4f; //Set the caughtDuration to 4 seconds if it is a ghost
        state = dogState.playerCaught;

        //Kill player if the dog reaches them, don't kill if the target is a ghost
        if (target.root.CompareTag("Player"))
        {
            playerLife.Die();
        }
        
    }

    //TODO: Mayble delete function later, only used for testing purposes. State would need to be reset if game over.
    void UpdatePlayerCaught()
    {
        /*if (!ReachedPlayer())
        {
            dogAgent.isStopped = false;
            if (CanSeePlayer())
            {
                state = dogState.chase;
            }
            else
            {
                state = dogState.patrol;
            }
        }*/
        caughtTimer += Time.deltaTime;
        dogAgent.isStopped = true;
        if (caughtTimer >= caughtDuration)
        {
            EnterPatrol();
        }
    }
    
    //Helper used to draw gizmos in the editor for detection
    void OnDrawGizmosSelected()
    {
        if (this.target == null) return;

        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 target = this.target.position + Vector3.up * catHeight;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, detectionRadius);

        // Draw line to player (full distance)
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(origin, target);

        // Draw raycast (actual vision check)
        if (dist <= detectionRadius)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                // Green if player is visible
                if (hit.transform == this.target)
                {
                    Gizmos.color = Color.green;
                }
                else // Red if something is blocking
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawLine(origin, hit.point);

                // Draw hit point
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }
    }
}

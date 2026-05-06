using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class DogAI : MonoBehaviour
{
    [Header("Detection")]
    public LayerMask detectionMask;
    public float detectionRadius = 10f;
    
    public enum dogState : int
    {
        idle,
        patrol,
        chase,
        playerCaught,
        resetting, // state to prevent Update from interfering during warp
    }

    public dogState state = dogState.patrol;
    
    [Header("References")]
    public Transform target;
    public NavMeshAgent dogAgent;
    public Transform player;
    public Transform ghostTarget;

    //Script references
    public PlayerLife playerLife;
    public GhostManager ghostManager;
    
    [Header("Place Destination Transforms in this list for Dog Movement")]
    public Transform[] destinationPoints;
    public Vector3 destinationPos;
    
    //Start position used to reset dog
    private Vector3 startPosition;
    
    //Idle Variables
    private float idleTimer = 0f;
    private float idleDuration = 1f;

    //PlayerCaught variables
    private float catchRadius = 1f;
    private float caughtTimer = 0f;
    private float caughtDuration = 4f;
    
    //Chase variables
    private float lostSightTimer = 0f;
    private float lostSightDelay = 1f;

    //Change These when resizing cat/dog objects, used for detection
    private float dogHeight = 0.75f;
    private float catHeight = 1.0f;
    private float ghostHeight = 0.3f;
    private float targetHeight;

    //Used for different heights of ghosts vs. player
    public bool isTargetPlayer = true;

    //Tunable speed variables
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    private void Start()
    {
        //On first life, set target height to catHeight. Initialize startPosition for dog
        targetHeight = catHeight;
        startPosition = transform.position;
    }

    /// <summary>
    /// Called by PlayerSpawner via OnPlayerReset event.
    /// Resets the dog to spawn immediately.
    /// </summary>
    public void OnPlayerDied()
    {
        StopAllCoroutines();
        target = player;
        isTargetPlayer = true;
        ghostTarget = null;
        state = dogState.resetting;
        StartCoroutine(WarpAndPatrolDelayed(startPosition));
    }

    //Try to detect ghosts if they exist. Detect the closest non-null ghost to dog.
    bool TryDetectGhost()
    {
        if (ghostManager == null) return false;
        
        ghostManager.activeGhosts.RemoveAll(g => g == null);

        float closestDist = float.MaxValue;
        Transform closestGhost = null;

        foreach (GameObject ghostObj in ghostManager.activeGhosts)
        {
            if (ghostObj == null) continue;

            //Only detect ghost if it is the closest and is within detection radius
            float dist = Vector3.Distance(transform.position, ghostObj.transform.position);
            if (dist > detectionRadius) continue;
            if (dist >= closestDist) continue;

            // Check line of sight to this ghost
            Vector3 origin = transform.position + Vector3.up * dogHeight;
            Vector3 targetPos = ghostObj.transform.position + Vector3.up * ghostHeight;
            Vector3 dir = (targetPos - origin).normalized;
            float rayDist = Vector3.Distance(origin, targetPos);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, rayDist, detectionMask, QueryTriggerInteraction.Collide))
            {
                if (hit.transform.root == ghostObj.transform.root)
                {
                    closestDist = dist;
                    closestGhost = ghostObj.transform;
                }
            }
        }

        if (closestGhost != null)
        {
            target = closestGhost;
            ghostTarget = closestGhost;
            isTargetPlayer = false;
            return true;
        }

        return false;
    }

    void Update()
    {
        // Don't do anything while resetting
        if (state == dogState.resetting) return;

        if (target == null)
        {
            // Try to fall back to player
            if (player != null)
            {
                target = player;
                isTargetPlayer = true;
            }
            else
            {
                return;
            }
        }

        //Switch to current state
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

    //Enter the Idle state, stop the dog agetn.
    void EnterIdle()
    {
        state = dogState.idle;
        dogAgent.isStopped = true;
        idleTimer = 0f;
    }

    void UpdateIdle()
    {
        // Prioritize ghosts over player
        if (TryDetectGhost())
        {
            EnterChase();
            return;
        }

        //If target is detected, enter chase state
        if (CanSeeTarget())
        {
            EnterChase();
            return;
        }

        //When idle wait for idleDuration amount of time
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleDuration)
        {
            EnterPatrol();
        }
    }

    //Helper used to determine if the agent has reached its destination.
    bool HasReachedDestination()
    {
        return !dogAgent.pathPending &&
               dogAgent.remainingDistance <= dogAgent.stoppingDistance &&
               (!dogAgent.hasPath || dogAgent.velocity.sqrMagnitude < 0.01f);
    }

    //Enter the patrol state
    void EnterPatrol()
    {
        if (!dogAgent.isOnNavMesh) return;

        dogAgent.isStopped = false;
        dogAgent.speed = patrolSpeed;
        dogAgent.angularSpeed = 120f; // smooth turning
        dogAgent.acceleration = 8f;

        //Randomly select a destination point to walk to
        destinationPos = destinationPoints[Random.Range(0, destinationPoints.Length)].position;
        dogAgent.SetDestination(destinationPos);

        state = dogState.patrol;
    }


    void UpdatePatrol()
    {
        // Prioritize ghosts over player
        if (TryDetectGhost())
        {
            EnterChase();
            return;
        }

        //If player is detected enter the chase state
        if (CanSeeTarget())
        {
            EnterChase();
            return;
        }

        //If agent reached its patrol point, enter idle
        if (HasReachedDestination())
        {
            EnterIdle();
        }
    }

    //Enter the chase state
    void EnterChase()
    {
        if (!dogAgent.isOnNavMesh) return;

        lostSightTimer = 0f;
        
        //Play 1 of 2 dog bark sounds when entering the chase states
        if (AudioManager.Instance != null)
        {
            bool dogBark1 = Random.Range(0, 2) == 0;
            if (dogBark1)
            {
                AudioManager.Instance.PlayDogBark1();
            }
            else
            {
                AudioManager.Instance.PlayDogBark2();
            }
        }

        dogAgent.isStopped = false;
        dogAgent.speed = chaseSpeed;
        dogAgent.angularSpeed = 360f; // faster turning during chase
        dogAgent.acceleration = 12f;

        state = dogState.chase;
    }

    void UpdateChase()
    {
        //Don't chase if ghost or player is null
        if (target == null)
        {
            EnterIdle();
            return;
        }

        //Enter the player caught state if agent reached the target. Target can be ghost or player.
        if (ReachedTarget())
        {
            EnterPlayerCaught();
            return;
        }

        //Continuously set agent destination to target to chase them
        if (CanSeeTarget())
        {
            dogAgent.SetDestination(target.position);
            return;
        }

        //Timer used to prevent spamming audio and consistent chasing
        lostSightTimer += Time.deltaTime;

        if (lostSightTimer >= lostSightDelay)
        {
            lostSightTimer = 0f;
            dogAgent.ResetPath();
            EnterIdle();
        }
    }

    //Helper used to check if the agent has reached the target
    bool ReachedTarget()
    {
        if (target == null) return false;
        return Vector3.Distance(target.position, transform.position) <= catchRadius;
    }

    //Returns true if the target is completely in view and within the detection radius
    bool CanSeeTarget()
    {
        if (target == null) return false;

        targetHeight = isTargetPlayer ? catHeight : ghostHeight; //ghost has different height on its collider

        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 dir = (targetPosition - origin).normalized;

        float dist = Vector3.Distance(origin, targetPosition);

        if (dist > detectionRadius) return false;

        //Cast a ray to detect target
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, detectionMask, QueryTriggerInteraction.Collide))
        {
            return hit.transform.root == target.root;
        }

        return false;
    }

    //Enter the player caught state
    void EnterPlayerCaught()
    {
        dogAgent.isStopped = true;
        dogAgent.ResetPath();
        caughtTimer = 0f;
        state = dogState.playerCaught;

        //If target is player, kill the player and reset the scene for next run
        if (target != null && target.root.CompareTag("Player"))
        {
            caughtDuration = 1f;
            playerLife.Die();
            // Dog reset handled by OnPlayerDied event
        }
        
        //Otherwise, if the target is a ghost, destroy the ghost and move on
        else if (target != null && target.root.CompareTag("Ghost"))
        {
            caughtDuration = 1f;
            // Destroy the ghost
            GhostReplay ghost = target.GetComponent<GhostReplay>();
            if (ghost == null) ghost = target.GetComponentInParent<GhostReplay>();
            if (ghost != null)
            {
                Destroy(ghost.gameObject);
            }
            target = null;
        }
    }

    void UpdatePlayerCaught()
    {
        caughtTimer += Time.deltaTime;
        dogAgent.isStopped = true;

        if (caughtTimer >= caughtDuration)
        {
            // Pick new target then patrol
            if (ghostManager != null)
            {
                ghostManager.SelectNewDogTarget.Invoke();
            }
            state = dogState.resetting;
            StartCoroutine(WarpAndPatrolDelayed(transform.position));
        }
    }

    //Resets the dog agent, prevents navmesh glitching
    IEnumerator WarpAndPatrolDelayed(Vector3 position)
    {
        state = dogState.resetting;

        if (dogAgent.isOnNavMesh)
        {
            dogAgent.isStopped = true;
            dogAgent.ResetPath();
        }

        dogAgent.enabled = false;
        transform.position = position;

        //wait a few frames 
        yield return null;
        yield return null;

        dogAgent.enabled = true;

        yield return null;

        if (dogAgent.isOnNavMesh)
        {
            dogAgent.Warp(position);
        }

        // Only reset target to player if target is null (ghost was destroyed)
        if (target == null && player != null)
        {
            target = player;
            isTargetPlayer = true;
        }

        //Wait to guarantee dog is on the navMesh on reset
        int maxWaitFrames = 10;
        int waited = 0;
        while (!dogAgent.isOnNavMesh && waited < maxWaitFrames)
        {
            yield return null;
            waited++;
        }

        //Reset the dog if it is on the nav mesh, otherwise keep idle
        if (dogAgent.isOnNavMesh)
        {
            dogAgent.isStopped = false;
            dogAgent.speed = patrolSpeed;
            destinationPos = destinationPoints[Random.Range(0, destinationPoints.Length)].position;
            dogAgent.SetDestination(destinationPos);
            state = dogState.patrol;
        }
        else
        {
            state = dogState.idle;
        }
    }

    //Editor helper for visualizing detection radius and detection rays
    void OnDrawGizmosSelected()
    {
        if (this.target == null) return;

        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 target = this.target.position + Vector3.up * targetHeight;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, detectionRadius);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(origin, target);

        if (dist <= detectionRadius)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                if (hit.transform.root == this.target.root)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }
    }
}
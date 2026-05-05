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
        resetting, // new state to prevent Update from interfering during warp
        numStates
    }

    public dogState state = dogState.patrol;

    public Transform target;
    public NavMeshAgent dogAgent;

    public Transform[] destinationPoints;
    public Vector3 destinationPos;
    private Vector3 startPosition;
    private float idleTimer = 0f;
    private float idleDuration = 1f;

    private float catchRadius = 1f;
    private float caughtTimer = 0f;
    private float caughtDuration = 4f;

    //Change These when resizing cat/dog objects
    private float dogHeight = 0.75f;
    private float catHeight = 1.0f;
    private float ghostHeight = 0.3f;
    private float targetHeight;

    public bool isTargetPlayer = true;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    public PlayerLife playerLife;
    public GhostManager ghostManager;
    public Transform player;
    public Transform ghostTarget;

    private void Start()
    {
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
        print("target: " + target.name + " called by " + gameObject.name);
        isTargetPlayer = true;
        ghostTarget = null;
        state = dogState.resetting;
        StartCoroutine(WarpAndPatrolDelayed(startPosition));
    }

    bool TryDetectGhost()
    {
        if (ghostManager == null) return false;

        ghostManager.activeGhosts.RemoveAll(g => g == null);

        float closestDist = float.MaxValue;
        Transform closestGhost = null;

        foreach (GameObject ghostObj in ghostManager.activeGhosts)
        {
            if (ghostObj == null) continue;

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

        if (CanSeeTarget())
        {
            EnterChase();
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleDuration)
        {
            EnterPatrol();
        }
    }

    bool HasReachedDestination()
    {
        return !dogAgent.pathPending &&
               dogAgent.remainingDistance <= dogAgent.stoppingDistance &&
               (!dogAgent.hasPath || dogAgent.velocity.sqrMagnitude < 0.01f);
    }

    void EnterPatrol()
    {
        if (!dogAgent.isOnNavMesh) return;

        dogAgent.isStopped = false;
        dogAgent.speed = patrolSpeed;
        dogAgent.angularSpeed = 120f; // smooth turning
        dogAgent.acceleration = 8f;

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

        if (CanSeeTarget())
        {
            EnterChase();
            return;
        }

        if (HasReachedDestination())
        {
            EnterIdle();
        }
    }

    float DistanceDifferencePlayerToGhost()
    {
        if (player == null || ghostTarget == null) return 0f;
        return Vector3.Distance(player.position, transform.position) - Vector3.Distance(ghostTarget.position, transform.position);
    }

    void EnterChase()
    {
        if (!dogAgent.isOnNavMesh) return;
        
        if(AudioManager.Instance != null) AudioManager.Instance.PlayDogBark();

        dogAgent.isStopped = false;
        dogAgent.speed = chaseSpeed;
        dogAgent.angularSpeed = 360f; // faster turning during chase
        dogAgent.acceleration = 12f;

        state = dogState.chase;
    }

    void UpdateChase()
    {
        if (target == null)
        {
            EnterIdle();
            return;
        }

        if (ReachedTarget())
        {
            EnterPlayerCaught();
            return;
        }

        if (CanSeeTarget())
        {
            dogAgent.SetDestination(target.position);
            return;
        }

        dogAgent.ResetPath();
        EnterIdle();
    }

    bool ReachedTarget()
    {
        if (target == null) return false;
        return Vector3.Distance(target.position, transform.position) <= catchRadius;
    }

    bool CanSeeTarget()
    {
        if (target == null) return false;

        targetHeight = isTargetPlayer ? catHeight : ghostHeight;

        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 dir = (targetPosition - origin).normalized;

        float dist = Vector3.Distance(origin, targetPosition);

        if (dist > detectionRadius) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, detectionMask, QueryTriggerInteraction.Collide))
        {
            return hit.transform.root == target.root;
        }

        return false;
    }

    void EnterPlayerCaught()
    {
        dogAgent.isStopped = true;
        dogAgent.ResetPath();
        caughtTimer = 0f;
        state = dogState.playerCaught;

        if (target != null && target.root.CompareTag("Player"))
        {
            caughtDuration = 1f;
            playerLife.Die();
            // Dog reset handled by OnPlayerDied event
        }
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

        int maxWaitFrames = 10;
        int waited = 0;
        while (!dogAgent.isOnNavMesh && waited < maxWaitFrames)
        {
            yield return null;
            waited++;
        }

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
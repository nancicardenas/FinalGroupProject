using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{

    public enum dogState : int
    {
        patrol,
        chase,
        playerCaught,
        numStates
    }

    public dogState state;
    
    public Transform player;
    public NavMeshAgent dogAgent;

    private Vector3 destinationPos;
    private float distanceToPlayer;
    
    private float viewDistance = 5f;
    
    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
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

    void UpdatePatrol()
    {
        //Move randomly until player gets within range
        //When player gets within range change state to chase
        distanceToPlayer = Vector3.Distance(player.position, transform.position);
        
        destinationPos = new Vector3(Random.Range(-4, 4), 1,  Random.Range(-4, 4));
        if (!dogAgent.hasPath)
        {
            dogAgent.SetDestination(destinationPos);
        }
        
        if (distanceToPlayer <= 2f && CanSeePlayer())
        {
            state = dogState.chase;
        }
    }

    void UpdateChase()
    {
        //Move towards the player if the player is in sight and is within distance
        //Otherwise change state to patrol

        destinationPos = player.position;
        dogAgent.SetDestination(destinationPos);
    }

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        if (dist > viewDistance) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            return hit.transform == player;
        }

        return false;
    }

    void UpdatePlayerCaught()
    {
        
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position * viewDistance);
    }
}

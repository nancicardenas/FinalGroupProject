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

    public Vector3 destinationPos;
    //public float distanceToPlayer;
    
    //private float viewDistance = 2f;
    private float detectionRadius = 10f;
    
    //Change These when resizing cat/dog objects
    private float dogHeight = 0.75f;
    private float catHeight = 0.5f;
    
    // Update is called once per frame
    void Update()
    {
        //distanceToPlayer = Vector3.Distance(player.position, transform.position);
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
        
        if (!dogAgent.hasPath)
        {
            //TODO Change to a array of points or wider area
            destinationPos = new Vector3(Random.Range(-4, 4), 1,  Random.Range(-4, 4));
            dogAgent.SetDestination(destinationPos);
        }
        
        //If the player is within distance and is visible set state to chase
        if (CanSeePlayer())
        {
            state = dogState.chase;
        }
    }

    void UpdateChase()
    {
        //Move towards the player if the player is in sight and is within distance
        //Otherwise change state to patrol
        if (CanSeePlayer())
        {
            dogAgent.SetDestination(player.position);
        }
        else{
            state = dogState.patrol;
            dogAgent.ResetPath();
            print("Path Reset");
        }
    }

    //
    bool CanSeePlayer()
    {
        //Ray setup
        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 target = player.position + Vector3.up * catHeight;
        Vector3 dir = (target - origin).normalized;
        
        float dist = Vector3.Distance(origin, target);

        //The distance between the player and the dog has to be less than the detection radius to return true
        if (dist > detectionRadius) return false;

        //Cast a ray toward the player to see if there is an obstacle in the way
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            print("chase");
            return hit.transform == player;
        }

        return false;
    }

    void UpdatePlayerCaught()
    {
        
    }
    
    //Helper used to draw gizmos in the editor for detection
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Vector3 origin = transform.position + Vector3.up * dogHeight;
        Vector3 target = player.position + Vector3.up * catHeight;
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
                if (hit.transform == player)
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

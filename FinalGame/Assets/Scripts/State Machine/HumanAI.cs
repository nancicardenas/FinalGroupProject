using UnityEngine;
using UnityEngine.AI;

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
        if (HasReachedDestination())
        {
            EnterIdle();
        }
    }
    
    private void EnterAlert()
    {
        
    }

    private void UpdateAlert()
    {
        
    }
    
    private void EnterPlayerCaught()
    {
        
    }

    private void UpdatePlayerCaught()
    {
        
    }
}

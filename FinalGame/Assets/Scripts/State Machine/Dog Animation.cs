using UnityEngine;
using UnityEngine.AI;

public class DogAnimation : MonoBehaviour
{
    public Animator dogAnimator;
    public DogAI dogAI;
    
    void Update()
    {
        //Play the correct animation based on the current state
        switch (dogAI.state)
        {
            case DogAI.dogState.idle:
                dogAnimator.SetBool("IsIdle", true);
                dogAnimator.SetBool("IsWalking", false);
                dogAnimator.SetBool("IsRunning",false);
                dogAnimator.SetBool("CaughtPlayer", false);
                break;
            case DogAI.dogState.patrol:
                dogAnimator.SetBool("IsIdle", false);
                dogAnimator.SetBool("IsWalking", true);
                dogAnimator.SetBool("IsRunning",false);
                dogAnimator.SetBool("CaughtPlayer", false);
                break;
            case DogAI.dogState.chase:
                dogAnimator.SetBool("IsIdle", false);
                dogAnimator.SetBool("IsWalking", false);
                dogAnimator.SetBool("IsRunning",true);
                dogAnimator.SetBool("CaughtPlayer", false);
                break;
            case DogAI.dogState.playerCaught:
                dogAnimator.SetBool("IsIdle", false);
                dogAnimator.SetBool("IsWalking", false);
                dogAnimator.SetBool("IsRunning",false);
                dogAnimator.SetBool("CaughtPlayer", true);
                break;
        }
    }
}

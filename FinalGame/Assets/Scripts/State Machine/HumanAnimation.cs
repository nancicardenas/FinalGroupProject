using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    public Animator humanAnimator;
    public HumanAI humanAI;
    
    void Update()
    {
        //Play the correct animation based on the current state
        switch (humanAI.state)
        {
            case HumanAI.humanState.idle:
                humanAnimator.SetBool("IsIdle", true);
                humanAnimator.SetBool("IsWalking", false);
                humanAnimator.SetBool("IsRunning",false);
                break;
            case HumanAI.humanState.walking:
                humanAnimator.SetBool("IsIdle", false);
                humanAnimator.SetBool("IsWalking", true);
                humanAnimator.SetBool("IsRunning",false);
                break;
            case HumanAI.humanState.search:
                humanAnimator.SetBool("IsIdle", false);
                humanAnimator.SetBool("IsWalking", true);
                humanAnimator.SetBool("IsRunning",false);
                break;
            case HumanAI.humanState.alert:
                humanAnimator.SetBool("IsIdle", false);
                humanAnimator.SetBool("IsWalking", false);
                humanAnimator.SetBool("IsRunning",true);
                break;
            case HumanAI.humanState.playerCaught:
                humanAnimator.SetBool("IsIdle", true);
                humanAnimator.SetBool("IsWalking", false);
                humanAnimator.SetBool("IsRunning",false);
                break;
        }
    }
}

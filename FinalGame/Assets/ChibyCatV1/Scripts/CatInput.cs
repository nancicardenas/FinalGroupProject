using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatInput : MonoBehaviour {
    public float speed = 1.0f;
    public Animator animator;

    public Slider velocityForwardSlider;
    public Slider velocityRightSlider;
    public Toggle triggerSitToggle;
    public Toggle triggerJumpToggle;

    private float MIN_FORWARD_VELOCITY = 0.0f;
    private float MAX_FORWARD_VELOCITY = 2.0f;
    private float MIN_RIGHT_VELOCITY = -1.0f;
    private float MAX_RIGHT_VELOCITY = 1.0f;

    private float currentForwardVelocity = 0.0f;
    private float currentRightVelocity = 0.0f;
    // Start is called before the first frame update
    void Start() {
        velocityForwardSlider.minValue = MIN_FORWARD_VELOCITY;
        velocityForwardSlider.maxValue = MAX_FORWARD_VELOCITY;
        velocityRightSlider.minValue = MIN_RIGHT_VELOCITY;
        velocityRightSlider.maxValue = MAX_RIGHT_VELOCITY;
    }

    // Update is called once per frame
    void Update() {
        UpdateAnimation();
        UpdateGUI();
    }

    void UpdateAnimation() {
        //Forward
        if (Input.GetKey(KeyCode.W)) {
            currentForwardVelocity += speed * Time.deltaTime;
            currentForwardVelocity = Mathf.Min(currentForwardVelocity, MAX_FORWARD_VELOCITY);
        }
        //Backward
        else if (Input.GetKey(KeyCode.S)) {
            currentForwardVelocity -= speed * Time.deltaTime;
            currentForwardVelocity = Mathf.Max(currentForwardVelocity, MIN_FORWARD_VELOCITY);
        }

        //Left
        if (Input.GetKey(KeyCode.A)) {
            currentRightVelocity -= speed * Time.deltaTime;
            currentRightVelocity = Mathf.Max(currentRightVelocity, MIN_RIGHT_VELOCITY);
        }
        //Right
        else if (Input.GetKey(KeyCode.D)) {
            currentRightVelocity += speed * Time.deltaTime;
            currentRightVelocity = Mathf.Min(currentRightVelocity, MAX_RIGHT_VELOCITY);
        }
        animator.SetFloat("velocityForward", currentForwardVelocity);
        animator.SetFloat("velocityRight", currentRightVelocity);
        //Sit
        if (Input.GetKey(KeyCode.LeftControl)) {
            animator.SetTrigger("triggerSit");
        }
        //Jump
        if (Input.GetKey(KeyCode.Space)) {
            animator.SetTrigger("triggerJump");
        }
    }

    void UpdateGUI() {
        float velocityForward = animator.GetFloat("velocityForward");
        float velocityRight = animator.GetFloat("velocityRight");
        bool triggerSit = animator.GetBool("triggerSit");
        bool triggerJump = animator.GetBool("triggerJump");

        triggerSitToggle.isOn = triggerSit;
        triggerJumpToggle.isOn = triggerJump;
        velocityRightSlider.value = velocityRight;
        velocityForwardSlider.value = velocityForward;
    }
}

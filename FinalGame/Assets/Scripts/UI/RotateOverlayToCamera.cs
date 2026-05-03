using System;
using UnityEngine;

public class RotateOverlayToCamera : MonoBehaviour
{
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        
       transform.forward = mainCamera.transform.forward;
            
    }
}

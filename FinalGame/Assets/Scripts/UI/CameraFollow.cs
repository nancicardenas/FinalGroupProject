using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 8f, -6f);
    public float smoothSpeed = 5f;

    public float currentAngle = 0f;
    public float rotationSpeed = 120f;
    void LateUpdate()
    {
        if (target == null) return;

        float h = Input.GetAxis("Horizontal");
        currentAngle += h * rotationSpeed * Time.deltaTime; 
        
        // Rotate offset around Y axis
        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 rotatedOffset = rotation * offset;

        // Follow player with rotated offset
        Vector3 desiredPosition = target.position + rotatedOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}

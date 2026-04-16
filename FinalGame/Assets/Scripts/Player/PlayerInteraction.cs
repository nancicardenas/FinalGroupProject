using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 2.5f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.Mouse0; // Left Click

    [HideInInspector] public bool hasKey = false;
    private List<GameObject> nearbyInteractables = new List<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // Find nearest interactable within range
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);
        float closestDist = float.MaxValue;
        GameObject closest = null;

        foreach (Collider hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.gameObject;
            }
        }

        if (closest != null)
        {
            // Check for Key
            KeyPickup key = closest.GetComponent<KeyPickup>();

            if (key != null)
            {
                key.Pickup(this);
                return;
            }

            // Check for Gate
            Gate gate = closest.GetComponent<Gate>();
            if (gate != null)
            {
                gate.TryOpen(this);
                return;
            }
        }
    }
}

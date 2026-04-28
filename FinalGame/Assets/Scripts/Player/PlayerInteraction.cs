using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 2.5f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.Mouse0; // Left Click

    [HideInInspector] public bool hasKey = false;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
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

        if (closest == null) return;

        // Check object and its parent for interactable components
        // (handles cases where collider is on a child mesh)
        GameObject[] toCheck = new GameObject[] { closest, closest.transform.root.gameObject };
        if (closest.transform.parent != null)
        {
            toCheck = new GameObject[] { closest, closest.transform.parent.gameObject, closest.transform.root.gameObject };
        }

        foreach (GameObject obj in toCheck)
        {
            KeyPickup key = obj.GetComponent<KeyPickup>();
            if (key != null && !key.isPickedUp)
            {
                key.Pickup(this);

                PlayerAnimator playerAnimator = GetComponentInChildren<PlayerAnimator>();
                if (playerAnimator != null)
                {
                    playerAnimator.TriggerSearch();
                }
                return;
            }

            Gate gate = obj.GetComponent<Gate>();
            if (gate != null && !gate.isOpen)
            {
                gate.TryOpen(this);
                return;
            }

            ExitDoor door = obj.GetComponent<ExitDoor>();
            if (door != null)
            {
                door.Use();
                return;
            }
        }
    }
}
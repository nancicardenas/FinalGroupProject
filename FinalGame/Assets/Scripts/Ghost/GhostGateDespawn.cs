using UnityEngine;

/// <summary>
/// Attach to the Gate object. Detects when a ghost touches the gate
/// and despawns the ghost if it doesn't have a key.
/// </summary>
public class GhostGateDespawn : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ghost")) return;

        GhostReplay ghost = other.GetComponent<GhostReplay>();
        if (ghost == null) ghost = other.GetComponentInParent<GhostReplay>();

        if (ghost == null) return;

        // If ghost has no key and gate is still closed, despawn the ghost
        Gate gate = GetComponent<Gate>();
        if (gate != null && !gate.isOpen && !ghost.HasKey())
        {
            Debug.Log("Ghost hit closed gate without key — despawning.");
            ghost.DespawnAtGate();
        }
    }
}
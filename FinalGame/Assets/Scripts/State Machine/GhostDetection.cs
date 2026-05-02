using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GhostDetection : MonoBehaviour
{
    private DogAI dogAIScript;
    public GhostManager ghostManager;

    public Transform player;

    private void Start()
    {
        dogAIScript = GetComponent<DogAI>();
        ghostManager.SelectNewDogTarget.AddListener(SelectNewTarget);
    }

    private void SelectNewTarget()
    {
        if (dogAIScript == null) return;

        // Remove destroyed ghosts from the list
        ghostManager.activeGhosts.RemoveAll(g => g == null);

        bool ghostsActive = ghostManager.activeGhosts.Count > 0;

        if (ghostsActive)
        {
            // Pick a random ghost
            GameObject ghostObj = ghostManager.activeGhosts[Random.Range(0, ghostManager.activeGhosts.Count)];

            // Safety check — ghost might have been destroyed between RemoveAll and now
            if (ghostObj != null)
            {
                dogAIScript.target = ghostObj.transform;
                dogAIScript.ghostTarget = ghostObj.transform;
                dogAIScript.isTargetPlayer = false;
            }
            else
            {
                // Fall back to player
                FallbackToPlayer();
            }
        }
        else
        {
            FallbackToPlayer();
        }

        if (dogAIScript.target != null)
        {
            Debug.Log("Dog target: " + dogAIScript.target.name);
        }
    }

    void FallbackToPlayer()
    {
        if (player != null)
        {
            dogAIScript.target = player;
            dogAIScript.isTargetPlayer = true;
            dogAIScript.ghostTarget = null;
        }
    }
}
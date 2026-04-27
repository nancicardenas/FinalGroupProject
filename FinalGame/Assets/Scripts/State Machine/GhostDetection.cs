using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GhostDetection : MonoBehaviour
{
    private DogAI dogAIScript;
    public GhostManager ghostManager;

    public Transform player;
    
    private void Start()
    {
        //Assign instance of DogAI on current object, and add listener SelectNewTarget to SelectNewDogTarget Event
        dogAIScript = GetComponent<DogAI>();
        ghostManager.SelectNewDogTarget.AddListener(SelectNewTarget);
    }

    private void SelectNewTarget()
    {
        bool ghostsActive = ghostManager.activeGhosts.Count > 0;
        dogAIScript.target = ghostsActive ? ghostManager.activeGhosts[Random.Range(0, ghostManager.activeGhosts.Count)].transform : player;
        dogAIScript.isTargetPlayer = dogAIScript.target.gameObject.CompareTag("Player");
        print(dogAIScript.target.name);
    }
}

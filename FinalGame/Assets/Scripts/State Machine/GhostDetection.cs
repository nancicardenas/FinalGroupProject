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
        dogAIScript.target = ghostManager.activeGhosts.Count > 0 ? ghostManager.activeGhosts[Random.Range(0, ghostManager.activeGhosts.Count)].transform : player;
        print(dogAIScript.target.name);
    }
}

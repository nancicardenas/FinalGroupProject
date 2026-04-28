using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GhostDetection : MonoBehaviour
{
    private DogAI dogAIScript;
    public GhostManager ghostManager;

    public Transform player;

    //public List<int> usedGhostIndexes = new List<int>();
    
    private void Start()
    {
        //Assign instance of DogAI on current object, and add listener SelectNewTarget to SelectNewDogTarget Event
        dogAIScript = GetComponent<DogAI>();
        ghostManager.SelectNewDogTarget.AddListener(SelectNewTarget);
    }

    private void SelectNewTarget()
    {
        bool ghostsActive = ghostManager.activeGhosts.Count > 0;
        int selectedIndex = Random.Range(0, ghostManager.activeGhosts.Count);
        GhostReplay selectedGhostReplay = ghostManager.activeGhosts[selectedIndex].GetComponent<GhostReplay>();
        
        dogAIScript.target = ghostsActive && selectedGhostReplay.isPlaying ? ghostManager.activeGhosts[selectedIndex].transform : player;
        dogAIScript.isTargetPlayer = dogAIScript.target.gameObject.CompareTag("Player");

        //Set the ghostTarget to the current ghost target
        if (!dogAIScript.isTargetPlayer)
        {
            dogAIScript.ghostTarget = dogAIScript.target;
        }
        print(dogAIScript.target.name);
    }
}

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public int NumberOfKeys { get; private set; }

    public UnityEvent<PlayerInventory> OnKeyCollected;

    public void KeyCollected()
    {
        NumberOfKeys++;
        OnKeyCollected.Invoke(this);
    }
}

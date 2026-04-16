using UnityEngine;

public class TrapZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLife life = other.GetComponent<PlayerLife>();

            if (life != null)
            {
                Debug.Log("Trap triggered!");
                life.Die();
            }
        }
    }
}

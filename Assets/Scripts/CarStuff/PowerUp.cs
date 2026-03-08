using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // public GameObject pickUpEffect;


    void OnTriggerEnter (Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Pickup(other);
        }
    }

    void Pickup(Collider player)
    {
        // Spawn a cool effect
        // Instantiate(pickUpEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}

using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f; // Time before the bullet is destroyed

    void Start()
    {
        // Destroy the bullet after a certain time to prevent memory leaks
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Here you can add logic for what happens when the bullet hits something
        // For example, destroy the bullet and the object it hits
        Destroy(gameObject);

        // Optionally: Add logic to destroy the object that was hit
        // if (collision.gameObject.CompareTag("Target"))
        // {
        //     Destroy(collision.gameObject);
        // }
    }
}

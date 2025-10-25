using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    public int damage = 25;
    public float explosionDuration = 0.2f;

    void Start()
    {
        Destroy(gameObject, explosionDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Explosão acertou o Player!");
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }
    }
}
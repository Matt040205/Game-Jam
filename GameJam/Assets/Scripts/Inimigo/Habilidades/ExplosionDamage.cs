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
            Debug.Log("Explos�o acertou o Player!");

            // C�DIGO ANTIGO: PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // C�DIGO ANTIGO: if (playerHealth != null) { ... playerHealth.TakeDamage(damage, transform.position); }

            // NOVO: Dispara o evento de dano (Substitui o GetComponent)
            GlobalDamageEvents.FirePlayerDamage(other.gameObject, damage, transform.position);
        }
    }
}
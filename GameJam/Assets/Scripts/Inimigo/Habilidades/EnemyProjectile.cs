using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lifeTime = 3.0f;
    public int damageAmount = 10;

    [Header("Audio")]
    public AudioClip hitSound;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    void FixedUpdate()
    {
        if (rb != null && moveDirection != Vector2.zero)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // CORREÇÃO do erro de codificação "Projtil"
            Debug.Log("Projétil Inimigo acertou o Player!");

            // CÓDIGO ANTIGO: PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // CÓDIGO ANTIGO: if (playerHealth != null) { ... playerHealth.TakeDamage(damageAmount, transform.position); }

            // NOVO: Dispara o evento de dano (Substitui o GetComponent)
            GlobalDamageEvents.FirePlayerDamage(other.gameObject, damageAmount, transform.position);

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
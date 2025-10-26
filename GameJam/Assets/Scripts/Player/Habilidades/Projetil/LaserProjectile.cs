using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float lifeTime = 2.0f;
    public int damageAmount = 10;

    [Header("Audio")]
    public AudioClip hitSound;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.freezeRotation = true;
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
            rb.linearVelocity = moveDirection * moveSpeed; // Corrigido para velocity
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[LaserProjectile] Trigger Enter com: {other.name}");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("[LaserProjectile] Acertou o INIMIGO!");

            // --- CORREÇÃO: Sempre dispara o evento OnEnemyTakeDamage ---
            GlobalDamageEvents.FireEnemyDamage(other.gameObject, damageAmount);
            Debug.Log($"[LaserProjectile] Evento FireEnemyDamage disparado para {other.name}.");
            // --- FIM CORREÇÃO ---

            if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Debug.Log("[LaserProjectile] Acertou a Parede.");
            Destroy(gameObject);
        }
    }
}
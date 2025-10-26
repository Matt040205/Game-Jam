using UnityEngine;
using FMODUnity; // <- Adicionado

public class EnemyProjectile : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lifeTime = 3.0f;
    public int damageAmount = 10;

    [Header("FMOD Events")]
    [EventRef] public string hitSoundEvent; // <- Substitui AudioClip
    // Remove: public AudioClip hitSound;

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

    void FixedUpdate() { if (rb != null && moveDirection != Vector2.zero) rb.linearVelocity = moveDirection * moveSpeed; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalDamageEvents.FirePlayerDamage(other.gameObject, damageAmount, transform.position);
            // --- FMOD ---
            if (!string.IsNullOrEmpty(hitSoundEvent)) RuntimeManager.PlayOneShot(hitSoundEvent, transform.position);
            // --- FIM FMOD ---
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall")) Destroy(gameObject);
    }
}
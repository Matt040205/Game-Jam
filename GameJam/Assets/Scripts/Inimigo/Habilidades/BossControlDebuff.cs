using UnityEngine;

public class BossControlDebuff : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float lifeTime = 3.0f;
    public int damageAmount = 20;
    public float debuffDuration = 5f;

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
            Debug.Log("Proj�til do Boss acertou o Player!");

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform.position);
            }

            PlayerDebuffs playerDebuffs = other.GetComponent<PlayerDebuffs>();
            if (playerDebuffs != null && !playerDebuffs.controlsInverted)
            {
                playerDebuffs.ApplyInvertControls(debuffDuration);
            }

            Destroy(gameObject);
        }
    }
}
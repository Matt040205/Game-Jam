using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float lifeTime = 2.0f;
    public int damageAmount = 10;

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
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Proj�til acertou o Inimigo!");

            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"Tentando causar {damageAmount} de dano (Laser)...");
                enemyHealth.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("Proj�til tocou em 'Enemy' mas n�o encontrou o script EnemyHealth.");
            }

            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
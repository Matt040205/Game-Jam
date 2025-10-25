using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Hitbox do Player tocou em: {other.name}");

            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"Tentando causar {damage} de dano ao inimigo.");
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("Hitbox tocou em 'Enemy' mas não encontrou o script EnemyHealth.");
            }
        }
    }
}
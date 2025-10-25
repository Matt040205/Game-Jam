using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Hitbox do Player tocou em: {other.name}");

            // CÓDIGO ANTIGO: EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            // CÓDIGO ANTIGO: if (enemyHealth != null) { ... enemyHealth.TakeDamage(damage); }

            // NOVO: Dispara o evento de dano (Substitui o GetComponent)
            GlobalDamageEvents.FireEnemyDamage(other.gameObject, damage);

            Debug.Log($"Tentando causar {damage} de dano ao inimigo. (Via Evento)");
        }
    }
}
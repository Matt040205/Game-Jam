using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[PlayerHitbox] Trigger Enter com: {other.name} (Tag: {other.tag})");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"[PlayerHitbox] Hitbox tocou em INIMIGO: {other.name}");

            // --- CORREÇÃO: Sempre dispara o evento OnEnemyTakeDamage ---
            GlobalDamageEvents.FireEnemyDamage(other.gameObject, damage);
            Debug.Log($"[PlayerHitbox] Evento FireEnemyDamage disparado para {other.name}.");
            // --- FIM CORREÇÃO ---
        }
        else
        {
            Debug.Log($"[PlayerHitbox] Colidiu com objeto sem tag 'Enemy'.");
        }
    }
}
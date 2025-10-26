using UnityEngine;

public class BossMeleeDamage : MonoBehaviour
{
    public int damage = 30;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[BossMeleeDamage] Hitbox Melee acertou o Player!");
            // --- GARANTIR: Dispara o evento de dano no Player ---
            GlobalDamageEvents.FirePlayerDamage(other.gameObject, damage, transform.position);
            // --- FIM GARANTIR ---
        }
    }
}
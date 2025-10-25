using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BossHealth : MonoBehaviour
{
    public int maxHealth = 500;
    private int currentHealth;

    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private AudioSource audioSource;
    private Animator animator;

    private BossJulgador bossController;
    private bool isDead = false;

    // NOVO: Assinatura do Evento de Dano
    void OnEnable()
    {
        GlobalDamageEvents.OnEnemyTakeDamage += OnDamageReceived;
    }

    // NOVO: Cancelamento da Assinatura
    void OnDisable()
    {
        GlobalDamageEvents.OnEnemyTakeDamage -= OnDamageReceived;
    }

    void Start()
    {
        //... (código Start inalterado)
    }

    // NOVO: Método que o Evento Estático chama
    private void OnDamageReceived(GameObject target, int damage)
    {
        // Garante que o dano é para este objeto
        if (target != gameObject) return;

        InternalTakeDamage(damage);
    }

    // O método TakeDamage original foi renomeado para Private
    private void InternalTakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Boss tomou {damage} de dano. Vida restante: {currentHealth}");

        if (hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Boss MORREU!");
        bossController.enabled = false;

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 5f);
    }
}
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    private Animator animator;

    [Header("Death Explosion")]
    [SerializeField] private bool explodeOnDeath = false;
    [SerializeField] private GameObject explosionPrefab;

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
        currentHealth = maxHealth;

        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        Debug.Log($"Inimigo {gameObject.name} Iniciado com {currentHealth} de vida.");
    }

    // NOVO: M�todo que o Evento Est�tico chama
    private void OnDamageReceived(GameObject target, int damage)
    {
        // Garante que o dano � para este objeto
        if (target != gameObject) return;

        InternalTakeDamage(damage);
    }

    // O m�todo TakeDamage original foi renomeado para Private
    private void InternalTakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"Inimigo {gameObject.name} tomou {damage} de dano. Vida restante: {currentHealth}");

        if (hurtSound != null)
        //... (restante do c�digo Die e OnDestroy inalterado)
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
        Debug.Log($"Inimigo {gameObject.name} Morreu!");
        currentHealth = 0;

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (enemyMovement != null) enemyMovement.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 2f);
    }

    private void OnDestroy()
    {
        if (explodeOnDeath)
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Debug.Log("Inimigo EXPLODIU ao morrer!");
            }
            else
            {
                Debug.LogWarning($"Inimigo {gameObject.name} 'explodeOnDeath' � true, mas o 'explosionPrefab' n�o foi atribu�do no Inspetor.");
            }
        }
    }
}
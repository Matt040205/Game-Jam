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

    void Start()
    {
        currentHealth = maxHealth;

        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        Debug.Log($"Inimigo {gameObject.name} Iniciado com {currentHealth} de vida.");
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"Inimigo {gameObject.name} tomou {damage} de dano. Vida restante: {currentHealth}");

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
                Debug.LogWarning($"Inimigo {gameObject.name} 'explodeOnDeath' é true, mas o 'explosionPrefab' não foi atribuído no Inspetor.");
            }
        }
    }
}
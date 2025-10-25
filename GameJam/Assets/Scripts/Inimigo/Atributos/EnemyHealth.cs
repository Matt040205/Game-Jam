using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;

        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
        animator = GetComponent<Animator>();
        Debug.Log($"Inimigo {gameObject.name} Iniciado com {currentHealth} de vida.");
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"Inimigo {gameObject.name} tomou {damage} de dano. Vida restante: {currentHealth}");

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
}
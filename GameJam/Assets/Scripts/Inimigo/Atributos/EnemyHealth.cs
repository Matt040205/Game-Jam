using UnityEngine;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class EnemyHealth : MonoBehaviour
{
    // NOVO: Classe Serializável para o Drop Table
    [Serializable]
    public class DropItem
    {
        public GameObject prefab;
        [Range(0f, 100f)]
        public float dropChance; // Chance individual para este item
    }

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

    // NOVO: Tabela de Drops, permite configurar vários itens com chances individuais
    [Header("Item Drop Settings")]
    [Tooltip("Lista de itens que podem ser dropados e suas respectivas chances percentuais (0-100).")]
    public DropItem[] dropTable;

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
        // Lógica de explosão existente
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

        // NOVO: Chama a lógica de drop de item
        TryDropItem();
    }

    // NOVO: Função para gerenciar o drop de itens (agora iterando sobre a DropTable)
    private void TryDropItem()
    {
        if (dropTable == null || dropTable.Length == 0)
        {
            return; // Sai se não houver itens configurados
        }

        // Itera sobre cada item na tabela de drops
        foreach (DropItem item in dropTable)
        {
            if (item.prefab == null) continue; // Pula slots vazios

            // Testa a chance de drop individual para este item
            if (Random.Range(0f, 100f) <= item.dropChance)
            {
                // Instancia o item na posição do inimigo
                Instantiate(item.prefab, transform.position, Quaternion.identity);
                Debug.Log($"Inimigo {gameObject.name} dropou um item: {item.prefab.name} com {item.dropChance}% de chance.");

                // NOTA: Se você quiser que apenas UM item caia por inimigo,
                // adicione um 'return;' aqui. Deixei sem 'return' para permitir múltiplos drops.
            }
        }
    }
}
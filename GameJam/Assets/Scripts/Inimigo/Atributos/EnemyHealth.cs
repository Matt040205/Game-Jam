using UnityEngine;
using System;
using Random = UnityEngine.Random;
using FMODUnity; // <- Adicionado

public class EnemyHealth : MonoBehaviour
{
    [Serializable] public class DropItem { public GameObject prefab; [Range(0f, 100f)] public float dropChance; }

    public int maxHealth = 50; private int currentHealth;

    [Header("FMOD Events")]
    [EventRef] public string hurtSoundEvent; // <- Substitui AudioClip
    [EventRef] public string deathSoundEvent; // <- Substitui AudioClip
    // Remove: private AudioSource audioSource;

    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    private Animator animator;
    private Rigidbody2D rb; // <- Adicionado para o fix

    [Header("Death Explosion")][SerializeField] private bool explodeOnDeath; [SerializeField] private GameObject explosionPrefab;
    [Header("Item Drop")] public DropItem[] dropTable;

    void OnEnable() { GlobalDamageEvents.OnEnemyTakeDamage += OnDamageReceived; }
    void OnDisable() { GlobalDamageEvents.OnEnemyTakeDamage -= OnDamageReceived; }

    void Start()
    {
        currentHealth = maxHealth;
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // <- Pega o Rigidbody
        // Remove: audioSource = GetComponent<AudioSource>();
    }

    private void OnDamageReceived(GameObject target, int damage)
    {
        if (target != gameObject) return;
        InternalTakeDamage(damage);
    }

    private void InternalTakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        // --- FMOD ---
        if (!string.IsNullOrEmpty(hurtSoundEvent)) RuntimeManager.PlayOneShot(hurtSoundEvent, transform.position);
        // --- FIM FMOD ---
        if (currentHealth <= 0) Die();
        else if (animator != null) animator.SetTrigger("Hurt");
    }

    private void Die()
    {
        currentHealth = 0; // Garante que n�o tome mais dano
        Debug.Log($"[EnemyHealth] Inimigo {gameObject.name} Morreu!");

        // --- FIX SLIDING ---
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Para imediatamente
            rb.bodyType = RigidbodyType2D.Kinematic; // Impede f�sica futura
        }
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false; // Desativa colis�es
        // --- FIM FIX SLIDING ---

        // Desativa scripts de controle
        if (enemyMovement != null) enemyMovement.enabled = false;
        if (enemyAttack != null) enemyAttack.enabled = false;
        this.enabled = false; // Desativa o pr�prio script de vida

        // --- FMOD ---
        if (!string.IsNullOrEmpty(deathSoundEvent)) RuntimeManager.PlayOneShot(deathSoundEvent, transform.position);
        // --- FIM FMOD ---

        if (animator != null) animator.SetTrigger("Die");

        Destroy(gameObject, 2f); // Destr�i AP�S som/anima��o
    }

    private void OnDestroy() { if (explodeOnDeath && explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity); TryDropItem(); }
    private void TryDropItem() {/* ... Sem altera��es ... */}
}
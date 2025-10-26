using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Knockback Settings")]
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.2f;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // Para UI

    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private PlayerCombat playerCombat;
    private PlayerMovement playerMovement;
    private Animator animator;

    private bool isAlive = true;
    private bool isGettingKnockedBack = false;

    // --- CORREÇÃO: Ouve o evento ---
    void OnEnable()
    {
        GlobalDamageEvents.OnPlayerTakeDamage += OnDamageReceived; // Descomentado
        Debug.Log("[PlayerHealth] OnEnable - Assinando evento OnPlayerTakeDamage.");
    }

    void OnDisable()
    {
        GlobalDamageEvents.OnPlayerTakeDamage -= OnDamageReceived; // Descomentado
        Debug.Log("[PlayerHealth] OnDisable - Cancelando assinatura do evento OnPlayerTakeDamage.");
    }
    // --- FIM CORREÇÃO ---

    void Start()
    {
        currentHealth = maxHealth;
        isAlive = true;
        rb = GetComponent<Rigidbody2D>();
        playerCombat = GetComponent<PlayerCombat>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[PlayerHealth] Start. Vida: {currentHealth}/{maxHealth}");
    }

    // --- CORREÇÃO: Função chamada pelo evento ---
    private void OnDamageReceived(GameObject target, int damage, Vector2 damageSourcePosition)
    {
        if (target != gameObject) return; // Garante que é para este Player
        Debug.Log($"[PlayerHealth] Evento OnPlayerTakeDamage recebido! Dano: {damage}");
        InternalTakeDamage(damage, damageSourcePosition);
    }
    // --- FIM CORREÇÃO ---

    // Função privada que aplica o dano (chamada pelo OnDamageReceived)
    private void InternalTakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (!isAlive || isGettingKnockedBack)
        {
            Debug.LogWarning("[PlayerHealth] Dano ignorado (morto ou knockback).");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log($"[PlayerHealth] VIDA ATUAL: {currentHealth}. Tomou {damage} de dano.");

        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;
        if (knockbackDirection == Vector2.zero && playerMovement != null)
        {
            knockbackDirection = -playerMovement.GetLastMoveDirection();
        }

        playerCombat?.PlayHurtAnimation(knockbackDirection);
        StartCoroutine(ApplyKnockback(knockbackDirection));

        if (currentHealth <= 0) Die();
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isGettingKnockedBack = true;
        Debug.Log("[PlayerHealth] Knockback INICIO");
        if (playerMovement != null) playerMovement.enabled = false;

        rb.linearVelocity = Vector2.zero; // <-- Corrigido para velocity
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        Debug.Log("[PlayerHealth] Knockback FIM");
        rb.linearVelocity = Vector2.zero; // <-- Corrigido para velocity

        if (playerMovement != null && isAlive) playerMovement.enabled = true;
        isGettingKnockedBack = false;
    }

    public void Heal(int healAmount) {/* ... */} // Sem alterações

    private void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        Debug.Log("[PlayerHealth] Player Morreu!");

        // --- CORREÇÃO: Carrega cena "FinalRuim" diretamente ---
        SceneManager.LoadScene("FinalRuim");
        // --- FIM CORREÇÃO ---

        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        if (animator != null) animator.SetTrigger("Die");
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // <-- Corrigido para velocity
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Não precisa mais do GameManager para carregar a cena aqui
        // Destroy(gameObject); // O carregamento da cena já destrói
    }

    // --- CORREÇÃO: REMOVER OnDestroy ---
    // private void OnDestroy() { /* SceneManager.LoadScene("FinalRuim"); */ } // Removido!
    // --- FIM CORREÇÃO ---
}
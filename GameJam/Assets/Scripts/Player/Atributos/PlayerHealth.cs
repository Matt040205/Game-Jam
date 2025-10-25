using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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
    public UnityEvent<int, int> OnHealthChanged;

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

    // NOVO: Assinatura do Evento de Dano
    void OnEnable()
    {
        GlobalDamageEvents.OnPlayerTakeDamage += OnDamageReceived;
    }

    // NOVO: Cancelamento da Assinatura
    void OnDisable()
    {
        GlobalDamageEvents.OnPlayerTakeDamage -= OnDamageReceived;
    }

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

        Debug.Log($"Vida inicial do Player: {currentHealth}/{maxHealth}");
    }

    // NOVO: Método que o Evento Estático chama
    private void OnDamageReceived(GameObject target, int damage, Vector2 damageSourcePosition)
    {
        // Garante que o dano é para este objeto
        if (target != gameObject) return;

        InternalTakeDamage(damage, damageSourcePosition);
    }

    // O método TakeDamage original foi renomeado para Private
    private void InternalTakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (!isAlive || isGettingKnockedBack)
        {
            Debug.LogWarning("Player tomou dano mas está imune (morreu ou knockback).");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player tomou {damage} de dano. Vida restante: {currentHealth}");

        if (hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;

        if (knockbackDirection == Vector2.zero)
        {
            // Assume que playerMovement.GetLastMoveDirection() está disponível
            // (Assumindo que essa função existe no PlayerMovement.cs, que não foi enviado)
            // Se não existir, use Vector2.down ou o que for apropriado.
            playerMovement.GetLastMoveDirection();
        }

        playerCombat?.PlayHurtAnimation(knockbackDirection);

        StartCoroutine(ApplyKnockback(knockbackDirection));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    //... (restante do código ApplyKnockback e Die inalterado)
    {
        isGettingKnockedBack = true;
        Debug.Log("Player Knockback INICIO");

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        Debug.Log("Player Knockback FIM");
        rb.linearVelocity = Vector2.zero;

        if (playerMovement != null && isAlive)
        {
            playerMovement.enabled = true;
        }

        isGettingKnockedBack = false;
    }

    public void Heal(int healAmount)
    {
        if (!isAlive) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Garante que não excede o máximo

        Debug.Log($"Player curou {healAmount} de vida. Vida restante: {currentHealth}");

        // Opcional: Toca um som de cura aqui se você tiver um.
        // audioSource.PlayOneShot(healSound);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        Debug.Log("Player Morreu!");

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        float deathAnimationTime = 2f;
        Destroy(gameObject, deathAnimationTime);
    }
}
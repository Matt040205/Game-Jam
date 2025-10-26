using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <-- **ADICIONAR** para usar Image

// REMOVA a linha abaixo se você removeu o AudioSource (era para compatibilidade antiga)
// [RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    // --- **NOVO:** Referência para a Barra de Vida ---
    [Header("UI")]
    [Tooltip("Arraste a Image da barra de vida (com Image Type = Filled) para cá.")]
    public Image healthBarImage;
    // --- **FIM NOVO** ---

    [Header("Knockback Settings")]
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.2f;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // Ainda pode ser útil para outras coisas

    [Header("Audio")]
    public AudioClip hurtSound; // Manter se ainda usar AudioSource legado
    public AudioClip deathSound; // Manter se ainda usar AudioSource legado
    private AudioSource audioSource; // Manter se ainda usar AudioSource legado

    private Rigidbody2D rb;
    private PlayerCombat playerCombat;
    private PlayerMovement playerMovement;
    private Animator animator;

    private bool isAlive = true;
    private bool isGettingKnockedBack = false;

    void OnEnable()
    {
        GlobalDamageEvents.OnPlayerTakeDamage += OnDamageReceived;
        Debug.Log("[PlayerHealth] OnEnable - Assinando evento OnPlayerTakeDamage.");
    }

    void OnDisable()
    {
        GlobalDamageEvents.OnPlayerTakeDamage -= OnDamageReceived;
        Debug.Log("[PlayerHealth] OnDisable - Cancelando assinatura do evento OnPlayerTakeDamage.");
    }

    void Start()
    {
        currentHealth = maxHealth;
        isAlive = true;
        rb = GetComponent<Rigidbody2D>();
        playerCombat = GetComponent<PlayerCombat>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // Manter se usar AudioSource

        // **NOVO:** Atualiza a barra no início
        UpdateHealthBar();

        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Mantém o evento
        Debug.Log($"[PlayerHealth] Start. Vida: {currentHealth}/{maxHealth}");
    }

    private void OnDamageReceived(GameObject target, int damage, Vector2 damageSourcePosition)
    {
        if (target != gameObject) return;
        Debug.Log($"[PlayerHealth] Evento OnPlayerTakeDamage recebido! Dano: {damage}");
        InternalTakeDamage(damage, damageSourcePosition);
    }

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

        // **NOVO:** Atualiza a barra após tomar dano
        UpdateHealthBar();

        if (hurtSound != null && audioSource != null) audioSource.PlayOneShot(hurtSound); // Manter se usar AudioSource
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Mantém o evento

        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;
        if (knockbackDirection == Vector2.zero && playerMovement != null)
        {
            knockbackDirection = -playerMovement.GetLastMoveDirection();
        }

        playerCombat?.PlayHurtAnimation(knockbackDirection);
        StartCoroutine(ApplyKnockback(knockbackDirection));

        if (currentHealth <= 0) Die();
    }

    // --- **NOVO:** Função para atualizar a barra de vida ---
    private void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            // Calcula a porcentagem de vida (0.0 a 1.0)
            float fillAmount = (float)currentHealth / maxHealth;
            healthBarImage.fillAmount = fillAmount;
            Debug.Log($"[PlayerHealth] Barra de vida atualizada para {fillAmount * 100}%.");
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] Referência da Imagem da barra de vida (healthBarImage) não definida no Inspetor!");
        }
    }
    // --- **FIM NOVO** ---

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isGettingKnockedBack = true;
        if (playerMovement != null) playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero; // Corrigido para velocity
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        rb.linearVelocity = Vector2.zero; // Corrigido para velocity
        if (playerMovement != null && isAlive) playerMovement.enabled = true;
        isGettingKnockedBack = false;
    }

    public void Heal(int healAmount)
    {
        if (!isAlive) return;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Não ultrapassa o máximo

        // **NOVO:** Atualiza a barra após curar
        UpdateHealthBar();

        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Mantém o evento
        Debug.Log($"[PlayerHealth] Player curou {healAmount}. Vida: {currentHealth}");
    }

    private void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        Debug.Log("[PlayerHealth] Player Morreu!");
        SceneManager.LoadScene("FinalRuim");
        if (deathSound != null && audioSource != null) audioSource.PlayOneShot(deathSound); // Manter se usar AudioSource
        if (animator != null) animator.SetTrigger("Die");
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
    }
}
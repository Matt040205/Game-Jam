using UnityEngine;
using UnityEngine.SceneManagement;

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

    // --- CORRE��O: Ouve o evento ---
    void OnEnable()
    {
        GlobalDamageEvents.OnEnemyTakeDamage += OnDamageReceived;
        Debug.Log("[BossHealth] OnEnable - Assinando evento OnEnemyTakeDamage.");
        // Garante que a vida seja resetada se o Boss for reutilizado (opcional)
        // if (isDead) { currentHealth = maxHealth; isDead = false; }
    }

    void OnDisable()
    {
        GlobalDamageEvents.OnEnemyTakeDamage -= OnDamageReceived;
        Debug.Log("[BossHealth] OnDisable - Cancelando assinatura do evento OnEnemyTakeDamage.");
    }
    // --- FIM CORRE��O ---

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossJulgador>();
        Debug.Log($"[BossHealth] Start. Vida: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth() { return currentHealth; }

    // --- CORRE��O: Fun��o chamada pelo evento ---
    private void OnDamageReceived(GameObject target, int damage)
    {
        if (target != gameObject || !this.enabled) return; // Garante que � para este Boss e que est� ativo (Fase 2+)
        Debug.Log($"[BossHealth] Evento OnEnemyTakeDamage recebido! Dano: {damage}");
        InternalTakeDamage(damage);
    }
    // --- FIM CORRE��O ---

    // Fun��o privada que aplica o dano (chamada pelo OnDamageReceived)
    private void InternalTakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Garante que n�o fique negativo
        Debug.Log($"[BossHealth] VIDA ATUAL: {currentHealth}. Tomou {damage} de dano.");

        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[BossHealth] Boss MORREU!");

        // --- CORRE��O: Carrega cena "FinalBom" diretamente ---
        SceneManager.LoadScene("FinalBom");
        // --- FIM CORRE��O ---

        if (bossController != null) bossController.enabled = false;
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        if (animator != null) animator.SetTrigger("Die");
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // N�o precisa mais do GameManager para carregar a cena aqui
        // Destroy(gameObject, deathAnimationTime); // O carregamento da cena j� destr�i
    }
}
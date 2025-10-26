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

    // --- CORREÇÃO: Ouve o evento ---
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
    // --- FIM CORREÇÃO ---

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossJulgador>();
        Debug.Log($"[BossHealth] Start. Vida: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth() { return currentHealth; }

    // --- CORREÇÃO: Função chamada pelo evento ---
    private void OnDamageReceived(GameObject target, int damage)
    {
        if (target != gameObject || !this.enabled) return; // Garante que é para este Boss e que está ativo (Fase 2+)
        Debug.Log($"[BossHealth] Evento OnEnemyTakeDamage recebido! Dano: {damage}");
        InternalTakeDamage(damage);
    }
    // --- FIM CORREÇÃO ---

    // Função privada que aplica o dano (chamada pelo OnDamageReceived)
    private void InternalTakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Garante que não fique negativo
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

        // --- CORREÇÃO: Carrega cena "FinalBom" diretamente ---
        SceneManager.LoadScene("FinalBom");
        // --- FIM CORREÇÃO ---

        if (bossController != null) bossController.enabled = false;
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        if (animator != null) animator.SetTrigger("Die");
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // Não precisa mais do GameManager para carregar a cena aqui
        // Destroy(gameObject, deathAnimationTime); // O carregamento da cena já destrói
    }
}
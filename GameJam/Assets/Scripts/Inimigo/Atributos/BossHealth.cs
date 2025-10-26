using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity; // <- Adicionado

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 500; private int currentHealth;
    [Header("FMOD Events")]
    [EventRef] public string hurtSoundEvent; // <- Substitui AudioClip
    [EventRef] public string deathSoundEvent; // <- Substitui AudioClip
    // Remove: private AudioSource audioSource;

    private Animator animator;
    private BossJulgador bossController;
    private Rigidbody2D rb; // <- Adicionado para o fix
    private bool isDead = false;

    void OnEnable() { GlobalDamageEvents.OnEnemyTakeDamage += OnDamageReceived; }
    void OnDisable() { GlobalDamageEvents.OnEnemyTakeDamage -= OnDamageReceived; }

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossJulgador>();
        rb = GetComponent<Rigidbody2D>(); // <- Pega o Rigidbody
        // Remove: audioSource = GetComponent<AudioSource>();
    }

    public int GetCurrentHealth() { return currentHealth; }

    private void OnDamageReceived(GameObject target, int damage)
    {
        if (target != gameObject || !this.enabled) return;
        InternalTakeDamage(damage);
    }

    private void InternalTakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage; currentHealth = Mathf.Max(currentHealth, 0);
        // --- FMOD ---
        if (!string.IsNullOrEmpty(hurtSoundEvent)) RuntimeManager.PlayOneShot(hurtSoundEvent, transform.position);
        // --- FIM FMOD ---
        if (currentHealth <= 0) Die();
        else if (animator != null) animator.SetTrigger("Hurt");
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[BossHealth] Boss MORREU!");
        SceneManager.LoadScene("FinalBom"); // Carrega cena diretamente

        // --- FIX SLIDING ---
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        // --- FIM FIX SLIDING ---

        if (bossController != null) bossController.enabled = false;

        // --- FMOD ---
        if (!string.IsNullOrEmpty(deathSoundEvent)) RuntimeManager.PlayOneShot(deathSoundEvent, transform.position);
        // --- FIM FMOD ---

        if (animator != null) animator.SetTrigger("Die");
        // N�o precisa mais do Destroy delayed
    }
}
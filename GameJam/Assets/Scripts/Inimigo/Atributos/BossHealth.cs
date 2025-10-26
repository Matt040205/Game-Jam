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

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        bossController = GetComponent<BossJulgador>();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Boss tomou {damage} de dano. Vida restante: {currentHealth}");

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
        isDead = true;
        Debug.Log("Boss MORREU!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ganhou = true;
            Debug.Log("GameManager: Estado de Vitória REGISTRADO.");
        }

        bossController.enabled = false;

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        GetComponent<Collider2D>().enabled = false;

        float deathAnimationTime = 5f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadFinalSceneDelayed(deathAnimationTime);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.LoadScene("FinalBom");
    }
}
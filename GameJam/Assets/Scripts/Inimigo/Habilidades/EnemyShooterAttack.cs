using UnityEngine;
using FMODUnity; // <- Adicionado

public class EnemyShooterAttack : MonoBehaviour
{
    [Header("Shooting")] public GameObject enemyProjectilePrefab; public Transform firePoint; public float fireRate = 2f;
    [Header("FMOD Events")]
    [EventRef] public string shootSoundEvent; // <- Substitui AudioClip
    // Remove: private AudioSource audioSource;

    private Animator animator;
    private Transform target;
    private bool canShoot = false;
    private float fireTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Remove: audioSource = GetComponent<AudioSource>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void Update() { if (canShoot && Time.time >= fireTimer) { Shoot(); fireTimer = Time.time + fireRate; } }

    private void Shoot()
    {
        if (target == null || enemyProjectilePrefab == null || firePoint == null) return;
        if (animator != null) animator.SetTrigger("Attack");

        // --- FMOD ---
        if (!string.IsNullOrEmpty(shootSoundEvent)) RuntimeManager.PlayOneShot(shootSoundEvent, firePoint.position);
        // --- FIM FMOD ---

        Vector2 directionToPlayer = (target.position - firePoint.position).normalized;
        GameObject projectileObj = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();
        if (projectile != null) projectile.SetDirection(directionToPlayer);
    }

    public void SetCanShoot(bool isAllowed) { canShoot = isAllowed; }
}
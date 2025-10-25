using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyShooterAttack : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject enemyProjectilePrefab;
    public Transform firePoint;
    public float fireRate = 2f;

    [Header("Audio")]
    public AudioClip shootSound;
    private AudioSource audioSource;
    private Animator animator;

    private Transform target;
    private bool canShoot = false;
    private float fireTimer = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void Update()
    {
        if (canShoot && Time.time > fireTimer)
        {
            Shoot();
            fireTimer = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (target == null || enemyProjectilePrefab == null || firePoint == null)
        {
            return;
        }

        Debug.Log("Inimigo Atirador DISPARA!");
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Vector2 directionToPlayer = (target.position - firePoint.position).normalized;

        GameObject projectileObj = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(directionToPlayer);
        }
    }

    public void SetCanShoot(bool isAllowed)
    {
        canShoot = isAllowed;
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyShooterMovement : MonoBehaviour
{
    [Header("Distances")]
    public float shootDistance = 10f;
    public float runDistance = 5f;

    [Header("Stats")]
    public float moveSpeed = 4f;

    private Transform target;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyShooterAttack shooterAttack;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        shooterAttack = GetComponent<EnemyShooterAttack>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        if (runDistance >= shootDistance)
        {
            Debug.LogError("Erro de L�gica: 'runDistance' deve ser MENOR que 'shootDistance'.");
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        Vector2 direction = Vector2.zero;
        bool isMoving = false;

        if (distanceToPlayer < runDistance)
        {
            direction = (transform.position - target.position).normalized;
            isMoving = true;
            shooterAttack.SetCanShoot(false);
        }
        else if (distanceToPlayer <= shootDistance)
        {
            direction = Vector2.zero;
            isMoving = false;
            shooterAttack.SetCanShoot(true);
        }
        else
        {
            direction = (target.position - transform.position).normalized;
            isMoving = true;
            shooterAttack.SetCanShoot(false);
        }

        rb.linearVelocity = direction * moveSpeed;

        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                animator.SetFloat("MoveX", direction.x);
                animator.SetFloat("MoveY", direction.y);
            }
        }
    }
}
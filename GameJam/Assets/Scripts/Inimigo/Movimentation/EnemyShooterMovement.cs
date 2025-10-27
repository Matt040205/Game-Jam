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

    // --- NOVO ---
    private SpriteRenderer spriteRenderer;
    // --- FIM NOVO ---

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        shooterAttack = GetComponent<EnemyShooterAttack>();

        // --- NOVO: Pega o SpriteRenderer ---
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[EnemyShooterMovement] Não foi encontrado um SpriteRenderer em '{gameObject.name}' ou em seus filhos.");
        }
        // --- FIM NOVO ---

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        if (runDistance >= shootDistance)
        {
            Debug.LogError("Erro de Lógica: 'runDistance' deve ser MENOR que 'shootDistance'.");
        }
    }

    void FixedUpdate()
    {
        if (target == null || spriteRenderer == null)
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
            // Fugindo
            direction = (transform.position - target.position).normalized;
            isMoving = true;
            shooterAttack.SetCanShoot(false);
        }
        else if (distanceToPlayer <= shootDistance)
        {
            // Parado para atirar
            direction = Vector2.zero;
            isMoving = false;
            shooterAttack.SetCanShoot(true);
        }
        else
        {
            // Perseguindo
            direction = (target.position - transform.position).normalized;
            isMoving = true;
            shooterAttack.SetCanShoot(false);
        }

        // --- NOVO: Lógica para inverter a Sprite ---
        // Só vira se estivermos ativamente nos movendo
        if (isMoving)
        {
            if (direction.x > 0.01f)
            {
                spriteRenderer.flipX = false; // Olhando para a direita
            }
            else if (direction.x < -0.01f)
            {
                spriteRenderer.flipX = true; // Olhando para a esquerda (invertido)
            }
        }
        // Se não estiver se movendo, ele mantém a última direção (bom para atirar)
        // --- FIM NOVO ---

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
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Transform target;
    private Rigidbody2D rb;
    private Animator animator;

    // --- NOVO ---
    private SpriteRenderer spriteRenderer;
    // --- FIM NOVO ---

    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // --- NOVO: Pega o SpriteRenderer ---
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[EnemyMovement] Não foi encontrado um SpriteRenderer em '{gameObject.name}' ou em seus filhos.");
        }
        // --- FIM NOVO ---

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void FixedUpdate()
    {
        if (!canMove || target == null || spriteRenderer == null)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // --- NOVO: Lógica para inverter a Sprite ---
        // Usamos uma pequena "zona morta" (0.01f) para evitar flips desnecessários
        if (direction.x > 0.01f)
        {
            spriteRenderer.flipX = false; // Olhando para a direita
        }
        else if (direction.x < -0.01f)
        {
            spriteRenderer.flipX = true; // Olhando para a esquerda (invertido)
        }
        // Se direction.x for quase 0, ele mantém a última direção que estava olhando
        // --- FIM NOVO ---

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }
}
using UnityEngine;

public class BossBeam : MonoBehaviour
{
    public float duration = 5f; // Aumentei a duração para dar tempo de cruzar a tela
    public float debuffDuration = 5f;
    public float speed = 8f; // Velocidade do feixe

    private Vector2 moveDirection;

    void Start()
    {
        Destroy(gameObject, duration);
        Debug.Log($"[BossBeam] Start concluído. Velocidade: {speed}. Duração: {duration}s.");
    }

    // Função chamada pelo BossJulgador para definir a direção
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Debug.Log($"[BossBeam] Direção definida para: {moveDirection}");
    }

    void FixedUpdate()
    {
        // Movimenta o feixe na direção definida
        if (moveDirection != Vector2.zero)
        {
            // Usar Translate é mais simples para um objeto que não precisa de física complexa
            transform.Translate(moveDirection * speed * Time.fixedDeltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[BossBeam] Trigger Enter com: {other.name}");
        if (other.CompareTag("Player"))
        {
            PlayerDebuffs playerDebuffs = other.GetComponent<PlayerDebuffs>();
            if (playerDebuffs != null && !playerDebuffs.powersDisabled)
            {
                Debug.Log("[BossBeam] Acertou o Player! Desabilitando poderes.");
                playerDebuffs.ApplyDisablePowers(debuffDuration);
                // Opcional: Destruir o feixe ao acertar o player?
                // Destroy(gameObject);
            }
            else if (playerDebuffs == null)
            {
                Debug.LogWarning("[BossBeam] Acertou Player, mas não encontrou PlayerDebuffs script.");
            }
            else if (playerDebuffs.powersDisabled)
            {
                Debug.Log("[BossBeam] Acertou Player, mas poderes já estavam desabilitados.");
            }
        }
    }
}
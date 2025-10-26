using UnityEngine;

public class BossBeam : MonoBehaviour
{
    public float duration = 5f; // Aumentei a dura��o para dar tempo de cruzar a tela
    public float debuffDuration = 5f;
    public float speed = 8f; // Velocidade do feixe

    private Vector2 moveDirection;

    void Start()
    {
        Destroy(gameObject, duration);
        Debug.Log($"[BossBeam] Start conclu�do. Velocidade: {speed}. Dura��o: {duration}s.");
    }

    // Fun��o chamada pelo BossJulgador para definir a dire��o
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Debug.Log($"[BossBeam] Dire��o definida para: {moveDirection}");
    }

    void FixedUpdate()
    {
        // Movimenta o feixe na dire��o definida
        if (moveDirection != Vector2.zero)
        {
            // Usar Translate � mais simples para um objeto que n�o precisa de f�sica complexa
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
                Debug.LogWarning("[BossBeam] Acertou Player, mas n�o encontrou PlayerDebuffs script.");
            }
            else if (playerDebuffs.powersDisabled)
            {
                Debug.Log("[BossBeam] Acertou Player, mas poderes j� estavam desabilitados.");
            }
        }
    }
}
using UnityEngine;

// Garante que o componente SpriteRenderer exista
[RequireComponent(typeof(SpriteRenderer))]
public class AttackIndicator : MonoBehaviour
{
    [Header("Configura��o")]
    [Tooltip("Dist�ncia que o indicador ficar� do centro do jogador.")]
    public float orbitDistance = 1.5f;
    [Tooltip("Ajuste da rota��o (use -90 se o sprite aponta para CIMA por padr�o).")]
    public float rotationOffset = -90f;

    // --- Refer�ncias ---
    private Transform playerTransform;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Pega o SpriteRenderer garantido pelo RequireComponent
        mainCamera = Camera.main;

        // Verifica se � filho de algum objeto
        if (transform.parent != null)
        {
            playerTransform = transform.parent;
            Debug.Log($"[AttackIndicator] Encontrou pai: {playerTransform.name}. Assumindo que � o Player.");
        }
        else
        {
            // Erro Cr�tico: N�o � filho de ningu�m
            Debug.LogError("[AttackIndicator] ERRO: Este GameObject PRECISA ser filho do Player na Hierarchy!");
            spriteRenderer.enabled = false; // Esconde o indicador
            this.enabled = false; // Desativa o script
            return;
        }

        // Verifica a C�mera
        if (mainCamera == null)
        {
            Debug.LogError("[AttackIndicator] ERRO: C�mera principal (com tag 'MainCamera') n�o encontrada!");
            spriteRenderer.enabled = false;
            this.enabled = false;
            return;
        }

        // Verifica se o Sprite foi atribu�do no Inspector
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("[AttackIndicator] AVISO: Nenhum Sprite foi atribu�do ao SpriteRenderer no Inspetor! O indicador ficar� invis�vel.");
            // N�o desativa o script, apenas avisa.
        }

        Debug.Log("[AttackIndicator] Start conclu�do com sucesso.");
    }

    void Update()
    {
        // Se algo deu errado no Start, sai
        if (!this.enabled || mainCamera == null || playerTransform == null) return;

        // Posi��o do mouse e convers�o para mundo
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.nearClipPlane + Mathf.Abs(playerTransform.position.z - mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // Dire��o Player -> Mouse
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)playerTransform.position).normalized;

        // Se a dire��o for zero (mouse exatamente em cima do player), evita erros
        if (direction == Vector2.zero)
        {
            // Opcional: manter a �ltima dire��o v�lida ou usar uma padr�o
            // Por agora, apenas n�o atualiza a posi��o/rota��o
            return;
        }


        // Define a POSI��O local (relativa ao pai/Player)
        transform.localPosition = direction * orbitDistance;

        // Define a ROTA��O para apontar
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
    }

    public void ShowIndicator(bool show)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = show;
        }
    }
}
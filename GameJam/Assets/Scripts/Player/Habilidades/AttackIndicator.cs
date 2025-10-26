using UnityEngine;

// Garante que o componente SpriteRenderer exista
[RequireComponent(typeof(SpriteRenderer))]
public class AttackIndicator : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Distância que o indicador ficará do centro do jogador.")]
    public float orbitDistance = 1.5f;
    [Tooltip("Ajuste da rotação (use -90 se o sprite aponta para CIMA por padrão).")]
    public float rotationOffset = -90f;

    // --- Referências ---
    private Transform playerTransform;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Pega o SpriteRenderer garantido pelo RequireComponent
        mainCamera = Camera.main;

        // Verifica se é filho de algum objeto
        if (transform.parent != null)
        {
            playerTransform = transform.parent;
            Debug.Log($"[AttackIndicator] Encontrou pai: {playerTransform.name}. Assumindo que é o Player.");
        }
        else
        {
            // Erro Crítico: Não é filho de ninguém
            Debug.LogError("[AttackIndicator] ERRO: Este GameObject PRECISA ser filho do Player na Hierarchy!");
            spriteRenderer.enabled = false; // Esconde o indicador
            this.enabled = false; // Desativa o script
            return;
        }

        // Verifica a Câmera
        if (mainCamera == null)
        {
            Debug.LogError("[AttackIndicator] ERRO: Câmera principal (com tag 'MainCamera') não encontrada!");
            spriteRenderer.enabled = false;
            this.enabled = false;
            return;
        }

        // Verifica se o Sprite foi atribuído no Inspector
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("[AttackIndicator] AVISO: Nenhum Sprite foi atribuído ao SpriteRenderer no Inspetor! O indicador ficará invisível.");
            // Não desativa o script, apenas avisa.
        }

        Debug.Log("[AttackIndicator] Start concluído com sucesso.");
    }

    void Update()
    {
        // Se algo deu errado no Start, sai
        if (!this.enabled || mainCamera == null || playerTransform == null) return;

        // Posição do mouse e conversão para mundo
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.nearClipPlane + Mathf.Abs(playerTransform.position.z - mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // Direção Player -> Mouse
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)playerTransform.position).normalized;

        // Se a direção for zero (mouse exatamente em cima do player), evita erros
        if (direction == Vector2.zero)
        {
            // Opcional: manter a última direção válida ou usar uma padrão
            // Por agora, apenas não atualiza a posição/rotação
            return;
        }


        // Define a POSIÇÃO local (relativa ao pai/Player)
        transform.localPosition = direction * orbitDistance;

        // Define a ROTAÇÃO para apontar
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
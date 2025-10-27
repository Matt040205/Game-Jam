using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpawnAndMoveCamera : MonoBehaviour
{
    [Header("Configuração da Horda")]
    public List<GameObject> enemyPrefabs;
    public List<GameObject> spawnAreas;
    [Range(1, 10)] public int minEnemies = 3;
    [Range(1, 10)] public int maxEnemies = 5;
    public float spawnDelay = 0.8f;

    [Header("Configuração da Câmera")]
    public List<Transform> cameraLockPoints;
    public float cameraMoveSpeed = 5f;
    [Tooltip("Tamanho ortográfico da câmera durante a luta (maior = mais afastado).")]
    public float zoomedOutSize = 10f;
    [Tooltip("Velocidade da transição do zoom.")]
    public float zoomSpeed = 3f;

    [Header("Configuração da Arena")]
    [Tooltip("Arraste TODOS os GameObjects de barreira para esta lista.")]
    public GameObject[] arenaBarriers;
    // --- MUDANÇA 1: Tempo para as barreiras fecharem ---
    [Tooltip("Tempo (em segundos) antes das barreiras se tornarem sólidas.")]
    public float barrierLockDelay = 5f;
    // --- FIM DA MUDANÇA ---

    private CameraController mainCameraScript;
    private Camera mainCamera;
    private float originalCameraSize;

    private bool isCombatActive = false;
    private List<GameObject> liveEnemies = new List<GameObject>();
    private Coroutine cameraLockCoroutine;
    private Coroutine cameraZoomCoroutine;
    // --- MUDANÇA 2: Referência para a Corrotina que trava as barreiras ---
    private Coroutine lockBarriersCoroutine;
    // --- FIM DA MUDANÇA ---
    private Transform chosenLockPoint;
    private Collider2D triggerArea;

    void Start()
    {
        // ... (código do Start, igual ao anterior) ...
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraScript = mainCamera.GetComponent<CameraController>();
            if (mainCameraScript == null) Debug.LogError("SpawnAndMoveCamera NÃO ENCONTROU 'CameraController'!");
            originalCameraSize = mainCamera.orthographicSize;
        }
        else Debug.LogError("SpawnAndMoveCamera: Camera.main é NULA!");
        triggerArea = GetComponent<Collider2D>();
        if (!triggerArea.isTrigger) Debug.LogWarning($"Colisor em '{gameObject.name}' NÃO é Trigger.");
        if (enemyPrefabs.Count == 0) Debug.LogWarning($"'{gameObject.name}' não tem 'Enemy Prefabs'.");
        if (spawnAreas == null || spawnAreas.Count == 0) Debug.LogError($"'{gameObject.name}' não tem 'Spawn Areas'!");
        if (cameraLockPoints == null || cameraLockPoints.Count == 0) Debug.LogWarning($"'{gameObject.name}' não tem 'Camera Lock Points'.");

        // Loop para desligar todas as barreiras (igual ao anterior)
        if (arenaBarriers != null && arenaBarriers.Length > 0)
        {
            foreach (GameObject barrier in arenaBarriers)
            {
                if (barrier != null) barrier.SetActive(false);
            }
        }

        Debug.Log($"[SpawnAndMoveCamera] Start para '{gameObject.name}'. Zoom Original: {originalCameraSize}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCombatActive)
        {
            StartCoroutine(CombatSequence());
        }
    }

    private IEnumerator CombatSequence()
    {
        isCombatActive = true;
        Debug.Log($"[SpawnAndMoveCamera] Encontro iniciado em '{gameObject.name}'!");

        // --- MUDANÇA 3: Ligar barreiras como TRIGGERS (Sensores) ---
        if (arenaBarriers != null && arenaBarriers.Length > 0)
        {
            foreach (GameObject barrier in arenaBarriers)
            {
                if (barrier != null)
                {
                    barrier.SetActive(true); // Liga o objeto
                    Collider2D col = barrier.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        col.isTrigger = true; // Garante que é um sensor (deixa passar)
                    }
                }
            }
            Debug.Log("[SpawnAndMoveCamera] Barreiras da Arena ATIVADAS (Modo Sensor).");
        }
        // --- FIM DA MUDANÇA ---

        if (mainCameraScript != null)
        {
            // ... (código de travar a câmera e dar zoom, igual ao anterior) ...
            Debug.Log("Câmera PARADA e Zoom Out iniciado.");
            mainCameraScript.enabled = false;
            if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);
            cameraZoomCoroutine = StartCoroutine(ZoomCamera(zoomedOutSize));
            if (cameraLockPoints != null && cameraLockPoints.Count > 0)
            {
                chosenLockPoint = cameraLockPoints[Random.Range(0, cameraLockPoints.Count)];
                if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
                cameraLockCoroutine = StartCoroutine(LockCameraPosition());
            }
        }

        // ... (código de instanciar inimigos, igual ao anterior) ...
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        Debug.Log($"Instanciando {enemyCount} inimigos...");
        Bounds bounds = triggerArea.bounds;
        for (int i = 0; i < enemyCount; i++)
        {
            // ... (lógica de spawn) ...
            GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject randomSpawnObject = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Collider2D spawnCollider = randomSpawnObject.GetComponent<Collider2D>();
            if (spawnCollider == null) { Debug.LogError($"'{randomSpawnObject.name}' não tem Collider2D!"); continue; }
            bounds = spawnCollider.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);
            GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            liveEnemies.Add(spawnedEnemy);
            yield return new WaitForSeconds(spawnDelay);
        }

        // --- MUDANÇA 4: Iniciar a contagem regressiva para TRANCAR as barreiras ---
        Debug.Log($"Iniciando timer de {barrierLockDelay}s para trancar as barreiras.");
        lockBarriersCoroutine = StartCoroutine(LockBarriersAfterDelay());
        // --- FIM DA MUDANÇA ---

        Debug.Log("Aguardando derrota da horda...");
        while (liveEnemies.Count > 0)
        {
            liveEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("HORDA DERROTADA!");
        EndCombatEncounter();
    }

    // ... (Corrotina LockCameraPosition - igual) ...
    private IEnumerator LockCameraPosition()
    {
        Vector3 targetPos = chosenLockPoint.position;
        targetPos.z = mainCamera.transform.position.z;
        Debug.Log($"[SpawnAndMoveCamera] Movendo câmera para {targetPos}");

        while (mainCameraScript != null && !mainCameraScript.enabled && Vector3.Distance(mainCamera.transform.position, targetPos) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);
            yield return null;
        }
        if (mainCameraScript != null && !mainCameraScript.enabled) mainCamera.transform.position = targetPos;
        Debug.Log("[SpawnAndMoveCamera] Câmera chegou ao ponto.");
    }

    // ... (Corrotina ZoomCamera - igual) ...
    private IEnumerator ZoomCamera(float targetSize)
    {
        float currentSize = mainCamera.orthographicSize;
        Debug.Log($"[SpawnAndMoveCamera] Iniciando Zoom de {currentSize} para {targetSize}");
        while (Mathf.Abs(mainCamera.orthographicSize - targetSize) > 0.05f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.orthographicSize = targetSize;
        Debug.Log($"[SpawnAndMoveCamera] Zoom concluído em {mainCamera.orthographicSize}");
        cameraZoomCoroutine = null;
    }


    // --- MUDANÇA 5: NOVA CORROTINA para trancar as barreiras ---
    /// <summary>
    /// Aguarda um tempo e depois transforma as barreiras em paredes sólidas.
    /// </summary>
    private IEnumerator LockBarriersAfterDelay()
    {
        yield return new WaitForSeconds(barrierLockDelay);

        Debug.Log("[SpawnAndMoveCamera] TEMPO ESGOTADO! Trancando barreiras.");
        if (arenaBarriers != null && arenaBarriers.Length > 0)
        {
            foreach (GameObject barrier in arenaBarriers)
            {
                if (barrier != null && barrier.activeSelf) // Só mexe se ainda estiver ativa
                {
                    Collider2D col = barrier.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        col.isTrigger = false; // Transforma em PAREDE SÓLIDA
                    }
                }
            }
        }
        lockBarriersCoroutine = null; // Limpa a referência
    }
    // --- FIM DA MUDANÇA ---


    private void EndCombatEncounter()
    {
        Debug.Log("[SpawnAndMoveCamera] EndCombatEncounter.");

        // Para a corrotina que ia trancar as portas, caso a luta termine antes
        if (lockBarriersCoroutine != null)
        {
            StopCoroutine(lockBarriersCoroutine);
            lockBarriersCoroutine = null;
        }

        // Loop para desligar todas as barreiras (igual ao anterior)
        if (arenaBarriers != null && arenaBarriers.Length > 0)
        {
            foreach (GameObject barrier in arenaBarriers)
            {
                if (barrier != null) barrier.SetActive(false);
            }
            Debug.Log("[SpawnAndMoveCamera] Barreiras da Arena DESATIVADAS.");
        }

        // ... (código para parar câmera e zoom, igual ao anterior) ...
        if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
        if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);
        cameraZoomCoroutine = StartCoroutine(ZoomCamera(originalCameraSize));
        if (mainCameraScript != null)
        {
            Debug.Log("Câmera LIBERTADA.");
            mainCameraScript.enabled = true;
        }

        Destroy(gameObject);
    }
}
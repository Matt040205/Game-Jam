using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpawnAndMoveCamera : MonoBehaviour
{
    [Header("Configura��o da Horda")]
    public List<GameObject> enemyPrefabs;
    public List<GameObject> spawnAreas;
    [Range(1, 10)] public int minEnemies = 3;
    [Range(1, 10)] public int maxEnemies = 5;
    public float spawnDelay = 0.8f;

    [Header("Configura��o da C�mera")]
    public List<Transform> cameraLockPoints;
    public float cameraMoveSpeed = 5f;
    [Tooltip("Tamanho ortogr�fico da c�mera durante a luta (maior = mais afastado).")]
    public float zoomedOutSize = 10f;
    [Tooltip("Velocidade da transi��o do zoom.")]
    public float zoomSpeed = 3f;

    [Header("Configura��o da Arena")]
    [Tooltip("Arraste TODOS os GameObjects de barreira para esta lista.")]
    public GameObject[] arenaBarriers;
    // --- MUDAN�A 1: Tempo para as barreiras fecharem ---
    [Tooltip("Tempo (em segundos) antes das barreiras se tornarem s�lidas.")]
    public float barrierLockDelay = 5f;
    // --- FIM DA MUDAN�A ---

    private CameraController mainCameraScript;
    private Camera mainCamera;
    private float originalCameraSize;

    private bool isCombatActive = false;
    private List<GameObject> liveEnemies = new List<GameObject>();
    private Coroutine cameraLockCoroutine;
    private Coroutine cameraZoomCoroutine;
    // --- MUDAN�A 2: Refer�ncia para a Corrotina que trava as barreiras ---
    private Coroutine lockBarriersCoroutine;
    // --- FIM DA MUDAN�A ---
    private Transform chosenLockPoint;
    private Collider2D triggerArea;

    void Start()
    {
        // ... (c�digo do Start, igual ao anterior) ...
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraScript = mainCamera.GetComponent<CameraController>();
            if (mainCameraScript == null) Debug.LogError("SpawnAndMoveCamera N�O ENCONTROU 'CameraController'!");
            originalCameraSize = mainCamera.orthographicSize;
        }
        else Debug.LogError("SpawnAndMoveCamera: Camera.main � NULA!");
        triggerArea = GetComponent<Collider2D>();
        if (!triggerArea.isTrigger) Debug.LogWarning($"Colisor em '{gameObject.name}' N�O � Trigger.");
        if (enemyPrefabs.Count == 0) Debug.LogWarning($"'{gameObject.name}' n�o tem 'Enemy Prefabs'.");
        if (spawnAreas == null || spawnAreas.Count == 0) Debug.LogError($"'{gameObject.name}' n�o tem 'Spawn Areas'!");
        if (cameraLockPoints == null || cameraLockPoints.Count == 0) Debug.LogWarning($"'{gameObject.name}' n�o tem 'Camera Lock Points'.");

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

        // --- MUDAN�A 3: Ligar barreiras como TRIGGERS (Sensores) ---
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
                        col.isTrigger = true; // Garante que � um sensor (deixa passar)
                    }
                }
            }
            Debug.Log("[SpawnAndMoveCamera] Barreiras da Arena ATIVADAS (Modo Sensor).");
        }
        // --- FIM DA MUDAN�A ---

        if (mainCameraScript != null)
        {
            // ... (c�digo de travar a c�mera e dar zoom, igual ao anterior) ...
            Debug.Log("C�mera PARADA e Zoom Out iniciado.");
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

        // ... (c�digo de instanciar inimigos, igual ao anterior) ...
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        Debug.Log($"Instanciando {enemyCount} inimigos...");
        Bounds bounds = triggerArea.bounds;
        for (int i = 0; i < enemyCount; i++)
        {
            // ... (l�gica de spawn) ...
            GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject randomSpawnObject = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Collider2D spawnCollider = randomSpawnObject.GetComponent<Collider2D>();
            if (spawnCollider == null) { Debug.LogError($"'{randomSpawnObject.name}' n�o tem Collider2D!"); continue; }
            bounds = spawnCollider.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);
            GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            liveEnemies.Add(spawnedEnemy);
            yield return new WaitForSeconds(spawnDelay);
        }

        // --- MUDAN�A 4: Iniciar a contagem regressiva para TRANCAR as barreiras ---
        Debug.Log($"Iniciando timer de {barrierLockDelay}s para trancar as barreiras.");
        lockBarriersCoroutine = StartCoroutine(LockBarriersAfterDelay());
        // --- FIM DA MUDAN�A ---

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
        Debug.Log($"[SpawnAndMoveCamera] Movendo c�mera para {targetPos}");

        while (mainCameraScript != null && !mainCameraScript.enabled && Vector3.Distance(mainCamera.transform.position, targetPos) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);
            yield return null;
        }
        if (mainCameraScript != null && !mainCameraScript.enabled) mainCamera.transform.position = targetPos;
        Debug.Log("[SpawnAndMoveCamera] C�mera chegou ao ponto.");
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
        Debug.Log($"[SpawnAndMoveCamera] Zoom conclu�do em {mainCamera.orthographicSize}");
        cameraZoomCoroutine = null;
    }


    // --- MUDAN�A 5: NOVA CORROTINA para trancar as barreiras ---
    /// <summary>
    /// Aguarda um tempo e depois transforma as barreiras em paredes s�lidas.
    /// </summary>
    private IEnumerator LockBarriersAfterDelay()
    {
        yield return new WaitForSeconds(barrierLockDelay);

        Debug.Log("[SpawnAndMoveCamera] TEMPO ESGOTADO! Trancando barreiras.");
        if (arenaBarriers != null && arenaBarriers.Length > 0)
        {
            foreach (GameObject barrier in arenaBarriers)
            {
                if (barrier != null && barrier.activeSelf) // S� mexe se ainda estiver ativa
                {
                    Collider2D col = barrier.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        col.isTrigger = false; // Transforma em PAREDE S�LIDA
                    }
                }
            }
        }
        lockBarriersCoroutine = null; // Limpa a refer�ncia
    }
    // --- FIM DA MUDAN�A ---


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

        // ... (c�digo para parar c�mera e zoom, igual ao anterior) ...
        if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
        if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);
        cameraZoomCoroutine = StartCoroutine(ZoomCamera(originalCameraSize));
        if (mainCameraScript != null)
        {
            Debug.Log("C�mera LIBERTADA.");
            mainCameraScript.enabled = true;
        }

        Destroy(gameObject);
    }
}
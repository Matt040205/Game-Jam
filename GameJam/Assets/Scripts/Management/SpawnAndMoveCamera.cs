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
    public float zoomedOutSize = 10f; // <-- NOVO: Tamanho do Zoom Out
    [Tooltip("Velocidade da transição do zoom.")]
    public float zoomSpeed = 3f; // <-- NOVO: Velocidade do Zoom
    private CameraController mainCameraScript;
    private Camera mainCamera;
    private float originalCameraSize; // <-- NOVO: Guarda o zoom original

    private bool isCombatActive = false;
    private List<GameObject> liveEnemies = new List<GameObject>();
    private Coroutine cameraLockCoroutine;
    private Coroutine cameraZoomCoroutine; // <-- NOVO: Corrotina para o zoom
    private Transform chosenLockPoint;
    private Collider2D triggerArea;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraScript = mainCamera.GetComponent<CameraController>();
            if (mainCameraScript == null) Debug.LogError("SpawnAndMoveCamera NÃO ENCONTROU 'CameraController'!");
            originalCameraSize = mainCamera.orthographicSize; // <-- NOVO: Guarda o tamanho inicial
        }
        else Debug.LogError("SpawnAndMoveCamera: Camera.main é NULA!");

        triggerArea = GetComponent<Collider2D>();
        if (!triggerArea.isTrigger) Debug.LogWarning($"Colisor em '{gameObject.name}' NÃO é Trigger.");
        if (enemyPrefabs.Count == 0) Debug.LogWarning($"'{gameObject.name}' não tem 'Enemy Prefabs'.");
        if (spawnAreas == null || spawnAreas.Count == 0) Debug.LogError($"'{gameObject.name}' não tem 'Spawn Areas'!");
        if (cameraLockPoints == null || cameraLockPoints.Count == 0) Debug.LogWarning($"'{gameObject.name}' não tem 'Camera Lock Points'.");
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

        if (mainCameraScript != null)
        {
            Debug.Log("Câmera PARADA e Zoom Out iniciado.");
            mainCameraScript.enabled = false;

            // Inicia Zoom Out
            if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);
            cameraZoomCoroutine = StartCoroutine(ZoomCamera(zoomedOutSize)); // <-- NOVO

            if (cameraLockPoints != null && cameraLockPoints.Count > 0)
            {
                chosenLockPoint = cameraLockPoints[Random.Range(0, cameraLockPoints.Count)];
                if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
                cameraLockCoroutine = StartCoroutine(LockCameraPosition());
            }
        }

        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        Debug.Log($"Instanciando {enemyCount} inimigos...");
        // ... (lógica de spawn igual) ...
        Bounds bounds = triggerArea.bounds; // Default se spawnAreas falhar
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject randomSpawnObject = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Collider2D spawnCollider = randomSpawnObject.GetComponent<Collider2D>();

            if (spawnCollider == null)
            {
                Debug.LogError($"'{randomSpawnObject.name}' não tem Collider2D!"); continue;
            }
            bounds = spawnCollider.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);
            GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            liveEnemies.Add(spawnedEnemy);
            yield return new WaitForSeconds(spawnDelay);
        }


        Debug.Log("Aguardando derrota da horda...");
        while (liveEnemies.Count > 0)
        {
            liveEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("HORDA DERROTADA!");
        EndCombatEncounter();
    }

    // Corrotina para mover a câmera
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

    // --- NOVO: Corrotina para o Zoom ---
    private IEnumerator ZoomCamera(float targetSize)
    {
        float currentSize = mainCamera.orthographicSize;
        Debug.Log($"[SpawnAndMoveCamera] Iniciando Zoom de {currentSize} para {targetSize}");
        while (Mathf.Abs(mainCamera.orthographicSize - targetSize) > 0.05f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.orthographicSize = targetSize; // Garante o valor exato no final
        Debug.Log($"[SpawnAndMoveCamera] Zoom concluído em {mainCamera.orthographicSize}");
        cameraZoomCoroutine = null; // Libera a corrotina
    }
    // --- FIM NOVO ---

    private void EndCombatEncounter()
    {
        Debug.Log("[SpawnAndMoveCamera] EndCombatEncounter.");
        // Para corrotinas de movimento e zoom se estiverem ativas
        if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
        if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);

        // Inicia Zoom In de volta ao original
        cameraZoomCoroutine = StartCoroutine(ZoomCamera(originalCameraSize)); // <-- NOVO

        if (mainCameraScript != null)
        {
            Debug.Log("Câmera LIBERTADA.");
            mainCameraScript.enabled = true; // Reativa o CameraController
        }

        Destroy(gameObject);
    }
}
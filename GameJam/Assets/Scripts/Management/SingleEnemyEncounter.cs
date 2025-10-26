using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SingleEnemyEncounter : MonoBehaviour
{
    [Header("Configuração do Inimigo")]
    public GameObject specificEnemyPrefab;

    [Header("Configuração da Câmera")]
    public List<Transform> cameraLockPoints;
    public float cameraMoveSpeed = 5f;
    [Tooltip("Tamanho ortográfico da câmera durante a luta (maior = mais afastado).")]
    public float zoomedOutSize = 10f; // <-- NOVO
    [Tooltip("Velocidade da transição do zoom.")]
    public float zoomSpeed = 3f; // <-- NOVO
    private CameraController mainCameraScript;
    private Camera mainCamera;
    private float originalCameraSize; // <-- NOVO

    private bool isCombatActive = false;
    private GameObject spawnedEnemy;
    private Coroutine cameraLockCoroutine;
    private Coroutine cameraZoomCoroutine; // <-- NOVO
    private Transform chosenLockPoint;
    private Collider2D triggerArea;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraScript = mainCamera.GetComponent<CameraController>();
            if (mainCameraScript == null) Debug.LogError("SingleEnemyEncounter NÃO ENCONTROU 'CameraController'!");
            originalCameraSize = mainCamera.orthographicSize; // <-- NOVO
        }
        else Debug.LogError("SingleEnemyEncounter: Camera.main é NULA!");

        triggerArea = GetComponent<Collider2D>();
        if (!triggerArea.isTrigger) Debug.LogWarning($"Colisor em '{gameObject.name}' NÃO é Trigger.");
        if (specificEnemyPrefab == null) Debug.LogError($"'{gameObject.name}' não tem 'Specific Enemy Prefab'.");
        if (cameraLockPoints == null || cameraLockPoints.Count == 0) Debug.LogWarning($"'{gameObject.name}' não tem 'Camera Lock Points'.");
        Debug.Log($"[SingleEnemyEncounter] Start para '{gameObject.name}'. Zoom Original: {originalCameraSize}");
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
        Debug.Log($"[SingleEnemyEncounter] Encontro iniciado em '{gameObject.name}'!");

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

        Debug.Log($"Instanciando inimigo: {specificEnemyPrefab.name}...");
        Vector3 spawnPos = new Vector3(29.5f, 12.5f, 0f);
        spawnedEnemy = Instantiate(specificEnemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Inimigo instanciado em: {spawnPos}");

        Debug.Log("Aguardando derrota do inimigo...");
        while (spawnedEnemy != null)
        {
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Inimigo especial DERROTADO!");
        EndCombatEncounter();
    }

    // Corrotina para mover a câmera
    private IEnumerator LockCameraPosition()
    {
        Vector3 targetPos = chosenLockPoint.position;
        targetPos.z = mainCamera.transform.position.z;
        Debug.Log($"[SingleEnemyEncounter] Movendo câmera para {targetPos}");

        while (mainCameraScript != null && !mainCameraScript.enabled && Vector3.Distance(mainCamera.transform.position, targetPos) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);
            yield return null;
        }
        if (mainCameraScript != null && !mainCameraScript.enabled) mainCamera.transform.position = targetPos;
        Debug.Log("[SingleEnemyEncounter] Câmera chegou ao ponto.");
    }

    // --- NOVO: Corrotina para o Zoom (igual à do outro script) ---
    private IEnumerator ZoomCamera(float targetSize)
    {
        float currentSize = mainCamera.orthographicSize;
        Debug.Log($"[SingleEnemyEncounter] Iniciando Zoom de {currentSize} para {targetSize}");
        while (Mathf.Abs(mainCamera.orthographicSize - targetSize) > 0.05f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.orthographicSize = targetSize;
        Debug.Log($"[SingleEnemyEncounter] Zoom concluído em {mainCamera.orthographicSize}");
        cameraZoomCoroutine = null;
    }
    // --- FIM NOVO ---

    private void EndCombatEncounter()
    {
        Debug.Log("[SingleEnemyEncounter] EndCombatEncounter.");
        // Para corrotinas
        if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
        if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);

        // Inicia Zoom In
        cameraZoomCoroutine = StartCoroutine(ZoomCamera(originalCameraSize)); // <-- NOVO

        if (mainCameraScript != null)
        {
            Debug.Log("Câmera LIBERTADA.");
            mainCameraScript.enabled = true; // Reativa o CameraController
        }

        Debug.Log("Ganhou");
        Destroy(gameObject);
    }
}
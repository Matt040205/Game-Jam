using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SingleEnemyEncounter : MonoBehaviour
{
    [Header("Configura��o do Inimigo")]
    public GameObject specificEnemyPrefab;

    [Header("Configura��o da C�mera")]
    public List<Transform> cameraLockPoints;
    public float cameraMoveSpeed = 5f;
    [Tooltip("Tamanho ortogr�fico da c�mera durante a luta (maior = mais afastado).")]
    public float zoomedOutSize = 10f;
    [Tooltip("Velocidade da transi��o do zoom.")]
    public float zoomSpeed = 3f;

    // --- CAMPO ADICIONADO ---
    [Header("Configura��o da Arena")]
    [Tooltip("GameObject 'pai' que cont�m os colisores da arena.")]
    public GameObject arenaBarriers;
    // --- FIM DA ADI��O ---

    private CameraController mainCameraScript;
    private Camera mainCamera;
    private float originalCameraSize;

    private bool isCombatActive = false;
    private GameObject spawnedEnemy;
    private Coroutine cameraLockCoroutine;
    private Coroutine cameraZoomCoroutine;
    private Transform chosenLockPoint;
    private Collider2D triggerArea;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraScript = mainCamera.GetComponent<CameraController>();
            if (mainCameraScript == null) Debug.LogError("SingleEnemyEncounter N�O ENCONTROU 'CameraController'!");
            originalCameraSize = mainCamera.orthographicSize;
        }
        else Debug.LogError("SingleEnemyEncounter: Camera.main � NULA!");

        triggerArea = GetComponent<Collider2D>();
        if (!triggerArea.isTrigger) Debug.LogWarning($"Colisor em '{gameObject.name}' N�O � Trigger.");
        if (specificEnemyPrefab == null) Debug.LogError($"'{gameObject.name}' n�o tem 'Specific Enemy Prefab'.");
        if (cameraLockPoints == null || cameraLockPoints.Count == 0) Debug.LogWarning($"'{gameObject.name}' n�o tem 'Camera Lock Points'.");

        // --- ADICIONADO: Garante que as barreiras comecem desligadas ---
        if (arenaBarriers != null)
        {
            arenaBarriers.SetActive(false);
        }
        // --- FIM DA ADI��O ---

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

        // --- ADICIONADO: LIGA AS BARREIRAS ---
        if (arenaBarriers != null)
        {
            arenaBarriers.SetActive(true);
            Debug.Log("[SingleEnemyEncounter] Barreiras da Arena ATIVADAS.");
        }
        // --- FIM DA ADI��O ---

        if (mainCameraScript != null)
        {
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

        Debug.Log($"Instanciando inimigo: {specificEnemyPrefab.name}...");
        Vector3 spawnPos = new Vector3(29.5f, 12.5f, 0f); // Nota: Este spawn parece estar fixo no c�digo.
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

    private IEnumerator LockCameraPosition()
    {
        Vector3 targetPos = chosenLockPoint.position;
        targetPos.z = mainCamera.transform.position.z;
        Debug.Log($"[SingleEnemyEncounter] Movendo c�mera para {targetPos}");

        while (mainCameraScript != null && !mainCameraScript.enabled && Vector3.Distance(mainCamera.transform.position, targetPos) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);
            yield return null;
        }
        if (mainCameraScript != null && !mainCameraScript.enabled) mainCamera.transform.position = targetPos;
        Debug.Log("[SingleEnemyEncounter] C�mera chegou ao ponto.");
    }

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
        Debug.Log($"[SingleEnemyEncounter] Zoom conclu�do em {mainCamera.orthographicSize}");
        cameraZoomCoroutine = null;
    }

    private void EndCombatEncounter()
    {
        Debug.Log("[SingleEnemyEncounter] EndCombatEncounter.");

        // --- ADICIONADO: DESLIGA AS BARREIRAS ---
        if (arenaBarriers != null)
        {
            arenaBarriers.SetActive(false);
            Debug.Log("[SingleEnemyEncounter] Barreiras da Arena DESATIVADAS.");
        }
        // --- FIM DA ADI��O ---

        if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
        if (cameraZoomCoroutine != null) StopCoroutine(cameraZoomCoroutine);

        cameraZoomCoroutine = StartCoroutine(ZoomCamera(originalCameraSize));

        if (mainCameraScript != null)
        {
            Debug.Log("C�mera LIBERTADA.");
            mainCameraScript.enabled = true;
        }

        Debug.Log("Ganhou");
        Destroy(gameObject);
    }
}
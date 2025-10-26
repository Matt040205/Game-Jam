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
    private CameraController mainCameraScript;
    private Camera mainCamera;

    private bool isCombatActive = false;
    private GameObject spawnedEnemy;
    private Coroutine cameraLockCoroutine;
    private Transform chosenLockPoint;
    private Collider2D triggerArea;

    void Start()
    {
        mainCamera = Camera.main;
        mainCameraScript = mainCamera.GetComponent<CameraController>();
        triggerArea = GetComponent<Collider2D>();

        if (!triggerArea.isTrigger)
        {
            Debug.LogWarning($"O colisor em '{gameObject.name}' N�O est� marcado como 'Is Trigger'. O encontro pode n�o funcionar.");
        }

        if (mainCameraScript == null)
        {
            Debug.LogError("SingleEnemyEncounter N�O ENCONTROU o script 'CameraController' na c�mera principal!");
        }

        if (specificEnemyPrefab == null)
        {
            Debug.LogError($"SingleEnemyEncounter '{gameObject.name}' n�o tem 'Specific Enemy Prefab' definido.");
        }

        if (cameraLockPoints == null || cameraLockPoints.Count == 0)
        {
            Debug.LogWarning($"SingleEnemyEncounter '{gameObject.name}' n�o tem 'Camera Lock Points' definidos. A c�mera ir� parar no lugar.");
        }
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
            Debug.Log("C�mera PARADA.");
            mainCameraScript.enabled = false;

            if (cameraLockPoints != null && cameraLockPoints.Count > 0)
            {
                chosenLockPoint = cameraLockPoints[Random.Range(0, cameraLockPoints.Count)];
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

    private IEnumerator LockCameraPosition()
    {
        Vector3 targetPos = chosenLockPoint.position;
        targetPos.z = mainCamera.transform.position.z;

        while (mainCameraScript != null && !mainCameraScript.enabled)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);
            yield return null;
        }
    }

    private void EndCombatEncounter()
    {
        if (cameraLockCoroutine != null)
        {
            StopCoroutine(cameraLockCoroutine);
            cameraLockCoroutine = null;
        }

        if (mainCameraScript != null)
        {
            Debug.Log("C�mera LIBERTADA.");
            mainCameraScript.enabled = true;
        }

        Debug.Log("Ganhou");

        Destroy(gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpawnAndMoveCamera : MonoBehaviour
{
    [Header("Configuração da Horda")]
    public List<GameObject> enemyPrefabs;
    public List<GameObject> spawnAreas;

    [Range(1, 10)]
    public int minEnemies = 3;
    [Range(1, 10)]
    public int maxEnemies = 5;

    public float spawnDelay = 0.8f;

    [Header("Configuração da Câmera")]
    public List<Transform> cameraLockPoints;
    public float cameraMoveSpeed = 5f;
    private CameraController mainCameraScript;
    private Camera mainCamera;

    private bool isCombatActive = false;
    private List<GameObject> liveEnemies = new List<GameObject>();
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
            Debug.LogWarning($"O colisor em '{gameObject.name}' NÃO está marcado como 'Is Trigger'. O encontro pode não funcionar.");
        }

        if (mainCameraScript == null)
        {
            Debug.LogError("SpawnAndMoveCamera NÃO ENCONTROU o script 'CameraController' na câmera principal!");
        }

        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"SpawnAndMoveCamera '{gameObject.name}' não tem 'Enemy Prefabs' definidos.");
        }

        if (spawnAreas == null || spawnAreas.Count == 0)
        {
            Debug.LogError($"SpawnAndMoveCamera '{gameObject.name}' não tem 'Spawn Areas' na lista! Onde os inimigos devem nascer?");
        }

        if (cameraLockPoints == null || cameraLockPoints.Count == 0)
        {
            Debug.LogWarning($"SpawnAndMoveCamera '{gameObject.name}' não tem 'Camera Lock Points' definidos. A câmera irá parar no lugar.");
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
        Debug.Log($"[SpawnAndMoveCamera] Encontro iniciado em '{gameObject.name}'!");

        if (mainCameraScript != null)
        {
            Debug.Log("Câmera PARADA.");
            mainCameraScript.enabled = false;

            if (cameraLockPoints != null && cameraLockPoints.Count > 0)
            {
                chosenLockPoint = cameraLockPoints[Random.Range(0, cameraLockPoints.Count)];
                cameraLockCoroutine = StartCoroutine(LockCameraPosition());
            }
        }

        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        Debug.Log($"Instanciando {enemyCount} inimigos...");

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject randomSpawnObject = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Collider2D spawnCollider = randomSpawnObject.GetComponent<Collider2D>();

            if (spawnCollider == null)
            {
                Debug.LogError($"Objeto de Spawn '{randomSpawnObject.name}' não tem um Collider2D! Pulando este inimigo.");
                continue;
            }

            Bounds bounds = spawnCollider.bounds;

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            liveEnemies.Add(spawnedEnemy);

            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log("Todos os inimigos instanciados. Aguardando derrota da horda...");

        while (liveEnemies.Count > 0)
        {
            liveEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("HORDA DERROTADA!");
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
            Debug.Log("Câmera LIBERTADA.");
            mainCameraScript.enabled = true;
        }

        Destroy(gameObject);
    }
}
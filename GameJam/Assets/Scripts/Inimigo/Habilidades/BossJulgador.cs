using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossJulgador : MonoBehaviour
{
    public enum BossState { Phase1, Phase2, Phase3 }
    public BossState currentState = BossState.Phase1;

    [Header("Referências Gerais")]
    private Transform playerTarget;
    private Animator animator;
    private BossHealth bossHealth;
    private Collider2D bossCollider;
    private Rigidbody2D rb; // Mantém a referência
    private GameObject playerObject;
    private Camera mainCamera;

    [Header("Fase 1")]
    public GameObject[] enemyPrefabsToSpawn;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 3;
    public int totalWaves = 2;
    private int currentWave = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    public GameObject beamHazardPrefab;
    public float beamSpawnRate = 10f;
    private float beamTimer;
    private bool isSpawningWave = false;
    public float beamSpawnOffset = 2f;

    [Header("Fase 2 & 3")]
    public float moveSpeed = 6f;
    public GameObject projectilePrefab;
    public GameObject meleeHitbox;
    public int meleeDamage = 30; // Usado pelo BossMeleeDamage.cs
    public Transform projectileFirePoint;
    public float attackCooldown = 3f;
    private float attackTimer;
    private bool isAttacking = false;
    private float teleportChargeTime = 1.5f;

    [Header("Fase 3")]
    public int phase3HealthThreshold = 250;
    public float teleportCooldown = 8f;
    private float teleportTimer;
    public float teleportMaxDistance = 15f;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("[BossJulgador] Câmera Principal não encontrada!");

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) { playerTarget = playerGO.transform; playerObject = playerGO; }
        else { Debug.LogError("[BossJulgador] Start falhou em encontrar o Player!"); }

        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        bossCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>(); // Pega o Rigidbody2D

        if (meleeHitbox != null) meleeHitbox.SetActive(false);
        else Debug.LogError("[BossJulgador] 'Melee Hitbox' não atribuída!");

        StartPhase1();
        Debug.Log("[BossJulgador] Start concluído.");
    }

    void Update()
    {
        if (playerTarget == null) return;
        if (currentState == BossState.Phase1) UpdatePhase1();
        else if (currentState == BossState.Phase2) UpdatePhase2();
        else if (currentState == BossState.Phase3) UpdatePhase3();
    }

    void StartPhase1()
    {
        currentState = BossState.Phase1;
        if (bossHealth != null) bossHealth.enabled = false;
        if (bossCollider != null) bossCollider.enabled = false;
        if (rb != null) rb.isKinematic = true; // Continua Kinematic
        isSpawningWave = true;
        StartCoroutine(SpawnWave());
        beamTimer = beamSpawnRate;
        Debug.Log("[BossJulgador] BOSS: FASE 1 COMEÇOU.");
    }

    void UpdatePhase1()
    {
        spawnedEnemies.RemoveAll(item => item == null);
        if (spawnedEnemies.Count == 0 && currentWave < totalWaves && !isSpawningWave)
        {
            Debug.Log("[BossJulgador] Fase 1: Wave derrotada, iniciando próxima wave.");
            isSpawningWave = true; StartCoroutine(SpawnWave());
        }
        else if (spawnedEnemies.Count == 0 && currentWave == totalWaves && !isSpawningWave)
        {
            Debug.Log("[BossJulgador] Fase 1: Última wave derrotada! Transicionando para Fase 2.");
            StartPhase2();
        }
        beamTimer -= Time.deltaTime;
        if (beamTimer <= 0) { SpawnBeam(); beamTimer = beamSpawnRate; }
    }

    IEnumerator SpawnWave()
    {
        isSpawningWave = true;
        currentWave++;
        Debug.Log($"[BossJulgador] Fase 1: Iniciando Wave {currentWave}/{totalWaves}");
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (enemyPrefabsToSpawn == null || enemyPrefabsToSpawn.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[BossJulgador] SpawnWave: Prefabs ou SpawnPoints não configurados!");
                yield break;
            }
            GameObject enemyPrefab = enemyPrefabsToSpawn[Random.Range(0, enemyPrefabsToSpawn.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject spawned = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            spawnedEnemies.Add(spawned);
            Debug.Log($"[BossJulgador] Fase 1: Inimigo {i + 1}/{enemiesPerWave} da wave {currentWave} instanciado.");
            yield return new WaitForSeconds(1f);
        }

        isSpawningWave = false;
        Debug.Log($"[BossJulgador] Fase 1: Wave {currentWave} totalmente instanciada.");
    }

    void SpawnBeam()
    {
        if (beamHazardPrefab == null || mainCamera == null) return;
        Debug.Log("[BossJulgador] Fase 1/3: Preparando para soltar Feixe!");
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        Vector2 camPos = mainCamera.transform.position;
        int edge = Random.Range(0, 4);
        Vector2 startPos = Vector2.zero, endPos = Vector2.zero;
        switch (edge)
        {
            case 0: // Cima
                startPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2), camPos.y + camHeight / 2 + beamSpawnOffset);
                endPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2), camPos.y - camHeight / 2 - beamSpawnOffset);
                break;
            case 1: // Baixo
                startPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2), camPos.y - camHeight / 2 - beamSpawnOffset);
                endPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2), camPos.y + camHeight / 2 + beamSpawnOffset);
                break;
            case 2: // Esquerda
                startPos = new Vector2(camPos.x - camWidth / 2 - beamSpawnOffset, Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                endPos = new Vector2(camPos.x + camWidth / 2 + beamSpawnOffset, Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
            case 3: // Direita
                startPos = new Vector2(camPos.x + camWidth / 2 + beamSpawnOffset, Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                endPos = new Vector2(camPos.x - camWidth / 2 - beamSpawnOffset, Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
        }
        Vector2 direction = (endPos - startPos).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        GameObject beam = Instantiate(beamHazardPrefab, startPos, rotation);
        Debug.Log($"[BossJulgador] Feixe instanciado em {startPos} com direção {direction}");
        BossBeam beamScript = beam.GetComponent<BossBeam>();
        if (beamScript != null) beamScript.SetDirection(direction);
        else Debug.LogError("[BossJulgador] Prefab do Feixe não tem o script BossBeam!");
    }

    void StartPhase2()
    {
        currentState = BossState.Phase2;
        if (bossHealth != null) bossHealth.enabled = true;
        if (bossCollider != null) bossCollider.enabled = true;
        if (rb != null) rb.isKinematic = false; // Mudar para Dynamic
        if (animator != null) animator.SetTrigger("StartPhase2");
        attackTimer = attackCooldown;
        Debug.Log("[BossJulgador] BOSS: FASE 2 COMEÇOU.");
    }

    void UpdatePhase2()
    {
        if (bossHealth != null && bossHealth.GetCurrentHealth() <= phase3HealthThreshold)
        {
            Debug.Log("[BossJulgador] Fase 2: Vida abaixo do limite! Transicionando para Fase 3.");
            StartPhase3();
            return;
        }
        if (isAttacking || playerTarget == null || rb == null) return;

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        // --- CORREÇÃO FÍSICA ---
        rb.linearVelocity = direction * moveSpeed; // Usa velocity
        // --- FIM CORREÇÃO ---
        if (animator != null) animator.SetBool("isMoving", true);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            Debug.Log("[BossJulgador] Fase 2: Timer de ataque zerou, escolhendo ataque.");
            ChooseAttack();
            attackTimer = attackCooldown;
        }
    }

    void StartPhase3()
    {
        currentState = BossState.Phase3;
        teleportTimer = teleportCooldown;
        beamTimer = beamSpawnRate;
        Debug.Log("[BossJulgador] BOSS: FASE 3 COMEÇOU.");
    }

    void UpdatePhase3()
    {
        if (isAttacking || playerTarget == null || rb == null) return;

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        // --- CORREÇÃO FÍSICA ---
        rb.linearVelocity = direction * moveSpeed; // Usa velocity
        // --- FIM CORREÇÃO ---
        if (animator != null) animator.SetBool("isMoving", true);

        beamTimer -= Time.deltaTime;
        if (beamTimer <= 0) { SpawnBeam(); beamTimer = beamSpawnRate; }
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            Debug.Log("[BossJulgador] Fase 3: Timer de ataque zerou, escolhendo ataque Phase 3.");
            ChooseAttackPhase3();
            attackTimer = attackCooldown;
        }
        teleportTimer -= Time.deltaTime;
        if (teleportTimer <= 0)
        {
            Debug.Log("[BossJulgador] Fase 3: Timer de teleporte zerou.");
            StartCoroutine(AttackTeleport());
            teleportTimer = teleportCooldown;
        }
    }

    void ChooseAttack() // Fase 2
    {
        if (playerTarget == null) return;
        float distance = Vector2.Distance(transform.position, playerTarget.position);
        if (distance < 3f)
        {
            Debug.Log("[BossJulgador] ChooseAttack (Fase 2): Distância curta, escolhendo Melee.");
            StartCoroutine(AttackMelee());
        }
        else
        {
            Debug.Log("[BossJulgador] ChooseAttack (Fase 2): Distância longa, escolhendo Projétil.");
            StartCoroutine(AttackProjectile());
        }
    }

    void ChooseAttackPhase3() // Fase 3
    {
        if (playerTarget == null) return;
        float distance = Vector2.Distance(transform.position, playerTarget.position);

        if (Random.Range(0f, 1f) < 0.25f)
        {
            Debug.Log("[BossJulgador] ChooseAttack (Fase 3): Escolheu Teleporte por chance.");
            StartCoroutine(AttackTeleport());
        }
        else if (distance < 3f)
        {
            Debug.Log("[BossJulgador] ChooseAttack (Fase 3): Distância curta, escolhendo Melee.");
            StartCoroutine(AttackMelee());
        }
        else
        {
            Debug.Log("[BossJulgador] ChooseAttack (Fase 3): Distância longa, escolhendo Projétil.");
            StartCoroutine(AttackProjectile());
        }
    }

    IEnumerator AttackProjectile()
    {
        isAttacking = true;
        if (rb != null) rb.linearVelocity = Vector2.zero; // Corrigido
        if (animator != null) { animator.SetBool("isMoving", false); animator.SetTrigger("Attack1"); }
        Debug.Log("[BossJulgador] Corotina AttackProjectile iniciada.");
        yield return new WaitForSeconds(0.5f);
        if (projectilePrefab == null || projectileFirePoint == null || playerTarget == null)
        {
            Debug.LogError("[BossJulgador] AttackProjectile: Prefab, FirePoint ou PlayerTarget nulo!");
            isAttacking = false; yield break;
        }
        GameObject proj = Instantiate(projectilePrefab, projectileFirePoint.position, Quaternion.identity);
        BossControlDebuff debuffControl = proj.GetComponent<BossControlDebuff>();
        if (debuffControl != null)
        {
            debuffControl.SetDirection((playerTarget.position - projectileFirePoint.position).normalized);
            Debug.Log("[BossJulgador] Projétil instanciado e direção definida.");
        }
        else { Debug.LogError("[BossJulgador] Projectile prefab não tem BossControlDebuff anexado!"); }
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        Debug.Log("[BossJulgador] Corotina AttackProjectile finalizada.");
    }

    IEnumerator AttackMelee()
    {
        isAttacking = true;
        if (rb != null) rb.linearVelocity = Vector2.zero; // Corrigido
        if (animator != null) { animator.SetBool("isMoving", false); animator.SetTrigger("Attack2"); }
        Debug.Log("[BossJulgador] Corotina AttackMelee iniciada.");
        yield return new WaitForSeconds(0.3f);
        if (meleeHitbox != null)
        {
            Debug.Log("[BossJulgador] >>> ATIVANDO Melee Hitbox <<<");
            meleeHitbox.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            Debug.Log("[BossJulgador] >>> DESATIVANDO Melee Hitbox <<<");
            meleeHitbox.SetActive(false);
        }
        else { Debug.LogError("[BossJulgador] Melee Hitbox não atribuída!"); }
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        Debug.Log("[BossJulgador] Corotina AttackMelee finalizada.");
    }

    IEnumerator AttackTeleport()
    {
        isAttacking = true;
        if (rb != null) rb.linearVelocity = Vector2.zero; // Corrigido
        if (animator != null) animator.SetBool("isMoving", false);
        Debug.Log("[BossJulgador] Corotina AttackTeleport iniciada.");
        if (animator != null) animator.SetTrigger("TeleportCharge");
        yield return new WaitForSeconds(teleportChargeTime);
        if (playerTarget == null)
        {
            Debug.LogWarning("[BossJulgador] AttackTeleport: PlayerTarget é nulo, cancelando teleporte.");
            isAttacking = false; yield break;
        }
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 targetPosition = (Vector2)playerTarget.position + randomDirection * teleportMaxDistance;
        float minTeleportDist = 5f;
        if (Vector2.Distance(targetPosition, (Vector2)playerTarget.position) < minTeleportDist)
        {
            Debug.Log("[BossJulgador] Posição de teleporte muito perto, ajustando para longe.");
            targetPosition = (Vector2)playerTarget.position - randomDirection * (teleportMaxDistance * 0.5f);
        }
        transform.position = targetPosition;
        Debug.Log($"[BossJulgador] Boss Teleportado para: {targetPosition}");
        if (animator != null) animator.SetTrigger("TeleportFinish");
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        Debug.Log("[BossJulgador] Corotina AttackTeleport finalizada.");
    }
}
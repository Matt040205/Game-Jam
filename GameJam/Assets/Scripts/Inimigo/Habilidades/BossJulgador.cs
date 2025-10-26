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
    private Rigidbody2D rb;

    [Header("Fase 1: Invocação")]
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

    [Header("Fase 2 & 3: Batalha")]
    public float moveSpeed = 6f;
    public GameObject projectilePrefab;
    public GameObject meleeHitbox;
    public Transform projectileFirePoint;
    public float attackCooldown = 3f;
    private float attackTimer;
    private bool isAttacking = false;
    private float teleportChargeTime = 1.5f;

    [Header("Fase 3: Transição & Teleporte")]
    public int phase3HealthThreshold = 250;
    public float teleportCooldown = 8f;
    private float teleportTimer;
    public float teleportMaxDistance = 15f;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        bossCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        StartPhase1();
    }

    void Update()
    {
        if (playerTarget == null) return;

        if (currentState == BossState.Phase1)
        {
            UpdatePhase1();
        }
        else if (currentState == BossState.Phase2)
        {
            UpdatePhase2();
        }
        else if (currentState == BossState.Phase3)
        {
            UpdatePhase3();
        }
    }

    void StartPhase1()
    {
        currentState = BossState.Phase1;
        bossHealth.enabled = false;
        bossCollider.enabled = false;
        rb.isKinematic = true;
        Debug.Log("BOSS: FASE 1 COMEÇOU.");

        isSpawningWave = true;
        StartCoroutine(SpawnWave());

        beamTimer = beamSpawnRate;
    }

    void UpdatePhase1()
    {
        spawnedEnemies.RemoveAll(item => item == null);

        if (spawnedEnemies.Count == 0 && currentWave < totalWaves && !isSpawningWave)
        {
            isSpawningWave = true;
            StartCoroutine(SpawnWave());
        }
        else if (spawnedEnemies.Count == 0 && currentWave == totalWaves && !isSpawningWave)
        {
            Debug.Log("Última wave da Fase 1 derrotada!");
            StartPhase2();
        }

        beamTimer -= Time.deltaTime;
        if (beamTimer <= 0)
        {
            SpawnBeam();
            beamTimer = beamSpawnRate;
        }
    }

    IEnumerator SpawnWave()
    {
        isSpawningWave = true;
        currentWave++;
        Debug.Log($"Iniciando Wave {currentWave}/{totalWaves}");
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < enemiesPerWave; i++)
        {
            GameObject enemyPrefab = enemyPrefabsToSpawn[Random.Range(0, enemyPrefabsToSpawn.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject spawned = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            spawnedEnemies.Add(spawned);
            yield return new WaitForSeconds(1f);
        }

        isSpawningWave = false;
        Debug.Log("Wave instanciada.");
    }

    void SpawnBeam()
    {
        Debug.Log("Boss: Soltando Feixe!");
        Vector3 spawnPos = playerTarget.position + (Vector3.up * 5);
        Instantiate(beamHazardPrefab, spawnPos, Quaternion.identity);
    }

    void StartPhase2()
    {
        currentState = BossState.Phase2;
        bossHealth.enabled = true;
        bossCollider.enabled = true;
        rb.isKinematic = false;
        animator.SetTrigger("StartPhase2");
        attackTimer = attackCooldown;
        Debug.Log("BOSS: FASE 2 COMEÇOU. Ele desceu para a batalha!");
    }

    void UpdatePhase2()
    {
        if (bossHealth.GetCurrentHealth() <= phase3HealthThreshold)
        {
            StartPhase3();
            return;
        }

        if (isAttacking) return;

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        animator.SetBool("isMoving", true);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            ChooseAttack();
            attackTimer = attackCooldown;
        }
    }

    void StartPhase3()
    {
        currentState = BossState.Phase3;
        teleportTimer = teleportCooldown;
        beamTimer = beamSpawnRate;
        Debug.Log("BOSS: FASE 3 COMEÇOU. Mistura de ataques e teleporte!");
    }

    void UpdatePhase3()
    {
        if (isAttacking) return;

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        animator.SetBool("isMoving", true);

        beamTimer -= Time.deltaTime;
        if (beamTimer <= 0)
        {
            SpawnBeam();
            beamTimer = beamSpawnRate;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            ChooseAttackPhase3();
            attackTimer = attackCooldown;
        }

        teleportTimer -= Time.deltaTime;
        if (teleportTimer <= 0)
        {
            StartCoroutine(AttackTeleport());
            teleportTimer = teleportCooldown;
        }
    }

    void ChooseAttack()
    {
        float distance = Vector2.Distance(transform.position, playerTarget.position);
        if (distance < 3f)
        {
            StartCoroutine(AttackMelee());
        }
        else
        {
            StartCoroutine(AttackProjectile());
        }
    }

    void ChooseAttackPhase3()
    {
        float distance = Vector2.Distance(transform.position, playerTarget.position);

        if (Random.Range(0f, 1f) < 0.25f)
        {
            StartCoroutine(AttackTeleport());
        }
        else if (distance < 3f)
        {
            StartCoroutine(AttackMelee());
        }
        else
        {
            StartCoroutine(AttackProjectile());
        }
    }

    IEnumerator AttackProjectile()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("Attack1");
        Debug.Log("Boss: Ataque 1 (Projétil)");

        yield return new WaitForSeconds(0.5f);

        GameObject proj = Instantiate(projectilePrefab, projectileFirePoint.position, Quaternion.identity);

        BossControlDebuff debuffControl = proj.GetComponent<BossControlDebuff>();
        if (debuffControl != null)
        {
            debuffControl.SetDirection((playerTarget.position - projectileFirePoint.position).normalized);
        }
        else
        {
            Debug.LogError("Projectile prefab não tem BossControlDebuff anexado!");
        }

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator AttackMelee()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("Attack2");
        Debug.Log("Boss: Ataque 2 (Melee)");

        yield return new WaitForSeconds(0.3f);

        meleeHitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        meleeHitbox.SetActive(false);

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    IEnumerator AttackTeleport()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        Debug.Log("Boss: INICIANDO TELEPORTE!");

        animator.SetTrigger("TeleportCharge");
        yield return new WaitForSeconds(teleportChargeTime);

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 targetPosition = (Vector2)playerTarget.position + randomDirection * teleportMaxDistance;

        if (Vector2.Distance(targetPosition, (Vector2)playerTarget.position) < 5f)
        {
            targetPosition = (Vector2)playerTarget.position + randomDirection * (teleportMaxDistance * 0.5f);
        }

        transform.position = targetPosition;

        Debug.Log($"Boss Teleportado para: {targetPosition}");

        animator.SetTrigger("TeleportFinish");

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
}
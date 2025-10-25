using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossJulgador : MonoBehaviour
{
    public enum BossState { Phase1, Phase2 }
    public BossState currentState = BossState.Phase1;

    [Header("Refer�ncias Gerais")]
    private Transform playerTarget;
    private Animator animator;
    private BossHealth bossHealth;
    private Collider2D bossCollider;

    [Header("Fase 1: Invoca��o")]
    public GameObject[] enemyPrefabsToSpawn;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 3;
    public int totalWaves = 2;
    private int currentWave = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    public GameObject beamHazardPrefab;
    public float beamSpawnRate = 10f;
    private float beamTimer;

    [Header("Fase 2: Batalha")]
    public float moveSpeed = 6f;
    public GameObject projectilePrefab;
    public GameObject meleeHitbox;
    public Transform projectileFirePoint;
    public float attackCooldown = 3f;
    private float attackTimer;
    private Rigidbody2D rb;
    private bool isAttacking = false;

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
    }

    void StartPhase1()
    {
        currentState = BossState.Phase1;
        bossHealth.enabled = false;
        bossCollider.enabled = false;
        rb.isKinematic = true;
        Debug.Log("BOSS: FASE 1 COME�OU.");
        StartCoroutine(SpawnWave());
        beamTimer = beamSpawnRate;
    }

    void UpdatePhase1()
    {
        spawnedEnemies.RemoveAll(item => item == null);

        if (spawnedEnemies.Count == 0 && currentWave < totalWaves)
        {
            StartCoroutine(SpawnWave());
        }
        else if (spawnedEnemies.Count == 0 && currentWave == totalWaves)
        {
            Debug.Log("�ltima wave da Fase 1 derrotada!");
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
        Debug.Log("BOSS: FASE 2 COME�OU. Ele desceu para a batalha!");
    }

    void UpdatePhase2()
    {
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

    IEnumerator AttackProjectile()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("Attack1");
        Debug.Log("Boss: Ataque 1 (Proj�til)");

        yield return new WaitForSeconds(0.5f);

        GameObject proj = Instantiate(projectilePrefab, projectileFirePoint.position, Quaternion.identity);
        proj.GetComponent<BossControlDebuff>().SetDirection((playerTarget.position - projectileFirePoint.position).normalized);

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
}
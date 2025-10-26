using System.Collections;
using UnityEngine;
using FMODUnity; // <- Adicionado

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")] public float attackChargeTime = 1.0f; public float attackCooldown = 2.0f; public int damageAmount = 15;
    [Header("FMOD Events")]
    [EventRef] public string chargeSoundEvent; // <- Substitui AudioClip
    [EventRef] public string attackSoundEvent; // <- Substitui AudioClip
    // Remove: private AudioSource audioSource;

    private GameObject playerObject;
    private EnemyMovement enemyMovement;
    private Animator animator;
    private bool playerIsInRange = false;
    private bool canAttack = true;

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();
        // Remove: audioSource = GetComponent<AudioSource>();
    }

    void Update() { if (playerIsInRange && canAttack) StartCoroutine(AttackSequence()); }

    private IEnumerator AttackSequence()
    {
        canAttack = false; if (enemyMovement != null) enemyMovement.canMove = false;
        // --- FMOD ---
        if (!string.IsNullOrEmpty(chargeSoundEvent)) RuntimeManager.PlayOneShot(chargeSoundEvent, transform.position);
        // --- FIM FMOD ---
        if (animator != null) animator.SetTrigger("ChargeAttack");
        yield return new WaitForSeconds(attackChargeTime);
        if (playerIsInRange && playerObject != null)
        {
            // --- FMOD ---
            if (!string.IsNullOrEmpty(attackSoundEvent)) RuntimeManager.PlayOneShot(attackSoundEvent, transform.position);
            // --- FIM FMOD ---
            GlobalDamageEvents.FirePlayerDamage(playerObject, damageAmount, transform.position);
            if (animator != null) animator.SetTrigger("Attack");
        }
        if (enemyMovement != null) enemyMovement.canMove = true;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void OnChildTriggerEnter(Collider2D other) { if (other.CompareTag("Player")) { playerIsInRange = true; playerObject = other.gameObject; } }
    public void OnChildTriggerExit(Collider2D other) { if (other.CompareTag("Player")) { playerIsInRange = false; playerObject = null; } }
}
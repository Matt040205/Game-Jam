using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackChargeTime = 1.0f;
    public float attackCooldown = 2.0f;
    public int damageAmount = 15;

    private PlayerHealth playerHealth;
    private EnemyMovement enemyMovement;
    private Animator animator;

    private bool playerIsInRange = false;
    private bool canAttack = true;

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerIsInRange && canAttack)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        canAttack = false;
        if (enemyMovement != null) enemyMovement.canMove = false;

        Debug.Log("Inimigo está a 'carregar' o ataque...");
        if (animator != null) animator.SetTrigger("ChargeAttack");

        yield return new WaitForSeconds(attackChargeTime);

        if (playerIsInRange && playerHealth != null)
            
        {
            Debug.Log("Inimigo ATACA!");
            playerHealth.TakeDamage(damageAmount, transform.position);

            if (animator != null) animator.SetTrigger("Attack");
        }
        else
        {
            Debug.Log("Inimigo cancelou o ataque (Player saiu do alcance).");
        }

        if (enemyMovement != null) enemyMovement.canMove = true;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }


    public void OnChildTriggerEnter(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player ENTROU na área de ataque.");
            playerIsInRange = true;
            playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    public void OnChildTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player SAIU na área de ataque.");
            playerIsInRange = false;
            playerHealth = null;
        }
    }
}
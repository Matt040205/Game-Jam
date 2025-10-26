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

    // MÉTODO START() RESTAURADO: Inicializa componentes necessários
    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();
       
    }

    // MÉTODO UPDATE() RESTAURADO: Checa a condição para iniciar o ataque
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

        
        if (animator != null) animator.SetTrigger("ChargeAttack");

        yield return new WaitForSeconds(attackChargeTime);

        if (playerIsInRange && playerHealth != null)
        {
           

            // CORREÇÃO MANTIDA: Usa o Evento Estático para desacoplamento e dano.
            // (Requer que a classe GlobalDamageEvents exista no seu projeto)
            GlobalDamageEvents.FirePlayerDamage(playerHealth.gameObject, damageAmount, transform.position);

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
            // É seguro pegar o PlayerHealth aqui, pois é feito apenas uma vez por entrada
            playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    public void OnChildTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player SAIU da área de ataque.");
            playerIsInRange = false;
            playerHealth = null;
        }
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackChargeTime = 1.0f;
    public float attackCooldown = 2.0f;
    public int damageAmount = 15;

    [Header("Audio")]
    public AudioClip chargeSound;
    public AudioClip attackSound;
    private AudioSource audioSource;

    private PlayerHealth playerHealth;
    private EnemyMovement enemyMovement;
    private Animator animator;

    private bool playerIsInRange = false;
    private bool canAttack = true;

    // M�TODO START() RESTAURADO: Inicializa componentes necess�rios
    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // M�TODO UPDATE() RESTAURADO: Checa a condi��o para iniciar o ataque
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

        Debug.Log("Inimigo est� a 'carregar' o ataque...");
        if (chargeSound != null)
        {
            audioSource.PlayOneShot(chargeSound);
        }
        if (animator != null) animator.SetTrigger("ChargeAttack");

        yield return new WaitForSeconds(attackChargeTime);

        if (playerIsInRange && playerHealth != null)
        {
            Debug.Log("Inimigo ATACA!");
            if (attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }

            // CORRE��O MANTIDA: Usa o Evento Est�tico para desacoplamento e dano.
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
            Debug.Log("Player ENTROU na �rea de ataque.");
            playerIsInRange = true;
            // � seguro pegar o PlayerHealth aqui, pois � feito apenas uma vez por entrada
            playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    public void OnChildTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player SAIU da �rea de ataque.");
            playerIsInRange = false;
            playerHealth = null;
        }
    }
}
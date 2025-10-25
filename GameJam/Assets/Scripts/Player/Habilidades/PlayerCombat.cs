using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Animator animator;

    public int comboStep = 0;
    public float comboTimeWindow = 0.5f;
    private float lastAttackTime = 0f;

    public GameObject hitboxUp;
    public GameObject hitboxDown;
    public GameObject hitboxLeft;
    public GameObject hitboxRight;

    public GameObject projetilLaserPrefab;
    public GameObject geradorGalvanicoArea;
    public GameObject relogioChapeleiroAreaPrefab;
    public Transform pontoDeDisparo;

    private bool isMantoAtivo = false;
    public float mantoDuracao = 3f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        hitboxUp.SetActive(false);
        hitboxDown.SetActive(false);
        hitboxLeft.SetActive(false);
        hitboxRight.SetActive(false);
        geradorGalvanicoArea.SetActive(false);

        Debug.Log("PlayerCombat Iniciado!");
    }

    void Update()
    {
        if (Time.time - lastAttackTime > comboTimeWindow)
        {
            comboStep = 0;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Habilidade_TripodeMarciano();
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            Habilidade_GeradorGalvanico(true);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Habilidade_GeradorGalvanico(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && !isMantoAtivo)
        {
            StartCoroutine(Habilidade_MantoDeNVOA());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Habilidade_RelogioDoChapeleiro();
        }
    }

    private void Attack()
    {
        if (comboStep == 0)
        {
            comboStep = 1;
        }
        else if (comboStep < 3)
        {
            comboStep++;
        }
        else
        {
            comboStep = 1;
        }

        lastAttackTime = Time.time;

        Debug.Log($"Ataque! Passo do combo: {comboStep}");

        animator.SetTrigger("Attack");
        animator.SetInteger("ComboStep", comboStep);

        StartCoroutine(ActivateHitbox(playerMovement.GetLastMoveDirection()));
    }

    private IEnumerator ActivateHitbox(Vector2 direction)
    {
        GameObject activeHitbox = null;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            activeHitbox = (direction.x > 0) ? hitboxRight : hitboxLeft;
        }
        else
        {
            activeHitbox = (direction.y > 0) ? hitboxUp : hitboxDown;
        }

        Debug.Log($"HITBOX ATIVADA: {activeHitbox.name}");
        activeHitbox.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        activeHitbox.SetActive(false);
    }

    void Habilidade_TripodeMarciano()
    {
        Debug.Log("HABILIDADE: Trípode Marciano (Disparando Laser)");

        GameObject laserObj = Instantiate(projetilLaserPrefab, pontoDeDisparo.position, Quaternion.identity);

        LaserProjectile projectile = laserObj.GetComponent<LaserProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(playerMovement.GetLastMoveDirection());
        }
    }

    void Habilidade_GeradorGalvanico(bool isActive)
    {
        if (geradorGalvanicoArea.activeSelf == isActive) return;

        if (isActive)
        {
            Debug.Log("HABILIDADE: Gerador Galvânico (ATIVADO)");
        }
        else
        {
            Debug.Log("HABILIDADE: Gerador Galvânico (DESATIVADO)");
        }

        geradorGalvanicoArea.SetActive(isActive);
    }

    IEnumerator Habilidade_MantoDeNVOA()
    {
        isMantoAtivo = true;

        Debug.Log("HABILIDADE: Manto de Névoa (ATIVADO - Invisível e Vulnerável)");

        Color tempColor = spriteRenderer.color;
        tempColor.a = 0.5f;
        spriteRenderer.color = tempColor;

        yield return new WaitForSeconds(mantoDuracao);

        Debug.Log("HABILIDADE: Manto de Névoa (DESATIVADO)");

        tempColor.a = 1f;
        spriteRenderer.color = tempColor;
        isMantoAtivo = false;
    }

    void Habilidade_RelogioDoChapeleiro()
    {
        Debug.Log("HABILIDADE: Relógio do Chapeleiro (Criando área de lentidão)");

        Instantiate(relogioChapeleiroAreaPrefab, transform.position, Quaternion.identity);
    }

    public void PlayHurtAnimation(Vector2 damageDirection)
    {
        Debug.Log("Jogador recebeu dano!");

        animator.SetTrigger("Hurt");
    }
}
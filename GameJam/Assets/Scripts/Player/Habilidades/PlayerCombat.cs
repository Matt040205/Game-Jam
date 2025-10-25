using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerCombat : MonoBehaviour
{
    // ENUM PÚBLICO: Define as opções de habilidades para o item coletável.
    public enum AbilityType
    {
        TripodeMarciano,
        GeradorGalvanico,
        MantoDeNVOA,
        RelogioDoChapeleiro
    }

    private PlayerMovement playerMovement;
    private Animator animator;
    private AudioSource audioSource;
    private Camera mainCamera;

    [Header("Unlocks de Habilidades")]
    // Estes booleans controlam se a habilidade pode ser usada
    public bool isTripodeMarcianoUnlocked = false;
    public bool isGeradorGalvanicoUnlocked = false;
    public bool isMantoDeNVOAUnlocked = false;
    public bool isRelogioDoChapeleiroUnlocked = false;

    // SLOTS DE HABILIDADE: Limite de slots (2)
    public int maxAbilitySlots = 2;
    private int currentAbilityCount = 0; // Contador de habilidades ativas

    [Header("Combo")]
    public int comboStep = 0;
    public float comboTimeWindow = 0.5f;
    private float lastAttackTime = 0f;

    [Header("Hitboxes")]
    public GameObject hitboxUp;
    public GameObject hitboxDown;
    public GameObject hitboxLeft;
    public GameObject hitboxRight;

    [Header("Habilidades")]
    public GameObject projetilLaserPrefab;
    public GameObject geradorGalvanicoArea;
    public GameObject relogioChapeleiroAreaPrefab;
    public Transform pontoDeDisparo;

    [Header("Manto")]
    private bool isMantoAtivo = false;
    public float mantoDuracao = 3f;
    private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioClip[] attackSounds;
    public AudioClip laserSound;
    public AudioSource galvanicAudioSource;
    public AudioClip mantoSound;
    public AudioClip relogioSound;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        mainCamera = Camera.main;

        hitboxUp.SetActive(false);
        hitboxDown.SetActive(false);
        hitboxLeft.SetActive(false);
        hitboxRight.SetActive(false);
        geradorGalvanicoArea.SetActive(false);

        // ATUALIZA A CONTAGEM INICIAL DE SLOTS (caso alguma bool comece TRUE)
        if (isTripodeMarcianoUnlocked) currentAbilityCount++;
        if (isGeradorGalvanicoUnlocked) currentAbilityCount++;
        if (isMantoDeNVOAUnlocked) currentAbilityCount++;
        if (isRelogioDoChapeleiroUnlocked) currentAbilityCount++;

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

        // CHECAGEM DE UNLOCK: Alpha1 (Trípode Marciano)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isTripodeMarcianoUnlocked) Habilidade_TripodeMarciano();
            else Debug.Log("Habilidade 'Trípode Marciano' está bloqueada!");
        }

        // CHECAGEM DE UNLOCK: Alpha2 (Gerador Galvânico)
        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(true);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Debug.Log("Habilidade 'Gerador Galvânico' está bloqueada!");
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(false);
        }

        // CHECAGEM DE UNLOCK: Alpha3 (Manto de Névoa)
        if (Input.GetKeyDown(KeyCode.Alpha3) && !isMantoAtivo)
        {
            if (isMantoDeNVOAUnlocked) StartCoroutine(Habilidade_MantoDeNVOA());
            else Debug.Log("Habilidade 'Manto de Névoa' está bloqueada!");
        }

        // CHECAGEM DE UNLOCK: Alpha4 (Relógio do Chapeleiro)
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (isRelogioDoChapeleiroUnlocked) Habilidade_RelogioDoChapeleiro();
            else Debug.Log("Habilidade 'Relógio do Chapeleiro' está bloqueada!");
        }
    }

    // NOVO: Único setter público que o item de desbloqueio chamará
    // Retorna TRUE se a habilidade foi desbloqueada/já estava, FALSE se o slot estava cheio.
    public bool UnlockAbility(AbilityType type)
    {
        // 1. CHECA SE A HABILIDADE JÁ ESTÁ DESBLOQUEADA
        if (IsAlreadyUnlocked(type))
        {
            Debug.LogWarning($"Habilidade '{type}' já está desbloqueada. Não consumiu slot.");
            return true;
        }

        // 2. CHECA SE HÁ SLOT DISPONÍVEL
        if (currentAbilityCount >= maxAbilitySlots)
        {
            Debug.LogWarning($"Não foi possível desbloquear '{type}'. Slots de habilidade cheios ({currentAbilityCount}/{maxAbilitySlots}).");
            return false;
        }

        // 3. DESBLOQUEIA E INCREMENTA O CONTADOR
        currentAbilityCount++;

        switch (type)
        {
            case AbilityType.TripodeMarciano:
                isTripodeMarcianoUnlocked = true;
                break;
            case AbilityType.GeradorGalvanico:
                isGeradorGalvanicoUnlocked = true;
                // Garante que a área do Gerador Galvânico esteja desativada ao desbloquear
                if (geradorGalvanicoArea.activeSelf) Habilidade_GeradorGalvanico(false);
                break;
            case AbilityType.MantoDeNVOA:
                isMantoDeNVOAUnlocked = true;
                break;
            case AbilityType.RelogioDoChapeleiro:
                isRelogioDoChapeleiroUnlocked = true;
                break;
        }

        Debug.Log($"Habilidade '{type}' desbloqueada! Slots ocupados: {currentAbilityCount}/{maxAbilitySlots}");
        return true;
    }

    // NOVO: Método de ajuda para checar se já está desbloqueada
    private bool IsAlreadyUnlocked(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.TripodeMarciano:
                return isTripodeMarcianoUnlocked;
            case AbilityType.GeradorGalvanico:
                return isGeradorGalvanicoUnlocked;
            case AbilityType.MantoDeNVOA:
                return isMantoDeNVOAUnlocked;
            case AbilityType.RelogioDoChapeleiro:
                return isRelogioDoChapeleiroUnlocked;
            default:
                return false;
        }
    }

    private void Attack()
    {
        if (Time.time - lastAttackTime > comboTimeWindow)
        {
            comboStep = 0;
        }

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

        if (attackSounds.Length >= comboStep && attackSounds[comboStep - 1] != null)
        {
            audioSource.PlayOneShot(attackSounds[comboStep - 1]);
        }

        animator.SetTrigger("Attack");
        animator.SetInteger("ComboStep", comboStep);

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDirection = (mousePosition - (Vector2)transform.position).normalized;

        StartCoroutine(ActivateHitbox(attackDirection));
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

        if (laserSound != null)
        {
            audioSource.PlayOneShot(laserSound);
        }

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDirection = (mousePosition - (Vector2)pontoDeDisparo.position).normalized;

        GameObject laserObj = Instantiate(projetilLaserPrefab, pontoDeDisparo.position, Quaternion.identity);

        LaserProjectile projectile = laserObj.GetComponent<LaserProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(shootDirection);
        }
    }

    void Habilidade_GeradorGalvanico(bool isActive)
    {
        if (geradorGalvanicoArea.activeSelf == isActive) return;

        if (isActive)
        {
            Debug.Log("HABILIDADE: Gerador Galvânico (ATIVADO)");
            if (galvanicAudioSource != null) galvanicAudioSource.Play();
        }
        else
        {
            Debug.Log("HABILIDADE: Gerador Galvânico (DESATIVADO)");
            if (galvanicAudioSource != null) galvanicAudioSource.Stop();
        }

        geradorGalvanicoArea.SetActive(isActive);
    }

    IEnumerator Habilidade_MantoDeNVOA()
    {
        isMantoAtivo = true;

        Debug.Log("HABILIDADE: Manto de Névoa (ATIVADO - Invisível e Vulnerável)");
        if (mantoSound != null)
        {
            audioSource.PlayOneShot(mantoSound);
        }

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
        if (relogioSound != null)
        {
            audioSource.PlayOneShot(relogioSound);
        }

        Instantiate(relogioChapeleiroAreaPrefab, transform.position, Quaternion.identity);
    }

    public void PlayHurtAnimation(Vector2 damageDirection)
    {
        Debug.Log("Jogador recebeu dano!");

        animator.SetTrigger("Hurt");
    }
}
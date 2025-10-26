using System.Collections;
using UnityEngine;
using FMODUnity; // <- Necess�rio para FMOD
using FMOD.Studio; // <- Necess�rio para EventInstance (loops)

public class PlayerCombat : MonoBehaviour
{
    // ENUM P�BLICO: Define as op��es de habilidades para o item colet�vel.
    public enum AbilityType
    {
        TripodeMarciano,
        GeradorGalvanico,
        MantoDeNVOA,
        RelogioDoChapeleiro
    }

    private PlayerMovement playerMovement;
    private Animator animator;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    [Header("Unlocks de Habilidades")]
    public bool isTripodeMarcianoUnlocked = false;
    public bool isGeradorGalvanicoUnlocked = false;
    public bool isMantoDeNVOAUnlocked = false;
    public bool isRelogioDoChapeleiroUnlocked = false;
    public int maxAbilitySlots = 2;
    private int currentAbilityCount = 0;

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

    [Header("FMOD Events")]
    [Tooltip("Array de eventos FMOD para cada passo do combo (1, 2, 3).")]
    [EventRef] public string[] attackSoundEvents; // Caminhos FMOD para os sons de ataque
    [EventRef] public string laserSoundEvent;
    [EventRef] public string galvanicLoopEvent; // Som cont�nuo para o Gerador
    [EventRef] public string mantoSoundEvent;
    [EventRef] public string relogioSoundEvent;

    // Remove refer�ncias ao AudioSource
    // private AudioSource audioSource;
    // public AudioSource galvanicAudioSource;

    private EventInstance galvanicSoundInstance; // Inst�ncia FMOD para o som em loop

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        mainCamera = Camera.main;

        hitboxUp.SetActive(false);
        hitboxDown.SetActive(false);
        hitboxLeft.SetActive(false);
        hitboxRight.SetActive(false);
        geradorGalvanicoArea.SetActive(false);

        // Atualiza a contagem inicial de slots
        if (isTripodeMarcianoUnlocked) currentAbilityCount++;
        if (isGeradorGalvanicoUnlocked) currentAbilityCount++;
        if (isMantoDeNVOAUnlocked) currentAbilityCount++;
        if (isRelogioDoChapeleiroUnlocked) currentAbilityCount++;

        // --- FMOD: Cria a inst�ncia para o som cont�nuo ---
        if (!string.IsNullOrEmpty(galvanicLoopEvent))
        {
            galvanicSoundInstance = RuntimeManager.CreateInstance(galvanicLoopEvent);
            // Anexa a inst�ncia ao GameObject do Player para que o som 3D o siga
            RuntimeManager.AttachInstanceToGameObject(galvanicSoundInstance, transform);
            Debug.Log("[PlayerCombat] Inst�ncia FMOD para Gerador Galv�nico criada.");
        }
        else
        {
            Debug.LogWarning("[PlayerCombat] Evento FMOD 'galvanicLoopEvent' n�o definido.");
        }
        // --- FIM FMOD ---

        Debug.Log("[PlayerCombat] Start conclu�do.");
    }

    // --- FMOD: Garante que a inst�ncia seja liberada ao destruir o objeto ---
    void OnDestroy()
    {
        // Verifica se a inst�ncia � v�lida antes de tentar parar e liberar
        if (galvanicSoundInstance.isValid())
        {
            galvanicSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // Para imediatamente
            galvanicSoundInstance.release(); // Libera os recursos
            Debug.Log("[PlayerCombat] Inst�ncia FMOD do Gerador Galv�nico liberada.");
        }
    }
    // --- FIM FMOD ---

    void Update()
    {
        // Reset do combo
        if (Time.time - lastAttackTime > comboTimeWindow)
        {
            comboStep = 0;
        }

        // Input de Ataque
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }

        // Inputs de Habilidades com checagem de Unlock
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isTripodeMarcianoUnlocked) Habilidade_TripodeMarciano();
            else Debug.Log("Habilidade 'Tr�pode Marciano' est� bloqueada!");
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(true);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Debug.Log("Habilidade 'Gerador Galv�nico' est� bloqueada!");
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && !isMantoAtivo)
        {
            if (isMantoDeNVOAUnlocked) StartCoroutine(Habilidade_MantoDeNVOA());
            else Debug.Log("Habilidade 'Manto de N�voa' est� bloqueada!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (isRelogioDoChapeleiroUnlocked) Habilidade_RelogioDoChapeleiro();
            else Debug.Log("Habilidade 'Rel�gio do Chapeleiro' est� bloqueada!");
        }
    }

    // Fun��o para desbloquear habilidades (chamada por itens, etc.)
    public bool UnlockAbility(AbilityType type)
    {
        if (IsAlreadyUnlocked(type))
        {
            Debug.LogWarning($"Habilidade '{type}' j� est� desbloqueada.");
            return true; // N�o consome slot se j� tiver
        }

        if (currentAbilityCount >= maxAbilitySlots)
        {
            Debug.LogWarning($"N�o foi poss�vel desbloquear '{type}'. Slots cheios.");
            return false; // Falha se n�o houver slots
        }

        currentAbilityCount++; // Ocupa um slot

        // Define o bool correspondente como true
        switch (type)
        {
            case AbilityType.TripodeMarciano: isTripodeMarcianoUnlocked = true; break;
            case AbilityType.GeradorGalvanico:
                isGeradorGalvanicoUnlocked = true;
                if (geradorGalvanicoArea.activeSelf) Habilidade_GeradorGalvanico(false); // Garante que comece desligado
                break;
            case AbilityType.MantoDeNVOA: isMantoDeNVOAUnlocked = true; break;
            case AbilityType.RelogioDoChapeleiro: isRelogioDoChapeleiroUnlocked = true; break;
        }

        Debug.Log($"Habilidade '{type}' desbloqueada! Slots: {currentAbilityCount}/{maxAbilitySlots}");
        return true; // Sucesso
    }

    // Fun��o auxiliar para verificar se a habilidade j� est� desbloqueada
    private bool IsAlreadyUnlocked(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.TripodeMarciano: return isTripodeMarcianoUnlocked;
            case AbilityType.GeradorGalvanico: return isGeradorGalvanicoUnlocked;
            case AbilityType.MantoDeNVOA: return isMantoDeNVOAUnlocked;
            case AbilityType.RelogioDoChapeleiro: return isRelogioDoChapeleiroUnlocked;
            default: return false;
        }
    }

    // Fun��o de Ataque (Combo)
    private void Attack()
    {
        // L�gica do combo
        if (Time.time - lastAttackTime > comboTimeWindow) comboStep = 0;
        if (comboStep == 0) comboStep = 1; else if (comboStep < 3) comboStep++; else comboStep = 1;
        lastAttackTime = Time.time;
        Debug.Log($"[PlayerCombat] Attack! Combo: {comboStep}");

        // --- FMOD: Toca o som do combo ---
        // Verifica se o array existe, tem tamanho suficiente e o evento n�o est� vazio
        if (attackSoundEvents != null && attackSoundEvents.Length >= comboStep && !string.IsNullOrEmpty(attackSoundEvents[comboStep - 1]))
        {
            RuntimeManager.PlayOneShot(attackSoundEvents[comboStep - 1], transform.position);
        }
        else if (attackSoundEvents == null || attackSoundEvents.Length < comboStep)
        {
            Debug.LogWarning($"[PlayerCombat] Array 'attackSoundEvents' n�o configurado corretamente para o combo step {comboStep}.");
        }
        // --- FIM FMOD ---

        // Anima��o
        animator.SetTrigger("Attack");
        animator.SetInteger("ComboStep", comboStep);

        // Ativa a hitbox na dire��o do mouse
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDirection = (mousePosition - (Vector2)transform.position).normalized;
        StartCoroutine(ActivateHitbox(attackDirection));
    }

    // Corrotina para ativar/desativar a hitbox correta
    private IEnumerator ActivateHitbox(Vector2 direction)
    {
        GameObject activeHitbox = null;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        { activeHitbox = (direction.x > 0) ? hitboxRight : hitboxLeft; }
        else
        { activeHitbox = (direction.y > 0) ? hitboxUp : hitboxDown; }

        if (activeHitbox != null)
        {
            Debug.Log($"[PlayerCombat] HITBOX ATIVADA: {activeHitbox.name}");
            activeHitbox.SetActive(true);
            yield return new WaitForSeconds(0.15f); // Dura��o
            activeHitbox.SetActive(false);
            Debug.Log($"[PlayerCombat] Hitbox {activeHitbox.name} desativada.");
        }
        else
        {
            Debug.LogError("[PlayerCombat] N�o foi poss�vel determinar a hitbox correta!");
        }
    }

    // Habilidade 1: Tr�pode Marciano (Laser)
    void Habilidade_TripodeMarciano()
    {
        Debug.Log("[PlayerCombat] HABILIDADE: Tr�pode Marciano");

        // --- FMOD: Toca o som do laser ---
        if (!string.IsNullOrEmpty(laserSoundEvent))
        {
            RuntimeManager.PlayOneShot(laserSoundEvent, pontoDeDisparo.position);
        }
        // --- FIM FMOD ---

        // Calcula a dire��o e instancia
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDirection = (mousePosition - (Vector2)pontoDeDisparo.position).normalized;
        GameObject laserObj = Instantiate(projetilLaserPrefab, pontoDeDisparo.position, Quaternion.identity);
        LaserProjectile projectile = laserObj.GetComponent<LaserProjectile>();
        if (projectile != null) { projectile.SetDirection(shootDirection); }
        else { Debug.LogError("[PlayerCombat] Prefab do Laser n�o tem script LaserProjectile!"); }
    }

    // Habilidade 2: Gerador Galv�nico (�rea Cont�nua)
    void Habilidade_GeradorGalvanico(bool isActive)
    {
        if (geradorGalvanicoArea.activeSelf == isActive) return; // Evita chamadas repetidas

        // --- FMOD: Controla a inst�ncia do loop ---
        if (galvanicSoundInstance.isValid()) // Verifica se a inst�ncia foi criada
        {
            PLAYBACK_STATE currentState;
            galvanicSoundInstance.getPlaybackState(out currentState);

            if (isActive)
            {
                Debug.Log("[PlayerCombat] HABILIDADE: Gerador Galv�nico (ATIVADO)");
                // S� inicia se n�o estiver tocando
                if (currentState != PLAYBACK_STATE.PLAYING)
                {
                    galvanicSoundInstance.start();
                    Debug.Log("[PlayerCombat] Som Galv�nico iniciado.");
                }
            }
            else
            {
                Debug.Log("[PlayerCombat] HABILIDADE: Gerador Galv�nico (DESATIVADO)");
                // S� para se estiver tocando
                if (currentState == PLAYBACK_STATE.PLAYING)
                {
                    // Usa ALLOWFADEOUT para permitir que o som termine suavemente se tiver um AHDSR no FMOD
                    galvanicSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    Debug.Log("[PlayerCombat] Som Galv�nico parado.");
                }
            }
        }
        else if (isActive) // Log de erro apenas se tentar ativar e a inst�ncia for inv�lida
        {
            Debug.LogError("[PlayerCombat] Tentou ativar Gerador Galv�nico, mas a inst�ncia FMOD � inv�lida!");
        }
        // --- FIM FMOD ---

        // Ativa/Desativa a �rea visual/collider
        geradorGalvanicoArea.SetActive(isActive);
    }

    // Habilidade 3: Manto de N�voa (Invisibilidade)
    IEnumerator Habilidade_MantoDeNVOA()
    {
        isMantoAtivo = true;
        Debug.Log("[PlayerCombat] HABILIDADE: Manto de N�voa (ATIVADO)");

        // --- FMOD: Toca o som do manto ---
        if (!string.IsNullOrEmpty(mantoSoundEvent))
        {
            RuntimeManager.PlayOneShot(mantoSoundEvent, transform.position);
        }
        // --- FIM FMOD ---

        // Efeito visual
        Color tempColor = spriteRenderer.color;
        tempColor.a = 0.5f; // Semi-transparente
        spriteRenderer.color = tempColor;

        // Espera a dura��o
        yield return new WaitForSeconds(mantoDuracao);

        // Reverte
        Debug.Log("[PlayerCombat] HABILIDADE: Manto de N�voa (DESATIVADO)");
        tempColor.a = 1f; // Opaco
        spriteRenderer.color = tempColor;
        isMantoAtivo = false;
    }

    // Habilidade 4: Rel�gio do Chapeleiro (�rea Lenta)
    void Habilidade_RelogioDoChapeleiro()
    {
        Debug.Log("[PlayerCombat] HABILIDADE: Rel�gio do Chapeleiro");

        // --- FMOD: Toca o som do rel�gio ---
        if (!string.IsNullOrEmpty(relogioSoundEvent))
        {
            RuntimeManager.PlayOneShot(relogioSoundEvent, transform.position);
        }
        // --- FIM FMOD ---

        // Instancia a �rea
        Instantiate(relogioChapeleiroAreaPrefab, transform.position, Quaternion.identity);
    }

    // Chamado pelo PlayerHealth para tocar anima��o de dano
    public void PlayHurtAnimation(Vector2 damageDirection)
    {
        Debug.Log("[PlayerCombat] PlayHurtAnimation chamada.");
        animator.SetTrigger("Hurt");
    }
}
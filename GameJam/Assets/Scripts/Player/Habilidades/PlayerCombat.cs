using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class PlayerCombat : MonoBehaviour
{
    // ENUMs, Structs, Referências Iniciais (Sem alterações)
    public enum AbilityType { TripodeMarciano, GeradorGalvanico, MantoDeNVOA, RelogioDoChapeleiro }
    [System.Serializable] public struct AbilityIconMapping { public AbilityType ability; public Sprite icon; }
    private PlayerMovement playerMovement;
    private Animator animator;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    // Habilidades, UI (Sem alterações)
    [Header("Unlocks de Habilidades")]
    public bool isTripodeMarcianoUnlocked, isGeradorGalvanicoUnlocked, isMantoDeNVOAUnlocked, isRelogioDoChapeleiroUnlocked;
    public int maxAbilitySlots = 2; private int currentAbilityCount = 0;
    private List<AbilityType> unlockedAbilities = new List<AbilityType>();
    [Header("UI Habilidades")]
    public List<AbilityIconMapping> abilityIcons; public List<Image> abilityUiSlots; public Sprite emptySlotSprite;

    // --- Combate (Ajustado) ---
    [Header("Combo")]
    public int comboStep = 0; // Qual passo o JOGADOR solicitou
    public float comboResetTime = 0.8f; // Tempo MÁXIMO entre cliques para continuar o combo
    private float lastAttackInputTime = -1f; // Tempo do último clique bem-sucedido que iniciou/continuou o combo
    private bool isAttacking = false; // Se a animação de ataque está ativa
    public string attackAnimationName = "AttackCombo";
    private int maxComboSteps = 3;
    // Remove: private bool canContinueCombo = false; // Não precisamos mais desta flag separada

    [Header("Hitboxes")]
    public GameObject hitboxUp, hitboxDown, hitboxLeft, hitboxRight;
    public float hitboxDuration = 0.15f;

    // Prefabs, Manto, FMOD (Sem alterações)
    [Header("Habilidades")]
    public GameObject projetilLaserPrefab; public GameObject geradorGalvanicoArea; public GameObject relogioChapeleiroAreaPrefab; public Transform pontoDeDisparo;
    [Header("Manto")]
    private bool isMantoAtivo = false; public float mantoDuracao = 3f;
    [Header("FMOD Events")]
    [EventRef] public string[] attackSoundEvents; [EventRef] public string laserSoundEvent; [EventRef] public string galvanicLoopEvent; [EventRef] public string mantoSoundEvent; [EventRef] public string relogioSoundEvent;
    private EventInstance galvanicSoundInstance;

    void Start()
    {
        // Setup inicial (igual)
        playerMovement = GetComponent<PlayerMovement>(); animator = GetComponent<Animator>(); spriteRenderer = GetComponentInChildren<SpriteRenderer>(); mainCamera = Camera.main;
        if (hitboxUp != null) hitboxUp.SetActive(false); if (hitboxDown != null) hitboxDown.SetActive(false); if (hitboxLeft != null) hitboxLeft.SetActive(false); if (hitboxRight != null) hitboxRight.SetActive(false); if (geradorGalvanicoArea != null) geradorGalvanicoArea.SetActive(false);
        unlockedAbilities.Clear(); currentAbilityCount = 0;
        if (isTripodeMarcianoUnlocked) { unlockedAbilities.Add(AbilityType.TripodeMarciano); currentAbilityCount++; }
        if (isGeradorGalvanicoUnlocked) { unlockedAbilities.Add(AbilityType.GeradorGalvanico); currentAbilityCount++; }
        if (isMantoDeNVOAUnlocked) { unlockedAbilities.Add(AbilityType.MantoDeNVOA); currentAbilityCount++; }
        if (isRelogioDoChapeleiroUnlocked) { unlockedAbilities.Add(AbilityType.RelogioDoChapeleiro); currentAbilityCount++; }
        if (!string.IsNullOrEmpty(galvanicLoopEvent)) { galvanicSoundInstance = RuntimeManager.CreateInstance(galvanicLoopEvent); RuntimeManager.AttachInstanceToGameObject(galvanicSoundInstance, transform); }
        UpdateAbilityUI();
    }

    void OnDestroy() { if (galvanicSoundInstance.isValid()) { galvanicSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); galvanicSoundInstance.release(); } }

    void Update()
    {
        // Input de Ataque
        if (Input.GetKeyDown(KeyCode.Mouse0)) RequestAttack();

        // Inputs Habilidades (Sem alterações)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { if (isTripodeMarcianoUnlocked) Habilidade_TripodeMarciano(); }
        if (Input.GetKey(KeyCode.Alpha2)) { if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(true); }
        if (Input.GetKeyUp(KeyCode.Alpha2)) { if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(false); }
        if (Input.GetKeyDown(KeyCode.Alpha3) && !isMantoAtivo) { if (isMantoDeNVOAUnlocked) StartCoroutine(Habilidade_MantoDeNVOA()); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { if (isRelogioDoChapeleiroUnlocked) Habilidade_RelogioDoChapeleiro(); }

        // --- Timeout Simplificado ---
        // Se passou muito tempo desde o último clique VÁLIDO, reseta o passo (mesmo se não estiver atacando)
        if (Time.time > lastAttackInputTime + comboResetTime)
        {
            if (comboStep != 0) // Só loga se realmente resetar
            {
                Debug.Log($"[PlayerCombat] Timeout do combo Reset Time ({comboResetTime}s) atingido. Resetando comboStep.");
                comboStep = 0;
            }
        }
        // --- FIM Timeout ---
    }

    public bool UnlockAbility(AbilityType type) { /*...*/ return true; } // Completo
    private bool IsAlreadyUnlocked(AbilityType type) { /*...*/ return false; } // Completo
    private void UpdateAbilityUI() { /*...*/ } // Completo
    private Sprite GetIconForAbility(AbilityType type) { /*...*/ return null; } // Completo

    private void RequestAttack()
    {
        float timeSinceLastAttack = Time.time - lastAttackInputTime;

        // Se não está atacando OU se o último clique foi há muito tempo, começa do 1
        if (!isAttacking || timeSinceLastAttack > comboResetTime)
        {
            isAttacking = true;
            comboStep = 1;
            lastAttackInputTime = Time.time; // Regista tempo do clique inicial
            Debug.Log("[PlayerCombat] RequestAttack: Iniciando combo (Passo 1).");

            if (animator != null)
            {
                animator.SetTrigger("AttackTrigger"); // Dispara a animação
                Debug.Log("[PlayerCombat] Trigger 'AttackTrigger' disparado.");
            }
        }
        // Se JÁ está atacando E o clique foi rápido o suficiente
        else if (isAttacking && timeSinceLastAttack <= comboResetTime)
        {
            // Tenta avançar para o próximo passo, se não atingiu o máximo
            if (comboStep < maxComboSteps)
            {
                comboStep++;
                lastAttackInputTime = Time.time; // Regista tempo do clique de continuação
                Debug.Log($"[PlayerCombat] RequestAttack: Avançando combo para Passo {comboStep}.");
                // Não precisa mexer no animator aqui, o CheckComboContinue vai decidir
            }
            else
            {
                Debug.Log("[PlayerCombat] RequestAttack: Combo máximo atingido, clique ignorado.");
            }
        }
    }

    // --- Funções Chamadas pelos Eventos de Animação ---

    // Eventos de Hitbox e Som (Modificados para usar o comboStep atual)
    public void AnimEvent_ActivateHitbox(int stepForThisEvent) // Recebe 1, 2 ou 3 do evento
    {
        // Ativa a hitbox APENAS se o comboStep atual for igual ao passo deste evento
        if (!isAttacking || comboStep != stepForThisEvent)
        {
            Debug.Log($"[PlayerCombat] EVENTO Hitbox (Passo {stepForThisEvent}): Ignorado. Combo atual: {comboStep}, Attacking: {isAttacking}");
            return;
        }

        if (mainCamera == null) return;
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDirection = (mousePosition - (Vector2)transform.position).normalized;
        GameObject hitboxToActivate = null;
        if (Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y)) { hitboxToActivate = (attackDirection.x > 0) ? hitboxRight : hitboxLeft; }
        else { hitboxToActivate = (attackDirection.y > 0) ? hitboxUp : hitboxDown; }
        if (hitboxToActivate != null)
        {
            Debug.Log($"[PlayerCombat] EVENTO ANIMAÇÃO (Passo {stepForThisEvent}): Ativando Hitbox {hitboxToActivate.name}");
            hitboxToActivate.SetActive(true);
            StartCoroutine(DeactivateHitboxAfterDelay(hitboxToActivate, hitboxDuration));
        }
    }

    private IEnumerator DeactivateHitboxAfterDelay(GameObject hitbox, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hitbox != null) { hitbox.SetActive(false); }
    }

    public void AnimEvent_PlayAttackSound(int stepToPlay) // Recebe 1, 2 ou 3 do evento
    {
        // Toca o som APENAS se o comboStep atual for igual ao passo deste evento
        if (!isAttacking || comboStep != stepToPlay)
        {
            Debug.Log($"[PlayerCombat] EVENTO Som (Passo {stepToPlay}): Ignorado. Combo atual: {comboStep}, Attacking: {isAttacking}");
            return;
        }

        Debug.Log($"[PlayerCombat] EVENTO ANIMAÇÃO: Tocar som do passo {stepToPlay}");
        if (attackSoundEvents != null && attackSoundEvents.Length >= stepToPlay && stepToPlay > 0 && !string.IsNullOrEmpty(attackSoundEvents[stepToPlay - 1]))
        {
            RuntimeManager.PlayOneShot(attackSoundEvents[stepToPlay - 1], transform.position);
        }
    }

    // --- **NOVO:** Evento de Verificação (Modificado) ---
    /// <summary>
    /// Chamado APÓS cada golpe (exceto o último). Verifica se o jogador JÁ CLICOU para o próximo passo.
    /// </summary>
    /// <param name="currentPhaseEnded">O número da fase que ACABOU (1 ou 2).</param>
    public void CheckComboContinue(int currentPhaseEnded)
    {
        if (!isAttacking) return; // Se já foi resetado, sai

        Debug.Log($"[PlayerCombat] EVENTO ANIMAÇÃO: CheckComboContinue({currentPhaseEnded}). Combo atual solicitado: {comboStep}");

        // Se o jogador NÃO solicitou o próximo passo (comboStep ainda é <= fase atual)
        if (comboStep <= currentPhaseEnded)
        {
            Debug.Log($"[PlayerCombat] Combo interrompido após passo {currentPhaseEnded}. Abortando animação.");
            if (animator != null) animator.SetTrigger("AbortAttackTrigger");
            ResetAttackState(); // Reseta o estado imediatamente
        }
        else // Se comboStep > currentPhaseEnded, significa que o jogador clicou a tempo
        {
            Debug.Log($"[PlayerCombat] Combo continua APÓS passo {currentPhaseEnded}. Animação segue.");
            // Não faz nada, deixa a animação continuar para a próxima parte
        }
    }
    // --- **FIM NOVO** ---

    // Função de Reset (Chamada pelo CheckComboContinue ou pelo fim da animação)
    public void ResetAttackState()
    {
        if (isAttacking)
        {
            isAttacking = false;
            comboStep = 0; // Reseta o passo solicitado
            // canContinueCombo = false; // Removido
            Debug.Log("[PlayerCombat] ResetAttackState: Estado de ataque resetado.");
        }
    }

    // Evento no FIM da animação COMPLETA
    public void AnimEvent_AttackEnd()
    {
        Debug.Log("[PlayerCombat] EVENTO ANIMAÇÃO: Fim da animação completa alcançado.");
        ResetAttackState();
    }

    // Habilidades (Sem alterações)
    void Habilidade_TripodeMarciano() { /*...*/ }
    void Habilidade_GeradorGalvanico(bool isActive) { /*...*/ }
    IEnumerator Habilidade_MantoDeNVOA() { /*...*/ yield return null; }
    void Habilidade_RelogioDoChapeleiro() { /*...*/ }
    public void PlayHurtAnimation(Vector2 damageDirection) { if (animator != null) animator.SetTrigger("Hurt"); }
}
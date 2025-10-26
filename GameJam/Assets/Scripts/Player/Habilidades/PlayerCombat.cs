using System.Collections;
using System.Collections.Generic; // Necessário para List<>
using UnityEngine;
using UnityEngine.UI; // Necessário para Image
using FMODUnity; // Necessário para FMOD
using FMOD.Studio; // Necessário para EventInstance (loops)

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

    // Struct para associar Habilidade e Ícone no Inspector
    [System.Serializable]
    public struct AbilityIconMapping
    {
        public AbilityType ability;
        public Sprite icon;
    }

    // --- Referências a Componentes ---
    private PlayerMovement playerMovement;
    private Animator animator;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    // --- Estado das Habilidades ---
    [Header("Unlocks de Habilidades")]
    public bool isTripodeMarcianoUnlocked = false;
    public bool isGeradorGalvanicoUnlocked = false;
    public bool isMantoDeNVOAUnlocked = false;
    public bool isRelogioDoChapeleiroUnlocked = false;
    public int maxAbilitySlots = 2; // Quantos slots de habilidade o jogador tem
    private int currentAbilityCount = 0; // Quantos slots estão ocupados
    private List<AbilityType> unlockedAbilities = new List<AbilityType>(); // Guarda a ordem das habilidades desbloqueadas

    // --- UI das Habilidades ---
    [Header("UI Habilidades")]
    [Tooltip("Lista para associar cada habilidade ao seu Sprite de ícone.")]
    public List<AbilityIconMapping> abilityIcons; // Mapeamento editável no Inspector
    [Tooltip("Lista dos componentes Image na UI que servirão de slots (ex: 2 Images). Arraste-os aqui na ordem desejada.")]
    public List<Image> abilityUiSlots; // Referências às Images da UI
    [Tooltip("Sprite opcional para mostrar quando um slot está vazio.")]
    public Sprite emptySlotSprite; // Sprite para slots vazios

    // --- Combate ---
    [Header("Combo")]
    public int comboStep = 0;
    public float comboTimeWindow = 0.5f; // Tempo para continuar o combo
    private float lastAttackTime = 0f;

    [Header("Hitboxes")]
    public GameObject hitboxUp;
    public GameObject hitboxDown;
    public GameObject hitboxLeft;
    public GameObject hitboxRight;

    // --- Prefabs e Pontos de Habilidade ---
    [Header("Habilidades")]
    public GameObject projetilLaserPrefab;
    public GameObject geradorGalvanicoArea; // Objeto filho ou prefab da área
    public GameObject relogioChapeleiroAreaPrefab; // Prefab da área
    public Transform pontoDeDisparo; // Ponto de onde sai o laser

    // --- Estado do Manto ---
    [Header("Manto")]
    private bool isMantoAtivo = false;
    public float mantoDuracao = 3f;

    // --- FMOD ---
    [Header("FMOD Events")]
    [EventRef] public string[] attackSoundEvents; // Sons para combo 1, 2, 3
    [EventRef] public string laserSoundEvent;
    [EventRef] public string galvanicLoopEvent; // Som contínuo
    [EventRef] public string mantoSoundEvent;
    [EventRef] public string relogioSoundEvent;

    private EventInstance galvanicSoundInstance; // Instância FMOD para o loop

    // --- Inicialização ---
    void Start()
    {
        // Pega componentes
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // Para o Manto
        mainCamera = Camera.main;

        // Garante que as hitboxes e áreas comecem desligadas
        if (hitboxUp != null) hitboxUp.SetActive(false);
        if (hitboxDown != null) hitboxDown.SetActive(false);
        if (hitboxLeft != null) hitboxLeft.SetActive(false);
        if (hitboxRight != null) hitboxRight.SetActive(false);
        if (geradorGalvanicoArea != null) geradorGalvanicoArea.SetActive(false);

        // Limpa e preenche a lista de habilidades iniciais
        unlockedAbilities.Clear();
        currentAbilityCount = 0;
        if (isTripodeMarcianoUnlocked) { unlockedAbilities.Add(AbilityType.TripodeMarciano); currentAbilityCount++; }
        if (isGeradorGalvanicoUnlocked) { unlockedAbilities.Add(AbilityType.GeradorGalvanico); currentAbilityCount++; }
        if (isMantoDeNVOAUnlocked) { unlockedAbilities.Add(AbilityType.MantoDeNVOA); currentAbilityCount++; }
        if (isRelogioDoChapeleiroUnlocked) { unlockedAbilities.Add(AbilityType.RelogioDoChapeleiro); currentAbilityCount++; }

        // Cria instância FMOD para o Gerador Galvânico
        if (!string.IsNullOrEmpty(galvanicLoopEvent))
        {
            galvanicSoundInstance = RuntimeManager.CreateInstance(galvanicLoopEvent);
            RuntimeManager.AttachInstanceToGameObject(galvanicSoundInstance, transform);
        }

        // Atualiza a UI das habilidades no início
        UpdateAbilityUI();

        Debug.Log("[PlayerCombat] Start concluído.");
    }

    // Garante a libertação da instância FMOD
    void OnDestroy()
    {
        if (galvanicSoundInstance.isValid())
        {
            galvanicSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            galvanicSoundInstance.release();
        }
    }

    // --- Lógica Principal (Update) ---
    void Update()
    {
        // Reseta o passo do combo se o tempo expirar
        if (Time.time - lastAttackTime > comboTimeWindow)
        {
            comboStep = 0;
        }

        // Input: Ataque Básico (Mouse Esquerdo)
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }

        // Inputs: Habilidades (Teclas 1 a 4) com checagem de unlock
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Trípode Marciano
        {
            if (isTripodeMarcianoUnlocked) Habilidade_TripodeMarciano();
            else Debug.Log("Habilidade 'Trípode Marciano' está bloqueada!");
        }

        // Gerador Galvânico (Segurar Tecla 2)
        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(true);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Debug.Log("Habilidade 'Gerador Galvânico' está bloqueada!"); // Log só ao pressionar
        }
        if (Input.GetKeyUp(KeyCode.Alpha2)) // Soltar Tecla 2
        {
            if (isGeradorGalvanicoUnlocked) Habilidade_GeradorGalvanico(false);
        }

        // Manto de Névoa (Tecla 3) - Só ativa se não estiver ativo
        if (Input.GetKeyDown(KeyCode.Alpha3) && !isMantoAtivo)
        {
            if (isMantoDeNVOAUnlocked) StartCoroutine(Habilidade_MantoDeNVOA());
            else Debug.Log("Habilidade 'Manto de Névoa' está bloqueada!");
        }

        // Relógio do Chapeleiro (Tecla 4)
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (isRelogioDoChapeleiroUnlocked) Habilidade_RelogioDoChapeleiro();
            else Debug.Log("Habilidade 'Relógio do Chapeleiro' está bloqueada!");
        }
    }

    // --- Gestão de Habilidades ---

    // Função pública para desbloquear uma habilidade (chamada externamente)
    public bool UnlockAbility(AbilityType type)
    {
        // Já tem?
        if (IsAlreadyUnlocked(type))
        {
            Debug.LogWarning($"Habilidade '{type}' já está desbloqueada.");
            return true; // Considera sucesso, não ocupa novo slot
        }

        // Slots cheios?
        if (currentAbilityCount >= maxAbilitySlots)
        {
            Debug.LogWarning($"Não foi possível desbloquear '{type}'. Slots cheios ({currentAbilityCount}/{maxAbilitySlots}).");
            return false; // Falha
        }

        // Desbloqueia
        currentAbilityCount++;
        unlockedAbilities.Add(type); // Adiciona à lista para manter a ordem

        // Ativa o bool correspondente
        switch (type)
        {
            case AbilityType.TripodeMarciano: isTripodeMarcianoUnlocked = true; break;
            case AbilityType.GeradorGalvanico:
                isGeradorGalvanicoUnlocked = true;
                if (geradorGalvanicoArea != null && geradorGalvanicoArea.activeSelf) Habilidade_GeradorGalvanico(false); // Garante que comece desligado
                break;
            case AbilityType.MantoDeNVOA: isMantoDeNVOAUnlocked = true; break;
            case AbilityType.RelogioDoChapeleiro: isRelogioDoChapeleiroUnlocked = true; break;
        }

        // Atualiza a representação na UI
        UpdateAbilityUI();

        Debug.Log($"Habilidade '{type}' desbloqueada! Slots: {currentAbilityCount}/{maxAbilitySlots}");
        return true; // Sucesso
    }

    // Verifica se uma habilidade específica já foi desbloqueada
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

    // --- Lógica de Ataque ---

    // Executa um passo do combo de ataque
    private void Attack()
    {
        // Avança ou reseta o combo
        if (Time.time - lastAttackTime > comboTimeWindow) comboStep = 0; // Reseta se demorar
        if (comboStep == 0) comboStep = 1; else if (comboStep < 3) comboStep++; else comboStep = 1; // Avança ou volta ao 1
        lastAttackTime = Time.time;
        Debug.Log($"[PlayerCombat] Attack! Combo: {comboStep}");

        // Toca o som FMOD correspondente
        if (attackSoundEvents != null && attackSoundEvents.Length >= comboStep && !string.IsNullOrEmpty(attackSoundEvents[comboStep - 1]))
        {
            RuntimeManager.PlayOneShot(attackSoundEvents[comboStep - 1], transform.position);
        }

        // Dispara a animação
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            animator.SetInteger("ComboStep", comboStep);
        }

        // Ativa a hitbox na direção do mouse
        if (mainCamera != null)
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 attackDirection = (mousePosition - (Vector2)transform.position).normalized;
            StartCoroutine(ActivateHitbox(attackDirection));
        }
        else
        {
            Debug.LogError("[PlayerCombat] Câmera principal não encontrada para calcular direção do ataque!");
        }
    }

    // Ativa e desativa a hitbox correta por um curto período
    private IEnumerator ActivateHitbox(Vector2 direction)
    {
        GameObject activeHitbox = null;
        // Escolhe a hitbox baseada na direção (predominância horizontal ou vertical)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        { activeHitbox = (direction.x > 0) ? hitboxRight : hitboxLeft; }
        else
        { activeHitbox = (direction.y > 0) ? hitboxUp : hitboxDown; }

        if (activeHitbox != null)
        {
            Debug.Log($"[PlayerCombat] HITBOX ATIVADA: {activeHitbox.name}");
            activeHitbox.SetActive(true);
            yield return new WaitForSeconds(0.15f); // Duração da hitbox ativa
            activeHitbox.SetActive(false);
            Debug.Log($"[PlayerCombat] Hitbox {activeHitbox.name} desativada.");
        }
        else
        {
            Debug.LogError("[PlayerCombat] Referências de Hitbox não configuradas corretamente!");
        }
    }

    // --- Implementação das Habilidades ---

    // Habilidade 1: Dispara um projétil laser na direção do mouse
    void Habilidade_TripodeMarciano()
    {
        Debug.Log("[PlayerCombat] HABILIDADE: Trípode Marciano");
        if (!string.IsNullOrEmpty(laserSoundEvent) && pontoDeDisparo != null)
            RuntimeManager.PlayOneShot(laserSoundEvent, pontoDeDisparo.position);

        if (mainCamera != null && pontoDeDisparo != null && projetilLaserPrefab != null)
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shootDirection = (mousePosition - (Vector2)pontoDeDisparo.position).normalized;
            GameObject laserObj = Instantiate(projetilLaserPrefab, pontoDeDisparo.position, Quaternion.identity);
            LaserProjectile projectile = laserObj.GetComponent<LaserProjectile>();
            if (projectile != null) { projectile.SetDirection(shootDirection); }
            else { Debug.LogError("[PlayerCombat] Prefab do Laser não tem script LaserProjectile!"); Destroy(laserObj); }
        }
        else
        {
            Debug.LogError("[PlayerCombat] Faltam referências (Câmera, PontoDeDisparo ou Prefab) para Trípode Marciano!");
        }
    }

    // Habilidade 2: Ativa/Desativa a área de dano contínuo e o som em loop
    void Habilidade_GeradorGalvanico(bool isActive)
    {
        if (geradorGalvanicoArea == null) { Debug.LogError("[PlayerCombat] Referência 'geradorGalvanicoArea' não definida!"); return; }
        if (geradorGalvanicoArea.activeSelf == isActive) return; // Já está no estado desejado

        // Controla a instância FMOD
        if (galvanicSoundInstance.isValid())
        {
            galvanicSoundInstance.getPlaybackState(out PLAYBACK_STATE currentState);
            if (isActive && currentState != PLAYBACK_STATE.PLAYING) { galvanicSoundInstance.start(); Debug.Log("[PlayerCombat] Som Galvânico iniciado."); }
            else if (!isActive && currentState == PLAYBACK_STATE.PLAYING) { galvanicSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); Debug.Log("[PlayerCombat] Som Galvânico parado."); }
        }
        else if (isActive) { Debug.LogError("[PlayerCombat] Instância FMOD Galvânica inválida!"); }

        // Ativa/Desativa o GameObject da área
        geradorGalvanicoArea.SetActive(isActive);
        Debug.Log($"[PlayerCombat] HABILIDADE: Gerador Galvânico ({(isActive ? "ATIVADO" : "DESATIVADO")})");
    }

    // Habilidade 3: Fica semi-transparente por um tempo
    IEnumerator Habilidade_MantoDeNVOA()
    {
        isMantoAtivo = true;
        Debug.Log("[PlayerCombat] HABILIDADE: Manto de Névoa (ATIVADO)");
        if (!string.IsNullOrEmpty(mantoSoundEvent))
            RuntimeManager.PlayOneShot(mantoSoundEvent, transform.position);

        Color originalColor = Color.white; // Default
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            Color tempColor = originalColor;
            tempColor.a = 0.5f; // Alpha para semi-transparente
            spriteRenderer.color = tempColor;
        }
        else { Debug.LogWarning("[PlayerCombat] SpriteRenderer não encontrado para Manto!"); }

        yield return new WaitForSeconds(mantoDuracao);

        Debug.Log("[PlayerCombat] HABILIDADE: Manto de Névoa (DESATIVADO)");
        if (spriteRenderer != null) spriteRenderer.color = originalColor; // Restaura cor original
        isMantoAtivo = false;
    }

    // Habilidade 4: Instancia uma área de efeito (lentidão/aceleração)
    void Habilidade_RelogioDoChapeleiro()
    {
        Debug.Log("[PlayerCombat] HABILIDADE: Relógio do Chapeleiro");
        if (!string.IsNullOrEmpty(relogioSoundEvent))
            RuntimeManager.PlayOneShot(relogioSoundEvent, transform.position);

        if (relogioChapeleiroAreaPrefab != null)
        {
            Instantiate(relogioChapeleiroAreaPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("[PlayerCombat] Prefab 'relogioChapeleiroAreaPrefab' não definido!");
        }
    }

    // --- Funções Auxiliares ---

    // Chamado pelo PlayerHealth para feedback visual de dano
    public void PlayHurtAnimation(Vector2 damageDirection)
    {
        if (animator != null) animator.SetTrigger("Hurt");
        Debug.Log("[PlayerCombat] PlayHurtAnimation chamada.");
    }

    // Atualiza os slots da UI com os ícones corretos
    private void UpdateAbilityUI()
    {
        Debug.Log("[PlayerCombat] Atualizando UI das Habilidades...");
        if (abilityUiSlots == null || abilityUiSlots.Count == 0)
        {
            Debug.LogWarning("[PlayerCombat] Lista 'abilityUiSlots' não configurada.");
            return;
        }

        // Itera por cada slot de Image na UI
        for (int i = 0; i < abilityUiSlots.Count; i++)
        {
            Image currentSlotImage = abilityUiSlots[i];
            if (currentSlotImage == null)
            {
                Debug.LogWarning($"[PlayerCombat] Slot UI {i} é nulo/não atribuído.");
                continue; // Pula este slot
            }

            // Se o índice 'i' for menor que o número de habilidades desbloqueadas,
            // significa que este slot deve mostrar uma habilidade.
            if (i < unlockedAbilities.Count)
            {
                AbilityType typeInSlot = unlockedAbilities[i]; // Pega a habilidade desta posição
                Sprite iconToShow = GetIconForAbility(typeInSlot); // Procura o ícone

                // Se encontrou um ícone, define-o no slot e ativa a Image
                if (iconToShow != null)
                {
                    currentSlotImage.sprite = iconToShow;
                    currentSlotImage.enabled = true;
                    Debug.Log($"Slot UI {i} preenchido com ícone de {typeInSlot}");
                }
                // Se não encontrou ícone, usa o sprite vazio (se houver)
                else
                {
                    currentSlotImage.sprite = emptySlotSprite;
                    currentSlotImage.enabled = (emptySlotSprite != null); // Só ativa se houver sprite vazio
                    Debug.LogWarning($"Ícone não encontrado para {typeInSlot}, usando slot vazio para slot UI {i}.");
                }
            }
            // Se o índice 'i' for maior ou igual ao número de habilidades,
            // significa que este slot está vazio.
            else
            {
                currentSlotImage.sprite = emptySlotSprite; // Define como vazio
                currentSlotImage.enabled = (emptySlotSprite != null); // Ativa só se houver sprite vazio
                Debug.Log($"Slot UI {i} definido como vazio.");
            }
        }
    }

    // Procura e retorna o Sprite associado a uma AbilityType
    private Sprite GetIconForAbility(AbilityType type)
    {
        if (abilityIcons == null) return null; // Lista de mapeamento não existe

        // Procura na lista definida no Inspector
        foreach (var mapping in abilityIcons)
        {
            if (mapping.ability == type)
            {
                return mapping.icon; // Encontrou!
            }
        }
        // Não encontrou
        Debug.LogWarning($"[PlayerCombat] Ícone para habilidade '{type}' não encontrado na lista 'abilityIcons'.");
        return null;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // Opcional: Se tiver bot�es no menu de pausa
using FMODUnity; // Para FMOD
using FMOD.Studio; // Para FMOD Bus

public class PauseManager : MonoBehaviour
{
    // --- Singleton ---
    public static PauseManager Instance { get; private set; }

    [Header("Configura��es")]
    [Tooltip("Tecla usada para pausar/retomar o jogo.")]
    public KeyCode pauseKey = KeyCode.Escape;
    [Tooltip("Caminho do Master Bus ou Bus principal do FMOD (ex: 'bus:/').")]
    public string fmodMasterBusPath = "bus:/"; // Caminho padr�o

    [Header("Refer�ncias de UI")]
    [Tooltip("O GameObject que cont�m a HUD principal do jogo.")]
    public GameObject gameHudPanel;
    [Tooltip("O GameObject que cont�m o menu de pausa.")]
    public GameObject pauseMenuPanel;

    // --- Estado ---
    private bool isPaused = false;
    public bool IsPaused => isPaused; // Propriedade p�blica para leitura

    // --- Refer�ncias Internas ---
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private Bus masterBus; // Para pausar FMOD

    void Awake()
    {
        // Configura��o do Singleton
        if (Instance == null)
        {
            Instance = this;
            // Opcional: DontDestroyOnLoad(gameObject); // Descomente se quiser que persista entre cenas
            Debug.Log("[PauseManager] Inst�ncia criada.");
        }
        else
        {
            Debug.LogWarning("[PauseManager] Inst�ncia duplicada encontrada. Destruindo novo objeto.");
            Destroy(gameObject);
            return; // Impede o resto do Awake de rodar
        }

        // Tenta encontrar os scripts do Player
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerCombat = FindObjectOfType<PlayerCombat>();
        if (playerMovement == null) Debug.LogWarning("[PauseManager] Script PlayerMovement n�o encontrado na cena.");
        if (playerCombat == null) Debug.LogWarning("[PauseManager] Script PlayerCombat n�o encontrado na cena.");

        // Pega o Bus FMOD
        try
        {
            masterBus = RuntimeManager.GetBus(fmodMasterBusPath);
            if (!masterBus.isValid()) Debug.LogError($"[PauseManager] Bus FMOD '{fmodMasterBusPath}' inv�lido!");
        }
        catch (FMODUnity.BusNotFoundException)
        {
            Debug.LogError($"[PauseManager] Bus FMOD '{fmodMasterBusPath}' n�o encontrado!");
        }

        // Garante que o menu de pausa comece desativado
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        else Debug.LogError("[PauseManager] Refer�ncia do Pause Menu Panel n�o definida!");

        if (gameHudPanel == null) Debug.LogWarning("[PauseManager] Refer�ncia do Game Hud Panel n�o definida!");
    }

    void Start()
    {
        // Garante que o jogo comece despausado (Time.timeScale pode persistir entre plays no Editor)
        Time.timeScale = 1f;
        isPaused = false; // Garante estado inicial correto
        Debug.Log("[PauseManager] Start: Jogo iniciado/retomado.");
    }

    void Update()
    {
        // Deteta a tecla de pausa
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log($"[PauseManager] Tecla '{pauseKey}' pressionada.");
            TogglePause();
        }
    }

    /// <summary>
    /// Alterna entre o estado pausado e n�o pausado.
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused; // Inverte o estado

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    /// <summary>
    /// Pausa o jogo.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Congela o tempo do Unity (F�sica, Anima��es baseadas em tempo)
        Debug.Log("[PauseManager] Jogo PAUSADO. Time.timeScale = 0.");

        // Pausa FMOD
        if (masterBus.isValid()) masterBus.setPaused(true);

        // Desativa scripts do Player
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        // Troca as HUDs
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (gameHudPanel != null) gameHudPanel.SetActive(false);

        // Mostra e libera o cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("[PauseManager] Cursor liberado.");
    }

    /// <summary>
    /// Retoma o jogo.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Retoma o tempo normal do Unity
        Debug.Log("[PauseManager] Jogo RETOMADO. Time.timeScale = 1.");

        // Retoma FMOD
        if (masterBus.isValid()) masterBus.setPaused(false);

        // Reativa scripts do Player
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;

        // Troca as HUDs
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameHudPanel != null) gameHudPanel.SetActive(true);

        // Esconde e bloqueia o cursor (se o seu jogo o fizer normalmente)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("[PauseManager] Cursor bloqueado.");
    }

    // --- Fun��es Opcionais para Bot�es no Menu de Pausa ---

    /// <summary>
    /// Fun��o para ser chamada por um bot�o "Retomar" no menu de pausa.
    /// </summary>
    public void BotaoRetomar()
    {
        if (isPaused) ResumeGame();
    }

    /// <summary>
    /// Fun��o para ser chamada por um bot�o "Voltar ao Menu Principal".
    /// </summary>
    public void BotaoMenuPrincipal(string nomeCenaMenu = "Menu") // Nome padr�o "Menu"
    {
        Debug.Log($"[PauseManager] Voltando para o Menu Principal (Cena: {nomeCenaMenu})");
        // Garante que o tempo volte ao normal antes de carregar a cena
        Time.timeScale = 1f;
        if (masterBus.isValid()) masterBus.setPaused(false); // Garante que FMOD n�o fique pausado
        SceneManager.LoadScene(nomeCenaMenu);
    }

    /// <summary>
    /// Fun��o para ser chamada por um bot�o "Sair do Jogo".
    /// </summary>
    public void BotaoSairJogo()
    {
        Debug.Log("[PauseManager] Saindo do Jogo...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
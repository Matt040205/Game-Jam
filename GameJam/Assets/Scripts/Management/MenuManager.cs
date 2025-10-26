using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Para TextMeshPro
using UnityEngine.UI; // Para o Slider
using FMODUnity; // Para FMOD
using FMOD.Studio; // Para FMOD Bus

public class MenuManager : MonoBehaviour
{
    [Header("Configura��o de Cena")]
    [Tooltip("Nome da cena do jogo a ser carregada (Ex: 'GameScene').")]
    public string gameSceneName = "Game";
    [Tooltip("Nome da cena final a ser carregada (Ex: 'Final').")]
    public string finalSceneName = "Final";

    [Header("Grupos de Telas (GameObjects)")]
    [Tooltip("Tela Inicial: Cont�m Play, Options, Quit.")]
    public GameObject mainMenuPanel;
    [Tooltip("Tela de Op��es: Cont�m Volume, Cr�ditos, Voltar.")]
    public GameObject optionsPanel;
    [Tooltip("Tela de Cr�ditos: Cont�m texto dos devs e o bot�o Voltar.")]
    public GameObject creditsPanel;
    [Tooltip("Tela de Volume: Cont�m o slider/bot�es de volume e o bot�o Voltar.")]
    public GameObject volumePanel;
    [Tooltip("Tela de Fim de Jogo (Win/Lose).")]
    public GameObject finalScreenPanel;

    [Header("Refer�ncias Individuais")]
    [Tooltip("Texto com os nomes dos desenvolvedores (para a tela de Cr�ditos).")]
    public GameObject devCreditsText;

    [Header("Tela Final")]
    [Tooltip("O componente TextMeshProUGUI que exibir� 'You Win' ou 'You Lose'.")]
    public TextMeshProUGUI finalResultText;

    [Header("Controle de Volume FMOD")]
    [Tooltip("Arraste o componente Slider da UI para c�.")]
    public Slider volumeSlider;
    [Tooltip("Caminho do Bus FMOD a ser controlado (ex: 'bus:/' para Master, 'bus:/Musica').")]
    public string fmodBusPath = "bus:/";
    private Bus masterBus;

    private void Awake()
    {
        Debug.Log($"[MenuManager] Awake na cena: {SceneManager.GetActiveScene().name}");
        // Garante que o painel final comece desativado em qualquer cena
        if (finalScreenPanel != null) finalScreenPanel.SetActive(false);

        // Pega a refer�ncia do Bus FMOD
        try
        {
            masterBus = RuntimeManager.GetBus(fmodBusPath);
            if (!masterBus.isValid())
            {
                Debug.LogError($"[MenuManager] Falha ao obter o Bus FMOD: '{fmodBusPath}'.");
            }
            else
            {
                Debug.Log($"[MenuManager] Bus FMOD '{fmodBusPath}' obtido com sucesso.");
            }
        }
        catch (FMODUnity.BusNotFoundException)
        {
            Debug.LogError($"[MenuManager] Bus FMOD n�o encontrado: '{fmodBusPath}'.");
        }
    }

    public void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[MenuManager] Start na cena: {currentScene}");

        // Inicializa o Slider com o volume atual
        if (volumeSlider != null && masterBus.isValid())
        {
            float currentVolume;
            masterBus.getVolume(out currentVolume);
            volumeSlider.value = currentVolume;
            // Remove listener antigo para evitar duplica��o se a cena for recarregada
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            // Adiciona o listener
            volumeSlider.onValueChanged.AddListener(SetVolume);
            Debug.Log($"[MenuManager] Slider inicializado com volume: {currentVolume}. Listener adicionado.");
        }
        else if (volumeSlider == null)
        {
            Debug.LogWarning("[MenuManager] Refer�ncia do Volume Slider n�o atribu�da no Inspetor.");
        }
        else if (!masterBus.isValid())
        {
            Debug.LogWarning("[MenuManager] N�o foi poss�vel inicializar o slider, Bus FMOD inv�lido.");
        }

        // L�gica para mostrar pain�is corretos dependendo da cena
        if (currentScene == finalSceneName)
        {
            Debug.Log("[MenuManager] Estamos na cena Final. Chamando DisplayFinalResult.");
            DisplayFinalResult();
        }
        else // Assume Menu
        {
            Debug.Log("[MenuManager] Estamos numa cena de Menu. Configurando pain�is.");
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true); else Debug.LogError("[MenuManager] MainMenuPanel n�o atribu�do!");
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (volumePanel != null) volumePanel.SetActive(false);
            if (finalScreenPanel != null) finalScreenPanel.SetActive(false);
        }
    }

    // Fun��o chamada pelo Slider para definir o volume FMOD
    public void SetVolume(float volume)
    {
        if (masterBus.isValid())
        {
            masterBus.setVolume(volume);
            // Debug.Log($"[MenuManager] Volume do Bus '{fmodBusPath}' definido para: {volume}"); // Log opcional (pode poluir)
        }
        else
        {
            Debug.LogError($"[MenuManager] Tentativa de definir volume, mas o Bus '{fmodBusPath}' � inv�lido.");
        }
    }

    // Mostra o resultado na cena Final
    private void DisplayFinalResult()
    {
        Debug.Log("[MenuManager] DisplayFinalResult chamado.");
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);

        if (finalScreenPanel != null) finalScreenPanel.SetActive(true);
        else { Debug.LogError("[MenuManager] Final Screen Panel n�o atribu�do!"); return; }

        if (finalResultText == null) { Debug.LogError("[MenuManager] Final Result Text (TextMeshProUGUI) n�o atribu�do!"); return; }

        bool ganhou = false; // Default � perder
        // Tenta ler do GameManager (se existir)
        if (GameManager.Instance != null)
        {
            // AQUI ESTAVA O ERRO ANTERIOR: Usar a vari�vel diretamente em vez do m�todo
            // ganhou = GameManager.Instance.ganhou; // Forma antiga
            ganhou = GameManager.Instance.OJogadorGanhou(); // Forma correta usando o m�todo p�blico
            Debug.Log($"[MenuManager] GameManager encontrado. Estado 'ganhou' lido: {ganhou}");
        }
        else { Debug.LogWarning("[MenuManager] GameManager Instance � null na cena Final."); }

        if (ganhou)
        {
            finalResultText.text = "VOC� VENCEU!";
            finalResultText.color = Color.green;
            Debug.Log("[MenuManager] Exibindo Tela Final: VENCEU!");
        }
        else
        {
            finalResultText.text = "VOC� PERDEU.";
            finalResultText.color = Color.red;
            Debug.Log("[MenuManager] Exibindo Tela Final: PERDEU!");
        }
    }

    // Carrega a cena do jogo (nome definido no Inspector)
    public void Jogar()
    {
        Debug.Log($"[MenuManager] Bot�o Jogar clicado. Carregando Cena: {gameSceneName}");
        // Reseta o estado de vit�ria antes de carregar (se GameManager existir)
        if (GameManager.Instance != null) { GameManager.Instance.ganhou = false; }
        SceneManager.LoadScene(gameSceneName);
    }

    // Volta para a cena "Menu" (usado na tela Final)
    public void Menu()
    {
        Debug.Log("[MenuManager] Bot�o Menu clicado. Carregando Cena: Menu");
        SceneManager.LoadScene("Menu");
    }

    // Fecha a aplica��o
    public void Sair()
    {
        Debug.Log("[MenuManager] Bot�o Sair clicado. Saindo da Aplica��o...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- Fun��es de Navega��o ---
    public void Options()
    {
        Debug.Log("[MenuManager] Bot�o Options clicado.");
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);
    }

    public void Creditos()
    {
        Debug.Log("[MenuManager] Bot�o Cr�ditos clicado.");
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (devCreditsText != null) devCreditsText.SetActive(true);
    }

    public void Volume()
    {
        Debug.Log("[MenuManager] Bot�o Volume clicado.");
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(true);
    }

    public void VoltarCreditos()
    {
        Debug.Log("[MenuManager] Bot�o Voltar (Cr�ditos) clicado.");
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (devCreditsText != null) devCreditsText.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void VoltarOptions()
    {
        Debug.Log("[MenuManager] Bot�o Voltar (Op��es) clicado.");
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void VoltarVolume()
    {
        Debug.Log("[MenuManager] Bot�o Voltar (Volume) clicado.");
        if (volumePanel != null) volumePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }
}
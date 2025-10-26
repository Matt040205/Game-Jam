using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Para TextMeshPro
using UnityEngine.UI; // Para o Slider
using FMODUnity; // Para FMOD
using FMOD.Studio; // Para FMOD Bus

public class MenuManager : MonoBehaviour
{
    [Header("Configuração de Cena")]
    [Tooltip("Nome da cena do jogo a ser carregada (Ex: 'GameScene').")]
    public string gameSceneName = "Game";
    [Tooltip("Nome da cena final a ser carregada (Ex: 'Final').")]
    public string finalSceneName = "Final";

    [Header("Grupos de Telas (GameObjects)")]
    [Tooltip("Tela Inicial: Contém Play, Options, Quit.")]
    public GameObject mainMenuPanel;
    [Tooltip("Tela de Opções: Contém Volume, Créditos, Voltar.")]
    public GameObject optionsPanel;
    [Tooltip("Tela de Créditos: Contém texto dos devs e o botão Voltar.")]
    public GameObject creditsPanel;
    [Tooltip("Tela de Volume: Contém o slider/botões de volume e o botão Voltar.")]
    public GameObject volumePanel;
    [Tooltip("Tela de Fim de Jogo (Win/Lose).")]
    public GameObject finalScreenPanel;

    [Header("Referências Individuais")]
    [Tooltip("Texto com os nomes dos desenvolvedores (para a tela de Créditos).")]
    public GameObject devCreditsText;

    [Header("Tela Final")]
    [Tooltip("O componente TextMeshProUGUI que exibirá 'You Win' ou 'You Lose'.")]
    public TextMeshProUGUI finalResultText;

    [Header("Controle de Volume FMOD")]
    [Tooltip("Arraste o componente Slider da UI para cá.")]
    public Slider volumeSlider;
    [Tooltip("Caminho do Bus FMOD a ser controlado (ex: 'bus:/' para Master, 'bus:/Musica').")]
    public string fmodBusPath = "bus:/";
    private Bus masterBus;

    private void Awake()
    {
        Debug.Log($"[MenuManager] Awake na cena: {SceneManager.GetActiveScene().name}");
        // Garante que o painel final comece desativado em qualquer cena
        if (finalScreenPanel != null) finalScreenPanel.SetActive(false);

        // Pega a referência do Bus FMOD
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
            Debug.LogError($"[MenuManager] Bus FMOD não encontrado: '{fmodBusPath}'.");
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
            // Remove listener antigo para evitar duplicação se a cena for recarregada
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            // Adiciona o listener
            volumeSlider.onValueChanged.AddListener(SetVolume);
            Debug.Log($"[MenuManager] Slider inicializado com volume: {currentVolume}. Listener adicionado.");
        }
        else if (volumeSlider == null)
        {
            Debug.LogWarning("[MenuManager] Referência do Volume Slider não atribuída no Inspetor.");
        }
        else if (!masterBus.isValid())
        {
            Debug.LogWarning("[MenuManager] Não foi possível inicializar o slider, Bus FMOD inválido.");
        }

        // Lógica para mostrar painéis corretos dependendo da cena
        if (currentScene == finalSceneName)
        {
            Debug.Log("[MenuManager] Estamos na cena Final. Chamando DisplayFinalResult.");
            DisplayFinalResult();
        }
        else // Assume Menu
        {
            Debug.Log("[MenuManager] Estamos numa cena de Menu. Configurando painéis.");
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true); else Debug.LogError("[MenuManager] MainMenuPanel não atribuído!");
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (volumePanel != null) volumePanel.SetActive(false);
            if (finalScreenPanel != null) finalScreenPanel.SetActive(false);
        }
    }

    // Função chamada pelo Slider para definir o volume FMOD
    public void SetVolume(float volume)
    {
        if (masterBus.isValid())
        {
            masterBus.setVolume(volume);
            // Debug.Log($"[MenuManager] Volume do Bus '{fmodBusPath}' definido para: {volume}"); // Log opcional (pode poluir)
        }
        else
        {
            Debug.LogError($"[MenuManager] Tentativa de definir volume, mas o Bus '{fmodBusPath}' é inválido.");
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
        else { Debug.LogError("[MenuManager] Final Screen Panel não atribuído!"); return; }

        if (finalResultText == null) { Debug.LogError("[MenuManager] Final Result Text (TextMeshProUGUI) não atribuído!"); return; }

        bool ganhou = false; // Default é perder
        // Tenta ler do GameManager (se existir)
        if (GameManager.Instance != null)
        {
            // AQUI ESTAVA O ERRO ANTERIOR: Usar a variável diretamente em vez do método
            // ganhou = GameManager.Instance.ganhou; // Forma antiga
            ganhou = GameManager.Instance.OJogadorGanhou(); // Forma correta usando o método público
            Debug.Log($"[MenuManager] GameManager encontrado. Estado 'ganhou' lido: {ganhou}");
        }
        else { Debug.LogWarning("[MenuManager] GameManager Instance é null na cena Final."); }

        if (ganhou)
        {
            finalResultText.text = "VOCÊ VENCEU!";
            finalResultText.color = Color.green;
            Debug.Log("[MenuManager] Exibindo Tela Final: VENCEU!");
        }
        else
        {
            finalResultText.text = "VOCÊ PERDEU.";
            finalResultText.color = Color.red;
            Debug.Log("[MenuManager] Exibindo Tela Final: PERDEU!");
        }
    }

    // Carrega a cena do jogo (nome definido no Inspector)
    public void Jogar()
    {
        Debug.Log($"[MenuManager] Botão Jogar clicado. Carregando Cena: {gameSceneName}");
        // Reseta o estado de vitória antes de carregar (se GameManager existir)
        if (GameManager.Instance != null) { GameManager.Instance.ganhou = false; }
        SceneManager.LoadScene(gameSceneName);
    }

    // Volta para a cena "Menu" (usado na tela Final)
    public void Menu()
    {
        Debug.Log("[MenuManager] Botão Menu clicado. Carregando Cena: Menu");
        SceneManager.LoadScene("Menu");
    }

    // Fecha a aplicação
    public void Sair()
    {
        Debug.Log("[MenuManager] Botão Sair clicado. Saindo da Aplicação...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- Funções de Navegação ---
    public void Options()
    {
        Debug.Log("[MenuManager] Botão Options clicado.");
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);
    }

    public void Creditos()
    {
        Debug.Log("[MenuManager] Botão Créditos clicado.");
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (devCreditsText != null) devCreditsText.SetActive(true);
    }

    public void Volume()
    {
        Debug.Log("[MenuManager] Botão Volume clicado.");
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(true);
    }

    public void VoltarCreditos()
    {
        Debug.Log("[MenuManager] Botão Voltar (Créditos) clicado.");
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (devCreditsText != null) devCreditsText.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void VoltarOptions()
    {
        Debug.Log("[MenuManager] Botão Voltar (Opções) clicado.");
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void VoltarVolume()
    {
        Debug.Log("[MenuManager] Botão Voltar (Volume) clicado.");
        if (volumePanel != null) volumePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }
}
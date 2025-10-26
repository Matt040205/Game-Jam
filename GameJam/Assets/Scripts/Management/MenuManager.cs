using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Importante para usar TextMeshPro

public class MenuManager : MonoBehaviour
{
    // CENA DO JOGO
    [Header("Configuração de Cena")]
    [Tooltip("Nome da cena do jogo a ser carregada (Ex: 'GameScene').")]
    public string gameSceneName = "Game"; 
    public string finalSceneName = "Final";

    // GRUPOS DE TELAS (melhor que botões individuais)
    [Header("Grupos de Telas (GameObjects)")]
    [Tooltip("Tela Inicial: Contém Play, Options, Quit.")]
    public GameObject mainMenuPanel;
    [Tooltip("Tela de Opções: Contém Volume, Créditos, Voltar.")]
    public GameObject optionsPanel;
    [Tooltip("Tela de Créditos: Contém texto dos devs e o botão Voltar.")]
    public GameObject creditsPanel;
    [Tooltip("Tela de Volume: Contém o slider/botões de volume e o botão Voltar.")]
    public GameObject volumePanel;
    // NOVO: Painel para exibir o resultado final
    [Tooltip("Tela de Fim de Jogo (Win/Lose).")]
    public GameObject finalScreenPanel;

    [Header("Referências Individuais")]
    [Tooltip("Texto com os nomes dos desenvolvedores (para a tela de Créditos).")]
    public GameObject devCreditsText;

    // NOVO: TextMeshPro para exibir o resultado (Win/Lose)
    [Header("Tela Final")]
    [Tooltip("O componente TextMeshPro que exibirá 'You Win' ou 'You Lose'.")]
    public TextMeshProUGUI finalResultText;

    // Lógica de Menu Principal no Awake
    private void Awake()
    {
        // ... (código Awake existente)
        if (finalScreenPanel != null) finalScreenPanel.SetActive(false);
    }

    // Lógica para a cena Final deve ser executada no Start
    public void Start()
    {
        // Verifica se a cena atual é a cena "Final"
        if (SceneManager.GetActiveScene().name == finalSceneName)
        {
            DisplayFinalResult();
        }
        else
        {
            // Inicializa o menu principal
            mainMenuPanel.SetActive(true);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            volumePanel.SetActive(false);
            if (finalScreenPanel != null) finalScreenPanel.SetActive(false);
        }
    }

    // NOVO: Método para ler o GameManager e exibir o resultado
    private void DisplayFinalResult()
    {
        // Desativa todos os painéis do menu principal
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        volumePanel.SetActive(false);

        // Ativa o painel de fim de jogo
        if (finalScreenPanel != null) finalScreenPanel.SetActive(true);
        else { Debug.LogError("Final Screen Panel não atribuído no Menu Manager!"); return; }

        if (finalResultText == null)
        {
            Debug.LogError("Final Result Text (TextMeshProUGUI) não atribuído no Menu Manager!");
            return;
        }

        bool ganhou = false;

        if (GameManager.Instance != null)
        {
            ganhou = GameManager.Instance.ganhou;
        }
        else
        {
            Debug.LogWarning("GameManager Instance is null. Defaulting to 'You Lose'.");
        }

        if (ganhou)
        {
            finalResultText.text = "VOCÊ VENCEU!";
            finalResultText.color = Color.green; // Opcional: cor verde para vitória
            Debug.Log("Exibindo Tela Final: VENCEU!");
        }
        else
        {
            finalResultText.text = "VOCÊ PERDEU.";
            finalResultText.color = Color.red; // Opcional: cor vermelha para derrota
            Debug.Log("Exibindo Tela Final: PERDEU!");
        }
    }


    public void Jogar()
    {
        Debug.Log("Iniciando Jogo...");
        // Garante que o estado de vitória seja resetado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ganhou = false;
        }
        // Carrega a cena do jogo
        SceneManager.LoadScene("Game");
    }


    public void Sair()
    {
        Debug.Log("Saindo da Aplicação...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    public void Options()
    {
        Debug.Log("Abrindo Opções.");
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        volumePanel.SetActive(false);
    }

    public void Creditos()
    {
        Debug.Log("Abrindo Créditos.");
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);

        // Ativa o texto dos desenvolvedores
        if (devCreditsText != null)
        {
            devCreditsText.SetActive(true);
        }
    }

    public void Volume()
    {
        Debug.Log("Abrindo Opções de Volume.");
        optionsPanel.SetActive(false);
        volumePanel.SetActive(true);
    }

    public void VoltarCreditos()
    {
        Debug.Log("Voltando dos Créditos para Opções.");
        creditsPanel.SetActive(false);

        // Desativa o texto dos desenvolvedores ao sair dos créditos
        if (devCreditsText != null)
        {
            devCreditsText.SetActive(false);
        }

        optionsPanel.SetActive(true);
    }

    public void VoltarOptions()
    {
        Debug.Log("Voltando das Opções para Menu Principal.");
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Método para Voltar do Volume para o Options
    public void VoltarVolume()
    {
        Debug.Log("Voltando do Volume para Opções.");
        volumePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
}
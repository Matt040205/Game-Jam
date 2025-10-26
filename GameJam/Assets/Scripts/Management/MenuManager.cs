using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Importante para usar TextMeshPro

public class MenuManager : MonoBehaviour
{
    // CENA DO JOGO
    [Header("Configura��o de Cena")]
    [Tooltip("Nome da cena do jogo a ser carregada (Ex: 'GameScene').")]
    public string gameSceneName = "Game"; 
    public string finalSceneName = "Final";

    // GRUPOS DE TELAS (melhor que bot�es individuais)
    [Header("Grupos de Telas (GameObjects)")]
    [Tooltip("Tela Inicial: Cont�m Play, Options, Quit.")]
    public GameObject mainMenuPanel;
    [Tooltip("Tela de Op��es: Cont�m Volume, Cr�ditos, Voltar.")]
    public GameObject optionsPanel;
    [Tooltip("Tela de Cr�ditos: Cont�m texto dos devs e o bot�o Voltar.")]
    public GameObject creditsPanel;
    [Tooltip("Tela de Volume: Cont�m o slider/bot�es de volume e o bot�o Voltar.")]
    public GameObject volumePanel;
    // NOVO: Painel para exibir o resultado final
    [Tooltip("Tela de Fim de Jogo (Win/Lose).")]
    public GameObject finalScreenPanel;

    [Header("Refer�ncias Individuais")]
    [Tooltip("Texto com os nomes dos desenvolvedores (para a tela de Cr�ditos).")]
    public GameObject devCreditsText;

    // NOVO: TextMeshPro para exibir o resultado (Win/Lose)
    [Header("Tela Final")]
    [Tooltip("O componente TextMeshPro que exibir� 'You Win' ou 'You Lose'.")]
    public TextMeshProUGUI finalResultText;

    // L�gica de Menu Principal no Awake
    private void Awake()
    {
        // ... (c�digo Awake existente)
        if (finalScreenPanel != null) finalScreenPanel.SetActive(false);
    }

    // L�gica para a cena Final deve ser executada no Start
    public void Start()
    {
        // Verifica se a cena atual � a cena "Final"
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

    // NOVO: M�todo para ler o GameManager e exibir o resultado
    private void DisplayFinalResult()
    {
        // Desativa todos os pain�is do menu principal
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        volumePanel.SetActive(false);

        // Ativa o painel de fim de jogo
        if (finalScreenPanel != null) finalScreenPanel.SetActive(true);
        else { Debug.LogError("Final Screen Panel n�o atribu�do no Menu Manager!"); return; }

        if (finalResultText == null)
        {
            Debug.LogError("Final Result Text (TextMeshProUGUI) n�o atribu�do no Menu Manager!");
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
            finalResultText.text = "VOC� VENCEU!";
            finalResultText.color = Color.green; // Opcional: cor verde para vit�ria
            Debug.Log("Exibindo Tela Final: VENCEU!");
        }
        else
        {
            finalResultText.text = "VOC� PERDEU.";
            finalResultText.color = Color.red; // Opcional: cor vermelha para derrota
            Debug.Log("Exibindo Tela Final: PERDEU!");
        }
    }


    public void Jogar()
    {
        Debug.Log("Iniciando Jogo...");
        // Garante que o estado de vit�ria seja resetado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ganhou = false;
        }
        // Carrega a cena do jogo
        SceneManager.LoadScene("Game");
    }


    public void Sair()
    {
        Debug.Log("Saindo da Aplica��o...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    public void Options()
    {
        Debug.Log("Abrindo Op��es.");
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        volumePanel.SetActive(false);
    }

    public void Creditos()
    {
        Debug.Log("Abrindo Cr�ditos.");
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
        Debug.Log("Abrindo Op��es de Volume.");
        optionsPanel.SetActive(false);
        volumePanel.SetActive(true);
    }

    public void VoltarCreditos()
    {
        Debug.Log("Voltando dos Cr�ditos para Op��es.");
        creditsPanel.SetActive(false);

        // Desativa o texto dos desenvolvedores ao sair dos cr�ditos
        if (devCreditsText != null)
        {
            devCreditsText.SetActive(false);
        }

        optionsPanel.SetActive(true);
    }

    public void VoltarOptions()
    {
        Debug.Log("Voltando das Op��es para Menu Principal.");
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // M�todo para Voltar do Volume para o Options
    public void VoltarVolume()
    {
        Debug.Log("Voltando do Volume para Op��es.");
        volumePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para carregar cenas

public class MenuManager : MonoBehaviour
{
    // CENA DO JOGO
    [Header("Configuração de Cena")]
    [Tooltip("Nome da cena do jogo a ser carregada (Ex: 'GameScene').")]
    public string gameSceneName = "SampleScene"; // Nome padrão, ajuste no Inspector

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

    [Header("Referências Individuais")]
    [Tooltip("Texto com os nomes dos desenvolvedores (para a tela de Créditos).")]
    public GameObject devCreditsText;

    private void Awake()
    {
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        volumePanel.SetActive(false);

        if (devCreditsText != null)
        {
            devCreditsText.SetActive(false);
        }
    }


    public void Jogar()
    {
        Debug.Log("Iniciando Jogo...");
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
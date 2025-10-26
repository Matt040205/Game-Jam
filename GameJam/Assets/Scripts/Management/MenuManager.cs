using UnityEngine;
using UnityEngine.SceneManagement; // Necess�rio para carregar cenas

public class MenuManager : MonoBehaviour
{
    // CENA DO JOGO
    [Header("Configura��o de Cena")]
    [Tooltip("Nome da cena do jogo a ser carregada (Ex: 'GameScene').")]
    public string gameSceneName = "SampleScene"; // Nome padr�o, ajuste no Inspector

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

    [Header("Refer�ncias Individuais")]
    [Tooltip("Texto com os nomes dos desenvolvedores (para a tela de Cr�ditos).")]
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
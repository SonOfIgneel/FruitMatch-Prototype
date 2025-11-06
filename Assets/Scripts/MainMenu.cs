using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown gridDropdown;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject mainMenuPanel, gamePlayPanel;
    [SerializeField] private GameManager manager;
    [SerializeField] private GameSaveData saveData;

    private int rows, cols;

    void Start()
    {
        gridDropdown.ClearOptions();
        gridDropdown.AddOptions(new System.Collections.Generic.List<string> { "2x2", "3x3", "4x4", "5x6" });
        gridDropdown.onValueChanged.AddListener(OnGridChanged);

        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new System.Collections.Generic.List<string> { "Easy", "Medium", "Hard" });
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

        startButton.onClick.AddListener(OnStartNewGame);
        loadButton.onClick.AddListener(OnLoadGame);
        quitButton.onClick.AddListener(OnQuitGame);

        if (saveData.rows == 0)
            loadButton.interactable = false;
    }

    void OnStartNewGame()
    {
        mainMenuPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
        manager.StartNewGame();
    }

    void OnLoadGame()
    {
        mainMenuPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
        manager.StartLoadGame();
    }

    void OnQuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void OnGridChanged(int index)
    {
        string selected = gridDropdown.options[index].text;

        string[] parts = selected.Split('x');

        rows = int.Parse(parts[0]);
        cols = int.Parse(parts[1]);

        manager.rows = rows;
        manager.cols = cols;

        Debug.Log("Selected Grid: " + rows + "x" + cols);
    }

    private void OnDifficultyChanged(int index)
    {
        if(index == 0)
            manager.revealTime = 2f;
        else if(index == 1)
            manager.revealTime = 1f;
        else
            manager.revealTime = 0.5f;
    }

}

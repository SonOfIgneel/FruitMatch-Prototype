using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalPairsText;
    [SerializeField] private TextMeshProUGUI pairsFoundText;
    [SerializeField] private TextMeshProUGUI turnsText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI totalPairsGameOverText;
    [SerializeField] private TextMeshProUGUI timerGameOverText;
    [SerializeField] private Button homeButton;
    [SerializeField] private GameObject gameOverScreen, gamePlayScreen, mainMenuScreen;

    private void Start()
    {
        homeButton.onClick.AddListener(OnHomeButton);
    }

    public void SetTotalPairs(int value)
    {
        if (totalPairsText != null)
            totalPairsText.text = "Total of pairs: " + value.ToString();
    }

    public void SetPairsFound(int value)
    {
        if (pairsFoundText != null)
            pairsFoundText.text = "Total pairs found: " + value.ToString();
    }

    public void SetTurns(int value)
    {
        if (turnsText != null)
            turnsText.text = "Total turns attempted: " + value.ToString();
    }

    public void SetTimer(float seconds)
    {
        if (timerText != null)
            timerText.text = "Time: " + seconds.ToString("0.0") + "s";
    }

    public void UpdateAll(int totalPairs, int foundPairs, int turns)
    {
        SetTotalPairs(totalPairs);
        SetPairsFound(foundPairs);
        SetTurns(turns);
    }

    public void UpdateGameover(int turns, float seconds)
    {
        totalPairsGameOverText.text = "Turns attempted: " + turns;
        timerGameOverText.text = "Game completed in: " + seconds.ToString("0.0") + "s";
    }

    public void OnHomeButton()
    {
        gamePlayScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
    }
}

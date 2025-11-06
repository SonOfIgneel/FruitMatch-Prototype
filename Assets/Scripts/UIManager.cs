using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalPairsText;
    [SerializeField] private TextMeshProUGUI pairsFoundText;
    [SerializeField] private TextMeshProUGUI turnsText;

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

    public void UpdateAll(int totalPairs, int foundPairs, int turns)
    {
        SetTotalPairs(totalPairs);
        SetPairsFound(foundPairs);
        SetTurns(turns);
    }
}

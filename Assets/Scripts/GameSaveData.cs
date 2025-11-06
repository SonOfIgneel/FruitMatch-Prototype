using UnityEngine;

[CreateAssetMenu(fileName = "GameSaveData", menuName = "MatchGame/GameSaveData")]
public class GameSaveData : ScriptableObject
{
    public int rows;
    public int cols;

    public int[] cardIds;        
    public bool[] matched;        
    public bool[] faceUp;         

    public int turnCount;
    public int foundPairs;
    public int totalPairs;
    public float savedTime;

    public bool hasSavedGame = false;

    public void Clear()
    {
        rows = 0;
        cols = 0;
        cardIds = null;
        matched = null;
        faceUp = null;
        turnCount = 0;
        foundPairs = 0;
        totalPairs = 0;
        savedTime = 0;
        hasSavedGame = false;
    }
}

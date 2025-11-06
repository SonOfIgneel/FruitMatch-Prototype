using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs & assets")]
    public GameObject cardPrefab;
    public List<Sprite> faceSprites;
    public Sprite backSprite;

    [Header("Grid settings")]
    public int rows = 2;
    public int cols = 3;
    public Vector2 cellSize = new Vector2(1.2f, 1.6f);
    public Vector2 gridOffset = new Vector2(0f, 0.8f);
    public float scale = 1f;

    [Header("Gameplay settings")]
    public float mismatchDelay = 0.5f;
    public int totalPairs = 0;
    public int foundPairs = 0;
    public int turnCount = 0;
    public float revealTime = 2f;
    public bool canInteract = false;
    public GameSaveData saveData;
    private bool resolvingMismatch = false;
    public UIManager uiManager;
    public GameObject gameOverPanel;
    public List<GameObject> existingCards;
    public float timer = 0f;
    private bool timerRunning = false;

    private List<Card> allCards = new List<Card>();
    private List<Card> faceUpOrder = new List<Card>();
    private HashSet<int> matchedIds = new HashSet<int>();
    private bool newGame;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (timerRunning)
        {
            timer += Time.deltaTime;
            uiManager.SetTimer(timer);
            saveData.savedTime = timer;
        }
    }

    public void StartNewGame()
    {
        faceUpOrder.Clear();
        newGame = true;
        GenerateGrid(rows, cols, true);
    }

    public void StartLoadGame()
    {
        LoadGame();
    }

    public void GenerateGrid(int r, int c, bool doReveal = true)
    {
        foreach (var cd in allCards) Destroy(cd.gameObject);
        allCards.Clear();
        faceUpOrder.Clear();
        matchedIds.Clear();

        rows = Mathf.Max(1, r);
        cols = Mathf.Max(1, c);
        turnCount = 0;

        int total = rows * cols;
        if (total % 2 != 0)
        {
            Debug.LogWarning("Total cells not even - making it even by reducing one column");
            if (cols > 1) cols--;
            total = rows * cols;
        }

        int pairs = total / 2;
        if (faceSprites.Count < pairs)
        {
            Debug.LogError("Not enough faceSprites assigned for requested grid size. Need at least " + pairs);
            return;
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < pairs; i++)
        {
            indices.Add(i);
            indices.Add(i);
        }
        Shuffle(indices);

        float gridWidth = cols * cellSize.x;
        float gridHeight = rows * cellSize.y;

        float fitScale = AdjustScaleToFitScreen(gridWidth, gridHeight);

        if (rows > 3)
        {
            float scaleBoost = 1f + Mathf.Clamp01((rows - 3) * 0.1f);
            fitScale *= scaleBoost;
        }

        Vector2 scaledCell = cellSize * fitScale;

        Vector2 startPosition = new Vector2(
            -(cols - 1) * scaledCell.x / 2f,
            (rows - 1) * scaledCell.y / 2f
        );

        Vector2 gridOffset = new Vector2(1.0f, 0.8f);

        int idx = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(
                    startPosition.x + x * scaledCell.x,
                    startPosition.y - y * scaledCell.y,
                    0f
                ) + (Vector3)gridOffset;

                GameObject go = Instantiate(cardPrefab, pos, Quaternion.identity);
                go.transform.localScale = Vector3.one * fitScale;
                existingCards.Add(go);

                Card card = go.GetComponent<Card>();
                if (card == null)
                {
                    Debug.LogError("Card prefab missing Card script!");
                    Destroy(go);
                    continue;
                }

                int spriteIndex = indices[idx];
                card.id = spriteIndex;
                card.frontSprite = faceSprites[spriteIndex];
                if (backSprite != null) card.backSprite = backSprite;
                card.isMatched = false;

                card.onFlipCompleted.AddListener(OnCardFlipCompleted);
                card.onFlipRequested.AddListener(OnCardFlipRequested);

                allCards.Add(card);
                idx++;
            }
        }

        totalPairs = pairs;
        foundPairs = 0;

        if (doReveal)
            StartCoroutine(RevealAllCardsTemporarily(revealTime));
        else
            canInteract = true;

        if(newGame)
            uiManager.UpdateAll(totalPairs, foundPairs, turnCount);
    }

    private float AdjustScaleToFitScreen(float gridWidth, float gridHeight)
    {
        Camera cam = Camera.main;
        float screenHeight = 2f * cam.orthographicSize;
        float screenWidth = screenHeight * cam.aspect;

        float padding = 0.9f;

        float widthRatio = (screenWidth * padding) / gridWidth;
        float heightRatio = (screenHeight * padding) / gridHeight;
        float fitRatio = Mathf.Min(widthRatio, heightRatio);

        fitRatio = Mathf.Min(fitRatio, 1f);

        Debug.Log($"Adjusted scale ratio: {fitRatio:F2}");
        return fitRatio;
    }

    private void OnCardFlipRequested(Card card)
    {
        Debug.Log("Flip requested: " + card.name);
    }

    private void OnCardFlipCompleted(Card card)
    {
        if (card.isMatched) return;

        if (!card.IsFaceUp())
        {
            faceUpOrder.Remove(card);
            return;
        }

        if (resolvingMismatch)
        {
            if (!faceUpOrder.Contains(card))
                faceUpOrder.Add(card);

            return;
        }

        if (faceUpOrder.Count == 2)
        {
            return;
        }

        if (!faceUpOrder.Contains(card))
            faceUpOrder.Add(card);

        if (faceUpOrder.Count < 2) return;

        Card a = faceUpOrder[0];
        Card b = faceUpOrder[1];

        turnCount++;
        Debug.Log("Turn: " + turnCount);

        if (a.id == b.id)
        {
            AudioManager.Instance.PlayMatch();
            a.isMatched = true;
            b.isMatched = true;

            DisableCardInteraction(a);
            DisableCardInteraction(b);

            foundPairs++;
            Debug.Log("Pair found! Total found: " + foundPairs + "/" + totalPairs);

            faceUpOrder.Clear();

            if (foundPairs >= totalPairs)
            {
                Debug.Log("🎉 Game Completed!");
                AudioManager.Instance.PlayGameOver();
                foreach (GameObject go in existingCards)
                {
                    Destroy(go);
                }
                existingCards.Clear();
                allCards.Clear();
                faceUpOrder.Clear();
                matchedIds.Clear();
                uiManager.UpdateGameover(turnCount, timer);
                gameOverPanel.SetActive(true);
                saveData.Clear();
            }
            else
            {
                SaveGame();
            }
        }
        else
        {
            StartCoroutine(FlipBackAfterDelay(a, b));
        }

        uiManager.UpdateAll(totalPairs, foundPairs, turnCount);
    }

    private IEnumerator FlipBackAfterDelay(Card a, Card b)
    {
        resolvingMismatch = true;

        yield return new WaitForSeconds(mismatchDelay);

        if (!a.isMatched && a.IsFaceUp())
            a.RequestFlip();

        if (!b.isMatched && b.IsFaceUp())
            b.RequestFlip();

        faceUpOrder.Clear();

        resolvingMismatch = false;
    }


    private void DisableCardInteraction(Card c)
    {
        c.isMatched = true;
        var coll = c.GetComponent<Collider2D>();
        if (coll) coll.enabled = false;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int k = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[k];
            list[k] = tmp;
        }
    }

    private IEnumerator RevealAllCardsTemporarily(float delay)
    {
        canInteract = false;

        foreach (var card in allCards)
            card.ForceShowFront();

        yield return new WaitForSeconds(delay);

        foreach (var card in allCards)
            card.FlipToBackAnimated();

        Debug.Log("All cards hidden — game start!");
        canInteract = true;
        timer = 0f;
        timerRunning = true;
    }

    public void SaveGame()
    {
        saveData.rows = rows;
        saveData.cols = cols;

        int size = rows * cols;

        saveData.cardIds = new int[size];
        saveData.matched = new bool[size];
        saveData.faceUp = new bool[size];

        for (int i = 0; i < allCards.Count; i++)
        {
            saveData.cardIds[i] = allCards[i].id;
            saveData.matched[i] = allCards[i].isMatched;
            saveData.faceUp[i] = allCards[i].IsFaceUp();
        }

        saveData.turnCount = turnCount;
        saveData.foundPairs = foundPairs;
        saveData.totalPairs = totalPairs;

        saveData.hasSavedGame = true;

        Debug.Log("✅ Game Saved");
    }

    public void LoadGame()
    {
        if (!saveData.hasSavedGame)
        {
            Debug.Log("No saved game found");
            return;
        }

        rows = saveData.rows;
        cols = saveData.cols;

        GenerateGrid(rows, cols, false);

        int size = rows * cols;

        for (int i = 0; i < size; i++)
        {
            allCards[i].id = saveData.cardIds[i];
            allCards[i].frontSprite = faceSprites[saveData.cardIds[i]];

            if (saveData.matched[i])
            {
                allCards[i].isMatched = true;
                allCards[i].ForceShowFront();
                DisableCardInteraction(allCards[i]); 
            }
            else
            {
                allCards[i].isMatched = false;

                if (saveData.faceUp[i])
                    allCards[i].ForceShowFront();
                else
                    allCards[i].ForceShowBack();
            }
        }


        turnCount = saveData.turnCount;
        foundPairs = saveData.foundPairs;
        totalPairs = saveData.totalPairs;

        canInteract = true;

        timer = saveData.savedTime;
        uiManager.SetTimer(timer);
        timerRunning = true;

        uiManager.UpdateAll(saveData.totalPairs, saveData.foundPairs, saveData.turnCount);

        Debug.Log("✅ Game Loaded");
    }
}

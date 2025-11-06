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
    public float mismatchDelay = 0.6f;
    public int totalPairs = 0;
    public int foundPairs = 0;
    public int turnCount = 0;
    public float revealTime = 2f;

    private List<Card> allCards = new List<Card>();
    private List<Card> faceUpOrder = new List<Card>();
    private HashSet<int> matchedIds = new HashSet<int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateGrid(rows, cols);
    }

    public void GenerateGrid(int r, int c)
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

        StartCoroutine(RevealAllCardsTemporarily(revealTime));
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

        if (card.IsFaceUp())
        {
            faceUpOrder.RemoveAll(c => c == null || c.isMatched);

            if (!faceUpOrder.Contains(card))
                faceUpOrder.Add(card);

            if (faceUpOrder.Count == 2)
            {
                turnCount++;
                Debug.Log("Turn: " + turnCount);

                Card a = faceUpOrder[0];
                Card b = faceUpOrder[1];

                if (a.id == b.id)
                {
                    matchedIds.Add(a.id);
                    a.isMatched = true;
                    b.isMatched = true;

                    DisableCardInteraction(a);
                    DisableCardInteraction(b);

                    foundPairs++;
                    Debug.Log("Pair found! Total found: " + foundPairs + "/" + totalPairs);

                    if (foundPairs >= totalPairs)
                    {
                        Debug.Log("🎉 Game Completed!");
                    }

                    faceUpOrder.Clear();
                }
                else
                {
                    StartCoroutine(FlipBackAfterDelay(a, b));
                }
            }
        }
        else
        {
            faceUpOrder.Remove(card);
        }
    }


    private IEnumerator FlipBackAfterDelay(Card a, Card b)
    {
        yield return new WaitForSeconds(mismatchDelay);

        if (!a.isMatched && a.IsFaceUp()) a.RequestFlip(); 
        if (!b.isMatched && b.IsFaceUp()) b.RequestFlip();
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
        foreach (var card in allCards)
        {
            card.ForceShowFront();
        }

        yield return new WaitForSeconds(delay);

        foreach (var card in allCards)
        {
            card.ForceShowBack();
        }

        Debug.Log("All cards hidden — game start!");
    }

}

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour
{
    public int id = -1;
    public bool isMatched = false;

    [Header("Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Flip settings")]
    public float flipDuration = 0.28f;
    public bool startsFaceUp = false;

    [Header("Events")]
    public UnityEvent<Card> onFlipRequested;
    public UnityEvent<Card> onFlipCompleted;

    private SpriteRenderer sr;
    private bool isFront = false;
    private bool isAnimating = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        isFront = startsFaceUp;
        sr.sprite = isFront ? frontSprite : backSprite;

        var bc = GetComponent<BoxCollider2D>();
        if (bc == null) bc = gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = false;
    }

    void OnMouseDown()
    {
        if (!GameManager.Instance.canInteract) return;
        if (isAnimating || isMatched) return;
        RequestFlip();
    }

    public void RequestFlip()
    {
        AudioManager.Instance.PlayFlip();
        onFlipRequested?.Invoke(this);
        StartCoroutine(FlipRoutine(!isFront));
    }

    public IEnumerator FlipRoutine(bool showFront)
    {
        isAnimating = true;
        float half = flipDuration / 2f;
        Vector3 start = transform.localScale;
        Vector3 mid = new Vector3(0f, start.y, start.z);
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, mid, t / half);
            yield return null;
        }

        isFront = showFront;
        sr.sprite = isFront ? frontSprite : backSprite;

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(mid, start, t / half);
            yield return null;
        }

        transform.localScale = start;
        isAnimating = false;

        onFlipCompleted?.Invoke(this);
    }

    public void ForceShowFront()
    {
        isFront = true;
        sr.sprite = frontSprite;
    }

    public void ForceShowBack()
    {
        AudioManager.Instance.PlayFlip();
        isFront = false;
        sr.sprite = backSprite;
    }

    public void FlipToBackAnimated()
    {
        if (isAnimating) return; // avoid animation stacking
        StartCoroutine(FlipRoutine(false));
    }

    public bool IsFaceUp() => isFront;
    public bool IsAnimating() => isAnimating;
}

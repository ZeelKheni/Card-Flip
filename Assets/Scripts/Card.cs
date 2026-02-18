using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int CardID { get; private set; }
    public bool IsFlipped { get; private set; }
    public bool IsMatched { get; private set; }

    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite backSprite;
    private Sprite frontSprite;

    private Action<Card> onCardClicked;
    private bool isAnimating = false;

    public void Initialize(int id, Sprite front, Action<Card> onClickCallback)
    {
        CardID = id;
        frontSprite = front;
        onCardClicked = onClickCallback;
        cardImage.sprite = backSprite;
        IsFlipped = false;
        IsMatched = false;
    }

    // Used for loading saved state
    public void SetState(bool flipped, bool matched)
    {
        IsFlipped = flipped;
        IsMatched = matched;
        cardImage.sprite = IsFlipped || IsMatched ? frontSprite : backSprite;

        if (IsMatched)
        {
            GetComponent<CanvasGroup>().alpha = 0.5f; // Visual cue for matched
            GetComponent<Button>().interactable = false;
        }
    }

    public void OnCardClick()
    {
        if (IsFlipped || IsMatched || isAnimating) return;

        onCardClicked?.Invoke(this);
        StartCoroutine(FlipRoutine(true));
    }

    public void FlipBack()
    {
        if (IsMatched) return;
        StartCoroutine(FlipRoutine(false));
    }

    public void MarkAsMatched()
    {
        IsMatched = true;
        GetComponent<CanvasGroup>().alpha = 0.8f;
        GetComponent<Button>().interactable = false;
    }

    private IEnumerator FlipRoutine(bool showFront)
    {
        isAnimating = true;
        IsFlipped = showFront;

        // Flip to 90 degrees
        float duration = 0.15f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Euler(0f, Mathf.Lerp(0f, 90f, elapsed / duration), 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Change sprite
        cardImage.sprite = showFront ? frontSprite : backSprite;

        // Flip to 0 degrees
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Euler(0f, Mathf.Lerp(90f, 0f, elapsed / duration), 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        isAnimating = false;
    }
}
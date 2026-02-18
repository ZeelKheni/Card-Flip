using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite[] availableFrontSprites;

    [Header("Managers")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private MenuManager menuManager;

    private List<Card> allCards = new List<Card>();
    private Queue<Card> flippedCardsQueue = new Queue<Card>(); // Allows continuous selection

    private int rows = 4;
    private int cols = 5;
    private int totalMatches;
    private int currentMatches;

  /*  private void Start()
    {
        if (saveManager.HasSaveData())
            LoadGame();
        else
            StartNewGame(4, 5); 
    }*/

    public void StartNewGame(int r, int c)
    {
        rows = r;
        cols = c;
        totalMatches = (rows * cols) / 2;
        currentMatches = 0;
        scoreManager.ResetScore();
        flippedCardsQueue.Clear();

        GenerateGrid();
        saveManager.SaveGame(allCards, rows, cols, scoreManager.CurrentScore);
    }

    private void GenerateGrid()
    {
        // Clean up existing
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
        allCards.Clear();

        // Calculate dynamic scaling
        float containerWidth = gridContainer.rect.width;
        float containerHeight = gridContainer.rect.height;

        // Add padding/spacing buffer
        float cellWidth = containerWidth / cols - 10f;
        float cellHeight = containerHeight / rows - 10f;
        float finalSize = Mathf.Min(cellWidth, cellHeight); // Maintain square aspect ratio

        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(finalSize, finalSize);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;

        // Generate Pairs
        List<int> cardIds = new List<int>();
        for (int i = 0; i < totalMatches; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        // Shuffle
        for (int i = 0; i < cardIds.Count; i++)
        {
            int temp = cardIds[i];
            int randomIndex = Random.Range(i, cardIds.Count);
            cardIds[i] = cardIds[randomIndex];
            cardIds[randomIndex] = temp;
        }

        // Instantiate
        for (int i = 0; i < cardIds.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            Card card = cardObj.GetComponent<Card>();
            card.Initialize(cardIds[i], availableFrontSprites[cardIds[i] % availableFrontSprites.Length], OnCardClicked);
            allCards.Add(card);
        }
    }

    private void OnCardClicked(Card clickedCard)
    {
        audioManager.PlayFlipSound();
        flippedCardsQueue.Enqueue(clickedCard);

        if (flippedCardsQueue.Count >= 2)
        {
            // Extract the first two cards from the queue to process them
            Card card1 = flippedCardsQueue.Dequeue();
            Card card2 = flippedCardsQueue.Dequeue();

            StartCoroutine(CheckMatchRoutine(card1, card2));
        }
    }

    // Processing matching asynchronously allows the user to click other cards simultaneously
    private IEnumerator CheckMatchRoutine(Card c1, Card c2)
    {
        // Wait a brief moment so the user can see the second card
        yield return new WaitForSeconds(0.6f);

        if (c1.CardID == c2.CardID)
        {
            c1.MarkAsMatched();
            c2.MarkAsMatched();
            audioManager.PlayMatchSound();
            scoreManager.AddScore(true);
            currentMatches++;

            if (currentMatches >= totalMatches)
            {
                audioManager.PlayGameOverSound();
                saveManager.ClearSave(); // Game won, clear save
                menuManager.ShowGameOver(scoreManager.CurrentScore);
                Debug.Log("Game Over! You Win!");
            }
        }
        else
        {
            c1.FlipBack();
            c2.FlipBack();
            audioManager.PlayMismatchSound();
            scoreManager.AddScore(false);
        }

        if (currentMatches < totalMatches)
            saveManager.SaveGame(allCards, rows, cols, scoreManager.CurrentScore);
    }

    internal void LoadGame()
    {
       
        SaveData data = saveManager.LoadGameData();

      
        rows = data.rows;
        cols = data.cols;
        totalMatches = (rows * cols) / 2;
        scoreManager.SetScore(data.score);
        currentMatches = 0;
        flippedCardsQueue.Clear();


        foreach (Transform child in gridContainer) Destroy(child.gameObject);
        allCards.Clear();

      
        float containerWidth = gridContainer.rect.width;
        float containerHeight = gridContainer.rect.height;
        float cellWidth = containerWidth / cols - 10f;
        float cellHeight = containerHeight / rows - 10f;
        float finalSize = Mathf.Min(cellWidth, cellHeight);

        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(finalSize, finalSize);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;

       
        for (int i = 0; i < data.cardIds.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            Card card = cardObj.GetComponent<Card>();

           
            int savedId = data.cardIds[i];
            bool isFlipped = data.cardFlippedStates[i];
            bool isMatched = data.cardMatchedStates[i];

 
            card.Initialize(savedId, availableFrontSprites[savedId % availableFrontSprites.Length], OnCardClicked);

       
            card.SetState(isFlipped, isMatched);

            allCards.Add(card);

          
            if (isMatched)
            {
                currentMatches++;
            }
        }

       
        currentMatches /= 2;

        Debug.Log("Game Loaded Successfully!");
    }
}
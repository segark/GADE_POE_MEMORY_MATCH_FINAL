using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    public Transform cardGrid; // Reference to the Grid Panel

    private List<Card> cards = new List<Card>();
    private Card firstRevealed;
    private Card secondRevealed;
    private int playerScore;
    private int aiScore;
    private bool isPlayerTurn = true;
    public enum DifficultyLevel { Easy, Medium, Hard }
    public DifficultyLevel difficultyLevel;
    public TMP_Dropdown difficultyDropdown;
    public int dropdownFontSize = 50; // Adjust this to set your desired font size
    private int turnCounter = 0;
    private int reshuffleThreshold = 7; // Reshuffle every 3 turns

    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI aiScoreText;
    public TextMeshProUGUI turnIndicatorText;
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;
    public GameObject reshuffleTextPanel; // Panel to show reshuffle text
    public GameObject difficultyPanel; // Panel to choose difficulty
    public Button startGameButton; // Button to start the game
    System.Random random = new System.Random();
    private int totalPairs;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        difficultyDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Easy"),
            new TMP_Dropdown.OptionData("Medium"),
            new TMP_Dropdown.OptionData("Hard")
        };
        difficultyDropdown.AddOptions(options);

        // Set the current difficulty level based on the enum
        difficultyDropdown.value = (int)difficultyLevel;
        SetDropdownFontSize(dropdownFontSize);

        // Add listener for when the dropdown value changes
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        // Show the difficulty selection panel at the beginning
        ShowDifficultyPanel();
        endGamePanel.SetActive(false);
        reshuffleTextPanel.SetActive(false);
    }

    private void SetDropdownFontSize(int fontSize)
    {
        // Adjust the font size for the caption and item texts
        TMP_Text captionText = difficultyDropdown.captionText;
        TMP_Text itemText = difficultyDropdown.itemText;

        if (captionText != null)
        {
            captionText.fontSize = fontSize;
        }

        if (itemText != null)
        {
            itemText.fontSize = fontSize;
        }
    }

    void InitializeCards()
    {
        List<int> cardIds = new List<int>();

        for (int i = 0; i < cardSprites.Length; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        Shuffle(cardIds);

        foreach (int id in cardIds)
        {
            GameObject cardObject = Instantiate(cardPrefab, cardGrid);
            Card card = cardObject.GetComponent<Card>();
            card.id = id;
            card.frontSprite = cardSprites[id];
            Debug.Log($"Card {id} sprite: {cardSprites[id]}");
            card.frontImage = cardObject.transform.Find("FrontImage").GetComponent<Image>();
            card.backImage = cardObject.transform.Find("BackImage").GetComponent<Image>();

            if (card.frontImage == null || card.backImage == null)
            {
                Debug.LogError("Card prefab is missing FrontImage or BackImage child objects.");
            }
            else
            {
                card.frontSprite = cardSprites[id];
                card.frontImage.sprite = cardSprites[id];
                card.HideCard();
            }

            cardObject.transform.SetParent(cardGrid, false);
            cards.Add(card);
        }

        totalPairs = cardSprites.Length;
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void CardRevealed(Card card)
    {
        Debug.Log("CardRevealed called with card " + card.id);
        if (firstRevealed == null)
        {
            firstRevealed = card;
        }
        else
        {
            secondRevealed = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1.0f);

        if (firstRevealed.id == secondRevealed.id)
        {
            Debug.Log("Match Found!");
            if (isPlayerTurn)
            {
                playerScore++;
            }
            else
            {
                aiScore++;
            }
        }
        else
        {
            Debug.Log("No Match!");
            firstRevealed.HideCard();
            secondRevealed.HideCard();
        }

        firstRevealed = null;
        secondRevealed = null;

        // Check if game over
        if (playerScore + aiScore == totalPairs)
        {
            EndGame();
            yield break; // Exiting the iterator
        }

        // Switch turns and reset game state
        isPlayerTurn = !isPlayerTurn;
        Debug.Log(isPlayerTurn ? "Player's Turn" : "AI's Turn");

        turnCounter++;

        if (turnCounter >= reshuffleThreshold && !isPlayerTurn)
        {
            StartCoroutine(ShowReshuffleMessage());
            ReshuffleUnrevealedCards();
            turnCounter = 0;
        }

        UpdateUI();

        if (!isPlayerTurn)
        {
            StartCoroutine(AITurn());
        }
    }

    IEnumerator AITurn()
    {
        Debug.Log("AITurn started");
        yield return new WaitForSeconds(1.5f);

        Debug.Log("AI's Turn");
        

        // AI logic to pick two cards
        var bestMove = FindBestMove();
        if (bestMove.Item1 != null && bestMove.Item2 != null)
        {
            Debug.Log("AI found a pair: " + bestMove.Item1.id + ", " + bestMove.Item2.id);
            bestMove.Item1.RevealCard();
            yield return new WaitForSeconds(1.0f); // Delay between card picks
            bestMove.Item2.RevealCard();
            CardRevealed(bestMove.Item1);
            CardRevealed(bestMove.Item2);
            Debug.Log("AI chose " + bestMove.Item1.id + " and " + bestMove.Item2.id);
        }
        else
        {
            Debug.LogError("AI could not find valid cards to reveal.");
        }
    }

    public (Card, Card) FindBestMove()
    {
        Debug.Log("FindBestMove started");

        List<Card> unrevealedCards = new List<Card>();
        foreach (var card in cards)
        {
            if (!card.isRevealed)
            {
                unrevealedCards.Add(card);
            }
        }

        if (unrevealedCards.Count < 2)
        {
            Debug.LogError("No more unrevealed cards left.");
            return (null, null);
        }

        List<Card> availableCards = new List<Card>();
        foreach (var card in unrevealedCards)
        {
            if (unrevealedCards.Count(c => c.id == card.id) == 2)
            {
                availableCards.Add(card);
            }
        }

        if (availableCards.Count < 2)
        {
            Debug.LogError("No more pairs left.");
            return (null, null);
        }
        int randomValue = random.Next(1, 11);

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                // Easy difficulty: choose two random cards from the list of available cards
                //int index1 = UnityEngine.Random.Range(0, availableCards.Count);
                //Card card1 = availableCards[index1];
                //availableCards.RemoveAt(index1);
                //int index2 = UnityEngine.Random.Range(0, availableCards.Count);
                //Card card2 = availableCards[index2];
                //return (card1, card2);
                if (randomValue <=8)
                {
                    // 80% chance to choose two random cards
                    int index1 = UnityEngine.Random.Range(0, availableCards.Count);
                    Card card1 = availableCards[index1];
                    availableCards.RemoveAt(index1);
                    int index2 = UnityEngine.Random.Range(0, availableCards.Count);
                    Card card2 = availableCards[index2];
                    return (card1, card2);
                }
                else
                {

                    //20% chance to choose the best pair
                    Card mediumCard1 = availableCards[0];
                    availableCards.RemoveAt(0);
                    Card mediumCard2 = availableCards.Find(c => c.id == mediumCard1.id);
                    return (mediumCard1, mediumCard2);
                }
            case DifficultyLevel.Medium:
                // Medium difficulty: choose two random cards from the list of available cards with a 50% chance
                if (randomValue <=5)
                {
                    // 60% chance to choose two random cards
                    int mediumIndex1 = UnityEngine.Random.Range(0, availableCards.Count);
                    Card mediumCard1 = availableCards[mediumIndex1];
                    availableCards.RemoveAt(mediumIndex1);
                    int mediumIndex2 = UnityEngine.Random.Range(0, availableCards.Count);
                    Card mediumCard2 = availableCards[mediumIndex2];
                    return (mediumCard1, mediumCard2);
                }
                else 
                {

                    //40% chance to choose the best pair
                    Card mediumCard1 = availableCards[0];
                    availableCards.RemoveAt(0);
                    Card mediumCard2 = availableCards.Find(c => c.id == mediumCard1.id);
                    return (mediumCard1, mediumCard2);
                }
            case DifficultyLevel.Hard:
                // Hard difficulty: choose the best pair
                //Card hardCard1 = availableCards[0];
                //availableCards.RemoveAt(0);
                //Card hardCard2 = availableCards.Find(c => c.id == hardCard1.id);
                //return (hardCard1, hardCard2);

                if (randomValue <= 8)
                {
                    // 80% chance to choose two random cards

                    //40% chance to choose the best pair
                    Card hardCard1 = availableCards[0];
                    availableCards.RemoveAt(0);
                    Card hardCard2 = availableCards.Find(c => c.id == hardCard1.id);
                    return (hardCard1, hardCard2);

                   
                }
                else
                {
                    int hardIndex1 = UnityEngine.Random.Range(0, availableCards.Count);
                    Card hardCard1 = availableCards[hardIndex1];
                    availableCards.RemoveAt(hardIndex1);
                    int hardIndex2 = UnityEngine.Random.Range(0, availableCards.Count);
                    Card hardCard2 = availableCards[hardIndex2];
                    return (hardCard1, hardCard2);


                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ResetGameState()
    {
        Debug.Log("ResetGameState called");
        foreach (var card in cards)
        {
            card.HideCard();
            card.isRevealed = false;
        }
        UpdateUI();
    }

    void OnDifficultyChanged(int value)
    {
        switch (value)
        {
            case 0:
                difficultyLevel = DifficultyLevel.Easy;
                break;
            case 1:
                difficultyLevel = DifficultyLevel.Medium;
                break;
            case 2:
                difficultyLevel = DifficultyLevel.Hard;
                break;
        }

        Debug.Log("Difficulty level changed to " + difficultyLevel);
    }

    private void ReshuffleUnrevealedCards()
    {
        List<Card> unrevealedCards = new List<Card>();
        foreach (var card in cards)
        {
            if (!card.isRevealed)
            {
                unrevealedCards.Add(card);
            }
        }

        if (unrevealedCards.Count > 1)
        {
            List<int> shuffledIds = unrevealedCards.Select(c => c.id).ToList();
            Shuffle(shuffledIds);

            for (int i = 0; i < unrevealedCards.Count; i++)
            {
                int newId = shuffledIds[i];
                unrevealedCards[i].id = newId;
                unrevealedCards[i].frontSprite = cardSprites[newId];
                unrevealedCards[i].frontImage.sprite = cardSprites[newId];
            }
        }
    }

    private void EndGame()
    {
        endGamePanel.SetActive(true);
        if (playerScore > aiScore)
        {
            endGameText.text = "Player Wins!";
        }
        else if (aiScore > playerScore)
        {
            endGameText.text = "AI Wins!";
        }
        else
        {
            endGameText.text = "It's a Draw!";
        }
    }

    private IEnumerator ShowReshuffleMessage()
    {
        reshuffleTextPanel.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        reshuffleTextPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        playerScoreText.text = "Player: " + playerScore;
        aiScoreText.text = "AI: " + aiScore;
        turnIndicatorText.text = isPlayerTurn ? "Player's Turn" : "AI's Turn";
    }

    private void ShowDifficultyPanel()
    {
        difficultyPanel.SetActive(true);
    }

    private void HideDifficultyPanel()
    {
        difficultyPanel.SetActive(false);
    }

    private void OnStartGameClicked()
    {
        HideDifficultyPanel();
        InitializeCards();
        ResetGameState();
    }
}

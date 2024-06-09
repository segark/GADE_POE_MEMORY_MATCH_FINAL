using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class HumanGameManager : MonoBehaviour
{
    public static HumanGameManager Instance;
    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    public Transform cardGrid; // Reference to the Grid Panel

    private List<Card> cards = new List<Card>();
    private Card firstRevealed;
    private Card secondRevealed;
    private int player1Score;
    private int player2Score;
    private bool isPlayer1Turn = true;

    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI turnIndicatorText;
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;
    public GameObject reshuffleTextPanel; // Panel to show reshuffle text

    private int turnCounter = 0;
    private int reshuffleThreshold = 5; // Reshuffle every 5 turns
    private int totalPairs;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeCards();
        ResetGameState();
        UpdateUI();
        endGamePanel.SetActive(false);
        reshuffleTextPanel.SetActive(false);
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
            if (isPlayer1Turn)
            {
                player1Score++;
            }
            else
            {
                player2Score++;
            }
        }
        else
        {
            firstRevealed.HideCard();
            secondRevealed.HideCard();
        }

        firstRevealed = null;
        secondRevealed = null;
        turnCounter++;

        if (turnCounter >= reshuffleThreshold && !isPlayer1Turn)
        {
            StartCoroutine(ShowReshuffleMessage());
            ReshuffleUnrevealedCards();
            turnCounter = 0;
        }

        if (player1Score + player2Score == totalPairs)
        {
            EndGame();
        }
        else
        {
            isPlayer1Turn = !isPlayer1Turn;
            UpdateUI();
        }
    }

    void ResetGameState()
    {
        foreach (var card in cards)
        {
            card.HideCard();
            card.isRevealed = false;
        }

        player1Score = 0;
        player2Score = 0;
        isPlayer1Turn = true;
        UpdateUI();
    }

    void UpdateUI()
    {
        player1ScoreText.text = "Player 1 Score: " + player1Score;
        player2ScoreText.text = "Player 2 Score: " + player2Score;
        turnIndicatorText.text = isPlayer1Turn ? "Player 1's Turn" : "Player 2's Turn";
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

        if (player1Score > player2Score)
        {
            endGameText.text = "Player 1 Wins!";
        }
        else if (player2Score > player1Score)
        {
            endGameText.text = "Player 2 Wins!";
        }
        else
        {
            endGameText.text = "It's a Tie!";
        }
    }

    IEnumerator ShowReshuffleMessage()
    {
        reshuffleTextPanel.SetActive(true);
        yield return new WaitForSeconds(2.0f); // Show message for 2 seconds
        reshuffleTextPanel.SetActive(false);
    }
}

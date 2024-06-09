//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using System;
//using System.Linq;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;
//    public GameObject cardPrefab;
//    public Sprite[] cardSprites;
//    public Transform cardGrid; // Reference to the Grid Panel

//    private List<Card> cards = new List<Card>();
//    private Card firstRevealed;
//    private Card secondRevealed;
//    private int playerScore;
//    private int aiScore;
//    private bool isPlayerTurn = true;
//    public enum DifficultyLevel { Easy, Medium, Hard }
//    public DifficultyLevel difficultyLevel;
//    public TMP_Dropdown difficultyDropdown;
//    public int dropdownFontSize = 50; // Adjust this to set your desired font size
//    private int turnCounter = 0;
//    private int reshuffleThreshold = 7; // Reshuffle every 7 turns

//    public TextMeshProUGUI playerScoreText;
//    public TextMeshProUGUI aiScoreText;
//    public TextMeshProUGUI turnIndicatorText;
//    public GameObject endGamePanel;
//    public TextMeshProUGUI endGameText;
//    public GameObject reshuffleTextPanel; // Panel to show reshuffle text
//    public GameObject difficultyPanel; // Panel to choose difficulty
//    public Button startGameButton; // Button to start the game
//    System.Random random = new System.Random();
//    private int totalPairs;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        difficultyDropdown.ClearOptions();
//        var options = new List<TMP_Dropdown.OptionData>
//        {
//            new TMP_Dropdown.OptionData("Easy"),
//            new TMP_Dropdown.OptionData("Medium"),
//            new TMP_Dropdown.OptionData("Hard")
//        };
//        difficultyDropdown.AddOptions(options);

//        // Set the current difficulty level based on the enum
//        difficultyDropdown.value = (int)difficultyLevel;
//        SetDropdownFontSize(dropdownFontSize);

//        // Add listener for when the dropdown value changes
//        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
//        startGameButton.onClick.AddListener(OnStartGameClicked);

//        // Show the difficulty selection panel at the beginning
//        ShowDifficultyPanel();
//        endGamePanel.SetActive(false);
//        reshuffleTextPanel.SetActive(false);
      
//    }

//    private void SetDropdownFontSize(int fontSize)
//    {
//        // Adjust the font size for the caption and item texts
//        TMP_Text captionText = difficultyDropdown.captionText;
//        TMP_Text itemText = difficultyDropdown.itemText;

//        if (captionText != null)
//        {
//            captionText.fontSize = fontSize;
//        }

//        if (itemText != null)
//        {
//            itemText.fontSize = fontSize;
//        }
//    }

//    void InitializeCards()
//    {
//        List<int> cardIds = new List<int>();

//        for (int i = 0; i < cardSprites.Length; i++)
//        {
//            cardIds.Add(i);
//            cardIds.Add(i);
//        }

//        Shuffle(cardIds);

//        foreach (int id in cardIds)
//        {
//            GameObject cardObject = Instantiate(cardPrefab, cardGrid);
//            Card card = cardObject.GetComponent<Card>();
//            card.id = id;
//            card.frontSprite = cardSprites[id];
//            Debug.Log($"Card {id} sprite: {cardSprites[id]}");
//            card.frontImage = cardObject.transform.Find("FrontImage").GetComponent<Image>();
//            card.backImage = cardObject.transform.Find("BackImage").GetComponent<Image>();

//            if (card.frontImage == null || card.backImage == null)
//            {
//                Debug.LogError("Card prefab is missing FrontImage or BackImage child objects.");
//            }
//            else
//            {
//                card.frontSprite = cardSprites[id];
//                card.frontImage.sprite = cardSprites[id];
//                card.HideCard();
//            }

//            cardObject.transform.SetParent(cardGrid, false);
//            cards.Add(card);
//        }

//        totalPairs = cardSprites.Length;
//    }

//    void Shuffle(List<int> list)
//    {
//        for (int i = 0; i < list.Count; i++)
//        {
//            int temp = list[i];
//            int randomIndex = UnityEngine.Random.Range(i, list.Count);
//            list[i] = list[randomIndex];
//            list[randomIndex] = temp;
//        }
//    }

//    public void CardRevealed(Card card)
//    {
//        Debug.Log("CardRevealed called with card " + card.id);
//        if (firstRevealed == null)
//        {
//            firstRevealed = card;
//        }
//        else
//        {
//            secondRevealed = card;
//            StartCoroutine(CheckMatch());
//        }
//    }

//    IEnumerator CheckMatch()
//    {
//        yield return new WaitForSeconds(1.0f);

//        if (firstRevealed.id == secondRevealed.id)
//        {
//            Debug.Log("Match Found!");
//            if (isPlayerTurn)
//            {
//                playerScore++;
//            }
//            else
//            {
//                aiScore++;
//            }
//        }
//        else
//        {
//            Debug.Log("No Match!");
//            firstRevealed.HideCard();
//            secondRevealed.HideCard();
//        }

//        firstRevealed = null;
//        secondRevealed = null;

//        // Check if game over
//        if (playerScore + aiScore == totalPairs)
//        {
//            EndGame();
//            yield break; // Exiting the iterator
//        }

//        // Switch turns and reset game state
//        isPlayerTurn = !isPlayerTurn;
//        Debug.Log(isPlayerTurn ? "Player's Turn" : "AI's Turn");

//        turnCounter++;

//        if (turnCounter >= reshuffleThreshold && !isPlayerTurn)
//        {
//            StartCoroutine(ShowReshuffleMessage());
//            ReshuffleUnrevealedCards();
//            turnCounter = 0;
//        }

//        UpdateUI();

//        if (!isPlayerTurn)
//        {
//            StartCoroutine(AITurn());
//        }
//    }

//    IEnumerator AITurn()
//    {
//        Debug.Log("AITurn started");
//        yield return new WaitForSeconds(1.5f);

//        Debug.Log("AI's Turn");

//        int iterations = GetIterationsBasedOnDifficulty();
//        // AI logic to pick two cards
//        var bestMove = FindBestMove(iterations);
//        if (bestMove.Item1 != null && bestMove.Item2 != null)
//        {
//            Debug.Log("AI found a pair: " + bestMove.Item1.id + ", " + bestMove.Item2.id);
//            bestMove.Item1.RevealCard();
//            yield return new WaitForSeconds(1.0f); // Delay between card picks
//            bestMove.Item2.RevealCard();
//            CardRevealed(bestMove.Item1);
//            CardRevealed(bestMove.Item2);
//            Debug.Log("AI chose " + bestMove.Item1.id + " and " + bestMove.Item2.id);
//        }
//        else
//        {
//            Debug.LogError("AI could not find valid cards to reveal.");
//        }
//    }

//    private int GetIterationsBasedOnDifficulty()
//    {
//        switch (difficultyLevel)
//        {
//            case DifficultyLevel.Easy:
//                return 200;
//            case DifficultyLevel.Medium:
//                return 500;
//            case DifficultyLevel.Hard:
//                return 1000;
//            default:
//                return 1000;
//        }
//    }

 
//    private int GetDepthLimitBasedOnDifficulty()
//    {
//        switch (difficultyLevel)
//        {
//            case DifficultyLevel.Easy:
//                return 2; // shallow search
//            case DifficultyLevel.Medium:
//                return 4; // medium search
//            case DifficultyLevel.Hard:
//                return 9; // deep search
//            default:
//                return 3; // default depth limit
//        }
//    }
//    class TreeNode
//    {
//        public List<Card> cards;
//        public List<TreeNode> children;
//        public int wins;
//        public int simulations;
//        public int depth;
//        public int heuristic; // Add this property

//        public TreeNode(List<Card> cards, int depth = 0, int heuristic = 0) // Add a heuristic parameter to the constructor
//        {
//            this.cards = new List<Card>(cards); // Create a copy of the cards list
//            children = new List<TreeNode>();
//            this.depth = depth; // Initialize the depth
//            this.heuristic = heuristic; // Initialize the heuristic
//        }

//        public void simulateGame()
//        {
//            if (cards.Count > 0)
//            {
//                Card cardToReveal = cards[UnityEngine.Random.Range(0, cards.Count)];
//                List<Card> newCards = new List<Card>(cards);
//                newCards.Remove(cardToReveal);

//                TreeNode newNode = new TreeNode(newCards, depth + 1, CalculateHeuristic(newCards)); // Pass the incremented depth and the heuristic to the new node
//                children.Add(newNode);

//                newNode.simulateGame();

//                wins += newNode.wins;
//                simulations += newNode.simulations;
//            }
//        }

        
//    }
//    public static int CalculateHeuristic(List<Card> cards)
//    {
//        int remainingPairs = CalculateRemainingPairs(cards);
//        int riskOfMismatch = CalculateRiskOfMismatch(cards);

//        // Heuristic formula: a linear combination of remaining pairs and risk of mismatch
//        // Adjust the weights to prioritize one factor over the other as needed
//        int heuristicValue = (int)remainingPairs - 1 * riskOfMismatch;
//        return heuristicValue;
//    }

//    private static int CalculateRemainingPairs(List<Card> cards)
//    {
//        // Count the number of unique pairs that can still be formed with the unrevealed cards
//        HashSet<int> uniqueIds = new HashSet<int>();
//        foreach (var card in cards)
//        {
//            if (!card.isRevealed && !uniqueIds.Contains(card.id))
//            {
//                uniqueIds.Add(card.id);
//            }
//        }
//        return uniqueIds.Count;
//    }

//    private static int CalculateRiskOfMismatch(List<Card> cards)
//    {
//        // Calculate the risk of revealing a mismatch by counting the number of cards with no matching pairs
//        int unmatchedCards = 0;
//        foreach (var card in cards)
//        {
//            if (!card.isRevealed && cards.Count(c => c.id == card.id && !c.isRevealed) == 1)
//            {
//                unmatchedCards++;
//            }
//        }
//        return unmatchedCards;
//    }

//    public (Card, Card) FindBestMove(int iterations)
//    {
//        List<Card> unrevealedCards = cards.Where(card => !card.isRevealed).ToList();

//        if (unrevealedCards.Count < 2)
//        {
//            Debug.LogError("No more unrevealed cards left.");
//            return (null, null);
//        }

//        int depthLimit = GetDepthLimitBasedOnDifficulty();
//        Dictionary<(Card, Card), float> cardPairs = new Dictionary<(Card, Card), float>();

//        // Generate all possible pairs of cards
//        for (int i = 0; i < unrevealedCards.Count; i++)
//        {
//            for (int j = i + 1; j < unrevealedCards.Count; j++)
//            {
//                Card card1 = unrevealedCards[i];
//                Card card2 = unrevealedCards[j];

//                // Calculate the heuristic value for each pair of cards
//                float heuristicValue = CalculateHeuristic(new List<Card> { card1, card2 });

//                // Store the pair and its heuristic value
//                cardPairs.Add((card1, card2), heuristicValue);
//            }
//        }

//        // Shuffle the dictionary to introduce randomness
//        cardPairs = cardPairs.OrderBy(x => UnityEngine.Random.value).ToDictionary(item => item.Key, item => item.Value);

//        // Choose the best pair of cards based on the heuristic values
//        var bestPair = cardPairs.OrderByDescending(x => x.Value).First();
//        int randomNumber = UnityEngine.Random.Range(1, 11); // Generates a number between 1 and 10
//        switch (difficultyLevel)
//        {
//            case DifficultyLevel.Easy:
//                // For Easy difficulty, choose a random pair with a certain probability
//                if (randomNumber <=2)
//                {
                   
//                    return bestPair.Key;
                    
//                }
//                else
//                {
                    
//                    // Choose a random pair randomly
//                    var randomPair = cardPairs.Keys.ElementAt(UnityEngine.Random.Range(0, cardPairs.Keys.Count));
//                    return randomPair;
//                }
//            case DifficultyLevel.Medium:
//                // For Medium difficulty, choose the best pair with higher probability
//                if (randomNumber<=6)
//                {
                   
//                    return bestPair.Key;
//                }
//                else
//                {
                   
//                    // Choose a random pair randomly
//                    var randomPair = cardPairs.Keys.ElementAt(UnityEngine.Random.Range(0, cardPairs.Keys.Count));
//                    return randomPair;
//                }
//            case DifficultyLevel.Hard:
//                if (randomNumber <= 8)
//                {
                    
//                    return bestPair.Key;
//                }
//                else
//                {
                   
//                    // Choose a random pair randomly
//                    var randomPair = cardPairs.Keys.ElementAt(UnityEngine.Random.Range(0, cardPairs.Keys.Count));
//                    return randomPair;
//                }

              
//            default:
//                // Default behavior (should not occur)
//                return bestPair.Key;
//        }

//        //return bestPair.Key;
//    }

//    void ResetGameState()
//    {
//        Debug.Log("ResetGameState called");
//        foreach (var card in cards)
//        {
//            card.HideCard();
//            card.isRevealed = false;
//        }
//        UpdateUI();
//    }

//    void OnDifficultyChanged(int value)
//    {
//        switch (value)
//        {
//            case 0:
//                difficultyLevel = DifficultyLevel.Easy;
//                break;
//            case 1:
//                difficultyLevel = DifficultyLevel.Medium;
//                break;
//            case 2:
//                difficultyLevel = DifficultyLevel.Hard;
//                break;
//        }

//        Debug.Log("Difficulty level changed to " + difficultyLevel);
//    }

//    private void ReshuffleUnrevealedCards()
//    {
//        List<Card> unrevealedCards = new List<Card>();
//        foreach (var card in cards)
//        {
//            if (!card.isRevealed)
//            {
//                unrevealedCards.Add(card);
//            }
//        }

//        if (unrevealedCards.Count > 1)
//        {
//            List<int> shuffledIds = unrevealedCards.Select(c => c.id).ToList();
//            Shuffle(shuffledIds);

//            for (int i = 0; i < unrevealedCards.Count; i++)
//            {
//                int newId = shuffledIds[i];
//                unrevealedCards[i].id = newId;
//                unrevealedCards[i].frontSprite = cardSprites[newId];
//                unrevealedCards[i].frontImage.sprite = cardSprites[newId];
//            }
//        }
//    }

//    private void EndGame()
//    {
//        endGamePanel.SetActive(true);
//        if (playerScore > aiScore)
//        {
//            endGameText.text = "Player Wins!";
//        }
//        else if (aiScore > playerScore)
//        {
//            endGameText.text = "AI Wins!";
//        }
//        else
//        {
//            endGameText.text = "It's a Draw!";
//        }
//    }

//    private IEnumerator ShowReshuffleMessage()
//    {
//        reshuffleTextPanel.SetActive(true);
//        yield return new WaitForSeconds(1.0f);
//        reshuffleTextPanel.SetActive(false);
//    }

//    private void UpdateUI()
//    {
//        playerScoreText.text = "Player: " + playerScore;
//        aiScoreText.text = "AI: " + aiScore;
//        turnIndicatorText.text = isPlayerTurn ? "Player's Turn" : "AI's Turn";
//    }

//    private void ShowDifficultyPanel()
//    {
//        difficultyPanel.SetActive(true);
//    }

//    private void HideDifficultyPanel()
//    {
//        difficultyPanel.SetActive(false);
//    }

//    private void OnStartGameClicked()
//    {
//        HideDifficultyPanel();
//        InitializeCards();
//        ResetGameState();
//    }
//}

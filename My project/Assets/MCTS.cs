using System;
using System.Collections.Generic;
using System.Linq;

public class MCTS
{
    private AIGameManager gameManager;
    private List<Card> cards;
    private int difficultyLevel;

    public MCTS(AIGameManager gameManager, List<Card> cards, int difficultyLevel)
    {
        this.gameManager = gameManager;
        this.cards = cards;
        this.difficultyLevel = difficultyLevel;
    }

    public Card GetBestMove(Card firstRevealed = null)
    {
        List<Card> possibleMoves = cards.Where(c => !c.isRevealed).ToList();

        if (possibleMoves.Count == 0) return null;

        if (difficultyLevel == 0)
        {
            // Easy difficulty: random move
            return possibleMoves[UnityEngine.Random.Range(0, possibleMoves.Count)];
        }
        else if (difficultyLevel == 1)
        {
            // Medium difficulty: adjust the number of iterations
            int iterations = 500; // Adjust this value to control difficulty
            return RunMCTS(possibleMoves,  firstRevealed, iterations); // Pass firstRevealed as the third argument
        }
        else
        {
            // Hard difficulty: adjust the number of iterations
            int iterations = 1000; // Adjust this value to control difficulty
            return RunMCTS(possibleMoves, firstRevealed, iterations); // Pass firstRevealed as the third argument
        }
    }


    private Card RunMCTS(List<Card> possibleMoves, Card firstRevealed, int iterations)
    {
        Dictionary<Card, int> visitCounts = new Dictionary<Card, int>();
        Dictionary<Card, float> winCounts = new Dictionary<Card, float>();

        foreach (var move in possibleMoves)
        {
            visitCounts[move] = 0;
            winCounts[move] = 0.0f;
        }

        for (int i = 0; i < iterations; i++)
        {
            foreach (var move in possibleMoves)
            {
                float result = SimulateMove(move, firstRevealed);
                visitCounts[move]++;
                winCounts[move] += result;
            }
        }

        Card bestMove = possibleMoves.OrderByDescending(move => winCounts[move] / visitCounts[move]).First();
        return bestMove;
    }

    private float SimulateMove(Card move, Card firstRevealed)
    {
        if (firstRevealed != null && move.id == firstRevealed.id)
        {
            return 1.0f;
        }

        // Simulate the rest of the game randomly
        List<Card> simulationCards = cards.Where(c => !c.isRevealed && c != move).ToList();
        Shuffle(simulationCards);

        int player1Score = gameManager.player1Score;
        int player2Score = gameManager.player2Score;

        bool isPlayer1Turn = false;
        for (int i = 0; i < simulationCards.Count; i += 2)
        {
            if (i + 1 >= simulationCards.Count) break;
            if (simulationCards[i].id == simulationCards[i + 1].id)
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
            isPlayer1Turn = !isPlayer1Turn;
        }

        return player2Score > player1Score ? 1.0f : 0.0f;
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

//using System.Collections.Generic;
//using Unity.Collections.LowLevel.Unsafe;
//using UnityEngine;

//public class AI : MonoBehaviour
//{
//    int aiScore;
//    int playerScore;
//    public void MakeMove(List<Card> cards)
//    {
//        // Simple random move for now
//        List<Card> availableCards = new List<Card>();
//        foreach (var card in cards)
//        {
//            if (!card.isRevealed)
//            {
//                availableCards.Add(card);
//            }
//        }

//        if (availableCards.Count > 0)
//        {
//            Card randomCard = availableCards[Random.Range(0, availableCards.Count)];
//            randomCard.RevealCard();
//            GameManager.Instance.CardRevealed(randomCard);
//        }
//    }

//    public int Minimax(List<Card> state, int depth, bool isMaximizing)
//    {
       
//        if (IsGameOver(state) || depth == 0)
//            return EvaluateState(state);

//        if (isMaximizing)
//        {
//            bool isPlayer = GameManager.Instance.isAITurn;
//            int maxEval = int.MinValue;
//            foreach (var move in GetAvailableMoves(state))
//            {
//                int eval = Minimax(ApplyMove(state, move), depth - 1, false);
//                maxEval = Mathf.Max(maxEval, eval);
//            }
//            return maxEval;
//        }
//        else
//        {
//            bool isPlayer = GameManager.Instance.isAITurn;
//            int minEval = int.MaxValue;
//            foreach (var move in GetAvailableMoves(state))
//            {
//                int eval = Minimax(ApplyMove(state, move), depth - 1, true);
//                minEval = Mathf.Min(minEval, eval);
//            }
//            return minEval;
//        }
//    }
//    private bool IsGameOver(List<Card> state)
//    {
//        foreach (var card in state)
//        {
//            if (!card.isRevealed)
//                return false;
//        }
//        return true;
//    }
//    private void ApplyMove(List<Card> state, Card card, bool isAI)
//    {
//        card.RevealCard();
//        if (isAI)
//        {
//            aiScore++;
//        }
//        else
//        {
//            playerScore++;
//        }
//    }
//    private int EvaluateState(List<Card> state)
//    {
//        return aiScore - playerScore;
//    }

//    private List<Card> GetAvailableMoves(List<Card> state)
//    {
//        List<Card> availableMoves = new List<Card>();
//        foreach (var card in state)
//        {
//            if (!card.isRevealed)
//            {
//                availableMoves.Add(card);
//            }
//        }
//        return availableMoves;
//    }


//    private void AITurn()
//    {
//        List<Card> AIcardList = new List<Card>();
//        foreach (Card gameCard in GameManager.Instance.cards)
//        {
//            AIcardList = GameManager.Instance.cards;
//        }
       
//        // Use Minimax to find the best move
//        Card bestMove = FindBestMove(AIcardList);
//        bestMove.RevealCard();
//       GameManager.Instance.CardRevealed(bestMove);
//    }

//    private Card FindBestMove(List<Card> cards)
//    {
//        int bestValue = int.MinValue;
//        Card bestMove = null;

//        foreach (var card in cards)
//        {
//            if (!card.isRevealed)
//            {
//                card.RevealCard();
//                int moveValue = Minimax(cards, depth, false);
//                card.HideCard();

//                if (moveValue > bestValue)
//                {
//                    bestMove = card;
//                    bestValue = moveValue;
//                }
//            }
//        }

//        return bestMove;
//    }

//}

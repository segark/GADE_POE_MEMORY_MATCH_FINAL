using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicTacToeController : MonoBehaviour
{
    public Button[] buttons;
    private int[] board;
    private const int HUMAN = -1;
    private const int AI = 1;

    void Start()
    {
        board = new int[9];
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnHumanMove(index));
        }
        ResetBoard();
    }

    void OnHumanMove(int index)
    {
        if (board[index] == 0 && !IsGameOver())
        {
            board[index] = HUMAN;
            buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = "X";
            if (!IsGameOver())
            {
                MakeAIMove();
            }
        }
    }

    void MakeAIMove()
    {
        int bestMove = -1;
        int bestScore = int.MinValue;

        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == 0)
            {
                board[i] = AI;
                int score = Minimax(board, 0, false);
                board[i] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }

        if (bestMove != -1)
        {
            board[bestMove] = AI;
            buttons[bestMove].GetComponentInChildren<TextMeshProUGUI>().text = "O";
        }
    }

    int Minimax(int[] newBoard, int depth, bool isMaximizing)
    {
        int score = Evaluate(newBoard);
        if (score == 10 || score == -10)
            return score - depth; // Subtract depth to prefer winning earlier and losing later.
        if (IsBoardFull(newBoard))
            return 0;

        if (isMaximizing)
        {
            int best = int.MinValue;
            for (int i = 0; i < newBoard.Length; i++)
            {
                if (newBoard[i] == 0)
                {
                    newBoard[i] = AI;
                    best = Mathf.Max(best, Minimax(newBoard, depth + 1, !isMaximizing));
                    newBoard[i] = 0;
                }
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            for (int i = 0; i < newBoard.Length; i++)
            {
                if (newBoard[i] == 0)
                {
                    newBoard[i] = HUMAN;
                    best = Mathf.Min(best, Minimax(newBoard, depth + 1, !isMaximizing));
                    newBoard[i] = 0;
                }
            }
            return best;
        }
    }

    int Evaluate(int[] newBoard)
    {
        int[,] winPositions = {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Rows
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Columns
            {0, 4, 8}, {2, 4, 6}  // Diagonals
        };

        for (int i = 0; i < 8; i++)
        {
            int a = newBoard[winPositions[i, 0]];
            int b = newBoard[winPositions[i, 1]];
            int c = newBoard[winPositions[i, 2]];

            if (a == b && b == c)
            {
                if (a == AI)
                    return 10;
                else if (a == HUMAN)
                    return -10;
            }
        }

        return 0;
    }

    bool IsBoardFull(int[] newBoard)
    {
        for (int i = 0; i < newBoard.Length; i++)
        {
            if (newBoard[i] == 0)
                return false;
        }
        return true;
    }

    bool IsGameOver()
    {
        return Evaluate(board) != 0 || IsBoardFull(board);
    }

    public void ResetBoard()
    {
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = 0;
           // buttons[i].GetComponentInChildren<Text>().text = "";
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }
}

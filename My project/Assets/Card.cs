using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int id;
    public Sprite frontSprite;
    public Sprite backSprite;
    public bool isRevealed = false;

    public Image frontImage;
    public Image backImage;

    void Start()
    {
        if (frontImage == null || backImage == null)
        {
            Debug.LogError("FrontImage or BackImage is not assigned!");
            return;
        }

        frontImage.sprite = frontSprite;
        backImage.sprite = backSprite;
        HideCard();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRevealed)
        {
            RevealCard();

            // Check the active scene and call the appropriate GameManager
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Human") // Replace with the actual scene name
            {
                HumanGameManager.Instance.CardRevealed(this);
            }
            else
            {
                GameManager.Instance.CardRevealed(this);
            }
        }
    }

    public void RevealCard()
    {
        isRevealed = true;
        if (frontImage != null) frontImage.enabled = true;
        if (backImage != null) backImage.enabled = false;
    }

    public void HideCard()
    {
        isRevealed = false;
        if (frontImage != null) frontImage.enabled = false;
        if (backImage != null) backImage.enabled = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public GameObject cardFace; // Previously 'figure'
    private GameObject gameController; // Previously 'gameManager' and 'gm'
    private int cardIdentifier; // Previously 'cardId' or 'id'
    public Material cardMaterial; // Previously 'cardImage' or 'image'
    public static bool isGameActive; // Previously 'isGameStarted' or 'startVar'

    // Start is called before the first frame update
    void Start()
    {
        isGameActive = false;
        gameController = GameObject.FindGameObjectWithTag("GameController"); // Find the GameManager
        Renderer renderer = cardFace.GetComponent<Renderer>();
        renderer.material = cardMaterial;
    }

    // Listener for when you click on the card
    void OnMouseDown()
    {
        // Check if the game has started
        if (!isGameActive) return;

        var gameManager = gameController.GetComponent<GameManager>();

        if (gameManager.CanSelectCard())
        {
            gameManager.SetCardClickAllowed(true);
            AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("CardFaceDown"))
            {
                if (gameManager.IsSelectionSlotAvailable())
                {
                    Animator animator = GetComponent<Animator>();
                    animator.SetTrigger("RevealCardTrigger");
                    gameManager.OnCardSelected(gameObject);
                }
            }
        }
    }

    // Hide the card with animation
    public void HideCardFace()
    {
        AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("CardFaceUp"))
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("HideCardTrigger");
        }
    }

    // Show the card with animation
    public void RevealCardFace()
    {
        AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("CardFaceDown"))
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("RevealCardTrigger");
        }
    }

    /* Setters and Getters */
    public Renderer GetCardFaceRenderer()
    {
        return cardFace.GetComponent<Renderer>();
    }

    public void SetGameActive(bool state)
    {
        isGameActive = state;
    }

    public void SetCardIdentifier(double idValue)
    {
        cardIdentifier = (int)idValue;
    }

    public int GetCardIdentifier()
    {
        return cardIdentifier;
    }
}

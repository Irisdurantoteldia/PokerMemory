using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public GameObject cardFace; 
    private GameObject gameController; 
    private int cardIdentifier; 
    public Material cardMaterial; 
    public static bool isGameActive; 

    // Inicialització abans del primer frame
    void Start()
    {
        isGameActive = false;
        gameController = GameObject.FindGameObjectWithTag("GameController"); // Find the GameManager
        Renderer renderer = cardFace.GetComponent<Renderer>();
        renderer.material = cardMaterial;
    }

    // Listener quan es fa clic sobre la carta
    void OnMouseDown()
    {
        // Si el joc no ha començat, no fa res
        if (!isGameActive) return;

        var gameManager = gameController.GetComponent<GameManager>();

        // Si es pot seleccionar una carta
        if (gameManager.CanSelectCard())
        {
            gameManager.SetCardClickAllowed(true);
            AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);

            // Si la carta està tapada, la revela
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

    // Amaga la cara de la carta amb animació
    public void HideCardFace()
    {
        AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("CardFaceUp"))
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("HideCardTrigger");
        }
    }

    // Mostra la cara de la carta amb animació
    public void RevealCardFace()
    {
        AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("CardFaceDown"))
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("RevealCardTrigger");
        }
    }

    // Obté el Renderer de la cara de la carta (per poder canviar el material)
    public Renderer GetCardFaceRenderer()
    {
        return cardFace.GetComponent<Renderer>();
    }

    // Activa o desactiva el joc (per començar o aturar el joc)
    public void SetGameActive(bool state)
    {
        isGameActive = state;
    }
    
    // Assigna un identificador a la carta
    public void SetCardIdentifier(double idValue)
    {
        cardIdentifier = (int)idValue;
    }

    // Obté l'identificador de la carta
    public int GetCardIdentifier()
    {
        return cardIdentifier;
    }
}
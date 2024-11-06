using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] cards; 
    public GameObject[] cardChildren; 
    private double cardIdentifierValue = 0; 
    private GameObject[] selectedCards = new GameObject[2]; 
    private int matchedCardsCount;
    private bool clickAllowed = false; 
    private float clickCooldown = 0; 
    private bool isGameStarted = false; 
    public Button startButton;
    public Button resetButton;
    public TextMeshProUGUI timeText;
    private double totalTime; 
    private int attemptsCount;
    public TextMeshProUGUI bestTimeText; 
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI attemptsText;
    public GameObject mainPanel;
    public GameObject endGamePanel;

    public AudioClip successClip;
    public AudioClip failureClip;
    public AudioClip backgroundMusic;  
    public AudioClip endGameClip;
    public AudioSource audioSource;

    void Start()
    {
        // Set up listeners for buttons
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(FinishScene);  
        }

        // Initialize UI and variables
        titleText.text = "MEM♦RY G♠ME";
        attemptsCount = 0;
        attemptsText.text = "Attempts: " + attemptsCount;

        // Load the best time from PlayerPrefs
        int bestTime = PlayerPrefs.GetInt("BestScore", int.MaxValue);
        if (bestTime == int.MaxValue)
        {
            bestTimeText.text = "Best Time: --"; 
        }
        else
        {
            bestTimeText.text = "Best Time: " + bestTime; 
        }

        timeText.text = "Time: " + 0;
        selectedCards[0] = null;
        selectedCards[1] = null;
        matchedCardsCount = 0;
        cards = GameObject.FindGameObjectsWithTag("CardTag");
        cardChildren = GameObject.FindGameObjectsWithTag("CardTagChild");

        audioSource.loop = true; 
    }

    void Update()
    {
        // Timer for the game
        if (isGameStarted)
        {
            totalTime += Time.deltaTime;
            timeText.text = "Time: " + (int)totalTime;
        }

        // Cooldown for clicking
        if (clickAllowed)
        {
            clickCooldown += Time.deltaTime;
            if (clickCooldown >= 1)
            {
                clickCooldown = 0;
                clickAllowed = false;
            }
        }

        // Check if there are two selected cards
        if (selectedCards[0] != null && selectedCards[1] != null)
        {
            AnimatorStateInfo stateInfo0 = selectedCards[0].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo stateInfo1 = selectedCards[1].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if (stateInfo0.IsName("CardFaceUp") && stateInfo1.IsName("CardFaceUp"))
            {
                CheckCardIdentifiers();
            }
        }

        // If 8 pairs have been matched, the game ends
        if (matchedCardsCount == 8)
        {
            // Check if the current time is a new best time
            int bestTime = PlayerPrefs.GetInt("BestScore", int.MaxValue);
            if (totalTime < bestTime)
            {
                titleText.text = "¡¡New best score!!";
                PlayerPrefs.SetInt("BestScore", (int)totalTime);
                bestTimeText.text = "Best Time: " + (int)totalTime; 
                titleText.color = Color.yellow;
            }

            isGameStarted = false;
            matchedCardsCount += 1;
            Invoke("ResetGame", 5);
        }
    }

    public void OnCardSelected(GameObject selectedCard)
    {
        if (selectedCards[0] == null && selectedCards[1] == null)
        {
            selectedCards[0] = selectedCard;
        }
        else
        {
            selectedCards[1] = selectedCard;
        }   
    }

    public void CheckCardIdentifiers()
    {
        if (selectedCards[0] != null && selectedCards[1] != null)
        {
            if (selectedCards[0].GetComponent<CardScript>().GetCardIdentifier() != selectedCards[1].GetComponent<CardScript>().GetCardIdentifier())
            {
                selectedCards[0].GetComponent<CardScript>().HideCardFace();
                selectedCards[1].GetComponent<CardScript>().HideCardFace();
                attemptsCount += 1;
                attemptsText.text = "Attempts: " + attemptsCount;
                audioSource.PlayOneShot(failureClip);
                ResetSelectedCards(12);
            }
            else
            {
                matchedCardsCount++;
                audioSource.PlayOneShot(successClip);
                ResetSelectedCards(12);
            }
        }
    }

    public bool IsSelectionSlotAvailable()
    {
        return selectedCards[0] == null || selectedCards[1] == null;
    }

    public void ResetSelectedCards(int num)
    {
        if (num == 0)
        {
            selectedCards[0] = null;
        }
        if (num == 1)
        {
            selectedCards[1] = null;
        }
        if (num == 12)
        {
            selectedCards[0] = null;
            selectedCards[1] = null;
        }
    }

    void Shuffle(GameObject[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int randomIndex = Random.Range(i, n);
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    void OnStartButtonClicked()
    {
        audioSource.clip = backgroundMusic;  
        audioSource.Play();
        audioSource.loop = true; 
        cardChildren[0].GetComponent<CardScript>().SetGameActive(true);
        startButton.gameObject.SetActive(false);
        isGameStarted = true;
        titleText.text = "";
        attemptsCount = 0;
        attemptsText.text = "Attempts: " + attemptsCount;

        mainPanel.SetActive(false);

        foreach (GameObject card in cardChildren)
        {
            card.GetComponent<CardScript>().SetCardIdentifier(cardIdentifierValue);
            cardIdentifierValue += 0.5;
            Material loadedMaterial = Resources.Load<Material>("Materials/Material" + card.GetComponent<CardScript>().GetCardIdentifier());
            card.GetComponent<CardScript>().GetCardFaceRenderer().material = loadedMaterial;
        }

        Shuffle(cards);

        int i = 0;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Vector3 position = new Vector3((float)((x * 2) - 2), (float)-1.3, (y * 2) - 2);
                cards[i].transform.position = position;
                i++;
            }
        }
    }

    public void ResetGame()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(endGameClip);

        // Resetting game variables and UI
        matchedCardsCount = 0;
        totalTime = 0;
        timeText.text = "Time: " + 0;
        attemptsCount = 0;
        attemptsText.text = "Attempts: " + attemptsCount;
        titleText.text = "MEM♦RY G♠ME";
        
        // Handle game panel visibility
        mainPanel.SetActive(false);
        endGamePanel.SetActive(true);

        // Reset the selected cards
        selectedCards[0] = null;
        selectedCards[1] = null;
    }

    // Finish the scene by reloading it
    void FinishScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /* Setters and Getters */
    public bool CanSelectCard()
    {
        return !clickAllowed;
    }

    public void SetCardClickAllowed(bool allowed)
    {
        clickAllowed = allowed;
    }
}

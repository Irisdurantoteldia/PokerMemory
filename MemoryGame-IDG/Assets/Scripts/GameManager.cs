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
    public GameObject loadingPanel;

    public AudioClip successClip;
    public AudioClip failureClip;
    public AudioClip backgroundMusic;  
    public AudioClip endGameClip;
    public AudioClip click;
    public AudioClip loading;
    public AudioClip jackpot;
    public AudioSource audioSource;

    void Start()
    {
        // Configura els botons per començar i reiniciar el joc
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(FinishScene);  
        }

        // Inicialitza la UI amb valors per defecte
        titleText.text = "MEM♦RY G♠ME";
        attemptsCount = 0;
        attemptsText.text = "♠ttempts: " + attemptsCount;

        // Carrega el millor temps guardat en PlayerPrefs
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

        // Reproducció de la música de fons (inicia de manera contínua)
        audioSource.clip = endGameClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        // Compta el temps mentre el joc està actiu
        if (isGameStarted)
        {
            totalTime += Time.deltaTime;
            timeText.text = "Time: " + (int)totalTime;
        }

        // Controla el temps de refresc entre clics
        if (clickAllowed)
        {
            clickCooldown += Time.deltaTime;
            if (clickCooldown >= 1)
            {
                clickCooldown = 0;
                clickAllowed = false;
            }
        }

        // Comprova les dues cartes seleccionades per veure si coincideixen
        if (selectedCards[0] != null && selectedCards[1] != null)
        {
            AnimatorStateInfo stateInfo0 = selectedCards[0].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo stateInfo1 = selectedCards[1].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if (stateInfo0.IsName("CardFaceUp") && stateInfo1.IsName("CardFaceUp"))
            {
                CheckCardIdentifiers();
            }
        }

        // Si s'han emparellat totes les cartes (8 parelles), finalitza el joc
        if (matchedCardsCount == 8)
        {
            int bestTime = PlayerPrefs.GetInt("BestScore", int.MaxValue);
            if (totalTime < bestTime)
            {
                titleText.text = "¡¡New best score!!";
                PlayerPrefs.SetInt("BestScore", (int)totalTime);
                bestTimeText.text = "Best Time: " + (int)totalTime; 
                titleText.color = Color.yellow;
            }

            // Espera uns segons abans de reiniciar el joc
            isGameStarted = false;
            matchedCardsCount += 1;
            Invoke("ResetGame", 5);
        }
    }

    public void OnCardSelected(GameObject selectedCard)
    {
        // Reprodueix el so de clic quan es selecciona una carta
        audioSource.PlayOneShot(click, 0.2f);

        // Assigna la carta seleccionada al primer o segon espai (segons si ja hi ha una carta seleccionada)
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
        // Comprova si les cartes seleccionades coincideixen (identificador)
        if (selectedCards[0] != null && selectedCards[1] != null)
        {
            if (selectedCards[0].GetComponent<CardScript>().GetCardIdentifier() != selectedCards[1].GetComponent<CardScript>().GetCardIdentifier())
            {
                // Si no coincideixen, amaga les cartes i incrementa els intents
                selectedCards[0].GetComponent<CardScript>().HideCardFace();
                selectedCards[1].GetComponent<CardScript>().HideCardFace();
                attemptsCount += 1;
                attemptsText.text = "♠ttempts: " + attemptsCount;
                audioSource.PlayOneShot(failureClip);
                ResetSelectedCards(12);
            }
            else
            {
                // Si coincideixen, incrementa les parelles emparellades
                matchedCardsCount++;
                audioSource.PlayOneShot(successClip);
                ResetSelectedCards(12);
            }
        }
    }

    public bool IsSelectionSlotAvailable()
    {
        // Comprova si hi ha espai per seleccionar més cartes
        return selectedCards[0] == null || selectedCards[1] == null;
    }

    public void ResetSelectedCards(int num)
    {
        // Reseteja la selecció de cartes (pot fer-se individualment o conjuntament)
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
        // Barreja les cartes de manera aleatòria (algorisme de Fisher-Yates)
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
        // Mostra el panell de càrrega i reprodueix la música de càrrega
        loadingPanel.SetActive(true);
        audioSource.Stop();
        audioSource.clip = loading;
        audioSource.Play();

        // Amaga la interfície principal mentre el panell de càrrega està actiu
        mainPanel.SetActive(false);

        // Registra l'event per ocultar el panell de càrrega després de 10 segons
        Invoke("StartGameAfterLoading", 10f);
    }

    void StartGameAfterLoading()
    {
        // Amaga el panell de càrrega després de 10 segons
        loadingPanel.SetActive(false);

        // Inicia la música de fons del joc
        audioSource.Stop();
        audioSource.clip = backgroundMusic;  
        audioSource.Play();
        audioSource.loop = true;

        // Inicia els elements del joc
        bestTimeText.gameObject.SetActive(true);
        attemptsText.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
        cardChildren[0].GetComponent<CardScript>().SetGameActive(true);
        startButton.gameObject.SetActive(false);
        isGameStarted = true;
        titleText.text = "";
        attemptsCount = 0;
        attemptsText.text = "♠ttempts: " + attemptsCount;

        // Configura les cartes
        foreach (GameObject card in cardChildren)
        {
            card.GetComponent<CardScript>().SetCardIdentifier(cardIdentifierValue);
            cardIdentifierValue += 0.5;
            Material loadedMaterial = Resources.Load<Material>("Materials/Material" + card.GetComponent<CardScript>().GetCardIdentifier());
            card.GetComponent<CardScript>().GetCardFaceRenderer().material = loadedMaterial;
        }

        // Barreja les cartes
        Shuffle(cards);

        // Col·loca les cartes a la vista
        int i = 0;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Vector3 position = new Vector3((float)((x * 2)), (float)0.1, (y * 2) - 3);
                cards[i].transform.position = position;
                i++;
            }
        }
    }

    public void ResetGame()
    {
        // Reposiciona els valors i mostra el panell final després de guanyar
        audioSource.Stop();
        audioSource.PlayOneShot(jackpot);

        matchedCardsCount = 0;
        totalTime = 0;
        timeText.text = "Time: " + 0;
        attemptsCount = 0;
        attemptsText.text = "♠ttempts: " + attemptsCount;
        titleText.text = "MEM♦RY G♠ME";

        // Activa el panell final
        mainPanel.SetActive(false);
        endGamePanel.SetActive(true);

        // Reseteja les cartes seleccionades
        selectedCards[0] = null;
        selectedCards[1] = null;
    }

    // Reinicia la escena del joc
    void FinishScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /* Setters i Getters */
    public bool CanSelectCard()
    {
        return !clickAllowed;
    }

    public void SetCardClickAllowed(bool allowed)
    {
        clickAllowed = allowed;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CardGameManager : MonoBehaviour, IDataPersistence
{

    public static CardGameManager Instance;
    public static int gameSize = 2;
    // gameobject instance
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private Sprite cardBack;
    // all possible sprite for card front
    [SerializeField]
    private Sprite[] sprites;
    // list of card
    private Card[] cards;

    //we place card on this panel
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private GridLayoutGroup layout;
    // for preloading
    [SerializeField] private GameObject fakeCard;
    // other UI
    [SerializeField] private TextMeshProUGUI sizeLabel;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Button loadGameBtn;

    private float time;
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;
    private bool giveUp;
    private bool gameLoaded = false;
    private List<bool> activePositions = new List<bool>();
    private List<int> spriteIDs = new List<int>();
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        gameStart = false;
        gameplayPanel.SetActive(false);
    }

    // Start a game
    public void StartCardGame()
    {
        if (gameStart) return; // return if game already running
        gameStart = true;
        // toggle UI
        gameplayPanel.SetActive(true);
        winPanel.SetActive(false);
        // set cards, size, position
        SetGamePanel();
        // renew gameplay variables
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        // allocate sprite to card
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        if(!gameLoaded)
            time = 0;
    }

    // Initialize cards, size, and position based on size of game
    private void SetGamePanel()
    {
        // if game is odd, we should have 1 card less
        layout.constraintCount = gameSize;
        int isOdd = gameSize % 2;
        int tempSize = gameSize * gameSize - isOdd;
        cards = new Card[tempSize];
        for(int i=0; i<tempSize; i++)
        {
            activePositions.Add(true);
        }
        if(spriteIDs.Count == 0)
        {
            for (int i = 0; i < tempSize / 2; i++)
            {
                spriteIDs.Add(-1);
            }
        }

        ClearBoard();
        GameObject tempCard = null;
        for (int i = 0; i < gameSize; i++)
        {
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                // if is the last card and game is odd, we instead move the middle card on the panel to last spot
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    tempCard = Instantiate(fakeCard);
                }
                else
                {
                    // create card prefab
                    c = Instantiate(prefab, layout.transform);
                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].ID = index;
                    
                }
            }
        }
        if(isOdd == 1)
        {
            float pos = (float)(gameSize * gameSize) / 2;
            Debug.Log(pos);
            int index = (int)Mathf.Round(pos);//+ 1;
            Debug.Log(index);
            tempCard.transform.parent = layout.transform;
            tempCard.transform.localScale = new Vector3(1, 1, 1);
            tempCard.transform.SetSiblingIndex(index);
        }
        
    } 
    void ClearBoard()
    {
        foreach (Transform child in layout.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    // reset face-down rotation of all cards
    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }
    // Flip all cards after a short period
    IEnumerator HideFace()
    {
        //display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);

    }
    // Allocate pairs of sprite to card instances
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        if (gameLoaded)
        {
            selectedID = new int[spriteIDs.Count];
            for (i = 0; i < spriteIDs.Count; i++)
            {
                selectedID[i] = spriteIDs[i];
            }
        }
        else
        {
            // sprite selection
            for (i = 0; i < cards.Length / 2; i++)
            {
                // get a random sprite
                int value = Random.Range(0, sprites.Length - 1);
                // check previous number has not been selection
                // if the number of cards is larger than number of sprites, it will reuse some sprites
                for (j = i; j > 0; j--)
                {
                    if (selectedID[j - 1] == value)
                        value = (value + 1) % sprites.Length;
                }
                selectedID[i] = value;
            }
        }
        // card sprite deallocation
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }
        // card sprite pairing allocation
        int lastID = -1;
        for (i = 0; i < cards.Length / 2; i++)
        {
            
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                
                while (cards[value].SpriteID != -1)
                {
                    value = (value + 1) % cards.Length;
                    Debug.Log(value);
                }
                Debug.LogError(value);

                if (gameLoaded && activePositions[value] == false)
                {
                    if(lastID == -1)
                    {
                        lastID = selectedID[i];
                    }
                    else
                    {
                        selectedID[i] = lastID;
                        lastID = -1;
                    }
                    cards[value].QuickFade();
                    cardLeft--;
                }
                cards[value].SpriteID = selectedID[i];
                spriteIDs[i] = selectedID[i];
                
            }
        }
        //Debug.Log("gl: " + gameLoaded);
        //if (gameLoaded)
        //{
        //    for (i = 0; i < cards.Length; i++)
        //    {
        //        if (activePositions[i] == false)
        //        {
        //            cards[i].QuickFade();
        //            cardLeft--;
        //        }
        //    }
        //}
    }
    // Slider update gameSize
    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        
        sizeLabel.text = gameSize + " X " + gameSize;
    }
    // return Sprite based on its id
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }
    // return card back Sprite
    public Sprite CardBack()
    {
        return cardBack;
    }
    // check if clickable
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    // card onclick event
    public void cardClicked(int spriteId, int cardId)
    {
        // first card selected
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        { // second card selected
            if (spriteSelected == spriteId)
            {
                //correctly matched
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                activePositions[cardId] = false;
                activePositions[cardSelected] = false;
                //spriteIDs.Remove(spriteId);
                cardLeft -= 2;
                CheckGameWin();
            }
            else
            {
                // incorrectly matched
                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }
    // check if game is completed
    private void CheckGameWin()
    {
        // win game
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(1);
        }
    }
    // stop game
    private void EndGame()
    {
        if(!giveUp)
            scoreLabel.text = "Score: "+timeLabel.text;
        else
            scoreLabel.text = "Score: 0";
        DataPersistenceManager.instance.ResetData();
        gameLoaded = false;
        spriteIDs.Clear();
        activePositions.Clear();
        loadGameBtn.interactable = false;
        gameStart = false;
        winPanel.SetActive(true);
        ClearBoard();
        gameplayPanel.SetActive(false);
    }
    public void Continue()
    {
        winPanel.SetActive(false);
    }
    public void GiveUp()
    {
        giveUp = true;
        EndGame();
    }
    // track elasped time
    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + Mathf.Round(time) + "s";
        }
    }
    public void LoadGame()
    {
        
        gameLoaded = true;
    }
    public void LoadData(GameData data)
    {
        gameSize = data.gameSize;
        int isOdd = gameSize % 2;
        int temp = (gameSize * gameSize) - isOdd;
        
        time = data.gameTimer;
        for(int i=0; i<temp; i++)
        {
            activePositions.Add(data.GetPosition(i));
            
        }
        for (int i = 0; i < data.spriteIDs.Count; i++)
        {
            spriteIDs.Add(data.GetID(i));
        }
        loadGameBtn.interactable = true;
    }

    public void SaveData(GameData data)
    {
        
        data.gameSize = gameSize;
        data.gameTimer = time;
        int isOdd = gameSize % 2;
        int temp = (gameSize * gameSize) - isOdd;
        data.positions.Clear();
        data.spriteIDs.Clear();
        for (int i = 0; i < temp; i++)
        {
            data.SetPosition(i, activePositions[i]);
        }
        for (int i = 0; i < spriteIDs.Count; i++)
        {
            data.SetID(i, spriteIDs[i]);
        }
    }
    private void OnApplicationQuit()
    {
        Debug.Log("saving game...");
        if(gameStart)
            DataPersistenceManager.instance.SaveGame();
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static int gameSizeX = 2;
    public static int gameSizeY = 2;

    private CardBehaviour[] cards;
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject cardList;
    [SerializeField] private Sprite cardBack;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject ResultPanel;
    [SerializeField] private GameObject cardFloor;
    [SerializeField] private Text sizeLabel;
    [SerializeField] private Slider sizeSliderX;
    [SerializeField] private Slider sizeSliderY;
    [SerializeField] private GameObject LoadGameButton;
    [SerializeField] private Text Score;
    [SerializeField] private Text EndScore;

    private float time;
    private int spriteSelectedId;
    private int cardSelectedIndex;
    private int cardLeft;
    private bool gameStart;
    private int _score;


    void Awake() {
        Instance=this;
    }

    void Start() {
        gameStart=false;
        ShowMenu();
    }

    void ShowMenu() {
        LoadGameButton.SetActive(CanShowLoadButton());
        panel.SetActive(false);
        ResultPanel.SetActive(false);
        menu.SetActive(true);
    }

    void ShowGamePanel() {
        menu.SetActive(false);
        panel.SetActive(true);
        ResultPanel.SetActive(false);
    }

    public void StartCardGame() {
        if (gameStart)
            return;
        gameStart=true;
        ShowGamePanel();
        SetGamePanel();
        cardSelectedIndex=spriteSelectedId=-1;
        cardLeft=cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time=0;
    }

    private void SetGamePanel() {
        int isOdd = (gameSizeX*gameSizeY)%2;
        cards=new CardBehaviour[gameSizeX*gameSizeY-isOdd];

        foreach (Transform child in cardList.transform) {
            Destroy(child.gameObject);
        }
        cardList.transform.localPosition=cardFloor.transform.localPosition;

        RectTransform panelsize = cardFloor.GetComponent<RectTransform>();
        float row_size = panelsize.rect.width;
        float col_size = panelsize.rect.height;
        float xInc = row_size/gameSizeX;
        float yInc = col_size/gameSizeY;
        float curX = -xInc*(float)(gameSizeX/2);
        float curY = -yInc*(float)(gameSizeY/2);
        Vector2 cardSize = new Vector2(xInc*0.9f, yInc*0.9f);

        if (isOdd==0) {
            if (gameSizeX%2==0)
                curX+=xInc/2;
            if (gameSizeY%2==0)
                curY+=yInc/2;
        }
        float initialX = curX;

        for (int i = 0; i<gameSizeY; i++) {
            curX=initialX;
            for (int j = 0; j<gameSizeX; j++) {
                GameObject c;
                if (isOdd==1&&i==gameSizeY-1&&j==gameSizeX-1) {
                    int index = gameSizeY/2*gameSizeX+gameSizeX/2;
                    c=cards[index].gameObject;
                }
                else {
                    c=Instantiate(prefab);
                    c.transform.SetParent(cardList.transform, false);
                    int index = i*gameSizeX+j;
                    cards[index]=c.GetComponent<CardBehaviour>();
                    cards[index].ID=index;
                    c.GetComponent<RectTransform>().sizeDelta=cardSize;
                }
                c.transform.localPosition=new Vector3(curX, curY, 0);
                curX+=xInc;
            }
            curY+=yInc;
        }
    }

    void ResetFace() {
        foreach (var card in cards)
            card.ResetRotation();
    }

    IEnumerator HideFace() {
        yield return new WaitForSeconds(.5f);
        foreach (var card in cards)
            card.Flip();
        yield return new WaitForSeconds(0.5f);
    }

    private void SpriteCardAllocation() {
        int[] selectedID = new int[cards.Length/2];
        for (int i = 0; i<cards.Length/2; i++) {
            int value = Random.Range(0, sprites.Length-1);
            for (int j = i; j>0; j--) {
                if (selectedID[j-1]==value)
                    value=(value+1)%sprites.Length;
            }
            selectedID[i]=value;
        }

        foreach (var card in cards) {
            card.Active();
            card.SpriteID=-1;
            card.ResetRotation();
        }

        for (int i = 0; i<cards.Length/2; i++) {
            for (int j = 0; j<2; j++) {
                int value = Random.Range(0, cards.Length-1);
                while (cards[value].SpriteID!=-1)
                    value=(value+1)%cards.Length;

                cards[value].SpriteID=selectedID[i];
            }
        }
    }

    public void SetGameSizeX() {
        gameSizeX=(int)sizeSliderX.value;
        UpdateGameSizeLabel();
    }

    public void SetGameSizeY() {
        gameSizeY=(int)sizeSliderY.value;
        UpdateGameSizeLabel();
    }

    void UpdateGameSizeLabel() {
        sizeLabel.text=$"{gameSizeX} X {gameSizeY}";
    }

    public Sprite GetSprite(int spriteId) => sprites[spriteId];

    public Sprite CardBack() => cardBack;

    public bool CanClick() => gameStart;

    public void CardClicked(int spriteId, int cardIndex) {
        if (spriteSelectedId==-1) {
            spriteSelectedId=spriteId;
            cardSelectedIndex=cardIndex;
        }
        else {
            if (spriteSelectedId==spriteId) {
                _score=_score+10;
                Score.text= "Score: " + _score.ToString();
                cards[cardSelectedIndex].Inactive();
                cards[cardIndex].Inactive();
                cardLeft-=2;
                if (!CheckGameWin())
                    AudioPlayer.Instance.PlayAudio(2);
                }
            else {
                cards[cardSelectedIndex].Flip();
                cards[cardIndex].Flip();
                AudioPlayer.Instance.PlayAudio(3);
            }
            cardSelectedIndex=spriteSelectedId=-1;
        }
    }

    private bool CheckGameWin() {
        if (cardLeft==0) {
            DisplayResult();
            AudioPlayer.Instance.PlayAudio(1);
            return true;
        }
        return false;
    }

    private void EndGame() {
        CancelInvoke(nameof(EndGame));
        gameStart=false;
        ShowMenu();
    }

    public void GiveUp() {
        SaveLoadManger.Instance.ClearData();
        EndGame();
    }

    public void SaveAndExit() {
        gameStart=false;
        var saveGameFrame = new GamePanel {
            time=time,
            gameSizeX=gameSizeX,
            gameSizeY=gameSizeY,
            cardLeft=cardLeft,
            cards=new _CardFrame[cards.Length]
        };

        for (int i = 0; i<cards.Length; i++) {
            var cardFrame = new _CardFrame {
                id=cards[i].ID,
                spriteID=cards[i].SpriteID,
                flipped=cards[i].Flipped,
                isInactive=cards[i].IsInactive
            };
            saveGameFrame.cards[i]=cardFrame;
        }

        SaveLoadManger.Instance.Save(saveGameFrame);
        EndGame();
    }

    public void LoadLastGame() {
        var gameFrame = SaveLoadManger.Instance.Load();
        if (gameFrame==null||gameStart)
            return;

        gameSizeX=gameFrame.gameSizeX;
        gameSizeY=gameFrame.gameSizeY;
        ShowGamePanel();
        SetGamePanel();
        cardSelectedIndex=spriteSelectedId=-1;
        cardLeft=gameFrame.cardLeft;

        for (int i = 0; i<cards.Length; i++) {
            cards[i].SpriteID=gameFrame.cards[i].spriteID;
            if (gameFrame.cards[i].isInactive)
                cards[i].Inactive();
            else
                cards[i].Active();
            cards[i].ResetRotation();
        }

        StartCoroutine(HideFace());
        gameStart=true;
        time=gameFrame.time;
    }

    public bool CanShowLoadButton() => SaveLoadManger.Instance.CanLoad();

    public void DisplayResult() {
        gameStart=false;
        ResultPanel.SetActive(true);
        EndScore.text=Score.text;
        SaveLoadManger.Instance.ClearData();
        Invoke(nameof(EndGame), 2f);
    }
}

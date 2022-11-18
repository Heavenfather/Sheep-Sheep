using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using UnityEngine.SceneManagement;

public class DeckUI : MonoBehaviour
{
    public RectTransform centerDeck;
    public RectTransform leftDeck;
    public RectTransform rightDeck;
    public RectTransform bottomLeftDeck;
    public RectTransform bottomRightDeck;
    public GameObject cardUI;
    public Transform pickDeckTran;
    public Button btnShuffle;
    public GameObject gameOver;
    public Button btnReplay;
    public Button btnReturn;
    public Button btnQuit;
    public Text resultTxt;
    public Text curLevel;

    private List<CardUI> _cards = new List<CardUI>();
    private List<CardData> _dataList = null;
    private List<CardUI> _pickCards = new List<CardUI>();
    private Vector2 _centerStartPos = new Vector2(-268.55f, 297.95f);
    private Vector2 _leftStartPos = new Vector2(-57.5f, 347.9501f);
    private Vector2 _leftBottomStartPos = new Vector2(-165.43f, 57.5f);
    private Vector2 _rightBottomStartPos = new Vector2(36.673f, 57.5f);

    private const float _cardWidth = 115;
    private const float _cardHeight = 115;

    void Awake()
    {
        btnShuffle.onClick.AddListener(OnShuffleClick);
        btnReplay.onClick.AddListener(() =>
        {
            Start();
        });
        btnReturn.onClick.AddListener(() =>
        {
            Start();
        });
        btnQuit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    void Start()
    {
        Clear();
        gameOver.SetActive(false);
        _dataList = DeckMgr.GetInstance().RandomDeck();
        LevelData levelData = DeckMgr.GetInstance().GetCurrentLevelData();
        curLevel.text = "当前难度:" + levelData.levelType.ToString();
        int leftIndex = 0;
        int rightIndex = 0;
        float scale = DeckMgr.GetInstance().GetScaling();
        for (int i = 0; i < _dataList.Count; i++)
        {
            CardData data = _dataList[i];
            GameObject go = Instantiate(cardUI);
            go.SetActive(true);
            go.transform.localScale = new Vector3(scale, scale, 1);
            if (data.cardPos == CardPosEnum.Center)
            {
                Vector3 cardGridData = data.GetGridData();
                int offsetX = data.offsetEnum == CardOffsetEnum.Left ? -1 : data.offsetEnum == CardOffsetEnum.Right ? 1 : 0;
                float posX = _centerStartPos.x + (_cardWidth * cardGridData.x) + (offsetX * (_cardWidth / 2));
                int offsetY = data.offsetEnum == CardOffsetEnum.Down ? -1 : data.offsetEnum == CardOffsetEnum.Up ? 1 : 0;
                float posY = _centerStartPos.y - (_cardHeight * cardGridData.y) + (offsetY * (_cardHeight / 2));
                SetCardGo(go, data, posX, posY, centerDeck.transform);
            }
            else if (data.cardPos == CardPosEnum.Left || data.cardPos == CardPosEnum.Right)
            {
                float posX = _leftStartPos.x;
                int index = data.cardPos == CardPosEnum.Left ? leftIndex : rightIndex;
                float posY = _leftStartPos.y - (index * _cardHeight / 8);
                SetCardGo(go, data, posX, posY, data.cardPos == CardPosEnum.Left ? leftDeck.transform : rightDeck.transform);
                if (data.cardPos == CardPosEnum.Left)
                    leftIndex++;
                else
                    rightIndex++;
            }
            else if (data.cardPos == CardPosEnum.BottomLeft)
            {
                int mulIndex = leftIndex >= 25 ? 25 : leftIndex;
                float offset = mulIndex * (_cardWidth / 10);
                float posX = _leftBottomStartPos.x + offset;
                float posY = _leftBottomStartPos.y;
                SetCardGo(go, data, posX, posY, bottomLeftDeck);
                leftIndex++;
            }
            else if (data.cardPos == CardPosEnum.BottomRight)
            {
                int mulIndex = rightIndex >= 25 ? 25 : rightIndex;
                float offset = mulIndex * (_cardWidth / 10);
                float posX = _rightBottomStartPos.x - offset;
                float posY = _rightBottomStartPos.y;
                SetCardGo(go, data, posX, posY, bottomRightDeck);
                rightIndex++;
            }
        }

    }

    void SetCardGo(GameObject go, CardData data, float posX, float posY, Transform parent)
    {
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
        CardUI card = go.GetComponent<CardUI>();
        card.RefreshUI(data);
        card.SetCardParentUI(this);
        _cards.Add(card);
    }

    void OnShuffleClick()
    {
        DeckMgr.GetInstance().ShuffleCard();
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].RefreshUI(_dataList[i]);
        }
    }

    bool IsFail()
    {
        return _pickCards.Count >= 7;
    }

    bool IsWin()
    {
        return _cards.Count <= 0;
    }

    void SortPickCard()
    {
        List<int> sortList = new List<int>();
        var groups = _pickCards.GroupBy(x => x.data.spriteId);
        foreach (var group in groups)
        {
            foreach (var item in group)
            {
                sortList.Add(item.data.spriteId);
            }
        }
        for (int i = 0; i < _pickCards.Count; i++)
        {
            _pickCards[i].RefreshSprite(sortList[i]);
        }
    }

    void RemovePickCard(bool isUseItem = false)
    {
        if (!isUseItem)
        {
            var groups = _pickCards.GroupBy(x => x.data.spriteId).Where(x => x.Count() >= 3);
            if (groups.Count() > 0)
            {
                foreach (var group in groups)
                {
                    foreach (var item in group)
                    {
                        _pickCards.Remove(item);
                        DestroyImmediate(item.gameObject);
                    }
                }
            }
            if (IsWin())
            {
                resultTxt.text = "通关了！！！！";
                gameOver.SetActive(true);
            }
        }
    }

    void Clear()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            DestroyImmediate(_cards[i].gameObject);
        }
        _cards.Clear();
        if (_dataList != null)
            _dataList.Clear();
        for (int i = 0; i < _pickCards.Count; i++)
        {
            DestroyImmediate(_pickCards[i].gameObject);
        }
        _pickCards.Clear();
    }

    public void OnCardUIClick(CardUI card)
    {
        _pickCards.Add(card);
        _cards.Remove(card);
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].data.RemoveCoverId(card.data.cardId);
            _cards[i].RefreshUI(_dataList[i]);
        }
        card.transform.SetAsLastSibling();
        card.transform.DOMove(pickDeckTran.position, 0.5f).OnComplete(() =>
        {
            card.transform.SetParent(pickDeckTran.transform, false);

            RemovePickCard();
            SortPickCard();
            if (IsFail())
            {
                resultTxt.text = "失败了！！！！";
                gameOver.SetActive(true);
            }
        });
    }


}

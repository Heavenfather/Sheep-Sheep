using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    private List<CardUI> _cards = new List<CardUI>();
    List<CardData> _dataList = null;
    private Vector2 _centerStartPos = new Vector2(-268.55f, 297.95f);
    private Vector2 _leftStartPos = new Vector2(-57.5f, 347.9501f);
    private Vector2 _leftBottomStartPos = new Vector2(-165.43f, 57.5f);
    private Vector2 _rightBottomStartPos = new Vector2(36.673f, 57.5f);

    private const float _cardWidth = 115;
    private const float _cardHeight = 115;

    void Awake()
    {
        btnShuffle.onClick.AddListener(OnShuffleClick);
    }

    void Start()
    {
        _dataList = DeckMgr.GetInstance().RandomDeck();
        int leftIndex = 0;
        int rightIndex = 0;
        for (int i = 0; i < _dataList.Count; i++)
        {
            CardData data = _dataList[i];
            GameObject go = Instantiate(cardUI);
            go.SetActive(true);
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
                float posX = _leftBottomStartPos.x + (leftIndex * (_cardWidth / 10));
                float posY = _leftBottomStartPos.y;
                SetCardGo(go, data, posX, posY, bottomLeftDeck);
                leftIndex++;
            }
            else if (data.cardPos == CardPosEnum.BottomRight)
            {
                float posX = _rightBottomStartPos.x - (rightIndex * (_cardWidth / 10));
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

    public void OnCardUIClick(CardUI card)
    {
        _cards.Remove(card);
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].data.RemoveCoverId(card.data.cardId);
            _cards[i].RefreshUI(_dataList[i]);
        }
        card.transform.SetAsLastSibling();
        card.transform.DOMove(pickDeckTran.position, 0.3f).OnComplete(() =>
        {
            
        });
    }

}

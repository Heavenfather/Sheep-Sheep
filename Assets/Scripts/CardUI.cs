using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Button btn;
    public Image img;
    public Sprite[] sprites;
    [HideInInspector]
    public CardData data;
    [HideInInspector]
    public RectTransform rt;

    private DeckUI _cardParent;

    void Start()
    {
        rt = this.gameObject.GetComponent<RectTransform>();
        btn.onClick.AddListener(OnSelfClick);
    }

    public void RefreshUI(CardData cardData)
    {
        data = cardData;
        bool haveCover = cardData.IsHaveCoverCard();
        img.color = haveCover ? Color.grey : Color.white;
        RefreshSprite(cardData.spriteId);
        Vector3 grid = data.GetGridData();
        int coverId = data.GetCoverId();
        gameObject.name = "id" + cardData.cardId + "pos(" + grid.x + "_" + grid.y + "_" + grid.z + ")_coverId:" + coverId + "_offset:" + data.offsetEnum.ToString();
    }

    public void SetCardParentUI(DeckUI deck)
    {
        _cardParent = deck;
    }

    public void RefreshSprite(int spriteId)
    {
        data.spriteId = spriteId;
        img.sprite = sprites[spriteId];
    }

    public void SetButtonEnable()
    {
        btn.interactable = false;
    }

    private void OnSelfClick()
    {
        if (data.IsHaveCoverCard())
            return;
        if (_cardParent != null)
        {
            SetButtonEnable();
            DeckMgr.GetInstance().RemoveCard(data);
            _cardParent.OnCardUIClick(this);
        }
    }
}

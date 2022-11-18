using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData
{
    public CardData(int id, CardPosEnum cardPos)
    {
        this.cardId = id;
        this.cardPos = cardPos;
    }

    public int cardId;
    public CardPosEnum cardPos;
    public CardOffsetEnum offsetEnum = CardOffsetEnum.None;
    //注意这个id是从0开始的
    public int spriteId = 0;

    //覆盖本卡牌的列表
    private List<int> _beCoverIds = new List<int>();
    //卡牌所处位置信息 x=横向 y=纵向 z=所处层级
    private Vector3 _gridData = Vector3.zero;

    //是否有卡牌覆盖自己
    public bool IsHaveCoverCard()
    {
        return _beCoverIds.Count > 0;
    }

    public void RemoveCoverId(int id)
    {
        if (!_beCoverIds.Contains(id))
            return;
        _beCoverIds.Remove(id);
    }

    public void AddCoverId(int id)
    {
        _beCoverIds.Add(id);
    }

    public int GetCoverId()
    {
        if (_beCoverIds.Count > 0)
        {
            return _beCoverIds[0];
        }
        return -1;
    }

    public void SetGridData(int x, int y, int layer)
    {
        _gridData.x = x;
        _gridData.y = y;
        _gridData.z = layer;
    }

    public Vector3 GetGridData()
    {
        return _gridData;
    }

    public void GenerateSpriteId(LevelData levelData)
    {
        int pairCount = levelData.piar * 3;
        spriteId = Mathf.CeilToInt(this.cardId / pairCount);
    }

    public CardOffsetEnum RandomOffset()
    {
        int offset = Random.Range(0, 5);
        offsetEnum = (CardOffsetEnum)offset;
        return offsetEnum;
    }

}

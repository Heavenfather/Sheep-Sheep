using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckMgr
{
    private static DeckMgr _instance;
    public static DeckMgr GetInstance()
    {
        if (_instance == null)
            _instance = new DeckMgr();
        return _instance;
    }

    private List<CardData> _randomCardList = new List<CardData>();
    private LevelData _currentLevelData;

    public List<CardData> RandomDeck()
    {
        ClearCardList();

        _currentLevelData = LevelMgr.GetInstance().GetRandomLevel();
        int totalCount = _currentLevelData.GetTotalCount();
        int createdCount = 0;
        //先生成中间的牌堆
        for (int i = 0; i < _currentLevelData.layer; i++)
        {
            for (int j = 0; j < _currentLevelData.row; j++)
            {
                for (int k = 0; k < _currentLevelData.col; k++)
                {
                    //配置1才生成
                    if (_currentLevelData.levelDatas[j, k] != 0)
                    {
                        CardData cardData = new CardData(createdCount, CardPosEnum.Center);
                        cardData.SetGridData(k, j, i);
                        cardData.RandomOffset();
                        cardData.GenerateSpriteId(_currentLevelData);
                        _randomCardList.Add(cardData);

                        createdCount++;
                    }
                }
            }
        }

        //剩余的摆放在两边或者底下
        int remainCount = totalCount - createdCount;
        bool isLeftRight = Random.Range(0, 2) == 0;
        //如果列数大于4,就必定是在下面，要不然界面显示会有重叠
        if (_currentLevelData.col >= 4)
            isLeftRight = false;
        int leftCount = Mathf.FloorToInt(remainCount / 2);
        int rightCount = Mathf.CeilToInt(remainCount / 2);
        if (isLeftRight)
        {
            for (int i = 0; i < leftCount; i++)
            {
                CreatedBorderCard(CardPosEnum.Left, ref createdCount);
            }
            for (int i = 0; i < rightCount; i++)
            {
                CreatedBorderCard(CardPosEnum.Right, ref createdCount);
            }
        }
        else
        {
            for (int i = 0; i < leftCount; i++)
            {
                CreatedBorderCard(CardPosEnum.BottomLeft, ref createdCount);
            }
            for (int i = 0; i < rightCount; i++)
            {
                CreatedBorderCard(CardPosEnum.BottomRight, ref createdCount);
            }
        }

        for (int i = 0; i < _randomCardList.Count; i++)
        {
            SetCoverData(_randomCardList[i]);
        }

        ShuffleCard();

        return _randomCardList;
    }

    //洗牌
    public void ShuffleCard()
    {
        List<int> spriteIds = new List<int>();
        for (int i = 0; i < _randomCardList.Count; i++)
        {
            spriteIds.Add(_randomCardList[i].spriteId);
        }
        //打乱spriteid
        System.Random rd = new System.Random();
        int index = 0;
        int temp;
        for (int i = 0; i < spriteIds.Count; i++)
        {
            index = rd.Next(0, spriteIds.Count - 1);
            if (index != i)
            {
                temp = spriteIds[i];
                spriteIds[i] = spriteIds[index];
                spriteIds[index] = temp;
            }
        }

        //重置回去数据的spriteid 就只是显示上打散了UI,卡牌上的数据是不变的
        for (int i = 0; i < _randomCardList.Count; i++)
        {
            _randomCardList[i].spriteId = spriteIds[i];
        }
    }

    public void RemoveCard(CardData card)
    {
        _randomCardList.Remove(card);
    }

    public void ClearCardList()
    {
        _randomCardList.Clear();
        _currentLevelData = null;
    }

    //如果个数太多 缩放一下图片
    public float GetScaling()
    {
        if (_currentLevelData.col >= 7)
        {
            return 6.0f / (_currentLevelData.col + 1.0f);
        }
        return 1.0f;
    }

    public LevelData GetCurrentLevelData()
    {
        return _currentLevelData;
    }

    private void CreatedBorderCard(CardPosEnum pos, ref int id)
    {
        CardData cardData = new CardData(id, pos);
        cardData.GenerateSpriteId(_currentLevelData);
        _randomCardList.Add(cardData);
        id++;
    }

    private void SetCoverData(CardData card)
    {
        for (int i = 0; i < _randomCardList.Count; i++)
        {
            CardData data = _randomCardList[i];
            if (card.cardPos == CardPosEnum.Center && data.cardPos == CardPosEnum.Center)
            {
                if (!IsNearCard(card, data))
                    continue;
                //两张卡牌是邻近的，比较一下偏移判定是否覆盖
                if (IsCover(card, data))
                {
                    card.AddCoverId(data.cardId);
                }
            }
            else
            {
                //四周的只要卡牌的id大于该id 其实就是盖住的
                if (card.cardPos == data.cardPos && data.cardId > card.cardId)
                {
                    card.AddCoverId(data.cardId);
                }
            }
        }
    }

    //是否是邻近的卡牌
    private bool IsNearCard(CardData curCard, CardData targetCard)
    {
        Vector3 curGridData = curCard.GetGridData();
        Vector3 targetGridData = targetCard.GetGridData();
        int subX = (int)Mathf.Abs(curGridData.x - targetGridData.x);
        int subY = (int)Mathf.Abs(curGridData.y - targetGridData.y);
        if ((subX == 1 && subY == 0) ||
            (subX == 0 && subY == 1) ||
            (subX == 1 && subY == 1) ||
            (subX == 0 && subY == 0 && targetGridData.z > curGridData.z))
        {
            return true;
        }
        return false;
    }

    private bool IsCover(CardData curCard, CardData targetCard)
    {
        //已经筛选过目标卡牌是在本卡牌的周围的
        Vector3 curGridData = curCard.GetGridData();
        Vector3 targetGridData = targetCard.GetGridData();
        Vector3 subVec = curGridData - targetGridData;
        //象限图 x轴向左 y轴向上
        if ((subVec.x == 0 && subVec.y == 0 && subVec.z < 0))
        {
            //同位置不同层 or 随机在中间层的某一个位置会有可能是相同的格子坐标
            if (curCard.offsetEnum == CardOffsetEnum.None || targetCard.offsetEnum == CardOffsetEnum.None)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Up && targetCard.offsetEnum != CardOffsetEnum.Down)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Down && targetCard.offsetEnum != CardOffsetEnum.Up)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Left && targetCard.offsetEnum != CardOffsetEnum.Right)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Right && targetCard.offsetEnum != CardOffsetEnum.Left)
                return true;
        }
        else if (subVec.x == -1 && subVec.y == 0 && subVec.z <= 0)
        {
            //在本卡牌的右边同层也有可能覆盖
            if (curCard.offsetEnum == CardOffsetEnum.Right && targetCard.offsetEnum != CardOffsetEnum.Right)
                return true;
            if (targetCard.offsetEnum == CardOffsetEnum.Left && curCard.offsetEnum != CardOffsetEnum.Left)
                return true;
        }
        else if (subVec.x == 1 && subVec.y == 0 && subVec.z < 0)
        {
            //在本卡牌左边只有大于该层才有可能覆盖 
            if (curCard.offsetEnum == CardOffsetEnum.Left && targetCard.offsetEnum != CardOffsetEnum.Left)
                return true;
            if (targetCard.offsetEnum == CardOffsetEnum.Right && curCard.offsetEnum != CardOffsetEnum.Right)
                return true;
        }
        else if (subVec.y == -1 && subVec.x == 0 && subVec.z <= 0)
        {
            //在本卡牌的下边同层也有可能覆盖
            if (curCard.offsetEnum == CardOffsetEnum.Down && targetCard.offsetEnum != CardOffsetEnum.Down)
                return true;
            if (targetCard.offsetEnum == CardOffsetEnum.Up && curCard.offsetEnum != CardOffsetEnum.Up)
                return true;
        }
        else if (subVec.y == 1 && subVec.x == 0 && subVec.z < 0)
        {
            //在本卡牌上边只有大于该层才有可能覆盖
            if (curCard.offsetEnum == CardOffsetEnum.Up && targetCard.offsetEnum != CardOffsetEnum.Up)
                return true;
            if (targetCard.offsetEnum == CardOffsetEnum.Down && curCard.offsetEnum != CardOffsetEnum.Down)
                return true;
        }
        else if (subVec.x == -1 && subVec.y == 1 && subVec.z < 0)
        {
            //在自己的右上
            if (curCard.offsetEnum == CardOffsetEnum.Up && targetCard.offsetEnum == CardOffsetEnum.Left)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Right && targetCard.offsetEnum == CardOffsetEnum.Down)
                return true;
        }
        else if (subVec.x == 1 && subVec.y == 1 && subVec.z < 0)
        {
            //在自己的左上
            if (curCard.offsetEnum == CardOffsetEnum.Up && targetCard.offsetEnum == CardOffsetEnum.Right)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Left && targetCard.offsetEnum == CardOffsetEnum.Down)
                return true;
        }
        else if (subVec.x == -1 && subVec.y == -1 && subVec.z <= 0)
        {
            //在自己的右下
            if (curCard.offsetEnum == CardOffsetEnum.Right && targetCard.offsetEnum == CardOffsetEnum.Up)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Down && targetCard.offsetEnum == CardOffsetEnum.Left)
                return true;
        }
        else if (subVec.x == 1 && subVec.y == -1 && subVec.z <= 0)
        {
            //在自己左下
            if (curCard.offsetEnum == CardOffsetEnum.Left && targetCard.offsetEnum == CardOffsetEnum.Up)
                return true;
            if (curCard.offsetEnum == CardOffsetEnum.Down && targetCard.offsetEnum == CardOffsetEnum.Right)
                return true;
        }

        return false;
    }
}

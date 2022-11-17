using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LevelMgr
{
    private static readonly string _path = Application.streamingAssetsPath + "/Level/";

    private static LevelMgr _instance;
    public static LevelMgr GetInstance()
    {
        if (_instance == null)
        {
            _instance = new LevelMgr();
        }
        return _instance;
    }

    private Dictionary<LevelType, List<LevelData>> _levelDatas = new Dictionary<LevelType, List<LevelData>>();

    public void ReadAllLevel()
    {
        string[] files = Directory.GetFiles(_path, "*.json", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string key = files[i].Substring(files[i].Length - 6, 1);
            string levelJson = File.ReadAllText(files[i]);

            JObject levelData = JObject.Parse(levelJson);
            AddLevel(LevelType.Simple, int.Parse(key), JObject.Parse(levelData["simple"].ToString()));
        }
    }

    private void AddLevel(LevelType type, int key, JObject data)
    {
        int[,] value = null;
        LevelData levelData = new LevelData();
        levelData.levelType = type;
        levelData.levelId = key;
        levelData.layer = (int)data["layer"];
        levelData.cardNum = (int)data["cardNum"];
        levelData.piar = (int)data["piar"];
        JArray rowArr = JArray.Parse(data["center"].ToString());
        for (int j = 0; j < rowArr.Count; j++)
        {
            levelData.row = rowArr.Count;
            JArray colArr = JArray.Parse(rowArr[j].ToString());
            for (int k = 0; k < colArr.Count; k++)
            {
                levelData.col = colArr.Count;
                if (value == null) value = new int[rowArr.Count, colArr.Count];
                value[j, k] = int.Parse(rowArr[j][k].ToString());
            }
        }
        levelData.levelDatas = value;
        if (!_levelDatas.ContainsKey(type))
            _levelDatas.Add(type, new List<LevelData>());
        if (levelData.GetTotalCount() < (levelData.layer * levelData.col * levelData.row))
        {
            Debug.LogError($"关卡{key}的{type.ToString()}生成的总牌数不可以低于布置中间牌堆的最低数量!!");
            return;
        }
        _levelDatas[type].Add(levelData);
    }

    public LevelData GetLevel(int level, LevelType type = LevelType.Hard)
    {
        List<LevelData> datas = null;
        if (_levelDatas.TryGetValue(type, out datas))
        {
            // Debug.Log("当前难度:" + type.ToString() + " 关卡:" + level);
            return _levelDatas[type].Find(x => x.levelId == level);
        }
        Debug.LogError("关卡=" + level + "缺少:" + type.ToString() + "类型!!");
        return null;
    }

    public LevelData GetRandomLevel()
    {
        // LevelType type = (LevelType)Random.Range(0, 3);
        // int level = Random.Range(1, GetLevelCount() + 1);
        return GetLevel(1, LevelType.Simple);
    }

    public int GetLevelCount()
    {
        string[] files = Directory.GetFiles(_path, "*.json", SearchOption.AllDirectories);
        return files.Length;
    }

}

public class LevelData
{
    public LevelType levelType;
    public int levelId;
    public int layer;
    public int col;
    public int row;
    public int cardNum;
    public int piar;
    public int[,] levelDatas;

    public int GetTotalCount()
    {
        return this.piar * 3 * this.cardNum;
    }

}

public enum LevelType
{
    Simple,
    Medium,
    Hard
}

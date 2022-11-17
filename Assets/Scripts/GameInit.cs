using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : MonoBehaviour
{

    void Awake()
    {
        DontDestroyOnLoad(this);
        LevelMgr.GetInstance().ReadAllLevel();
    }

}

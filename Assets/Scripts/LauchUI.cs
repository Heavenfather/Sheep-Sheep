using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LauchUI : MonoBehaviour
{
    public Button btnStart;


    void Start()
    {
        btnStart.onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync(1);
        });
    }

}

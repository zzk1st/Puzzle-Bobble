﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PreTutorial : MonoBehaviour
{
    public Sprite[] pictures;

    // Use this for initialization
    void Start()
    {
        GetComponent<Image>().sprite = pictures[(int)CoreManager.Instance.levelData.stageMoveMode];
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.swish[0]);
    }
	
    // 注意此函数在动画结束被调用了！
    public void  Stop()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.swish[1]);

        UIManager.Instance.PreTutorialDone();
        gameObject.SetActive(false);
    }
}

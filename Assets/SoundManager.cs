using UnityEngine;
using System.Collections;

// 管理所有可能会连续播放的音效，播放间隔可能会快于音效本身，同时可能有连续音效。比如球消掉，球掉进坛子，生成球等等
using System.Collections.Generic;


// 注意：SoundManager所有child gameobject的顺序必须和这个enum一致！
public enum SoundSeqType
{
    BallFallInPot
}

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    void Awake()
    {
        // sound manager和其他关系不大, 不用Initialize()来管理顺序
        Instance = this;
    }

    public void Play(SoundSeqType type)
    {
        transform.GetChild((int) type).GetComponent<SoundSeq>().Play();
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BallFX
{
    [SerializeField]
    public BallColor color;
    [SerializeField]
    public GameObject fireTrailPrefab;
    [SerializeField]
    public AudioClip fireAudio;
    [SerializeField]
    public GameObject explosionPrefab;
    [SerializeField]
    public AudioClip explosionAudio;
}

public class BallFXManager : MonoBehaviour {

    [SerializeField]
    private BallFX[] ballFXArray;
    public Dictionary<BallColor, BallFX> ballFXs = new Dictionary<BallColor, BallFX>();

	// Use this for initialization
	void Start ()
    {
        foreach(BallFX ballFX in ballFXArray)
        {
            ballFXs[ballFX.color] = ballFX;
        }
	}
}

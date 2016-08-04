using UnityEngine;
using System.Collections;

[System.Serializable]
public class BallFX
{
    [SerializeField]
    public BallColor color;
    [SerializeField]
    public GameObject particlePrefab;
    [SerializeField]
    public AudioClip audio;
}

public class BallFXManager : MonoBehaviour {

    [SerializeField]
    private BallFX[] ballFXArray;
    public Hashtable ballFXs;

	// Use this for initialization
	void Start ()
    {
        foreach(BallFX ballFX in ballFXArray)
        {
            ballFXs.Add(ballFX.color, ballFX);
        }
	}
}

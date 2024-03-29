﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {

    public static ScoreManager Instance;


    private static int highScore = 0;
    private static float fastestTime = 0;
    private static int currentLevel = 0;
    private static int currentScore = 0;
    private static int timeScoreLowerBound = 1000;
    private static int timeScoreUpperBound = 100000;
    private static int comboCount = 0;
    private static int doubleScore = 1;
    public static int playedTime = 0;

    public GameObject popupScore;
    public GameObject perfect;
    public GameObject scoreText;
    public GameObject canvas;

    public Vector3 lastStopBallPos;     // Last position of ball before it stops

    public int Score
    {
        get { return currentScore; }
        set
        {
            currentScore = value;
            scoreText.GetComponent<TotalScoreCounter>().updateScore(currentScore);
            BoostManager.Instance.SetFreeBoostCrystal (currentScore);
        }
    }

    public int DoubleScore
    {
        get { return doubleScore; }
    }

    public int ComboCount
    {
        get { return comboCount; }
        set
        {
            comboCount = value;

            if (value > 0)
            {
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.combo[Mathf.Clamp(value - 1, 0, 5)]);
                BugManager.Instance.CreateBug(lastStopBallPos, value);
                if (value >= 6)
                {
                    SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.combo[5]);
                    //FireEffect.SetActive(true);
                    CoreManager.Instance.FireEffect.SetActive(true);
                    doubleScore = 2;
                }
            }
            else
            {
                //FireEffect.SetActive(false);
                doubleScore = 1;
                BugManager.Instance.DestroyBugs();
                CoreManager.Instance.FireEffect.SetActive(false);
            }
        }
    }

    // Use this for initialization
    void Start () {
        Instance = this;
        // 初始化各种分数参数
        currentLevel = PlayerPrefs.GetInt("OpenLevel");
        string scoreName = "HighScore" + currentLevel.ToString();
        if (PlayerPrefs.HasKey(scoreName))
            highScore = PlayerPrefs.GetInt(scoreName);
        string timeName = "Time" + currentLevel.ToString();
        if (PlayerPrefs.HasKey(timeName))
            fastestTime = PlayerPrefs.GetFloat(timeName);
        comboCount = 0;
        doubleScore = 1;
	}

    public int UpdateComboScore(int numBalls)
    {
        int singleBallScore = 10 + (comboCount-1) * 5;
        int val = numBalls * singleBallScore;
        Score += val;
        return singleBallScore;
    }

    public int UpdatePlayedTimeScore(int playedTime)
    {
        int val = (int)(timeScoreLowerBound + (fastestTime / playedTime) * (timeScoreUpperBound - timeScoreLowerBound));
        Score += val;
        return val;
    }

    public int UpdatePotScore(int val)
    {
        int score = val * doubleScore;
        Score += score;
        return score;
    }

    public int UpdateBugScore(int val)
    {
        Score += val;
        return val;
    }

    // 在Combo时候跳出来的text
    public void PopupComboScore(int value, Vector3 pos)
    {
        if (value != 0)
        {
            Transform parent = GameObject.Find("Canvas").transform;
            GameObject poptxt = Instantiate(popupScore, pos, Quaternion.identity) as GameObject;
            poptxt.transform.GetComponentInChildren<Text>().text = "" + value;
            poptxt.transform.SetParent(parent);
            poptxt.transform.localScale = Vector3.one;
            Destroy(poptxt, 1);
        }
    }

    public void PopupPotScore(double value, Vector3 pos)
    {
        Transform parent = GameObject.Find("Canvas").transform;
        GameObject poptxt = Instantiate(popupScore, pos, Quaternion.identity) as GameObject;
        poptxt.transform.GetComponentInChildren<Text>().text = "" + value;
        poptxt.transform.SetParent(parent);
        poptxt.transform.localScale = Vector3.one;
        Destroy(poptxt, 1);
    }

    // Update is called once per frame
    void Update () {
	    // 可以当分数或者某些条件满足的时候放一些特效在此处
	}

}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {

    public static ScoreManager Instance;

    private static int numLevels = 666;
    private static int highScore = 0;
    private static float fastestTime = 0;
    private static int currentLevel = 0;
    private static int currentScore = 0;
    private static int timeScoreLowerBound = 1000;
    private static int timeScoreUpperBound = 100000;
    public static int comboFactor = 1;
    public static int playedTime = 0;
    public GameObject popupScore;

    public static int Score
    {
        get { return ScoreManager.currentScore; }
        set { ScoreManager.currentScore = value; }
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

	}

    public int UpdateComboScore(int numBalls)
    {
        int val = numBalls * comboFactor * 10;
        currentScore += val;
        return val;
    }
    
    int Fib(int n)
    {
        if (n <= 0)
            return 0;
        if (n == 1)
            return 1;
        return Fib(n - 1) + Fib(n - 2);
    }

    public int UpdateFallingScore(int numBalls)
    {
        int val = numBalls * numBalls * 10;
        currentScore += val;
        return val;
    }

    public int UpdatePlayedTimeScore(int playedTime)
    {
        int val = (int)(timeScoreLowerBound + (fastestTime / playedTime) * (timeScoreUpperBound - timeScoreLowerBound));
        currentScore += val;
        return val;
    }

    public void PopupComboScore(int value, Vector3 pos)
    {
        Transform parent = GameObject.Find("Scores").transform;
        GameObject poptxt = Instantiate(popupScore, pos, Quaternion.identity) as GameObject;
        poptxt.transform.GetComponentInChildren<Text>().text = "" + value;
        poptxt.transform.SetParent(parent);
        poptxt.transform.localScale = Vector3.one;
        Destroy(poptxt, 1);
    }

    // Update is called once per frame
    void Update () {
	    // 显示分数
	}
}

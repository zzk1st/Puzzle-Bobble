using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour {

    public static ScoreManager Instance;

    private static int numLevels = 666;
    private int highScore = 0;
    private float fastestTime = 0;
    private int currentLevel = 0;
    private int currentScore = 0;
    private int timeScoreLowerBound = 1000;
    private int timeScoreUpperBound = 100000;
    public int comboFactor = 1;
    public int playedTime = 0;

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

    int ComboScore(int numBalls)
    {
        return numBalls * comboFactor * 10;
    }
    
    int Fib(int n)
    {
        if (n <= 0)
            return 0;
        if (n == 1)
            return 1;
        return Fib(n - 1) + Fib(n - 2);
    }

    int FallingScore(int numBalls)
    {
        return Fib(numBalls) * 10;
    }

    int PlayedTimeScore(int playedTime)
    {
        return (int)(timeScoreLowerBound + (fastestTime / playedTime) * (timeScoreUpperBound - timeScoreLowerBound));
    }
	
	// Update is called once per frame
	void Update () {
	    // 显示分数
	}
}

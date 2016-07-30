using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour {

    private static int numLevels = 666;
    private int[] highScores = new int[numLevels];
    private int[] fastestTime = new int[numLevels];
    private int currentLevel = 0;
    private long currentScore = 0;
    private int timeScoreLowerBound = 1000;
    private int timeScoreUpperBound = 100000;
    public int comboFactor = 1;
    public int playedTime = 0;

	// Use this for initialization
	void Start () {
	    // 初始化各种分数参数
        // 调入当前关卡currentLevel和当前分数(应该是0)

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
        return (int)(timeScoreLowerBound + ((float)fastestTime[currentLevel] / playedTime) * (timeScoreUpperBound - timeScoreLowerBound));
    }
	
	// Update is called once per frame
	void Update () {
        // 更新分数
	    // 显示分数
	}
}

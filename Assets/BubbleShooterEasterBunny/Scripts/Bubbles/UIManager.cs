using UnityEngine;
using System.Collections;
using InitScriptName;


public enum GameMode
{
    Opening = 0,
    // Opening模式下没有输入，游戏自动生成球并发射
    Playing,
    // Tutorial
}

public enum GameStatus
{
    None,
    Playing,
    Demo,
    Pause,
    Win,
    GameOver,
    StageMovingUp,
    // Demo模式下播放动画，但不能使用任何操作
    Tutorial,
    PreTutorial,
    BossArriving,
}

/// <summary>
/// Game manager: 主要控制游戏进程，停止／继续，和游戏输赢后的各种动作
///               对GameStatus，我们应该只调用它读取当前状态，而不应该通过其直接修改游戏状态
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameMode gameMode;
    private GameStatus lastGameStatus;
    bool winStarted;

    private GameStatus _gameStatus;
    public GameStatus gameStatus
    {
        get { return _gameStatus; }
    }

    // popups
    public GameObject levelClearedGO;
    public GameObject menuCompleteGO;
    public GameObject preTutorialGO;
    public GameObject outOfMovesGO;
    public GameObject menuGameOverGO;

    // Use this for initialization
    void Awake()
    {
        Instance = this;
        setGameStatus(GameStatus.None);
    }

    void Update()
    {
    }

    public void PreTutorialDone()
    {
        if (CoreManager.Instance.levelData.missionType == MissionType.BossBattle)
        {
            BossArriving();
        }
        else
        {
            Play();
        }
    }

    public void BossArriving()
    {
        BossManager.Instance.GameStartBossMove();
        setGameStatus(GameStatus.BossArriving);
    }

    public void Play()
    {
        if (this.gameStatus != GameStatus.Playing)
        {
            CoreManager.Instance.ballShooter.Initialize();
            setGameStatus(GameStatus.Playing);
        }
    }

    public void Demo()
    {
        CoreManager.Instance.ballShooter.isLocked = true;
        setGameStatus(GameStatus.Demo);
    }

    public void Win()
    {
        setGameStatus(GameStatus.Win);
        if (!winStarted)
            StartCoroutine(WinAction());
    }

    public void Pause()
    {
        setGameStatus(GameStatus.Pause);
        Time.timeScale = 0;
    }

    public void Resume()
    {
        setGameStatus(lastGameStatus);
        CoreManager.Instance.ballShooter.isLocked = false;
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        setGameStatus(GameStatus.GameOver);
        StartCoroutine(LoseAction());
    }

    public void StageMovingUp()
    {
        setGameStatus(GameStatus.StageMovingUp);
    }

    public void PreTutorial()
    {
        setGameStatus(GameStatus.PreTutorial);
        ShowPreTutorial();
    }

    void setGameStatus(GameStatus newStatus)
    {
        lastGameStatus = _gameStatus;
        _gameStatus = newStatus;
    }

    // Update is called once per frame
    IEnumerator WinAction()
    {
        // 注意这步很关键，如果还保持Aim Mode，那么有的球会从左右边界外溜出去
        CoreManager.Instance.ballShooter.SetStageCollidersMode(BallShooter.StageCollidersMode.FireMode);

        winStarted = true;
        InitScript.Instance.AddLife(1);
        levelClearedGO.SetActive(true);
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.winSound);
        yield return new WaitForSeconds(1f);
        if (CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
        {
            yield return new WaitForSeconds(1f);
        }

        foreach (GameObject item in GameObject.FindGameObjectsWithTag("Ball"))
        {
            if (item.GetComponent<Ball>().state == Ball.BallState.Fixed)
                item.GetComponent<GameItem>().StartFall();
                                   
        }
        // StartCoroutine( PushRestBalls() );
        Transform b = CoreManager.Instance.gameItemsNode.transform;
        Ball[] balls = CoreManager.Instance.gameItemsNode.GetComponentsInChildren<Ball>();
        foreach (Ball item in balls)
        {
            if (item.GetComponent<Ball>().state == Ball.BallState.Fixed)
                item.StartFall();
        }

        while (CoreManager.Instance.levelData.limitAmount >= 0)
        {
            if (CoreManager.Instance.ballShooter.CatapultBall != null)
            {
                Ball ball = CoreManager.Instance.ballShooter.CatapultBall.GetComponent<Ball>();
                CoreManager.Instance.ballShooter.CatapultBall = null;
                ball.transform.parent = CoreManager.Instance.gameItemsNode.transform;
                ball.tag = "Ball";
                ball.PushBallAFterWin();
            }
            CoreManager.Instance.levelData.limitAmount--;
            yield return new WaitForSeconds(0.33f);
            /*if (mainscript.Instance.ballShooter.boxCatapult.GetComponent<Grid>().AttachedGameItem != null)
            {
                mainscript.Instance.levelData.LimitAmount--;
                Ball ball = mainscript.Instance.ballShooter.boxCatapult.GetComponent<Grid>().AttachedGameItem.GetComponent<Ball>();
                mainscript.Instance.ballShooter.boxCatapult.GetComponent<Grid>().AttachedGameItem = null;
                ball.transform.parent = mainscript.Instance.gameItemsNode.transform;
                ball.tag = "Ball";
                ball.PushBallAFterWin();
            }*/
            yield return new WaitForEndOfFrame();
        }

        foreach (Ball item in balls)
        {
            if (item != null)
                item.StartFall();
        }

        yield return new WaitForSeconds(2f);
        while (GameObject.FindGameObjectsWithTag("Ball").Length > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.aplauds);
        if (PlayerPrefs.GetInt(string.Format("Level.{0:000}.StarsCount", CoreManager.Instance.currentLevel), 0) < CoreManager.Instance.stars)
            PlayerPrefs.SetInt(string.Format("Level.{0:000}.StarsCount", CoreManager.Instance.currentLevel), CoreManager.Instance.stars);


        if (PlayerPrefs.GetInt("HighScore" + CoreManager.Instance.currentLevel) < ScoreManager.Instance.Score)
        {
            PlayerPrefs.SetInt("HighScore" + CoreManager.Instance.currentLevel, ScoreManager.Instance.Score);

        }
        levelClearedGO.SetActive(false);
        menuCompleteGO.SetActive(true);

    }

    void ShowPreTutorial()
    {
        preTutorialGO.SetActive(true);

    }

    IEnumerator LoseAction()
    {
        CoreManager.Instance.ballShooter.SetStageCollidersMode(BallShooter.StageCollidersMode.FireMode);
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.OutOfMoves);
        outOfMovesGO.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        outOfMovesGO.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        menuGameOverGO.SetActive(true);
    }

    //----------------------------------------------------------------------
    // GameManager的外部UI component的回调方法
    //----------------------------------------------------------------------
    public void OnStageMoveComplete()
    {
        if (gameMode == GameMode.Playing)
        {
            if (gameStatus == GameStatus.StageMovingUp)
            {
                PreTutorial();
            }
        }
        else // GameMode.Opening
        {
            Play();
        }
    }
}
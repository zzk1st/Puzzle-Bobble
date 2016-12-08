using UnityEngine;
using System.Collections;
using InitScriptName;


public enum GameStatus
{
    None,
    Playing,
    Pause,
    Win,
    GameOver,
    StageMovingUp,       // Demo模式下播放动画，但不能使用任何操作
    Tutorial,
    PreTutorial,
    BossArriving,
}

/// <summary>
/// Game manager: 主要控制游戏进程，停止／继续，和游戏输赢后的各种动作
///               对GameStatus，我们应该只调用它读取当前状态，而不应该通过其直接修改游戏状态
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameStatus lastGameStatus;
    bool winStarted;

    private GameStatus _gameStatus;
    public GameStatus gameStatus
    {
        get { return _gameStatus; }
    }

	// Use this for initialization
	void Start () 
    {
        Instance = this;
        setGameStatus(GameStatus.None);
	}

    void Update()
    {
    }
	
    public void PreTutorialDone()
    {
        if (mainscript.Instance.levelData.missionType == MissionType.BossBattle)
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
        mainscript.Instance.ballShooter.Initialize();
        setGameStatus(GameStatus.Playing);
    }

    public void Win()
    {
        setGameStatus(GameStatus.Win);
        if( !winStarted )
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
        winStarted = true;
        InitScript.Instance.AddLife( 1 );
        GameObject.Find( "Canvas" ).transform.Find( "LevelCleared" ).gameObject.SetActive( true );
  //       yield return new WaitForSeconds( 1f );
        //if( GameObject.Find( "Music" ) != null)
        //    GameObject.Find( "Music" ).SetActive( false );
        //    GameObject.Find( "CanvasPots" ).transform.Find( "Black" ).gameObject.SetActive( true );
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.winSound );
         yield return new WaitForSeconds( 1f );
        if( mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Vertical )
         {
           //  SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.swish[0] );
           //  GameObject.Find( "Canvas" ).transform.Find( "PreComplete" ).gameObject.SetActive( true );
            yield return new WaitForSeconds( 1f );
            //     SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.swish[0] );
          //  yield return new WaitForSeconds( 1.5f );
            yield return new WaitForSeconds( 0.5f );
         }

        foreach( GameObject item in GameObject.FindGameObjectsWithTag("Ball") )
        {
            if (item.GetComponent<Ball>().state == Ball.BallState.Fixed)
                item.GetComponent<GameItem>().StartFall();
                                   
        }
       // StartCoroutine( PushRestBalls() );
        Transform b = mainscript.Instance.gameItemsNode.transform;
        Ball[] balls = mainscript.Instance.gameItemsNode.GetComponentsInChildren<Ball>();
        foreach( Ball item in balls )
        {
            if (item.GetComponent<Ball>().state == Ball.BallState.Fixed)
                item.StartFall();
        }

        while (mainscript.Instance.levelData.limitAmount >= 0)
        {
            if (mainscript.Instance.ballShooter.CatapultBall != null)
            {
                Ball ball = mainscript.Instance.ballShooter.CatapultBall.GetComponent<Ball>();
                mainscript.Instance.ballShooter.CatapultBall = null;
                ball.transform.parent = mainscript.Instance.gameItemsNode.transform;
                ball.tag = "Ball";
                ball.PushBallAFterWin();
            }
            mainscript.Instance.levelData.limitAmount--;
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

        yield return new WaitForSeconds( 2f );
        while( GameObject.FindGameObjectsWithTag( "Ball" ).Length > 0  )
        {
            yield return new WaitForSeconds( 0.1f );
        }
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.aplauds );
        if( PlayerPrefs.GetInt( string.Format( "Level.{0:000}.StarsCount", mainscript.Instance.currentLevel ),0 ) < mainscript.Instance.stars )
            PlayerPrefs.SetInt( string.Format( "Level.{0:000}.StarsCount", mainscript.Instance.currentLevel ), mainscript.Instance.stars );


        if( PlayerPrefs.GetInt( "HighScore" + mainscript.Instance.currentLevel ) < ScoreManager.Instance.Score )
        {
            PlayerPrefs.SetInt( "HighScore" + mainscript.Instance.currentLevel, ScoreManager.Instance.Score );

        }
        GameObject.Find( "Canvas" ).transform.Find( "LevelCleared" ).gameObject.SetActive( false );
        GameObject.Find( "Canvas" ).transform.Find( "MenuComplete" ).gameObject.SetActive( true );

    }

    void ShowPreTutorial()
    {
        GameObject.Find( "Canvas" ).transform.Find( "PreTutorial" ).gameObject.SetActive( true );

    }

    IEnumerator LoseAction()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.OutOfMoves );
        GameObject.Find( "Canvas" ).transform.Find( "OutOfMoves" ).gameObject.SetActive( true );
        yield return new WaitForSeconds( 1.5f );
        GameObject.Find( "Canvas" ).transform.Find( "OutOfMoves" ).gameObject.SetActive( false );
        yield return new WaitForSeconds( 0.1f );
    }
}
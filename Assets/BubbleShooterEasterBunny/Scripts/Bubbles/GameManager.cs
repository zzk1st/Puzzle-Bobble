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
    Demo,       // Demo模式下播放动画，但不能使用任何操作
    Tutorial,
    PreTutorial
}

/// <summary>
/// Game manager: 主要控制游戏进程，停止／继续，和游戏输赢后的各种动作
///               对GameStatus，我们应该只调用它读取当前状态，而不应该通过其直接修改游戏状态
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameStatus gameStatus;
    private GameStatus lastGameStatus;
    bool winStarted;
    public GameStatus GameStatus
    {
        get { return GameManager.Instance.gameStatus; }
    }
    public int emptyTopGrid;

	// Use this for initialization
	void Start () {
        Instance = this;
        setGameStatus(GameStatus.None);
	}

    void Update()
    {
    }
	
    public void Play()
    {
        setGameStatus(GameStatus.Playing);
    }

    public void Win()
    {
        setGameStatus(GameStatus.Win);
        if( !winStarted )
            StartCoroutine( WinAction ());
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

    public void Demo()
    {
        setGameStatus(GameStatus.Demo);
    }

    public void PreTutorial()
    {
        setGameStatus(GameStatus.PreTutorial);
        ShowPreTutorial();
    }

    void setGameStatus(GameStatus newStatus)
    {
        lastGameStatus = gameStatus;
        gameStatus = newStatus;
    }

    public bool checkWin()
    {
        if (GameManager.Instance.gameStatus != GameStatus.Playing)
        {
            return false;
        }

        // TODO：将来这个要被“拯救动物”，“除掉魔鬼”等游戏模式代替
        if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
        {
            emptyTopGrid = 0;
            for (int i = 0; i < GridManager.Instance.colCount; i++)
            {
                if (GridManager.Instance.Grid(0, i).GetComponent<Grid>().AttachedGameItem == null)
                {
                    emptyTopGrid++;
                }
            }

            if (emptyTopGrid >= 6)
            {
                return true;
            }
        }
        else // 圆形模式
        {
            int animalRow = LevelData.AnimalRow;
            int animalCol = LevelData.AnimalCol;
            if (
                GridManager.Instance.Grid(animalRow-1, animalCol-1).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow-1, animalCol).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow-1, animalCol+1).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow, animalCol-1).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow, animalCol+1).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow+1, animalCol-1).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow+1, animalCol).GetComponent<Grid>().AttachedGameItem == null &&
                GridManager.Instance.Grid(animalRow+1, animalCol+1).GetComponent<Grid>().AttachedGameItem == null
            )
            {
                return true;
            }
        }

        return false;
    }

	// Update is called once per frame
	IEnumerator WinAction () 
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
            item.GetComponent<Ball>().StartFall();
                                   
        }
       // StartCoroutine( PushRestBalls() );
        Transform b = mainscript.Instance.gameItemsNode.transform;
        Ball[] balls = mainscript.Instance.gameItemsNode.GetComponentsInChildren<Ball>();
        foreach( Ball item in balls )
        {
            item.StartFall();
        }

        foreach( Ball item in balls )
        {
            if(item != null)
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
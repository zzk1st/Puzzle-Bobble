using UnityEngine;
using System.Collections;
using InitScriptName;


public enum GameState
{
    Playing,
    Highscore,
    GameOver,
    Pause,
    Win,
    WaitForPopup,
    WaitAfterClose,
    BlockedGame,
    Tutorial,
    PreTutorial,
    WaitForChicken
}


public class GamePlay : MonoBehaviour {
    public static GamePlay Instance;
    private GameState gameStatus;
    bool winStarted;
    public GameState GameStatus
    {
        get { return GamePlay.Instance.gameStatus; }
        set 
        {
            if( GamePlay.Instance.gameStatus != value )
            {
                if( value == GameState.Win )
                {
                    if( !winStarted )
                        StartCoroutine( WinAction ());
                }
                else if( value == GameState.GameOver )
                {
                    StartCoroutine( LoseAction() );
                }
                else if( value == GameState.Tutorial && gameStatus != GameState.Playing )
                {
                    value = GameState.Playing;
                    gameStatus = value;
                  //  ShowTutorial();
                }
                else if( value == GameState.PreTutorial && gameStatus != GameState.Playing )
                {
                    ShowPreTutorial();
                }

            }
            if( value == GameState.WaitAfterClose )
                StartCoroutine( WaitAfterClose() );

            if( value == GameState.Tutorial )
            {
                if( gameStatus != GameState.Playing )
                    GamePlay.Instance.gameStatus = value;

            }
          
                    GamePlay.Instance.gameStatus = value;

        }
    }

	// Use this for initialization
	void Start () {
        Instance = this;
	}

    void Update()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            if( Input.GetKey( KeyCode.W ) ) GamePlay.Instance.GameStatus = GameState.Win;
            if( Input.GetKey( KeyCode.L ) ) { LevelData.LimitAmount = 0; GamePlay.Instance.GameStatus = GameState.GameOver; }
            if( Input.GetKey( KeyCode.D ) ) mainscript.Instance.destroyAllballs() ;
            if( Input.GetKey( KeyCode.M ) ) LevelData.LimitAmount = 1;

        }
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
         if( LevelData.mode == ModeGame.Vertical )
         {
           //  SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.swish[0] );
           //  GameObject.Find( "Canvas" ).transform.Find( "PreComplete" ).gameObject.SetActive( true );
            yield return new WaitForSeconds( 1f );
            GameObject.Find( "CanvasPots" ).transform.Find( "Black" ).gameObject.SetActive( false );
            //     SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.swish[0] );
          //  yield return new WaitForSeconds( 1.5f );
            yield return new WaitForSeconds( 0.5f );
         }

        foreach( GameObject item in GameObject.FindGameObjectsWithTag("Ball") )
        {
            item.GetComponent<ball>().StartFall();
                                   
        }
       // StartCoroutine( PushRestBalls() );
        Transform b = GameObject.Find( "-Ball" ).transform;
        ball[] balls = GameObject.Find( "-Ball" ).GetComponentsInChildren<ball>();
        foreach( ball item in balls )
        {
            item.StartFall();
        }

        while( LevelData.LimitAmount > 0 )
        {
            if( mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy != null )
            {
                LevelData.LimitAmount--;
                ball ball = mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy.GetComponent<ball>();
                mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy = null;
                ball.transform.parent = mainscript.Instance.Balls;
                ball.tag = "Ball";
                ball.PushBallAFterWin();

            }
            yield return new WaitForEndOfFrame();
        }
        foreach( ball item in balls )
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


        if( PlayerPrefs.GetInt( "Score" + mainscript.Instance.currentLevel ) < mainscript.Score )
        {
            PlayerPrefs.SetInt( "Score" + mainscript.Instance.currentLevel, mainscript.Score );

        }
        GameObject.Find( "Canvas" ).transform.Find( "LevelCleared" ).gameObject.SetActive( false );
        GameObject.Find( "Canvas" ).transform.Find( "MenuComplete" ).gameObject.SetActive( true );

    }

    //IEnumerator PushRestBalls()
    //{

    //    while( LevelData.limitAmount  > 0)
    //    {
    //        if( mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy != null )
    //        {
    //            LevelData.limitAmount--;
    //            ball b = mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy.GetComponent<ball>();
    //            mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy = null;
    //            b.transform.parent = mainscript.Instance.Balls;
    //            b.tag = "Ball";
    //            b.PushBallAFterWin();

    //        }
    //        yield return new WaitForEndOfFrame();
    //    }

    //}

    void ShowTutorial()
    {
        //GameObject.Find( "Canvas" ).transform.Find( "Tutorial" ).gameObject.SetActive( true );
        

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
        if(LevelData.LimitAmount <= 0)
            GameObject.Find( "Canvas" ).transform.Find( "MenuPreGameOver" ).gameObject.SetActive( true );
        yield return new WaitForSeconds( 0.1f );

    }

    IEnumerator WaitAfterClose()
    {
        yield return new WaitForSeconds( 1 );
        GameStatus = GameState.Playing;
    }
}

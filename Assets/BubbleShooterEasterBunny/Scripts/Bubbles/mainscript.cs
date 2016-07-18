using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using InitScriptName;


[RequireComponent(typeof(AudioSource))]
public class mainscript : MonoBehaviour {
    public int currentLevel;

	public static mainscript Instance;
	GameObject ball;
    Vector2 speed =                     // Star movement speed / second
        new Vector2(250, 250);          
	GameObject PauseDialogLD;
	GameObject OverDialogLD;
	GameObject PauseDialogHD;
	GameObject OverDialogHD;
	GameObject UI_LD;
	GameObject UI_HD;
	GameObject PauseDialog;
	GameObject OverDialog;
	GameObject FadeLD;
	GameObject FadeHD;
	GameObject AppearLevel;
	Target target;
	Vector2 worldPos;
	Vector2 startPos;
	float startTime;
	float duration = 1.0f;
	bool setTarget;
	float mTouchOffsetX;
	float mTouchOffsetY;
	float xOffset;
	float yOffset;
	public int bounceCounter = 0;
	GameObject[] fixedBalls;
	public Vector2[][] meshArray;
	int offset;
	public GameObject checkBall;
	public GameObject newBall;
	float waitTime = 0f;
	int revertButterFly = 1;
    private static int score;
    private float curFixedBallLocalMinY; // 当前所有fixed balls的最小y值，用来测试关卡是否过线

    public static int Score
    {
        get { return mainscript.score; }
        set { mainscript.score = value; }
    }
	public static int stage = 1;
	const int STAGE_1 = 0;
	const int STAGE_2 = 300;
	const int STAGE_3 = 750;
	const int STAGE_4 = 1400;
	const int STAGE_5 = 2850;
	const int STAGE_6 = 4100;
	const int STAGE_7 = 5500;
	const int STAGE_8 = 6900;
	const int STAGE_9 = 8500;
	public int arraycounter = 0;
	public ArrayList controlArray = new ArrayList();
	public bool dropingDown;
	public bool isPaused;
	public bool noSound;
	public bool gameOver;
	public bool arcadeMode;
	public float topBorder;
	public float leftBorder;
	public float rightBorder;
	public bool hd;
	public GameObject Fade;
	public int highScore;
	public AudioClip pops;
	public AudioClip click;
	public AudioClip levelBells;
	float appearLevelTime;
	public GameObject ElectricLiana;
	public GameObject BonusLiana;
	public GameObject BonusScore;
	public static bool ElectricBoost;
	bool BonusLianaCounter;
	bool gameOverShown;
	public static bool StopControl;
	public GameObject finger;

	public GameObject BoostChanging;

    public CreatorBall creatorBall;

    public GameObject TopBorder;
    public Transform Balls;
    public Hashtable animTable = new Hashtable();
    public static Vector3 lastBall;
    public GameObject FireEffect;
    public BallShooter ballShooter;
    public static int doubleScore=1;
    
    public int TotalTargets;

    public int countOfPreparedToDestroy;

    public int potSounds;

    public static Dictionary<int, BallColor> colorsDict = new Dictionary<int, BallColor>();

    public float StageBounceForce;

    private int _ComboCount;

    public int ComboCount
    {
        get { return _ComboCount; }
        set 
        { 
            _ComboCount = value;
            if( value > 0 )
            {
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.combo[Mathf.Clamp(value-1, 0 , 5)]);
                if( value >= 6 )
                {
                    SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.combo[5]);
                    FireEffect.SetActive( true );
                    doubleScore = 2;
                }
            }
            else
            {
                FireEffect.SetActive( false );
                doubleScore = 1;
            }
        }
    }

    public GameObject popupScore;

    private int TargetCounter;

    public int TargetCounter1
    {
        get { return TargetCounter; }
        set {
            TargetCounter = value;
        }
    }

    public GameObject[] starsObject;
    public int stars = 0;

    public GameObject perfect;

    public GameObject[] boosts;
    public GameObject[] locksBoosts;

    public GameObject arrows;
    private int maxCols;
    private int maxRows;
    private LIMIT limitType;
    private int limit;
    private int colorLimit;
    private GameObject bottomBorder;
    private GameObject meshes;

    //	public int[][] meshMatrix = new int[15][17];
    // Use this for initialization

    void Awake(){
        if( InitScript.Instance == null ) gameObject.AddComponent<InitScript>();


        currentLevel = PlayerPrefs.GetInt( "OpenLevel", 1 );
		stage = 1;
		mainscript.StopControl = false;
        animTable.Clear();
//		arcadeMode = InitScript.Arcade;
		if(Application.platform == RuntimePlatform.WindowsEditor){
			//SwitchLianaBoost();
			//arcadeMode = true;
		}

        creatorBall = GameObject.Find("Creator").GetComponent<CreatorBall>();
        bottomBorder = GameObject.Find("BottomBorder");
        meshes = GameObject.Find("-Meshes");

        ballShooter = GameObject.Find("BallShooter").GetComponent<BallShooter>();

		StartCoroutine( CheckColors());

	}

	IEnumerator CheckColors ()
	{
		while(true){
		GetColorsInGame();
		yield return new WaitForEndOfFrame();
		SetColorsForNewBall();
		}

	}

    IEnumerator ShowArrows()
    {
        while( true )
        {

            yield return new WaitForSeconds( 30 );
            if( GamePlay.Instance.GameStatus == GameState.Playing )
            {
                arrows.SetActive( true );

            }
                yield return new WaitForSeconds( 3 );
                arrows.SetActive( false );
        }
    }

    public void PopupScore(int value, Vector3 pos)
    {
        Score += value;
        Transform parent = GameObject.Find( "CanvasScore" ).transform;
        GameObject poptxt = Instantiate( popupScore, pos, Quaternion.identity ) as GameObject;
        poptxt.transform.GetComponentInChildren<Text>().text = "" + value;
        poptxt.transform.SetParent( parent );
        poptxt.transform.localScale = Vector3.one;
        Destroy( poptxt, 1 );
    }

	public void SwitchLianaBoost(){
		if(!ElectricBoost){
			ElectricBoost = true;
			ElectricLiana.SetActive(true);
		}
		else{
			ElectricBoost = false;
			ElectricLiana.SetActive(false);
		}
	}

    void Start()
    {
        Instance = this;

        RandomizeWaitTime();
        score = 0;
        if (PlayerPrefs.GetInt("noSound") == 1) noSound = true;
        //		if(PlayerPrefs.GetInt("arcade")==1){ arcadeMode = true; 		highScore = PlayerPrefs.GetInt("scoreArcade");}
        //		if(PlayerPrefs.GetInt("arcade")==0){ arcadeMode = false;		highScore = PlayerPrefs.GetInt("score");}

        GamePlay.Instance.GameStatus = GameState.BlockedGame;
        //		GameObject.Find("GUIHighscore").GetComponent<GUIText>().text = "High Score: " + highScore+"";
    }
	
    void ConnectAndDestroyBalls()
    {
        // 游戏中最重要的算法部分：检测ball是否连上，销毁，以及判断是否有其它drop的balls
        // checkBall在ball.cs中被赋值，当ball停住的时候，就说明需要判断连接了，这个值也就被设定了
        if (checkBall != null && GamePlay.Instance.GameStatus == GameState.Playing)
        {
            // 找到同色的ball并将其销毁
            checkNearestColorAndDelete(checkBall);
            StartCoroutine(destroyAloneBalls());
            checkBall = null;
        }
    }

	// Update is called once per frame
	void Update ()
    {
        CheckLosing();

		if (noSound)
			GetComponent<AudioSource>().volume = 0;
		if (!noSound)
			GetComponent<AudioSource>().volume = 0.5f;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            //			GameObject.Find("PauseButton").GetComponent<clickButton>().OnMouseDown();
        }

        ConnectAndDestroyBalls();

        if ( LevelData.mode == ModeGame.Vertical && TargetCounter >= 6 && GamePlay.Instance.GameStatus == GameState.Playing )
        {
            GamePlay.Instance.GameStatus = GameState.Win;
        }
        else if ( LevelData.mode == ModeGame.Rounded && TargetCounter >= 1 && GamePlay.Instance.GameStatus == GameState.WaitForChicken )
            GamePlay.Instance.GameStatus = GameState.Win;
        else if ( LevelData.mode == ModeGame.Animals && TargetCounter >= TotalTargets && GamePlay.Instance.GameStatus == GameState.Playing )
            GamePlay.Instance.GameStatus = GameState.Win;

        ProgressBarScript.Instance.UpdateDisplay( (float)score * 100f / ( (float)LevelData.star1 / ( ( LevelData.star1 * 100f / LevelData.star3 ) ) * 100f ) / 100f );

        if ( score >= LevelData.star1 && stars <= 0 )
        {
            stars = 1;
        }
        if ( score >= LevelData.star2 && stars <= 1 )
        {
            stars = 2;
        }
        if ( score >= LevelData.star3 && stars <= 2 )
        {
            stars = 3;
        }

        if ( score >= LevelData.star1 )
        {
            starsObject[0].SetActive( true );
        }
        if ( score >= LevelData.star2 )
        {
            starsObject[1].SetActive( true );
        }
        if ( score >= LevelData.star3 )
        {
            starsObject[2].SetActive( true );
        }
        
	}

	public void CheckLosing()
    {
        if (GamePlay.Instance.GameStatus == GameState.Playing)
        {
            float stageMinYWorldSpace = meshes.transform.position.y + curFixedBallLocalMinY - creatorBall.BallColliderRadius;
            if (stageMinYWorldSpace < bottomBorder.transform.position.y)
            {
                // TODO: 如何结束游戏？
                GamePlay.Instance.GameStatus = GameState.GameOver;
            }
        }
    }

	public void connectNearBallsGlobal()
    {
		fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in fixedBalls)
        {
			if(obj.layer == LayerMask.NameToLayer("Ball"))  // 只能是ball layer里的，falling balls被排除
            {
                obj.GetComponent<Ball>().connectNearbyBalls();
            }
		}
	}

    public void dropUp()
    {
        if (!dropingDown)
        {
            creatorBall.AddMesh();
            dropingDown = true;
            GameObject meshes = GameObject.Find("-Meshes");
            iTween.MoveAdd(meshes, iTween.Hash("y", 0.5f, "time", 0.3, "easetype", iTween.EaseType.linear, "onComplete", "OnMoveFinished"));

        }
  
    }

    void OnMoveFinished()
    {
        dropingDown = false;
    }

	public void explode(GameObject gameObject){
		//gameObject.GetComponent<Detonator>().Explode();
	}
	
	void RandomizeWaitTime()
	{
	    const float minimumWaitTime = 5f;
	    const float maximumWaitTime = 10f;
	    waitTime = Time.time + Random.Range(minimumWaitTime, maximumWaitTime);
	}
	
	public IEnumerator destroyAloneBalls()
    {
        yield return new WaitForSeconds( Mathf.Clamp( (float)countOfPreparedToDestroy / 50, 0.6f, (float)countOfPreparedToDestroy / 50 ) );
            int willDestroy = 0;
			Camera.main.GetComponent<mainscript>().arraycounter = 0;
			GameObject[] fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];			// detect alone balls
			Camera.main.GetComponent<mainscript>().controlArray.Clear();
			foreach(GameObject obj in fixedBalls) {
				if(obj!=null){
					if(obj.layer == 9){

						if(!findInArray(Camera.main.GetComponent<mainscript>().controlArray, obj.gameObject) ){
                            if(obj.GetComponent<Ball>().nearbyBalls.Count<7 && obj.GetComponent<Ball>().nearbyBalls.Count>0){
                                yield return new WaitForEndOfFrame();
								ArrayList b = new ArrayList();
								// 详见checkNearestBall的注释
								obj.GetComponent<Ball>().checkNearestBall(b);
								if(b.Count >0 ){
                                    willDestroy++;
									// 删掉ball，并调用StartFall让ball掉落
									DropBalls(b);
								}
							}
						}
					}	
				}
			}

        // 当所有ball掉落之后，很多nearby balls发生改变，重新连接nearby balls
        connectNearBallsGlobal();
		StartCoroutine(creatorBall.connectAllBallsToMeshes());
		dropingDown = false;

        yield return new WaitForSeconds( 0.0f );
		// 下面这几步是为了让新产生的球不再有以前没有出现过的颜色
        GetColorsInGame();
        mainscript.Instance.newBall = null;
        SetColorsForNewBall();
	}

    public void SetColorsForNewBall()
    {
        // TODO: 重新使用这行代码
        /*
        GameObject ball = null;
        if( boxCatapult.GetComponent<Grid>().Busy != null && colorsDict .Count>0)
        {
            ball = boxCatapult.GetComponent<Grid>().Busy;
            BallColor color = ball.GetComponent<ColorBallScript>().mainColor;
            if( !colorsDict.ContainsValue( color ) )
            {
                ball.GetComponent<ColorBallScript>().SetColor( (BallColor)mainscript.colorsDict[Random.Range( 0, mainscript.colorsDict.Count )] ); 
            }
        }
        */
    }

    public void GetColorsInGame()
    {
        int i = 0;
        colorsDict.Clear();
        foreach( Transform item in Balls )
        {
            if( item.tag == "chicken" || item.tag == "empty" || item.tag == "Ball" ) continue;
            BallColor col = (BallColor)System.Enum.Parse( typeof( BallColor ), item.tag );
            if( !colorsDict.ContainsValue( col ) && (int)col <= (int) BallColor.random)
            {
                colorsDict.Add(i, col );
                i++;
            }
        }
    }

	public bool findInArray(ArrayList b, GameObject destObj)
    {
		foreach(GameObject obj in b) {
			
			if(obj == destObj) return true;
		}
		return false;
	}
	
	void playPop(){
	//	if(!Camera.main.GetComponent<mainscript>().noSound) SoundBase.Instance.audio.PlayOneShot(SoundBase.Instance.Pops);
			//AudioSource.PlayClipAtPoint(pops, transform.position);
	}
	
    // DropBalls, 注意和DestroyBalls并不相同，后者是让球爆炸，这个是让球落下
	public void DropBalls(ArrayList b)
    {
		Camera.main.GetComponent<mainscript>().bounceCounter = 0;
		int scoreCounter = 0;
		int rate = 0;

		foreach(GameObject obj in b) {
//			obj.GetComponent<OTSprite>().collidable = false;
			if(obj.name.IndexOf("ball")==0) obj.layer = 0;

			if(scoreCounter > 3){
				rate +=3;
				scoreCounter += rate;
			}
			scoreCounter ++;

			// 让没接上的ball都掉落
            obj.GetComponent<Ball>().StartFall();
		}
        UpdateLocalMinYFromAllFixedBalls();
	//	Score.Instance.addScore( scoreCounter);

	}

    public void UpdateLocalMinYFromSingleBall(Ball fixedBall)
    {
        // 我们在这用localMeshPos而不用localPosition, 因为我们在coroutine里，localPosition可能因为动画改变，而localMeshPos更稳定
        if (fixedBall.LocalMeshPos.y < curFixedBallLocalMinY)
        {
            curFixedBallLocalMinY = fixedBall.LocalMeshPos.y;
        }
    }

    public void UpdateLocalMinYFromAllFixedBalls()
    {
        curFixedBallLocalMinY = 9999f;
        GameObject fixedBalls = GameObject.Find( "-Ball" );

        foreach( Transform item in fixedBalls.transform )
        {
            GameObject fixedBall = item.gameObject;
            if (fixedBall.GetComponent<CircleCollider2D>().enabled)
            {
                UpdateLocalMinYFromSingleBall(fixedBall.GetComponent<Ball>());
            }
        }
        //Debug.Log(string.Format("MinY recalculated! MinY={0}", curFixedBallLocalMinY));
    }

    public void checkNearestColorAndDelete(GameObject checkBallGO)
    {
        // 该方法用来查找是否有其它ball与之相连，形成三个或以上的ball，如果有则将其销毁
        Ball checkBall = checkBallGO.GetComponent<Ball>();

        ArrayList ballsToDelete = new ArrayList ();
        ballsToDelete.Add (checkBallGO);
        checkBall.checkNextNearestColor(ballsToDelete);
        mainscript.Instance.countOfPreparedToDestroy = ballsToDelete.Count;

        if (ballsToDelete.Count >= 3)
        {
            mainscript.Instance.ComboCount++;
            // 在这里调用coroutine将其销毁
            DestroyBalls(ballsToDelete, 0.00001f);

            // 给整个关卡一个向上的force
            GameObject Meshes = GameObject.Find( "-Meshes" );
            Rigidbody2D rb = Meshes.GetComponent<Rigidbody2D>();
            rb.AddForce(Vector2.up * mainscript.Instance.StageBounceForce);
        }
        if (ballsToDelete.Count < 3)
        {
            Camera.main.GetComponent<mainscript> ().bounceCounter++;
            mainscript.Instance.ComboCount = 0;
        }

        Camera.main.GetComponent<mainscript> ().dropingDown = false;
    }

    // TODO: refactor this destroy method with the one in mainscript.cs
    public void DestroyBalls(ArrayList balls, float speed = 0.1f)
    {
        StartCoroutine(DestroyCor(balls, speed));
    }

    IEnumerator DestroyCor(ArrayList balls, float speed = 0.1f)
    {
        Camera.main.GetComponent<mainscript> ().bounceCounter = 0;
        int scoreCounter = 0;
        int rate = 0;
        int soundPool = 0;
        foreach (GameObject ballGO in balls)
        {
            Ball ball = ballGO.GetComponent<Ball>();
            ball.DisconnectFromCurrentGrid();   // 从meshes里删除引用
            ballGO.layer = LayerMask.NameToLayer("ExplodedBall");   // 从ball layer移除，防止之后connect nearball时候再连上
            ballGO.GetComponent<CircleCollider2D>().enabled = false;    //删掉CircleCollider，防止再碰撞检测

            // 让ball爆炸
            ball.Explode();

            soundPool++;
            if (scoreCounter > 3) {
                rate += 10;
                scoreCounter += rate;
            }
            scoreCounter += 10;
            if (balls.Count > 10 && Random.Range(0, 10) > 5)
                mainscript.Instance.perfect.SetActive(true);

            if (balls.Count < 10 || soundPool % 20 == 0)
                yield return new WaitForSeconds (speed);
        }
        mainscript.Instance.PopupScore(scoreCounter, transform.position);
        mainscript.Instance.UpdateLocalMinYFromAllFixedBalls();
    }
}



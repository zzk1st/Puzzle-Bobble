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
 	int stageTemp;
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

        //	AdmobAd.Instance().LoadInterstitialAd(true);
        //		audio.PlayClipAtPoint(ambient, new Vector3(5, 1, 2));
        //audio.loop = true;
        //		if(DisplayMetricsAndroid.WidthPixels>700){
        //			hd = true;
        //		}
        stageTemp = 1;
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
        if( checkBall != null &&( GamePlay.Instance.GameStatus == GameState.Playing || GamePlay.Instance.GameStatus == GameState.WaitForChicken ))
        {
            // 找到同色的ball并将其销毁
            checkBall.GetComponent<Ball>().checkNearestColorAndDelete();
            Destroy(checkBall.GetComponent<Rigidbody>());
            checkBall = null;
            //connectNearBallsGlobal();
            int missCount = 1;
            if(stage >= 3) missCount = 2;
            if(stage >= 9) missCount = 1;
            //Invoke("destroyAloneBall", 0.5f);
            StartCoroutine( destroyAloneBall() );

            if(!arcadeMode){
                if (bounceCounter >= missCount)
                {
                    bounceCounter = 0;
                    //Invoke("dropUp", 0.1f);
                }
            }
        }
    }

	// Update is called once per frame
	void Update () {
        CheckLosing();

		if(noSound)
			GetComponent<AudioSource>().volume = 0;
		if(!noSound)
			GetComponent<AudioSource>().volume = 0.5f;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            //			GameObject.Find("PauseButton").GetComponent<clickButton>().OnMouseDown();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
           // creatorBall.AddMesh();

        }
		if(gameOver && !gameOverShown){
			gameOverShown = true;

		//	return;
		}

        if( GamePlay.Instance.GameStatus == GameState.Win )
        {

         //   return;
        }

        ConnectAndDestroyBalls();

        if( LevelData.mode == ModeGame.Vertical && TargetCounter >= 6 && GamePlay.Instance.GameStatus == GameState.Playing )
        {
            GamePlay.Instance.GameStatus = GameState.Win;
        }
        else if( LevelData.mode == ModeGame.Rounded && TargetCounter >= 1 && GamePlay.Instance.GameStatus == GameState.WaitForChicken )
            GamePlay.Instance.GameStatus = GameState.Win;
        else if( LevelData.mode == ModeGame.Animals && TargetCounter >= TotalTargets && GamePlay.Instance.GameStatus == GameState.Playing )
            GamePlay.Instance.GameStatus = GameState.Win;

        ProgressBarScript.Instance.UpdateDisplay( (float)score * 100f / ( (float)LevelData.star1 / ( ( LevelData.star1 * 100f / LevelData.star3 ) ) * 100f ) / 100f );

        if( score >= LevelData.star1 && stars <= 0 )
        {
            stars = 1;
        }
        if( score >= LevelData.star2 && stars <= 1 )
        {
            stars = 2;
        }
        if( score >= LevelData.star3 && stars <= 2 )
        {
            stars = 3;
        }

        if( score >= LevelData.star1 )
        {
            starsObject[0].SetActive( true );
        }
        if( score >= LevelData.star2 )
        {
            starsObject[1].SetActive( true );
        }
        if( score >= LevelData.star3 )
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

	IEnumerator startBonusLiana(){
		while(true){
			yield return new WaitForSeconds(Random.Range(30,120));
			Instantiate(BonusLiana);
		}
	}

	IEnumerator startBonusScore(){
		while(true){
			yield return new WaitForSeconds(Random.Range(5,20));
			Instantiate(BonusScore);
		}
	}
	
	IEnumerator startButterfly(){
		while(true){
			yield return new WaitForSeconds(Random.Range(5,10));
			GameObject gm = GameObject.Find ("Creator");
			revertButterFly *=-1;
			gm.GetComponent<creatorButterFly>().createButterFly(revertButterFly);
		}

	}
	
	public void connectNearBallsGlobal(){
		///connect near balls
		fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in fixedBalls) {
			// layer 9就是ball
			if(obj.layer == 9)
				obj.GetComponent<Ball>().connectNearBalls();
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

	public void dropDown(){

		dropingDown = true;
		fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in fixedBalls) {
			if(obj.layer == 9)
				obj.GetComponent<bouncer>().dropDown();
		}
        creatorBall.createRow(0);
	//	Invoke("destroyAloneBall", 1f);
	//	destroyAloneBall();
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
	
	public IEnumerator destroyAloneBall(){
        //if( GamePlay.Instance.GameStatus == GameState.Playing )
        //    mainscript.Instance.newBall = null;
        yield return new WaitForSeconds( Mathf.Clamp( (float)countOfPreparedToDestroy / 50, 0.6f, (float)countOfPreparedToDestroy / 50 ) );
 //       yield return new WaitForSeconds( 0.6f );
		int i;
	//	while(true){
			// 剔除掉所有balls，第一步先连接所有的ball
			connectNearBallsGlobal();
			i=0;
            int willDestroy = 0;
			Camera.main.GetComponent<mainscript>().arraycounter = 0;
			GameObject[] fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];			// detect alone balls
			Camera.main.GetComponent<mainscript>().controlArray.Clear();
			foreach(GameObject obj in fixedBalls) {
				if(obj!=null){
					if(obj.layer == 9){

						if(!findInArray(Camera.main.GetComponent<mainscript>().controlArray, obj.gameObject) ){
							if(obj.GetComponent<Ball>().nearBalls.Count<7 && obj.GetComponent<Ball>().nearBalls.Count>0){
												i++;
							//	if(i>5){ i = 0; yield return new WaitForSeconds(0.5f); yield return new WaitForSeconds(0.5f);}
						//		if(dropingDown) yield return new WaitForSeconds(1f);
                                                yield return new WaitForEndOfFrame();
								ArrayList b = new ArrayList();
								// 详见checkNearestBall的注释
								obj.GetComponent<Ball>().checkNearestBall(b);
								if(b.Count >0 ){
                                    willDestroy++;
									// 删掉ball，并调用StartFall让ball掉落
									destroy (b);
								}
							}
						}
					}	
				}
			}

		StartCoroutine(creatorBall.connectAllBallsToMeshes());
		dropingDown = false;
        //if( willDestroy > 0)
        //    yield return new WaitForSeconds( 0.5f );

        if( LevelData.mode == ModeGame.Rounded )
        {
            CheckBallsBorderCross();
        }

        yield return new WaitForSeconds( 0.0f );
		// 下面这几步是为了让新产生的球不再有以前没有出现过的颜色
        GetColorsInGame();
        mainscript.Instance.newBall = null;
        SetColorsForNewBall();

		//	Debug.Log(i);
	//		yield return new WaitForSeconds(2f);
	//	}
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

    public void CheckFreeChicken()
    {
        if( LevelData.mode != ModeGame.Rounded ) return;
        if(GamePlay.Instance.GameStatus == GameState.Playing)
            StartCoroutine( CheckFreeChickenCor() );
    }

    IEnumerator CheckFreeChickenCor()
    {
        //  yield return new WaitForSeconds( Mathf.Clamp( (float)countOfPreparedToDestroy / 100, 1.5f, (float)countOfPreparedToDestroy / 100 ) );
        GamePlay.Instance.GameStatus = GameState.WaitForChicken;
        yield return new WaitForSeconds( 1.5f );
        bool finishGame = false;
        if( LevelData.mode == ModeGame.Rounded )
        {
            finishGame = true;

            GameObject balls = GameObject.Find( "-Ball" );
//            print( "check free chicken " +  balls.transform.childCount);
            foreach( Transform item in balls.transform )
            {
                if( item.tag != "Ball" && item.tag != "chicken" )
                {
                    finishGame = false;
                }
            }
        }
        if( !finishGame )
        {
            GetColorsInGame();

            GamePlay.Instance.GameStatus = GameState.Playing;
        }

        else if( finishGame )
        {
            GamePlay.Instance.GameStatus = GameState.WaitForChicken;

            GameObject chicken = GameObject.FindGameObjectWithTag( "chicken" );
            chicken.GetComponent<SpriteRenderer>().sortingLayerName = "UI layer";
            Vector3 targetPos = new Vector3( 2.3f, 6, 0 );
            mainscript.Instance.TargetCounter++;
            AnimationCurve curveX = new AnimationCurve( new Keyframe( 0, chicken.transform.position.x ), new Keyframe( 0.5f, targetPos.x ) );
            AnimationCurve curveY = new AnimationCurve( new Keyframe( 0, chicken.transform.position.y ), new Keyframe( 0.5f, targetPos.y ) );
            curveY.AddKey( 0.2f, chicken.transform.position.y - 1 );
            float startTime = Time.time;
            Vector3 startPos = chicken.transform.position;
            float speed = 0.2f;
            float distCovered = 0;
            while( distCovered < 0.6f )
            {
                distCovered = ( Time.time - startTime );
                chicken.transform.position = new Vector3( curveX.Evaluate( distCovered ), curveY.Evaluate( distCovered ), 0 );
                chicken.transform.Rotate( Vector3.back * 10 );
                yield return new WaitForEndOfFrame();
            }
            Destroy( chicken );

            //if(chicken.GetComponent<SmoothMove>() == null)
            //    chicken.AddComponent<SmoothMove>();
        }
    }


    void CheckBallsBorderCross()
    {
        foreach( Transform item in Balls )
        {
            item.GetComponent<Ball>().CheckBallCrossedBorder();
        }
    }

 

	public bool findInArray(ArrayList b, GameObject destObj){
		foreach(GameObject obj in b) {
			
			if(obj == destObj) return true;
		}
		return false;
	}
	
	public void destroy( GameObject obj){
		if(obj.name.IndexOf("ball")==0) obj.layer = 0;
		Camera.main.GetComponent<mainscript>().bounceCounter = 0;
	//	obj.GetComponent<OTSprite>().collidable = false;
	//	Destroy(obj);
        obj.GetComponent<Ball>().Destroyed = true;
		obj.GetComponent<Ball>().growUp();
		Camera.main.GetComponent<mainscript>().explode(obj.gameObject);
	//	Score.Instance.addScore( 3);

	}
	
	void playPop(){
	//	if(!Camera.main.GetComponent<mainscript>().noSound) SoundBase.Instance.audio.PlayOneShot(SoundBase.Instance.Pops);
			//AudioSource.PlayClipAtPoint(pops, transform.position);
	}
	
	public void destroy( ArrayList b){
		Camera.main.GetComponent<mainscript>().bounceCounter = 0;
		int scoreCounter = 0;
		int rate = 0;
		int soundPool=0;

		foreach(GameObject obj in b) {
//			obj.GetComponent<OTSprite>().collidable = false;
			if(obj.name.IndexOf("ball")==0) obj.layer = 0;
            if(!obj.GetComponent<Ball>().Destroyed){
                //if(soundPool<5)
                //    obj.GetComponent<ball>().growUpPlaySound();
                //else
                //    obj.GetComponent<ball>().growUp();
		//		soundPool++;
				if(scoreCounter > 3){
					rate +=3;
					scoreCounter += rate;
				}
				scoreCounter ++;
					// 让没接上的ball都掉落
                    obj.GetComponent<Ball>().StartFall();
			//	Destroy(obj);
			//	obj.GetComponent<ball>().destroyed = true;
			//	Camera.main.GetComponent<mainscript>().explode(obj.gameObject);
			}
		}
        CheckFreeChicken();
        UpdateLocalMinYFromAllFixedBalls();
	//	Score.Instance.addScore( scoreCounter);

	}

    public void UpdateLocalMinYFromSingleBall(Ball fixedBall)
    {
        // 我们在这用localMeshPos而不用localPosition, 因为我们在coroutine里，localPosition可能因为动画改变，而localMeshPos更稳定
        if (!fixedBall.Destroyed && fixedBall.LocalMeshPos.y < curFixedBallLocalMinY)
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
            Ball fixedBall = item.gameObject.GetComponent<Ball>();
            UpdateLocalMinYFromSingleBall(fixedBall);
        }
        //Debug.Log(string.Format("MinY recalculated! MinY={0}", curFixedBallLocalMinY));
    }

    public void destroyAllballs()
    {
        foreach( Transform item in Balls )
        {
            if( item.tag != "chicken" )
            {

                destroy( item.gameObject );
            }
        }
            CheckFreeChicken();
    }
}



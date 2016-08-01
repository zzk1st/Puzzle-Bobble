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
    private float curFixedBallLocalMinY; // 当前所有fixed balls的最小y值，用来测试关卡是否过线

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
	public ArrayList controlArray = new ArrayList();
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

    public GameObject TopBorder;
    public Transform Balls;
    public Hashtable animTable = new Hashtable();
    public GameObject FireEffect;
    private BallShooter _ballShooter;
    //private ScoreManager _scoreManager;
    public BallShooter ballShooter
    {
        get { return _ballShooter; }
    }
    /*
    public ScoreManager scoreManager
    {
        get { return _scoreManager; }
    }
    */

    public static int doubleScore=1;
    private PlatformController _platformController;
    public PlatformController platformController
    {
        get { return _platformController; }
    }

    private GridManager _gridManager;
    public GridManager gridManager
    {
        get { return _gridManager; }
    }
    
    public int TotalTargets;

    public int countOfPreparedToDestroy;

    public static Dictionary<int, BallColor> colorsDict = new Dictionary<int, BallColor>();

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

    void Awake()
    {
        Instance = this;
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


		StartCoroutine(CheckColors());
	}

    void Start()
    {
        meshes = GameObject.Find("-Grids");
        _ballShooter = GameObject.Find("BallShooter").GetComponent<BallShooter>();
        _platformController = meshes.GetComponent<PlatformController>();
        _gridManager = meshes.GetComponent<GridManager>();
        bottomBorder = GameObject.Find("BottomBorder");

        //RandomizeWaitTime();
        ScoreManager.Score = 0;
        if (PlayerPrefs.GetInt("noSound") == 1) noSound = true;
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
            if( GameManager.Instance.GameStatus == GameStatus.Playing )
            {
                arrows.SetActive( true );

            }
                yield return new WaitForSeconds( 3 );
                arrows.SetActive( false );
        }
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

    void ConnectAndDestroyBalls()
    {
        // 游戏中最重要的算法部分：检测ball是否连上，销毁，以及判断是否有其它drop的balls
        // checkBall在ball.cs中被赋值，当ball停住的时候，就说明需要判断连接了，这个值也就被设定了
        if (checkBall != null && GameManager.Instance.GameStatus == GameStatus.Playing)
        {
            // 找到同色的ball并将其销毁
            checkNearestColorAndDelete(checkBall);
            StartCoroutine(DestroyAloneBalls());

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

        if ( LevelData.mode == ModeGame.Vertical && TargetCounter >= 6 && GameManager.Instance.GameStatus == GameStatus.Playing )
        {
            GameManager.Instance.Win();
        }


        //计算进度条应显示当前分数占最高级别（三星）的百分之多少
        ProgressBarScript.Instance.UpdateDisplay((float)ScoreManager.Score / LevelData.stars[2]);

        //更新星星个数
        if (stars < 3)
            stars += Mathf.Min(1, ScoreManager.Score / LevelData.stars[stars]);
        for (int i = 0; i < stars; ++i)
            starsObject[i].SetActive(true);
        
	}

	public void CheckLosing()
    {
        if (GameManager.Instance.GameStatus == GameStatus.Playing)
        {
            float stageMinYWorldSpace = platformController.curPlatformMinY;
            if (stageMinYWorldSpace < bottomBorder.transform.position.y)
            {
                // TODO: 如何结束游戏？
                GameManager.Instance.GameOver();
            }
        }
    }

	public IEnumerator DestroyAloneBalls()
    {
		GameObject[] fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];			// detect alone balls
        mainscript.Instance.controlArray.Clear();

        ArrayList ballsToDrop = new ArrayList();
		foreach(GameObject obj in fixedBalls)
        {
            if (obj.layer == LayerMask.NameToLayer("FixedBall") && !findInArray(ballsToDrop, obj))
            {
                if (!findInArray(mainscript.Instance.controlArray, obj.gameObject))
                {
					ArrayList b = new ArrayList();
					// 详见checkNearestBall的注释
					obj.GetComponent<Ball>().checkNearestBall(b);
					if(b.Count >0 )
                    {
                        ballsToDrop.AddRange(b);
					}
				}
			}	
		}

        yield return new WaitForEndOfFrame();
        // 删掉ball，并调用StartFall让ball掉落
        if (ballsToDrop.Count > 0)
        {
            DropBalls(ballsToDrop);
            platformController.BallRemovedFromPlatform();
        }

		// 下面这几步是为了让新产生的球不再有以前没有出现过的颜色
        GetColorsInGame();
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
	
    // DropBalls, 注意和DestroyBalls并不相同，后者是让球爆炸，这个是让球落下
	public void DropBalls(ArrayList ballsToDrop)
    {
		Camera.main.GetComponent<mainscript>().bounceCounter = 0;

        // 这里的score累加似乎没用 先注释了
		/*int scoreCounter = 0;
		int rate = 0;*/

        foreach(GameObject ball in ballsToDrop) {
			/*if(scoreCounter > 3){
				rate +=3;
				scoreCounter += rate;
			}
			scoreCounter ++;*/

			// 让没接上的ball都掉落
            ball.GetComponent<Ball>().StartFall();
		}
        // 调用ScoreManager里针对掉落球的分数更新函数
        int val = ScoreManager.Instance.UpdateFallingScore(ballsToDrop.Count);

        //ScoreManager.Instance.PopupComboScore(val, transform.position);
    }

    public void checkNearestColorAndDelete(GameObject checkBallGO)
    {
        // 该方法用来查找是否有其它ball与之相连，形成三个或以上的ball，如果有则将其销毁
        Ball checkBall = checkBallGO.GetComponent<Ball>();

        ArrayList ballsToDelete = new ArrayList();
        ballsToDelete.Add(checkBallGO);
        checkBall.checkNextNearestColor(ballsToDelete);
        mainscript.Instance.countOfPreparedToDestroy = ballsToDelete.Count;

        if (ballsToDelete.Count >= 3)
        {
            ScoreManager.Instance.ComboCount++;
            // 在这里调用coroutine将其销毁
            DestroyBalls(ballsToDelete, 0.00001f);

            platformController.BallRemovedFromPlatform();
        }
        if (ballsToDelete.Count < 3)
        {
            Camera.main.GetComponent<mainscript> ().bounceCounter++;
            ScoreManager.Instance.ComboCount = 0;
        }
    }

    void DestroyBalls(ArrayList balls, float speed = 0.1f)
    {
        Camera.main.GetComponent<mainscript> ().bounceCounter = 0;
        int soundPool = 0;
        foreach (GameObject ballGO in balls)
        {
            Ball ball = ballGO.GetComponent<Ball>();
            ball.grid.DisonnectBall();
            ballGO.layer = LayerMask.NameToLayer("ExplodedBall");   // 从ball layer移除，防止之后connect nearball时候再连上
            ballGO.GetComponent<CircleCollider2D>().enabled = false;    //删掉CircleCollider，防止再碰撞检测

            // 让ball爆炸
            ball.Explode();

            soundPool++;
        }
        //调用ScoreManager里爆炸球的分数更新函数
        int val = ScoreManager.Instance.UpdateComboScore(balls.Count);

        ScoreManager.Instance.PopupComboScore(val, transform.position);
        mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
    }
}



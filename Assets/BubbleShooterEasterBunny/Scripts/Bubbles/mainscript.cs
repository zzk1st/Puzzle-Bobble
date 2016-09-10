using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using InitScriptName;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class mainscript : MonoBehaviour {
    public int currentLevel;
    public StageMoveMode currentGameMode;
    public LevelData levelData = new LevelData();
    public int minConsecutiveNumberCount;

	public static mainscript Instance;
	GameObject ball;
	public int bounceCounter = 0;
	public GameObject checkBall;
    public Color currentBallShooterColor = new Color(1,0,0,1);

    public Color[] BallRGB = new[] { new Color(24 / 255f, 121 / 255f, 1, 1),
                             new Color(19 / 255f, 161 / 255f, 30 / 255f, 1),
                             new Color(224 / 255f, 52 / 255f, 0, 1),
                             new Color(179 / 255f, 0, 222 / 255f, 1),
                             new Color(188 / 255f, 62 / 255f, 0, 1)};

    public List<GameObject> controlGrids = new List<GameObject>();
	public bool isPaused;
	public bool noSound;
	public bool gameOver;
	public GameObject ElectricLiana;
	public static bool ElectricBoost;
	public static bool StopControl;

    public GameObject TopBorder;
    public Hashtable animTable = new Hashtable();
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

    private PlatformController _platformController;
    public PlatformController platformController
    {
        get { return _platformController; }
    }

    public int countOfPreparedToDestroy;

    public static List<BallColor> curStageColors = new List<BallColor>();

    private int TargetCounter;

    public int TargetCounter1
    {
        get { return TargetCounter; }
        set {
            TargetCounter = value;
        }
    }

    public GameObject targetStarPrefab;

    public GameObject[] starsObject;
    public int stars = 0;

    public GameObject[] boosts;
    public GameObject[] locksBoosts;
    public GameObject FireEffect;

    public GameObject arrows;
    private int maxCols;
    private int maxRows;
    private LIMIT limitType;
    private int limit;
    private int colorLimit;
    public GameObject gridsNode;
    public GameObject gameItemsNode;

    public float BallColliderRadius;
    public float LineColliderRadius;
    public float BallRealRadius;

    public GameObject topBorder;

    public Vector3 lastStopBallPos;     // Last position of ball before it stops

    public int potSounds;
    public int bugSounds;

    public float ballExplosionTimeInterval;

    void Awake()
    {
        Instance = this;
        if( InitScript.Instance == null ) gameObject.AddComponent<InitScript>();

        currentLevel = PlayerPrefs.GetInt( "OpenLevel", 1 );
		mainscript.StopControl = false;
        animTable.Clear();
	}

    void Start()
    {
        gridsNode = GameObject.Find("-Grids");
        gameItemsNode = GameObject.Find("-GameItems");

        _ballShooter = GameObject.Find("BallShooter").GetComponent<BallShooter>();
        _platformController = gridsNode.GetComponent<PlatformController>();
        //RandomizeWaitTime();
        ScoreManager.Instance.Score = 0;
        if (PlayerPrefs.GetInt("noSound") == 1) noSound = true;
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
            List<GameObject> ballsToDelete = new List<GameObject>();
            ballsToDelete.AddRange(CheckNearbySameColorBalls(checkBall));
            // 数字模式，不开启
            //ballsToDelete.AddRange(checkNearbyConsecutiveNumberBalls(checkBall));
            // 去掉重复元素
            ballsToDelete = ballsToDelete.Distinct().ToList();


            if (ballsToDelete.Count >= 3)
            {
                PlayBallExplodeAudio(ballsToDelete.Count);
                ScoreManager.Instance.ComboCount++;
                // 在这里调用coroutine将其销毁
                ExplodeBalls(ballsToDelete);
            }
            else
            {
                // 如果没爆超过3个，说明球只是撞上其他球，播撞球的音效
                // TODO: 如果撞了topBorder，周围又没有球怎么情况？
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.hitBall);
                mainscript.Instance.bounceCounter++;
                ScoreManager.Instance.ComboCount = 0;
            }

            checkBall = null;
        }
    }

    void PlayBallExplodeAudio(int ballCount)
    {
        if (ballCount < 5)
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.ballExplode);
        }
        else if (ballCount >= 5 && ballCount < 9)
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.ballExplode5Hit);
        }
        else
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.ballExplode9Hit);
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
        DestroyDetachedGameItems();

        if (GameManager.Instance.checkWin())
        {
            GameManager.Instance.Win();
        }


        //计算进度条应显示当前分数占最高级别（三星）的百分之多少
        ProgressBarScript.Instance.UpdateDisplay((float)ScoreManager.Instance.Score / LevelData.stars[2]);

        //更新星星个数
        if (stars < 3)
            stars += Mathf.Min(1, ScoreManager.Instance.Score / LevelData.stars[stars]);
        for (int i = 0; i < stars; ++i)
            starsObject[i].SetActive(true);
        
	}

	public void CheckLosing()
    {
        if (GameManager.Instance.GameStatus == GameStatus.Playing)
        {
            /*
            float stageMinYWorldSpace = platformController.curPlatformMinY;
            if (stageMinYWorldSpace < bottomBorder.transform.position.y)
            {
                // TODO: 如何结束游戏？
                GameManager.Instance.GameOver();
            }
            */
        }
    }

    public void DestroyDetachedGameItems()
    {
        List<GameObject> gameItemsToDrop = GridManager.Instance.FindDetachedGameItems();

        // 删掉ball，并调用StartFall让ball掉落
        if (gameItemsToDrop.Count > 0)
        {
            DropGameItems(gameItemsToDrop);
        }

        // 下面这几步是为了让新产生的球不再有以前没有出现过的颜色
        // TODO: 重新写下面两个方法
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
        GameObject[] allGameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        foreach(GameObject obj in allGameObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("FixedBall"))
            {
                Ball ball = obj.GetComponent<Ball>();
                if (ball != null)
                {
                    if (!curStageColors.Contains(ball.color))
                        curStageColors.Add(ball.color);
                }
            }
        }
    }

    public bool FindInArray(List<GameObject> b, GameObject destObj)
    {
		foreach(GameObject obj in b) {
			
			if(obj == destObj) return true;
		}
		return false;
	}
	
    // DropBalls, 注意和DestroyBalls并不相同，后者是让球爆炸，这个是让球落下
    public void DropGameItems(List<GameObject> ballsToDrop)
    {
        mainscript.Instance.bounceCounter = 0;

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
            ball.GetComponent<GameItem>().StartFall();
		}
        // 调用ScoreManager里针对掉落球的分数更新函数
        // 暂时注释掉因为女巫泡泡里没有掉落得分 (只有掉入pot的加分)
        //int val = ScoreManager.Instance.UpdateFallingScore(ballsToDrop.Count);

        //ScoreManager.Instance.PopupFallingScore(val, transform.position+(new Vector3(1,0,0)));
    }

    List<GameObject> CheckNearbyConsecutiveNumberBalls(GameObject checkBallGO)
    {
        Ball checkBall = checkBallGO.GetComponent<Ball>();
        List<GameObject> ballsToDelete = new List<GameObject>();
        List<GameObject> longestIncreasePath = new List<GameObject>();
        List<GameObject> longestDecreasePath = new List<GameObject>();

        foreach(GameObject adjacentBallGO in checkBallGO.GetComponent<Ball>().grid.GetAdjacentGameItems())
        {
            Ball adjacentBall = adjacentBallGO.GetComponent<Ball>();
            if (adjacentBall.number == checkBall.number + 1)
            {
                // 搜寻最长的增加路径
                List<GameObject> currentPath = new List<GameObject>();
                adjacentBall.SearchNumberPath(ref longestIncreasePath, ref currentPath, true);
            }
            else if (adjacentBall.number == checkBall.number - 1)
            {
                // 搜寻最长的减少路径
                List<GameObject> currentPath = new List<GameObject>();
                adjacentBall.SearchNumberPath(ref longestDecreasePath, ref currentPath, false);
            }
        }

        //Debug.Log("Decrease List: " + ballListNames(longestDecreasePath));
        //Debug.Log("Increase List: " + ballListNames(longestIncreasePath));

        if (longestIncreasePath.Count + longestDecreasePath.Count >= minConsecutiveNumberCount - 1)
        {
            ballsToDelete.Add(checkBallGO);
            ballsToDelete.AddRange(longestDecreasePath);
            ballsToDelete.AddRange(longestIncreasePath);
        }

        return ballsToDelete;
    }

    string BallListNames(List<GameObject> balls)
    {
        string a = null;
        foreach(GameObject ball in balls)
        {
            a += ball.GetComponent<Ball>().number.ToString() + " ";
        }

        return a;
    }

    List<GameObject> CheckNearbySameColorBalls(GameObject checkBallGO)
    {
        // 该方法用来查找是否有其它ball与之相连，形成三个或以上的ball，如果有则将其销毁
        Ball checkBall = checkBallGO.GetComponent<Ball>();

        List<GameObject> ballsToDelete = new List<GameObject>();
        ballsToDelete.Add(checkBallGO);
        checkBall.CheckNextNearestColor(ballsToDelete);
        mainscript.Instance.countOfPreparedToDestroy = ballsToDelete.Count;

        return ballsToDelete;
    }

    void ExplodeBalls(List<GameObject> balls)
    {
        mainscript.Instance.bounceCounter = 0;
        //调用ScoreManager里爆炸球的分数更新函数
        int score = ScoreManager.Instance.UpdateComboScore(balls.Count);

        float delayedExplodeTime = 0f;
        foreach (GameObject ballGO in balls)
        {
            // 让ball爆炸
            Ball ball = ballGO.GetComponent<Ball>();
            ball.Explode(delayedExplodeTime, score);
            delayedExplodeTime += ballExplosionTimeInterval;
        }
    }

    public void GenerateTargetStar(Grid grid)
    {
        // TODO: 换成GameMode
        if (levelData.stageMoveMode == StageMoveMode.Vertical)
        {
            if (grid.Row == 0)
            {
                Instantiate(targetStarPrefab, grid.transform.position, grid.transform.rotation);
                GameObject movingTargetStar = Instantiate(targetStarPrefab, grid.transform.position, grid.transform.rotation) as GameObject;
                movingTargetStar.GetComponent<TargetStar>().fly();
            }
        }
    }
}



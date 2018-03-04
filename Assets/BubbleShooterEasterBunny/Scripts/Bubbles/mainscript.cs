using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using InitScriptName;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class mainscript : MonoBehaviour
{
    public int currentLevel;
    public LevelData levelData = new LevelData();
    public int minConsecutiveNumberCount;

    public static mainscript Instance;
    public int bounceCounter = 0;
    private GameObject _checkBall;

    public GameObject checkBall
    {
        get { return _checkBall; }
        set
        {
            _checkBall = value;
        }
    }

    public Color currentBallShooterColor = new Color(1, 0, 0, 1);

    public Sprite[] ballColorSprites;
    public Sprite[] ballColorHightlightSprites;
    public Sprite[] animalSingleSprites;
    public Sprite animalSingleShellSprite;
    public Sprite[] animalTriangleSprites;
    public Sprite animalTriangleShellSprite;
    public Sprite[] animalHexSprites;
    public Sprite animalHexShellSprite;

    public Color[] BallRGB = new[]
    { new Color(24 / 255f, 121 / 255f, 1, 1),
        new Color(19 / 255f, 161 / 255f, 30 / 255f, 1),
        new Color(224 / 255f, 52 / 255f, 0, 1),
        new Color(179 / 255f, 0, 222 / 255f, 1),
        new Color(188 / 255f, 62 / 255f, 0, 1),
        new Color(1.0f, 1.0f, 1.0f, 1), //random color reserve
        new Color(1.0f, 1.0f, 1.0f, 1), //white color for rainbow call
        new Color(224 / 255f, 52 / 255f, 0, 1) /*fire color*/
    };

    public List<GameObject> controlGrids = new List<GameObject>();
    public bool isPaused;
    public bool noSound;
    public bool gameOver;
    public bool scored;
    public GameObject ElectricLiana;
    public static bool ElectricBoost;

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

    public List<BallColor> curStageColors = new List<BallColor>();

    private int TargetCounter;

    public int TargetCounter1
    {
        get { return TargetCounter; }
        set { TargetCounter = value; }
    }

    public GameObject[] starsObject;
    public int stars = 0;

    public GameObject[] boosts;
    public GameObject[] locksBoosts;
    public GameObject FireEffect;
    public GameObject black_back;

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
    public GameObject leftBorder;
    public GameObject rightBorder;


    public int potSounds;
    public int bugSounds;

    public float ballExplosionTimeInterval;

    public delegate void DestroyBallsHandler();

    public event DestroyBallsHandler onGameItemsDestroyed;

    public delegate void BallShooterUnlocked();

    public event BallShooterUnlocked onBallShooterUnlocked;

    void Awake()
    {
        Instance = this;
        if (InitScript.Instance == null)
            gameObject.AddComponent<InitScript>();

        if (GameManager.Instance.gameMode == GameMode.Playing)
            currentLevel = PlayerPrefs.GetInt("OpenLevel", 1);
        else
            currentLevel = 0;	//开场动画关卡是0

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
        if (PlayerPrefs.GetInt("noSound") == 1)
            noSound = true;

        StageLoader.Load();
        InitializeTopBorder();
    }

    void InitializeTopBorder()
    {
        if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
        {
            float topBorderOffset = 0.35f;   // 这个值是grid高度一半，用来将topborder置于0排最上边
            // 这里我们要将topborder移动到grid下，这样border可以和grid一起移动
            topBorder.transform.parent = mainscript.Instance.gridsNode.transform;
            topBorder.transform.localPosition = new Vector3(0f, topBorderOffset, 0f);
        }
    }

    IEnumerator ShowArrows()
    {
        while (true)
        {

            yield return new WaitForSeconds(30);
            if (GameManager.Instance.gameStatus == GameStatus.Playing)
            {
                arrows.SetActive(true);

            }
            yield return new WaitForSeconds(3);
            arrows.SetActive(false);
        }
    }

    public void SwitchLianaBoost()
    {
        if (!ElectricBoost)
        {
            ElectricBoost = true;
            ElectricLiana.SetActive(true);
        }
        else
        {
            ElectricBoost = false;
            ElectricLiana.SetActive(false);
        }
    }

    void ConnectAndDestroyBalls()
    {
        // 找到同色的ball并将其销毁
        List<GameObject> ballsToDelete = CheckNearbySameColorBalls(checkBall);
        // 数字模式，不开启
        //ballsToDelete.AddRange(checkNearbyConsecutiveNumberBalls(checkBall));

        if (ballsToDelete.Count >= 3)
        {
            scored = true;
            DestroyGameItems(ballsToDelete);
        }
        else
        {
            // 如果没爆超过3个，说明球只是撞上其他球，播撞球的音效
            // TODO: 如果撞了topBorder，周围又没有球怎么情况？
            checkBall.GetComponent<Ball>().RemoveAdjacentBallsSmoke(); // 如果撞了什么也没发生，则单独除掉该球周围灰尘
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.hitBall);
            mainscript.Instance.bounceCounter++;
            ScoreManager.Instance.ComboCount = 0;
        }
    }

    public void DestroyGameItems(List<GameObject> gameItemsToDelete)
    {
        PlayBallExplodeAudio(gameItemsToDelete.Count);
        ScoreManager.Instance.ComboCount++;

        // 在这里调用coroutine将其销毁
        ExplodeGameItems(gameItemsToDelete);

        if (onGameItemsDestroyed != null)
        {
            onGameItemsDestroyed();
        }

        // 每次销毁任何颜色球都要检测是否有球掉落
        FindAndDestroyDetachedGameItems();
    }

    void PlayBallExplodeAudio(int ballCount)
    {
        if (ballCount > 1)
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.ballExplode);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckLosing();

        if (noSound)
            GetComponent<AudioSource>().volume = 0;
        if (!noSound)
            GetComponent<AudioSource>().volume = 0.5f;

        // 游戏中最重要的算法部分：检测ball是否连上，销毁，以及判断是否有其它drop的balls
        // checkBall在ball.cs中被赋值，当ball停住的时候，就说明需要判断连接了，这个值也就被设定了
        if (checkBall != null && GameManager.Instance.gameStatus == GameStatus.Playing)
        {
            ConnectAndDestroyBalls();
            checkBall = null;
        }

        //计算进度条应显示当前分数占最高级别（三星）的百分之多少
        ProgressBarScript.Instance.UpdateDisplay((float)ScoreManager.Instance.Score / levelData.starScores[2]);

        //更新星星个数
        if (stars < 3)
            stars += Mathf.Min(1, ScoreManager.Instance.Score / levelData.starScores[stars]);
        for (int i = 0; i < stars; ++i)
            starsObject[i].SetActive(true);
        
    }

    public void CheckLosing()
    {
        if (GameManager.Instance.gameStatus == GameStatus.Playing)
        {
            if (levelData.limitAmount < 0)
            {
                GameManager.Instance.GameOver();
            }
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

    public void FindAndDestroyDetachedGameItems()
    {
        List<GameObject> gameItemsToDrop = GridManager.Instance.FindDetachedGameItems();

        // 删掉ball，并调用StartFall让ball掉落
        if (gameItemsToDrop.Count > 0)
        {
            DropGameItems(gameItemsToDrop);
        }

        UpdateColorsInGame();
        ballShooter.UpdateBallColors();
        if (levelData.missionType == MissionType.BossBattle)
        {
            BossManager.Instance.UpdateBossPlaceHitColor();
        }
    }

    public void UpdateColorsInGame()
    {
        curStageColors.Clear();

        foreach (Transform gameItemTransform in gameItemsNode.transform)
        {
            Ball ball = gameItemTransform.gameObject.GetComponent<Ball>();
            if (ball != null)
            {
                if (!curStageColors.Contains(ball.color))
                {
                    curStageColors.Add(ball.color);
                }
            }
        }
    }

    public void UpdateColorsInGame(BallColor newColor, BallColor deleteColor)
    {
        curStageColors.Clear();

        foreach (Transform gameItemTransform in gameItemsNode.transform)
        {
            Ball ball = gameItemTransform.gameObject.GetComponent<Ball>();
            if (ball != null)
            {
                if (ball.color == deleteColor)
                {
                    ball.color = newColor;
                    ball.GetComponent<Ball>().SetTypeAndColor((LevelItemType)newColor);
                }
                if (!curStageColors.Contains(ball.color))
                {
                    curStageColors.Add(ball.color);
                }
            }
        }
    }

    public bool FindInArray(List<GameObject> b, GameObject destObj)
    {
        foreach (GameObject obj in b)
        {
			
            if (obj == destObj)
                return true;
        }
        return false;
    }
	
    // DropBalls, 注意和DestroyBalls并不相同，后者是让球爆炸，这个是让球落下
    public void DropGameItems(List<GameObject> gameItemsToDrop)
    {
        mainscript.Instance.bounceCounter = 0;

        // 这里的score累加似乎没用 先注释了
        /*int scoreCounter = 0;
		int rate = 0;*/

        foreach (GameObject gameItem in gameItemsToDrop)
        {
            /*if(scoreCounter > 3){
				rate +=3;
				scoreCounter += rate;
			}
			scoreCounter ++;*/

            // 让没接上的ball都掉落
            gameItem.GetComponent<GameItem>().StartFall();
        }
        // 调用ScoreManager里针对掉落球的分数更新函数
        // 暂时注释掉因为女巫泡泡里没有掉落得分 (只有掉入pot的加分)
        //int val = ScoreManager.Instance.UpdateFallingScore(ballsToDrop.Count);

        //ScoreManager.Instance.PopupFallingScore(val, transform.position+(new Vector3(1,0,0)));
    }

    public void DropAllBalls()
    {
        Transform b = gameItemsNode.transform;
        Ball[] balls = gameItemsNode.GetComponentsInChildren<Ball>();
        foreach (Ball item in balls)
        {
            if (item.GetComponent<Ball>().state == Ball.BallState.Fixed)
                item.StartFall();
        }
    }

    List<GameObject> CheckNearbyConsecutiveNumberBalls(GameObject checkBallGO)
    {
        Ball checkBall = checkBallGO.GetComponent<Ball>();
        List<GameObject> ballsToDelete = new List<GameObject>();
        List<GameObject> longestIncreasePath = new List<GameObject>();
        List<GameObject> longestDecreasePath = new List<GameObject>();

        foreach (GameObject adjacentBallGO in checkBallGO.GetComponent<Ball>().grid.GetAdjacentGameItems())
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
        foreach (GameObject ball in balls)
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
        checkBall.CheckNextNearestColor(ballsToDelete, 0);
        // 去掉重复元素
        ballsToDelete = ballsToDelete.Distinct().ToList();

        return ballsToDelete;
    }

    void ExplodeGameItems(List<GameObject> gameItems)
    {
        mainscript.Instance.bounceCounter = 0;
        //调用ScoreManager里爆炸球的分数更新函数
        int score = ScoreManager.Instance.UpdateComboScore(gameItems.Count);

        float delayedExplodeTime = 0f;
        foreach (GameObject gameItem in gameItems)
        {
            // 让ball爆炸
            Ball ball = gameItem.GetComponent<Ball>();
            if (ball)
            {
                ball.Explode(delayedExplodeTime, score);
                delayedExplodeTime += ballExplosionTimeInterval;
            }
        }
    }

    public BallColor GetRandomCurStageColor()
    {
        if (curStageColors.Count == 0)
        {
            return BallColor.blue;
        }

        return curStageColors[Random.Range(0, mainscript.Instance.curStageColors.Count)];
    }

    public void OnBallShooterUnlocked()
    {
        if (!ballShooter.isLocked)
        {
            if (onBallShooterUnlocked != null)
            {
                onBallShooterUnlocked();
            }
        }
    }

}

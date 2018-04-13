using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallShooter : MonoBehaviour
{
    public enum BallShooterState
    {
        ReadyToShoot,
        Reloading,
        Swapping,
        Freezing
    }

    public enum StageCollidersMode
    {
        AimMode,
        // 用辅助线瞄准时候用，collider会根据LineRadius计算
        FireMode
        // 在发射球时候用，真正进行碰撞检测
    }

    public BallShooterState state;
    public GameObject boxCatapult;
    public GameObject boxCartridge;
    public GameObject rootBall;
    public GameObject bottomBorderGO;

    public float camOrthographicSizeX;
    public float camOrthographicSizeY;

    public bool isLocked
    {
        get { return _isLocked; }
        set
        {
            _isLocked = value;
            if (_isLocked)
            {
                SetStageCollidersMode(StageCollidersMode.FireMode);
            }
            else
            {
                SetStageCollidersMode(StageCollidersMode.AimMode);
                CoreManager.Instance.OnBallShooterUnlocked();
            }
        }
    }

    public bool _isLocked;

    private GameObject catapultBall;
    private GameObject cartridgeBall;
    private GameObject tempBall;

    private float openingModeNextShootingTime = 0f;
    private float openingModeInterval = 1f;
    private int openingModeFireBallCount = 0;

    // 在有道具球时暂时保存之前的cartridgeBall

    public GameObject CatapultBall
    {
        get { return catapultBall; }
        set
        {
            catapultBall = value;
        }
    }

    public GameObject CartridgeBall
    {
        get { return cartridgeBall; }
        set
        {
            cartridgeBall = value;
        }
    }

    float bottomBorderY;
    //低于此线就不能发射球
    public float launchForce;
    // 发射力度

    public GameObject topBorder;
    public GameObject leftBorder;
    public GameObject rightBorder;

    UnityEngine.EventSystems.EventSystem currentES;

    bool boostInPosition;
    // 当boost准备发射时，不能切换球

    // Use this for initialization
    void Start()
    {
        rootBall = GameObject.Find("-GameItems");
        bottomBorderY = bottomBorderGO.transform.position.y;
        currentES = UnityEngine.EventSystems.EventSystem.current;
        camOrthographicSizeY = Camera.main.orthographicSize;
        camOrthographicSizeX = Camera.main.orthographicSize / Screen.height * Screen.width;
    }

    public void Initialize()
    {
        boostInPosition = false;
        isLocked = false;
        CreateCartridgeBall();
        Reload();
    }

    void Update()
    {
        if (UIManager.Instance.gameMode == GameMode.Playing)
        {
            PlayingModeCheckAndFire();
        }
        else
        {
            OpeningModeCheckAndFire();
        }

        if (UIManager.Instance.gameStatus == GameStatus.Win && catapultBall == null)
        {
            Reload();
        }
    }

    void OpeningModeCheckAndFire()
    {
        // currentES.IsPointerOverGameObject()用来检测是否鼠标点击的是GUI
        if (Time.time > openingModeNextShootingTime)
        {
            if (UIManager.Instance.gameStatus == GameStatus.Playing &&
                !CoreManager.Instance.gameOver &&
                !isLocked &&
                catapultBall != null)
            {
                openingModeFireBallCount++;
                // 设置一个偏移值是为了防止bug（似乎如果射的很正会有问题）
                Vector3 pos = new Vector3(0.2f, 0f, 0f);
                Fire(pos);
                Reload();

                if (openingModeFireBallCount > 12)
                {
                    CoreManager.Instance.DropAllBalls();
                    openingModeFireBallCount = 0;
                }
            }

            openingModeNextShootingTime = Time.time + openingModeInterval;
        }
    }

    void PlayingModeCheckAndFire()
    {
        if (UIManager.Instance.gameStatus == GameStatus.Playing && !CoreManager.Instance.gameOver && !isLocked)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // currentES.IsPointerOverGameObject()用来检测是否鼠标点击的是GUI
            if (pos.y > bottomBorderY && Input.GetMouseButtonUp(0) && !currentES.IsPointerOverGameObject())
            {
                if (catapultBall != null)
                {
                    Fire(pos);
                    Reload();
                }
            }
        }
    }

    void Fire(Vector3 pos)
    {
        // 如果是opening模式，不减球数量
        if (UIManager.Instance.gameMode != GameMode.Opening)
        {
            CoreManager.Instance.levelData.limitAmount--;
        }

        // 在这里给发射的ball赋予一个force，产生初速度
        Vector2 direction = pos - catapultBall.transform.position;
        catapultBall.GetComponent<Rigidbody2D>().AddForce(direction.normalized * launchForce, ForceMode2D.Force);

        // 根据gameItemType具体类型调用各自的Fire方法
        GameItem gameItem = catapultBall.GetComponent<GameItem>();
        gameItem.Fire();
        if (gameItem.itemType == GameItem.ItemType.RainbowBall || gameItem.itemType == GameItem.ItemType.FireBall)
        {
            boostInPosition = false;
        }

        isLocked = true;
        catapultBall = null;
    }

    public void UpdateBallColors()
    {
        if (catapultBall != null && !CoreManager.Instance.curStageColors.Contains(catapultBall.GetComponent<Ball>().color))
        {
            Destroy(catapultBall);
            CreateCatapultBall(false);
        }

        if (cartridgeBall != null && !CoreManager.Instance.curStageColors.Contains(cartridgeBall.GetComponent<Ball>().color))
        {
            Destroy(cartridgeBall);
            CreateCartridgeBall(false);
        }
    }

    void SetBordersOnAimMode()
    {
        if (CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
        {
            float leftBorderX = Camera.main.transform.position.x - camOrthographicSizeX + CoreManager.Instance.BallRealRadius;
            leftBorder.transform.position = new Vector3(leftBorderX, 0f, 0f);
            float rightBorderX = Camera.main.transform.position.x + camOrthographicSizeX - CoreManager.Instance.BallRealRadius;
            rightBorder.transform.position = new Vector3(rightBorderX, 0f, 0f);
            float topBorderY = Camera.main.transform.position.y + camOrthographicSizeY - CoreManager.Instance.BallRealRadius;
            topBorder.transform.position = new Vector3(0f, topBorderY, 0f);
        }
        else
        {
            GameObject topRowLeftGrid = GridManager.Instance.Grid(0, 0);
            leftBorder.transform.position = new Vector3(topRowLeftGrid.transform.position.x, 0f, 0f);
            GameObject topRowRightGrid = GridManager.Instance.Grid(1, GridManager.Instance.colCount - 1);
            rightBorder.transform.position = new Vector3(topRowRightGrid.transform.position.x, 0f, 0f);
        }
    }

    void SetBordersOnFireMode()
    {
        if (CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
        {
            float leftBorderX = Camera.main.transform.position.x - camOrthographicSizeX + (CoreManager.Instance.BallRealRadius - CoreManager.Instance.BallColliderRadius);
            leftBorder.transform.position = new Vector3(leftBorderX, 0f, 0f);
            float rightBorderX = Camera.main.transform.position.x + camOrthographicSizeX - (CoreManager.Instance.BallRealRadius - CoreManager.Instance.BallColliderRadius);
            rightBorder.transform.position = new Vector3(rightBorderX, 0f, 0f);
            float topBorderY = Camera.main.transform.position.y + camOrthographicSizeY - (CoreManager.Instance.BallRealRadius - CoreManager.Instance.BallColliderRadius);
            topBorder.transform.position = new Vector3(0f, topBorderY, 0f);
        }
        else
        {
            float borderOffset = CoreManager.Instance.BallColliderRadius;
            GameObject topRowLeftGrid = GridManager.Instance.Grid(0, 0);
            leftBorder.transform.position = new Vector3(topRowLeftGrid.transform.position.x - borderOffset, 0f, 0f);
            GameObject topRowRightGrid = GridManager.Instance.Grid(1, GridManager.Instance.colCount - 1);
            rightBorder.transform.position = new Vector3(topRowRightGrid.transform.position.x + borderOffset, 0f, 0f);
        }
    }

    public void SetStageCollidersMode(StageCollidersMode mode)
    {
        if (mode == StageCollidersMode.AimMode)
        {
            ChangeRadius(CoreManager.Instance.LineColliderRadius);
            SetBordersOnAimMode();
        }
        else
        {
            ChangeRadius(CoreManager.Instance.BallColliderRadius);
            SetBordersOnFireMode();
        }
    }

    void ChangeRadius(float r)
    {
        if (rootBall == null)
            return;
        foreach (Transform ball in rootBall.transform)
        {
            if (ball.gameObject.GetComponent<GameItem>().itemType == GameItem.ItemType.Ball)
                ball.gameObject.GetComponent<CircleCollider2D>().radius = r;
        }
    }

    void CreateCatapultBall(bool playAnimation = true)
    {
        catapultBall = GameItemFactory.Instance.CreateNewBall(boxCatapult.transform.position, playAnimation);
        catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;

    }

    void CreateBoost(BoostType boostType)
    {
        catapultBall = GameItemFactory.Instance.CreateBoost(boostType, boxCatapult.transform.position);
        boostInPosition = true;
    }

    void CreateCartridgeBall(bool playAnimation = true)
    {
        cartridgeBall = GameItemFactory.Instance.CreateNewBall(boxCartridge.transform.position, playAnimation);
        cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
    }

    public void SetBoost(BoostType boostType)
    {
        tempBall = cartridgeBall;
        //将tempBall隐藏起来
        tempBall.SetActive(false);
        cartridgeBall = catapultBall;
        iTween.MoveTo(cartridgeBall, iTween.Hash("position", boxCartridge.transform.position,
                "time", 0.3,
                "easetype", iTween.EaseType.linear));

        CreateBoost(boostType);
        CoreManager.Instance.levelData.limitAmount++;
    }

    /// <summary>
    /// 给shooter上膛，第二个球挪到第一个再产生一个新的
    /// </summary>
    void Reload()
    {
        if (catapultBall == null)
        {
            state = BallShooterState.Reloading;

            if (cartridgeBall != null)
            {
                catapultBall = cartridgeBall;
                catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;
                catapultBall.GetComponent<bouncer>().BounceToCatapult(boxCatapult.transform.position);
            }
            if (tempBall != null)
            {
                cartridgeBall = tempBall;
                cartridgeBall.SetActive(true);
                tempBall = null;
            }
            else if (CoreManager.Instance.levelData.limitAmount > 0)
            {
                CreateCartridgeBall();
            }
            else
                cartridgeBall = null;

        }
    }

    public void SwapBalls()
    {
        if (UIManager.Instance.gameStatus == GameStatus.Playing && !boostInPosition)
        {
            if (state == BallShooterState.ReadyToShoot && cartridgeBall != null)
            {
                state = BallShooterState.Swapping;
                // stopped here
                iTween.MoveTo(cartridgeBall, iTween.Hash("position", boxCatapult.transform.position,
                        "time", 0.3,
                        "easetype", iTween.EaseType.linear));
                iTween.MoveTo(catapultBall, iTween.Hash("position", boxCartridge.transform.position,
                        "time", 0.3,
                        "easetype", iTween.EaseType.linear,
                        "onComplete", "OnSwapBallsComplete"));
            }
        }
    }

    public void OnSwapBallsComplete()
    {
        GameObject tmpBall = catapultBall;
        catapultBall = cartridgeBall;
        cartridgeBall = tmpBall;

        cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
        catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;

        state = BallShooterState.ReadyToShoot;
    }

    public void OnBounceToCatapultComplete()
    {
        if (catapultBall != null)
        {
            catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;
            state = BallShooterState.ReadyToShoot;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boxCatapult.transform.position, boxCatapult.transform.localScale);
        Gizmos.DrawWireCube(boxCartridge.transform.position, boxCartridge.transform.localScale);
    }
}

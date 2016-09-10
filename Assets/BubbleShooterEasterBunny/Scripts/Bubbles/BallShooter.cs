using UnityEngine;
using System.Collections;

public class BallShooter : MonoBehaviour {
    public enum BallShooterState {
        ReadyToShoot,
        Reloading,
        Swapping,
        Freezing
    };

    public BallShooterState state;
    public GameObject boxCatapult;
    public GameObject boxCartridge;
    public GameObject rootBall;
    public bool isFreezing;

    GameObject catapultBall;
    GameObject cartridgeBall;

    public GameObject CatapultBall
    {
        get { return catapultBall; }
        set
        {
            catapultBall = value;
        }
    }

    float bottomBoarderY;  //低于此线就不能发射球

    UnityEngine.EventSystems.EventSystem currentES;
	// Use this for initialization
	void Start ()
    {
        rootBall = GameObject.Find("-GameItems");
        bottomBoarderY = GameObject.Find("BottomBorder").transform.position.y;
        currentES = UnityEngine.EventSystems.EventSystem.current;
    }

    public void Initialize()
    {
        cartridgeBall = GameItemFactory.Instance.CreateNewBall(boxCartridge.transform.position, LevelData.ItemType.random);
        cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
        Reload();
    }

    public void Update()
    {
        if (GameManager.Instance.GameStatus == GameStatus.Playing)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // currentES.IsPointerOverGameObject()用来检测是否鼠标点击的是GUI
            if (pos.y > bottomBoarderY && Input.GetMouseButtonUp (0) && !currentES.IsPointerOverGameObject())
            {
                Fire();
            }
        }
        if (GameManager.Instance.GameStatus == GameStatus.Win && catapultBall == null)
        {
            Reload();
        }
    }

    void Fire()
    {
        ChangeRadius(mainscript.Instance.BallColliderRadius);
        if (catapultBall != null && !isFreezing)
        {
            catapultBall.GetComponent<Ball>().Fire();
            mainscript.Instance.levelData.LimitAmount--;
            isFreezing = true;
            catapultBall = null;
            Reload();
        }
    }

    public void ChangeRadius(float r)
    {
        if (rootBall == null) return;
        foreach (Transform ball in rootBall.transform)
        {
            if (ball.gameObject.GetComponent<GameItem>().itemType == GameItem.ItemType.Ball)
                ball.gameObject.GetComponent<CircleCollider2D>().radius = r;
        }
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
            if (mainscript.Instance.levelData.LimitAmount > 0)
            {
                cartridgeBall = GameItemFactory.Instance.CreateNewBall(boxCartridge.transform.position, LevelData.ItemType.random);
                cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
            }
            else
                cartridgeBall = null;

        }
    }

    public void SwapBalls()
    {
        if (GameManager.Instance.GameStatus == GameStatus.Playing)
        {
            if (state == BallShooterState.ReadyToShoot && cartridgeBall != null)
            {
                state = BallShooterState.Swapping;
                // stopped here
                iTween.MoveTo(cartridgeBall, iTween.Hash("position", boxCatapult.transform.position,
                                                         "time", 0.3 ,
                                                         "easetype",iTween.EaseType.linear));
                iTween.MoveTo(catapultBall, iTween.Hash("position", boxCartridge.transform.position,
                                                        "time", 0.3 ,
                                                        "easetype",iTween.EaseType.linear,
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
}

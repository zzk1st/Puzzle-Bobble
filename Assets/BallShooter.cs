using UnityEngine;
using System.Collections;

public class BallShooter : MonoBehaviour {
    public enum BallShooterState {
        ReadyToShoot,
        Reloading,
        Swapping
    };

    public BallShooterState state;
    public GameObject boxCatapult;
    public GameObject boxCartridge;

    //spriteRenderer used to hold the ball direction indicator sprite
    SpriteRenderer spriteRenderer;

    GameObject catapultBall;
    GameObject cartridgeBall;

    CreatorBall creatorBall;
    UnityEngine.EventSystems.EventSystem currentES;
	// Use this for initialization
	void Start ()
    {
        creatorBall = GameObject.Find("Creator").GetComponent<CreatorBall>();
        currentES = UnityEngine.EventSystems.EventSystem.current;
        spriteRenderer = GameObject.Find("dir_indicator").GetComponent<SpriteRenderer>();
    }

    public void Initialize()
    {
        cartridgeBall = creatorBall.createBall(boxCartridge.transform.position, BallColor.random, true);
        cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
        Reload();
    }

    public void Update()
    {
        // currentES.IsPointerOverGameObject()用来检测是否鼠标点击的是GUI
        if (Input.GetMouseButtonUp (0) && !currentES.IsPointerOverGameObject())
        {
            Fire();
        }
    }

    void Fire()
    {
        if (catapultBall != null)
        {
            catapultBall.GetComponent<Ball>().Fire();
            catapultBall = null;
            Reload();
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

            catapultBall = cartridgeBall;
            catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;
            catapultBall.GetComponent<bouncer>().BounceToCatapult(boxCatapult.transform.position);

            // Currently Disabled. Find color according to texture.
            // Alternative way: get color string by catapultBall.tag, then assign color
            // via a mapping from color string to actual color.

            //Color col = catapultBall.GetComponent<Grid>().Busy.GetComponent<SpriteRenderer>().sprite.texture.GetPixelBilinear(0.1f, 0.6f);
            //col.a = 1;
            //spriteRenderer.color = col;
            

            cartridgeBall = creatorBall.createBall(boxCartridge.transform.position, BallColor.random, true);
            cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
        }
    }

    public void SwapBalls()
    {
        if (state == BallShooterState.ReadyToShoot)
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
        catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;
        state = BallShooterState.ReadyToShoot;
    }
}

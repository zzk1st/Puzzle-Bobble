﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallShooter : MonoBehaviour {
    public enum BallShooterState
    {
        ReadyToShoot,
        Reloading,
        Swapping,
        Freezing
    }

    public enum StageCollidersMode
    {
        AimMode,    // 用辅助线瞄准时候用，collider会根据LineRadius计算
        FireMode    // 在发射球时候用，真正进行碰撞检测
    }

    public BallShooterState state;
    public GameObject boxCatapult;
    public GameObject boxCartridge;
    public GameObject rootBall;
    public bool isFreezing;

    private GameObject catapultBall;
    private GameObject cartridgeBall;
    private GameObject tempBall; // 在有道具球时暂时保存之前的cartridgeBall

    public GameObject CatapultBall
    {
        get { return catapultBall; }
        set
        {
            catapultBall = value;
        }
    }

    float bottomBoarderY;  //低于此线就不能发射球

    public GameObject topBorder;
    public GameObject leftBorder;
    public GameObject rightBorder;

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
        SetStageCollidersMode(StageCollidersMode.AimMode);
        mainscript.Instance.UpdateColorsInGame();
        CreateCartridgeBall();
        Reload();
    }

    void Update()
    {
        if (GameManager.Instance.gameStatus == GameStatus.Playing)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // currentES.IsPointerOverGameObject()用来检测是否鼠标点击的是GUI
            if (pos.y > bottomBoarderY && Input.GetMouseButtonUp (0) && !currentES.IsPointerOverGameObject())
            {
                Fire();
            }
        }

        if (GameManager.Instance.gameStatus == GameStatus.Win && catapultBall == null)
        {
            Reload();
        }
    }

    void Fire()
    {
        SetStageCollidersMode(StageCollidersMode.FireMode);

        if (catapultBall != null && !isFreezing)
        {
            catapultBall.GetComponent<Ball>().Fire();
            mainscript.Instance.levelData.limitAmount--;
            isFreezing = true;
            catapultBall = null;
            Reload();
        }
    }

    public void UpdateBallColors()
    {
        if (catapultBall != null && !mainscript.Instance.curStageColors.Contains(catapultBall.GetComponent<Ball>().color))
        {
            Destroy(catapultBall);
            CreateCatapultBall(false);
        }

        if (cartridgeBall != null && !mainscript.Instance.curStageColors.Contains(cartridgeBall.GetComponent<Ball>().color))
        {
            Destroy(cartridgeBall);
            CreateCartridgeBall(false);
        }
    }

    void SetBordersOnAimMode()
    {
        GameObject topRowLeftGrid = GridManager.Instance.Grid(0, 0);
        leftBorder.transform.position = new Vector3(topRowLeftGrid.transform.position.x, 0f, 0f);
        GameObject topRowRightGrid = GridManager.Instance.Grid(0, GridManager.Instance.colCount - 1);
        rightBorder.transform.position = new Vector3(topRowRightGrid.transform.position.x, 0f, 0f);
    }

    void SetBordersOnFireMode()
    {
        float borderOffset = mainscript.Instance.BallColliderRadius;
        GameObject topRowLeftGrid = GridManager.Instance.Grid(0, 0);
        leftBorder.transform.position = new Vector3(topRowLeftGrid.transform.position.x - borderOffset, 0f, 0f);
        GameObject topRowRightGrid = GridManager.Instance.Grid(0, GridManager.Instance.colCount - 1);
        rightBorder.transform.position = new Vector3(topRowRightGrid.transform.position.x + borderOffset, 0f, 0f);
    }

    public void SetStageCollidersMode(StageCollidersMode mode)
    {
        if (mode == StageCollidersMode.AimMode)
        {
            ChangeRadius(mainscript.Instance.LineColliderRadius);
            SetBordersOnAimMode();
        }
        else
        {
            ChangeRadius(mainscript.Instance.BallColliderRadius);
            SetBordersOnFireMode();
        }
    }

    void ChangeRadius(float r)
    {
        if (rootBall == null) return;
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

    void CreateSpecialBall(BoostType boostType)
    {
        tempBall = cartridgeBall;
        cartridgeBall = catapultBall;
        catapultBall = GameItemFactory.Instance.CreateNewBall(boxCatapult.transform.position, false, boostType);
        catapultBall.GetComponent<Ball>().state = Ball.BallState.ReadyToShoot;
        mainscript.Instance.levelData.limitAmount++;
    }

    void CreateCartridgeBall(bool playAnimation = true)
    {
        cartridgeBall = GameItemFactory.Instance.CreateNewBall(boxCartridge.transform.position, playAnimation);
        cartridgeBall.GetComponent<Ball>().state = Ball.BallState.Waiting;
    }

    public void SetBoost(BoostType boostType)
    {
        CreateSpecialBall(boostType);
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
                tempBall = null;
            }
            else if (mainscript.Instance.levelData.limitAmount > 0)
            {
                CreateCartridgeBall();
            }
            else
                cartridgeBall = null;

        }
    }

    public void SwapBalls()
    {
        if (GameManager.Instance.gameStatus == GameStatus.Playing)
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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boxCatapult.transform.position, boxCatapult.transform.localScale);
        Gizmos.DrawWireCube(boxCartridge.transform.position, boxCartridge.transform.localScale);
    }
}

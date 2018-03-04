using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// 该类主要用来管理整个球的平台的运动，包括上下移动，加速下落等
/// </summary>
public class PlatformController : MonoBehaviour
{
    public float moveSpeed;
    // MinY指整个关卡最低的位置
    public float platformBottomLowerLimit;
    public float platformBottomUpperLimit;

    public GameObject topBorder;
    private float initialTopBorderPos;

    // 当前所有fixed balls的最小y值，用来测试关卡是否过线
    private float _curFixedBallLocalMinY;

    // 因为垂直关卡的球都是从(0, 0)开始向下，所以关卡顶部坐标可以这么计算
    private float curPlatformTopPos
    {
        get { return transform.position.y + mainscript.Instance.BallRealRadius; }
    }

    private float curPlatformBottomPos
    {
        get
        {
            float platformMinYWorldSpace = transform.position.y
                                  + _curFixedBallLocalMinY
                                  - mainscript.Instance.BallRealRadius;
            return platformMinYWorldSpace;
        }
    }

    private Quaternion targetRotation;

    void Start()
    {
        // TODO: 更好的获得屏幕上端坐标的方法？
        initialTopBorderPos = topBorder.transform.position.y;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        // 该代码用来探测是否某个stage碰到了某个它不想碰的东西
        Debug.Log(String.Format("Unexpected collision! coll1.name={0}, coll2.name={1}", 
                coll.contacts[0].collider.name,
                coll.contacts[0].otherCollider.name)
        );
    }

    void Update()
    {
        // 圆形模式下不进行更新
        if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
        {
            if (transform.rotation != targetRotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime);
        }
        else
        {    // Vertical
            float deltaPos = 0.1f;
            bool arrived = false;
            if (curPlatformTopPos - curPlatformBottomPos < initialTopBorderPos - platformBottomUpperLimit)
            {    // 如果当前关卡高度太短

                if (GameManager.Instance.gameStatus == GameStatus.StageMovingUp)
                {
                    if (curPlatformTopPos < initialTopBorderPos - deltaPos)
                    {
                        // 首要目标是让关卡顶部到达屏幕上端
                        transform.Translate(0f, moveSpeed * Time.deltaTime, 0f);
                    }
                    else if (curPlatformTopPos > initialTopBorderPos + deltaPos)
                    {
                        // 首要目标是让关卡顶部到达屏幕上端
                        transform.Translate(0f, -moveSpeed * Time.deltaTime, 0f);
                    }
                    else
                    {
                        arrived = true;
                    }
                }
            }
            else
            {
                // 关卡高度足够，就看底部是不在合理区域内
                if (GameManager.Instance.gameStatus == GameStatus.StageMovingUp)
                {     // 注意游戏开始比较特殊，要保证关卡minPos高于upperlimit
                    if (curPlatformBottomPos < platformBottomUpperLimit)
                    {
                        transform.Translate(0f, moveSpeed * Time.deltaTime, 0f);
                    }
                    else
                    {
                        arrived = true;
                    }
                }
                else
                {
                    if (curPlatformBottomPos < platformBottomLowerLimit)
                    {
                        transform.Translate(0f, moveSpeed * Time.deltaTime, 0f);
                    }
                    else if (curPlatformBottomPos > platformBottomUpperLimit)
                    {
                        transform.Translate(0f, -moveSpeed * Time.deltaTime, 0f);
                    }
                    else
                    {
                        arrived = true;
                    }
                }
            }

            if (arrived)
            {
                GameManager.Instance.OnStageMoveComplete();
            }
        }
    }

    void UpdateLocalMinYFromSingleBall(GameItem gameItem)
    {
        // TODO: 这里有一个bug，我们在这里只假设gameItem是小item，没有考虑大item的情况
        if (gameItem.centerGrid.localPos.y < _curFixedBallLocalMinY)
        {
            _curFixedBallLocalMinY = gameItem.centerGrid.localPos.y;
        }
    }

    public void UpdateLocalMinYFromAllFixedBalls()
    {
        // 圆形模式下不进行更新
        if (mainscript.Instance.levelData.stageMoveMode != StageMoveMode.Vertical)
        {
            return;
        }

        _curFixedBallLocalMinY = 9999f;

        foreach (Transform item in mainscript.Instance.gameItemsNode.transform)
        {
            GameObject go = item.gameObject;
            if (go.GetComponent<GameItem>() != null)
            {
                UpdateLocalMinYFromSingleBall(go.GetComponent<GameItem>());
            }
        }
    }

    float VectorAngle(Vector2 from, Vector2 to)
    {
        float angle;
        Vector3 cross = Vector3.Cross(from, to);
        angle = Vector2.Angle(from, to);
        return cross.z > 0 ? -angle : angle;
    }

    public void Rotate(Vector3 ballPos, Vector3 ballDir)
    {
        // 圆形模式下不进行更新
        if (mainscript.Instance.levelData.stageMoveMode != StageMoveMode.Rounded)
        {
            return;
        }

        float angle = VectorAngle(-ballDir, ballPos - transform.position);
        //if(transform.position.x < ballPos.x) angle *= -1;
        targetRotation = transform.rotation * Quaternion.AngleAxis(angle, Vector3.back);
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.kreakWheel);
    }

    public void StartGameMove()
    {
        if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
        {
            transform.position = new Vector3(0f, 1.6f, 0f);
            if (GameManager.Instance.gameStatus == GameStatus.StageMovingUp)
            {
                GameManager.Instance.PreTutorial();
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(-5f, platformBottomUpperLimit, 0f), new Vector3(5f, platformBottomUpperLimit, 0f));
        Gizmos.DrawLine(new Vector3(-5f, platformBottomLowerLimit, 0f), new Vector3(5f, platformBottomLowerLimit, 0f));
    }
}
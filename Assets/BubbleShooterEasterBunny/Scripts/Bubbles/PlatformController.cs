﻿using UnityEngine;
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
    public float lowerMinYLiimt;
    public float upperMinYLiimt;

    // 当前所有fixed balls的最小y值，用来测试关卡是否过线
    private float _curFixedBallLocalMinY;
    private float curPlatformMinY
    {
        get
        {
            float platformMinYWorldSpace = transform.position.y 
                                           + _curFixedBallLocalMinY
                                           - mainscript.Instance.BallColliderRadius;
            return platformMinYWorldSpace;
        }
    }

    private bool curMinYOutOfRange = false;
    private float targetMinYPos;
    private Quaternion targetRotation;

    void Start()
    {
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
            if( transform.rotation != targetRotation )
                transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, Time.deltaTime );
        }
        else    // Vertical
        {
            if (curMinYOutOfRange)
            {
                if (Mathf.Abs(curPlatformMinY - targetMinYPos) > 0.1f)
                {
                    if (curPlatformMinY > targetMinYPos)
                    {
                        transform.Translate(0f, -moveSpeed * Time.deltaTime, 0f);
                    }
                    else
                    {
                        transform.Translate(0f, moveSpeed * Time.deltaTime, 0f);
                    }
                }
                else
                {
                    curMinYOutOfRange = false;
                }
            }
            else
            {
                if (GameManager.Instance.GameStatus == GameStatus.Demo)
                {
                    GameManager.Instance.PreTutorial();
                }
            }
        }
    }

    void UpdateLocalMinYFromSingleBall(GameItem gameItem)
    {
        if (gameItem.grid.localPos.y < _curFixedBallLocalMinY)
        {
            _curFixedBallLocalMinY = gameItem.grid.localPos.y;
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

        foreach( Transform item in mainscript.Instance.gameItemsNode.transform)
        {
            GameObject go = item.gameObject;
            if (go.GetComponent<GameItem>() != null)
            {
                UpdateLocalMinYFromSingleBall(go.GetComponent<GameItem>());
            }
        }

        if (curPlatformMinY > upperMinYLiimt)
        {
            targetMinYPos = upperMinYLiimt;
            curMinYOutOfRange = true;
        }
        else if (curPlatformMinY < lowerMinYLiimt)
        {
            targetMinYPos = lowerMinYLiimt;
            curMinYOutOfRange = true;
        }
        else
        {
            curMinYOutOfRange = false;
        }
        //Debug.Log(string.Format("MinY recalculated! MinY={0}", curFixedBallLocalMinY));
    }

    float VectorAngle(Vector2 from, Vector2 to)
    {
        float angle;
        Vector3 cross=Vector3.Cross(from, to);
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
        targetRotation = transform.rotation*Quaternion.AngleAxis(angle, Vector3.back);
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.kreakWheel );
    }

    public void StartGameMoveUp()
    {
        if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
        {
            transform.position = new Vector3(0f, 0f, 0f);
            if (GameManager.Instance.GameStatus == GameStatus.Demo)
            {
                GameManager.Instance.PreTutorial();
            }
        }
        else
        {
            // 这里我们要将topborder移动到grid下，这样border可以和grid一起移动
            GameObject topBorder = GameObject.Find("TopBorder");
            topBorder.transform.parent = mainscript.Instance.gridsNode.transform;
            topBorder.transform.localPosition = new Vector3(0f, 0f, 0f);

            targetMinYPos = upperMinYLiimt;
            curMinYOutOfRange = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(-5f, upperMinYLiimt, 0f), new Vector3(5f, upperMinYLiimt, 0f));
        Gizmos.DrawLine(new Vector3(-5f, lowerMinYLiimt, 0f), new Vector3(5f, lowerMinYLiimt, 0f));
    }
}
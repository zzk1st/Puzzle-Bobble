using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 该类主要用来管理整个球的平台的运动，包括上下移动，加速下落等
/// </summary>
public class PlatformController : MonoBehaviour
{
    private GameObject platform;

    public float accDropUpperLimit; // 加速下落的上限，platform超过这个就一定会下落
    public float accDropLowerLimit; // 加速下落的下限，platform超过这个就不会加速下落
    public float accDropRate; // 每次下落到accDropLowerLimit的百分之多少
    public float platformBounceForce;
    public int bounceCountToAccDrop;   // 上弹多少次加速下落一次的计数器
    public float accDropSpeed;

    // 当前所有fixed balls的最小y值，用来测试关卡是否过线
    private float _curFixedBallLocalMinY;
    public float curPlatformMinY
    {
        get
        {
            float platformMinYWorldSpace = platform.transform.position.y 
                                           + _curFixedBallLocalMinY
                                           - CreatorBall.Instance.BallColliderRadius;
            return platformMinYWorldSpace;
        }
    }

    private int _bounceCounterForAcc;   // 上弹多少次加速下落一次的计数器

    void Start()
    {
        platform = GameObject.Find("-Grids");
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        // 该代码用来探测是否某个stage碰到了某个它不想碰的东西
        Debug.Log(String.Format("Unexpected collision! coll1.name={0}, coll2.name={1}", 
                                coll.contacts[0].collider.name,
                                coll.contacts[0].otherCollider.name)
                 );
    }

    public void BallRemovedFromPlatform()
    {
        // TODO: 在dropball结束之后运行该函数
        UpdateLocalMinYFromAllFixedBalls();
        float curY = curPlatformMinY;
        if (curY > accDropUpperLimit)
        {
            _bounceCounterForAcc = 0;
            AccDrop((curY - accDropUpperLimit) * 1.1f);
        }
        else if (curY < accDropLowerLimit)
        {
            _bounceCounterForAcc = 0;
            platform.GetComponent<Rigidbody2D>().AddForce(Vector2.up * platformBounceForce);
        }
        else
        {
            _bounceCounterForAcc++;
            if (_bounceCounterForAcc >= bounceCountToAccDrop)
            {
                _bounceCounterForAcc = 0;
                AccDrop((curY - accDropLowerLimit) * accDropRate);
            }
            else
            {
                platform.GetComponent<Rigidbody2D>().AddForce(Vector2.up * platformBounceForce);
            }
        }
    }

    void AccDrop(float dropYDiff)
    {
        StartCoroutine(AccDropCor(dropYDiff));
    }

    IEnumerator AccDropCor(float dropYDiff)
    {
        //float targetPlatformDropY = platform.transform.position.y - (curPlatformMinY - accDropLowerLimit) * accDropRate;
        float targetPlatformDropY = platform.transform.position.y - dropYDiff;
        while (platform.transform.position.y > targetPlatformDropY)
        {
            platform.transform.Translate(0f, -accDropSpeed * Time.deltaTime, 0f);
            yield return new WaitForEndOfFrame();
        }
    }

    public void UpdateLocalMinYFromSingleBall(Ball fixedBall)
    {
        if (fixedBall.grid.localPos.y < _curFixedBallLocalMinY)
        {
            _curFixedBallLocalMinY = fixedBall.grid.localPos.y;
        }
    }

    public void UpdateLocalMinYFromAllFixedBalls()
    {
        _curFixedBallLocalMinY = 9999f;
        GameObject fixedBalls = GameObject.Find( "-Ball" );

        foreach( Transform item in fixedBalls.transform )
        {
            GameObject fixedBall = item.gameObject;
            if (fixedBall.GetComponent<CircleCollider2D>().enabled)
            {
                UpdateLocalMinYFromSingleBall(fixedBall.GetComponent<Ball>());
            }
        }
        //Debug.Log(string.Format("MinY recalculated! MinY={0}", curFixedBallLocalMinY));
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(-5f, accDropUpperLimit, 0f), new Vector3(5f, accDropUpperLimit, 0f));
        Gizmos.DrawLine(new Vector3(-5f, accDropLowerLimit, 0f), new Vector3(5f, accDropLowerLimit, 0f));
    }
}

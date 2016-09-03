using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// 该类主要用来管理整个球的平台的运动，包括上下移动，加速下落等
/// </summary>
public class PlatformController : MonoBehaviour
{
    private GameObject platform;

    public float InitialMoveUpSpeed;

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
                                           - mainscript.Instance.BallColliderRadius;
            return platformMinYWorldSpace;
        }
    }

    private int _bounceCounterForAcc;   // 上弹多少次加速下落一次的计数器

    void Start()
    {
        platform = GameObject.Find("-Grids");
        MoveLevelUp();
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

    public void UpdateLocalMinYFromSingleBall(GameItem gameItem)
    {
        if (gameItem.grid.localPos.y < _curFixedBallLocalMinY)
        {
            _curFixedBallLocalMinY = gameItem.grid.localPos.y;
        }
    }

    public void UpdateLocalMinYFromAllFixedBalls()
    {
        _curFixedBallLocalMinY = 9999f;

        foreach( Transform item in mainscript.Instance.gameItemsNode.transform)
        {
            GameObject go = item.gameObject;
            if (go.GetComponent<GameItem>() != null)
            {
                UpdateLocalMinYFromSingleBall(go.GetComponent<GameItem>());
            }
        }
        //Debug.Log(string.Format("MinY recalculated! MinY={0}", curFixedBallLocalMinY));
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(-5f, accDropUpperLimit, 0f), new Vector3(5f, accDropUpperLimit, 0f));
        Gizmos.DrawLine(new Vector3(-5f, accDropLowerLimit, 0f), new Vector3(5f, accDropLowerLimit, 0f));
    }
    private void MoveLevelUp()
    {
        StartCoroutine( MoveUpDownCor() );
    }

    IEnumerator MoveUpDownCor( bool inGameCheck = false )
    {
        yield return new WaitForSeconds( 0.1f );
        if( !inGameCheck )
            GameManager.Instance.Demo();

        List<float> table = new List<float>();
        // lineY(WorldSpace)，表示整个球的底部应该移到的位置
        float lineY = -1.3f;//GameObject.Find( "GameOverBorder" ).transform.position.y;
        Transform bubbles = mainscript.Instance.gameItemsNode.transform;
        int i = 0;
        foreach( Transform item in bubbles )
        {
            if( !inGameCheck )
            {
                if( item.position.y < lineY )
                {
                    table.Add( item.position.y );
                }
            }
            else
            {
                if( item.position.y > lineY && mainscript.Instance.TopBorder.transform.position.y > 5f )
                {
                    table.Add( item.position.y );
                }
                else if( item.position.y < lineY + 1f )
                {
                    table.Add( item.position.y );
                }
            }
            i++;
        }


        if( table.Count > 0 )
        {
            //if( up ) AddMesh();

            // 球的底部和lineY要求的位置差多少（WorldSpace)
            float targetY = 0;
            table.Sort();
            if( !inGameCheck ) targetY = lineY - table[0] + 2.5f;
            else targetY = lineY - table[0] + 1.5f;
            GameObject Meshes = GameObject.Find( "-Grids" );
            Rigidbody2D rb = Meshes.GetComponent<Rigidbody2D>();
            Vector3 targetPos = Meshes.transform.position + Vector3.up * targetY;
            float startTime = Time.time;
            Vector3 startPos = Meshes.transform.position;
            //float speed = 0.5f;
            //float distCovered = 0;
            while (Math.Abs(Meshes.transform.position.y - targetPos.y) > 0.1f)
            {
                float realSpeed = InitialMoveUpSpeed * Time.deltaTime;
                if (targetPos.y > Meshes.transform.position.y)
                {
                    rb.MovePosition(new Vector2(rb.position.x, rb.position.y + realSpeed));
                    //Meshes.transform.Translate(0f, realSpeed, 0f);
                }
                else
                {
                    rb.MovePosition(new Vector2(rb.position.x, rb.position.y - realSpeed));
                    //Meshes.transform.Translate(0f, -realSpeed, 0f);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        if( GameManager.Instance.GameStatus == GameStatus.Demo )
            GameManager.Instance.PreTutorial();
    }
}

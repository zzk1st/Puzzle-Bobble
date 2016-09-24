using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RainbowBallBoost : MonoBehaviour
{
    private GameItem _gameItem;

    public void Initialize()
    {
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.fireFunc = Fire;
        GetComponent<CircleCollider2D>().radius = mainscript.Instance.BallColliderRadius;
    }

    public void Fire()
    {
        // TODO: 播放发射球的声音
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.shoot);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        OnTriggerEnter2D(coll.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (checkBorderAndContinue(other))
            return;

        StopBall();
        CollectAndDestroyNearbyBalls();
        Explode();
    }

    bool checkBorderAndContinue(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Border"))
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.hitBorder);

            // 圆形模式下topBorder依然起碰撞作用
            if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
            {
                return true;
            }
                
            if (other.gameObject != mainscript.Instance.topBorder)
            {
                return true;
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            return true;
        }

        // Flying的球位置在线以下不碰撞
        if (other.gameObject.layer == LayerMask.NameToLayer("Pot"))
        {
            return true;
        }

        return false;
    }

    void StopBall()
    {
        // 转动圆形关卡
        Vector2 ballVelocity = GetComponent<Rigidbody2D>().velocity;
        mainscript.Instance.platformController.Rotate(transform.position, ballVelocity);
        mainscript.Instance.ballShooter.isFreezing = false;

        // 连接grid
        _gameItem.ConnectToGrid();
        transform.position = _gameItem.centerGrid.transform.position;
    }

    List<GameObject> CollectNearbyBalls()
    {
        List<GameObject> results = new List<GameObject>();
        Grid grid = _gameItem.centerGrid;
        foreach (GameObject nearbyGameItem in grid.GetAdjacentGameItems())
        {
            if (nearbyGameItem.GetComponent<GameItem>().itemType == GameItem.ItemType.Ball)
            {
                List<GameObject> sameColorNearbyBalls = new List<GameObject>();
                sameColorNearbyBalls.Add(nearbyGameItem);
                nearbyGameItem.GetComponent<Ball>().CheckNextNearestColor(sameColorNearbyBalls);

                results.AddRange(sameColorNearbyBalls);
            }
        }

        // 去掉重复元素
        results = results.Distinct().ToList();

        return results;
    }

    void CollectAndDestroyNearbyBalls()
    {
        List<GameObject> ballsToDelete = CollectNearbyBalls();
        mainscript.Instance.DestroyFixedBalls(ballsToDelete);
    }

    void Explode()
    {
        // TODO: 彩虹球爆炸的特效写在这里
        _gameItem.DisconnectFromGrid();
        gameObject.layer = LayerMask.NameToLayer("ExplodedBall");   // 从ball layer移除，防止之后connect nearball时候再连上

        Destroy(gameObject);
    }
}

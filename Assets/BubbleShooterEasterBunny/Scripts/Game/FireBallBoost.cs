using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBallBoost : MonoBehaviour {
    private GameItem _gameItem;
    private int accumulatedCollisionTimes; //累计撞击次数 用于火球 仅允许一次碰撞 一旦达到2则火球自身销毁 则清零

    public void Initialize()
    {
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.fireFunc = Fire;
        GetComponent<CircleCollider2D>().radius = CoreManager.Instance.BallColliderRadius;
        accumulatedCollisionTimes = 0;
    }

    public void Fire()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.shoot);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        OnTriggerEnter2D(coll.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Border"))
        {
            accumulatedCollisionTimes++;
        }

        if (accumulatedCollisionTimes == 2 || other.gameObject == CoreManager.Instance.topBorder)
        {
            //达到碰撞上限，毁掉，同时发射器可以发射啦～
            CoreManager.Instance.ballShooter.isLocked = false;
            Explode();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("FixedBall"))
        {
            List<GameObject> ballsToDelete = new List<GameObject>();
            ballsToDelete.Add(other.gameObject);
            CoreManager.Instance.DestroyGameItems(ballsToDelete);
        }

        return;
    }

    void Explode()
    {
        // TODO: 彩虹球爆炸的特效写在这里
        gameObject.layer = LayerMask.NameToLayer("ExplodedBall");   // 从ball layer移除，防止之后connect nearball时候再连上
        Destroy(gameObject);
    }
}

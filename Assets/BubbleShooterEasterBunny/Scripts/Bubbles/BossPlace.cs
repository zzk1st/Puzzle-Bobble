using UnityEngine;
using System.Collections;

public class BossPlace : MonoBehaviour {
    private GameItem _gameItem;
    private int curShootCount;
    private int changeColorShootCount;
    private BallColor hitBallColor;
    public GameObject hitBall;
    private bool _isAlive;
    public bool isAlive
    {
        get { return _isAlive; }
        set
        {
            _isAlive = value;
            if (_isAlive)
            {
                ResetHitBallColor();
            }
        }
    }

    public void Initialize()
    {
        isAlive = false;
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.ConnectToGrid();
        mainscript.Instance.onBallShooterUnlocked += OnBallShooterUnlocked;
    }

    void OnDestroy()
    {
        // 我们在OnDestroy里需要unscribe from event handler, 否则随后event handler会调用已经被Destroy了的gameObject
        mainscript.Instance.onBallShooterUnlocked -= OnBallShooterUnlocked;
    }

    void OnBallShooterUnlocked()
    {
        if (isAlive)
        {
            curShootCount++;
            if (curShootCount >= changeColorShootCount)
            {
                ResetHitBallColor();
            }
        }
    }

    void ResetHitBallColor()
    {
        if (this == null)
        {
            int a = 1;
            Debug.Log(a);
        }

        if (isAlive && this.gameObject)
        {
            curShootCount = 0;
            changeColorShootCount = Random.Range(2,5);
            hitBallColor = mainscript.Instance.GetRandomCurStageColor();
            hitBall.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.ballColorSprites[(int) hitBallColor];
            // TODO: 更新boss颜色的动画
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        OnTriggerEnter2D(coll.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Ball otherBall = other.gameObject.GetComponent<Ball>();
        if (otherBall)
        {
            if (otherBall.color == hitBallColor)
            {
                // 销毁掉碰撞的球
                mainscript.Instance.ballShooter.isLocked = false;
                Destroy(other.gameObject);

                MissionManager.Instance.GainBossPoint();
                Explode();
                mainscript.Instance.BossMoveToNextPlace();
            }
        }
    }

    void Explode()
    {
        _gameItem.DisconnectFromGrid();
        // TODO: 玻璃破碎特效等
        Destroy(gameObject);
    }

    public void UpdateHitColor()
    {
        if (!mainscript.Instance.curStageColors.Contains(hitBallColor))
        {
            ResetHitBallColor();
        }
    }
}

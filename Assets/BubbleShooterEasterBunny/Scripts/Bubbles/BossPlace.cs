using UnityEngine;
using System.Collections;

public class BossPlace : MonoBehaviour {
    public float moveSpeed;
    public bool isAlive;
    public GameObject breakGlassParticle;

    private GameItem _gameItem;

    private int curShootCount;
    private int changeColorShootCount;

    private BallColor hitBallColor;

    private GameObject hitBall;
    private GameObject glass;
    private GameObject boss;
    private GameObject vortex;
    private GameObject background;


    public void Initialize()
    {
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.ConnectToGrid();

        hitBall = transform.FindChild("HitBall").gameObject;
        glass = transform.FindChild("HexagonForeground").gameObject;
        vortex = transform.FindChild("Vortex").gameObject;
        background = transform.FindChild("HexagonBackground").gameObject;

        // 初始状态，要给bossplace设置成空的
        SetEmptyPlace();

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

    public void SetAlive()
    {
        isAlive = true;

        hitBall.SetActive(true);
        vortex.SetActive(true);
        background.SetActive(true);

        ResetHitBallColor();
    }

    void SetEmptyPlace()
    {
        isAlive = false;

        hitBall.SetActive(false);
        vortex.SetActive(false);
        background.SetActive(false);
    }

    void ResetHitBallColor()
    {
        if (isAlive && this.gameObject)
        {
            curShootCount = 0;
            changeColorShootCount = Random.Range(5,8);
            hitBallColor = mainscript.Instance.GetRandomCurStageColor();
            SetBossPlaceColor(hitBallColor);
            // TODO: 更新boss颜色的动画
        }
    }

    void SetBossPlaceColor(BallColor newColor)
    {
        hitBall.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.ballColorSprites[(int) newColor];
        vortex.GetComponent<SpriteRenderer>().color = ColorManager.Instance.ballColors[(int) newColor];
        background.GetComponent<SpriteRenderer>().color = ColorManager.Instance.ballBackgroundColors[(int) newColor];
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
            }
            else
            {
                ResetHitBallColor();
            }
        }
    }

    void Explode()
    {
        _gameItem.DisconnectFromGrid();
        BossManager.Instance.RemoveLastBossPlace();
        // 创建玻璃
        GameObject glassParticle = Instantiate(breakGlassParticle, transform.position, transform.rotation) as GameObject;
        Destroy(glassParticle, 2f);
        // 播放音效
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.bossHit);
        // boss移动到下一个place
        BossManager.Instance.BossMoveToLastPlace();

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

using UnityEngine;
using System.Collections;
using System.Threading;
using InitScriptName;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum BallColor
{
    blue = 1,
    green,
    red,
    violet,
    yellow
}

public enum ItemType
{
    NormalColorBall = 1,
    Animal
}

public class Ball : MonoBehaviour
{
    public enum BallState
    {
        Waiting,
        ReadyToShoot,
        Flying,
        Fixed,
        Exploded,
        Dropped
    }

    private GameItem _gameItem;

    public Sprite[] colorSprites;
    public Sprite[] boostSprites;
    public BallColor color;

    private int _number;
    public int number
    {
        get { return _number; }
        set
        {
            _number = value;
            // 数字模式，不开启
            //gameObject.GetComponentInChildren<TextMesh>().text = _number.ToString();
        }
    }

    public Grid grid
    {
        get { return _gameItem.centerGrid; }
    }

    public BallState state;

    public float LaunchForce;

    //	 public OTSprite sprite;                    // This star's sprite class
    Vector3 forceVect;
    public float dropFadeTime;

    //	private OTSpriteBatch spriteBatch = null;
    float bottomBoarderY;  //低于此线就不能发射球
    float destroyBoarderY; //低于此线就销毁飞行的球
    bool isPaused;
    public AudioClip swish;
    public AudioClip pops;
    public AudioClip join;
    private static int fireworks;
    private bool touchedTop;
    private bool animStarted;

    private float ballAnimForce = 0.15f;    // 播放碰撞动画时，给每个球施加的力，力越大位移越大
    private float ballAnimSpeed = 5f;       // 播放碰撞动画的速度，数越大播放越快

    private float ballFallXSpeedRange = 3f;       // 球掉落时候水平速度的随机范围
    private float ballFallRotationSpeed = 0.0f;
    private float ballFallRotationSpeedRange = 600f;

    private int accumulatedCollisionTimes = 0; //累计撞击次数 用于火球 仅允许一次碰撞 一旦达到2则火球自身销毁 则清零

    private int hitBug;
    public int HitBug
    {
        get { return hitBug; }
        set
        {
            hitBug = value;
        }
    }

    private GameObject ballHighlightGO;
    private GameObject ballPicGO;

    public PhysicsMaterial2D fallingBallMaterial;

    // 初始化方法，在instantiate后手动调用
    public void Initialize()
    {
        // 初始化references
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.startFallFunc = StartFall;
        ballHighlightGO = transform.GetChild(0).gameObject;
        ballPicGO = transform.GetChild(1).gameObject;
    }

    // Use this for initialization
    void Start ()
    {
        isPaused = mainscript.Instance.isPaused;
        bottomBoarderY = GameObject.Find("BottomBorder").transform.position.y; //获取生死线的Y坐标
        destroyBoarderY = GameObject.Find("DestroyBorder").transform.position.y; //获取生死线的Y坐标
    }

    public void SetTypeAndColor(LevelData.ItemType itemType)
    {
        if (itemType == LevelData.ItemType.AnimalSingle)
        {
            _gameItem.itemType = GameItem.ItemType.Animal;
        }
        else if (itemType == LevelData.ItemType.RainbowBall)
        {
            _gameItem.itemType = GameItem.ItemType.RainbowBall;
            color = (BallColor)LevelData.ItemType.Rainbow;
            gameObject.tag = "rainbow";
            ballPicGO.GetComponent<SpriteRenderer>().sprite = boostSprites[0];
        }
        else if (itemType == LevelData.ItemType.FireBall)
        {
            _gameItem.itemType = GameItem.ItemType.FireBall;
            color = (BallColor)LevelData.ItemType.Fire;
            gameObject.tag = "fire";
            ballPicGO.GetComponent<SpriteRenderer>().sprite = boostSprites[1];
        }
        else
        {
            color = (BallColor)itemType;
            gameObject.tag = "" + color;

            foreach (Sprite item in colorSprites)
            {
                if (item.name == "" + color + "_ball")
                {
                    ballPicGO.GetComponent<SpriteRenderer>().sprite = item;
                }
                else if (item.name == "" + color + "_hl")
                {
                    ballHighlightGO.GetComponent<SpriteRenderer>().sprite = item;
                }
            }
        }

    }

    public void Fire()
    {
        // ClickOnGUI检查是否单击在gui上
        // newBall表示这是不是一个准备发射的ball
        if (!ClickOnGUI() &&
            state == BallState.ReadyToShoot && 
            !mainscript.Instance.gameOver && GameManager.Instance.gameStatus == GameStatus.Playing)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
            if (pos.y > bottomBoarderY && !mainscript.StopControl)
            {
                GetComponent<CircleCollider2D> ().enabled = true;

                // 取消circle collider的isTrigger, 以便触发ball和border的碰撞检测
                CircleCollider2D coll = GetComponent<CircleCollider2D>();
                coll.isTrigger = false;
                // 在这里给发射的ball赋予一个force，产生初速度
                Vector2 direction = pos - transform.position;
                GetComponent<Rigidbody2D>().AddForce(direction.normalized * LaunchForce, ForceMode2D.Force);

                // TODO: 播放发射球的声音
                SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot(SoundBase.Instance.shoot);

                state = BallState.Flying;
            }
        }
    }

    public void PushBallAFterWin()
    {
        StartFall();
        // 给球一个较大的初速度，其他部分和startfall一样
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-ballFallXSpeedRange, ballFallXSpeedRange) , 10.0f);
    }

    void Update()
    {
        if (state == BallState.Dropped)
        {
            ballPicGO.transform.Rotate(new Vector3(0f, 0f, ballFallRotationSpeed * Time.deltaTime));
        }
        if (state == BallState.Flying && gameObject.transform.position.y < destroyBoarderY)
        {
            GameObject.Find("BallShooter").GetComponent<BallShooter>().isFreezing = false;
            Destroy(gameObject);
        }
    }

    bool ClickOnGUI()
    {
        UnityEngine.EventSystems.EventSystem ct = UnityEngine.EventSystems.EventSystem.current;

        if (ct.IsPointerOverGameObject ())
            return true;
        return false;
    }

    public GameObject FindInArrayGameObject(List<GameObject> b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return obj;
        }
        return null;
    }


    public bool FindInArray(List<GameObject> b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return true;
        }
        return false;
    }

    public List<GameObject> AddFrom(List<GameObject> b, List<GameObject> b2)
    {
        foreach (GameObject obj in b) {
            if (!FindInArray (b2, obj)) {
                b2.Add (obj);
            }
        }
        return b2;
    }

    public void CheckNextNearestColor(List<GameObject> results)
    {
        foreach (GameObject nearbyBall in grid.GetAdjacentGameItems())
        {
            if (nearbyBall.tag == tag && !FindInArray(results, nearbyBall))
            {
                results.Add(nearbyBall);
                nearbyBall.GetComponent<Ball>().CheckNextNearestColor(results);
            }
        }
    }

    //和CheckNextNearestColor一样 只是忽略第一次的源球
    //而把源球周围所有球以及与他们同样颜色的球都收集起来
    public void CheckNearbyColor(List<GameObject> results)
    {
        foreach (GameObject nearbyBall in grid.GetAdjacentGameItems())
        {
            results.Add(nearbyBall);
            nearbyBall.GetComponent<Ball>().CheckNextNearestColor(results);
        }
    }

    public void SearchNumberPath(ref List<GameObject> longestPath, ref List<GameObject> currentPath, bool increasePath)
    {
        currentPath.Add(gameObject);
        bool isEndPoint = true;

        foreach(GameObject adjacentBallGO in grid.GetAdjacentGameItems())
        {
            Ball adjacentBall = adjacentBallGO.GetComponent<Ball>();
            if ((increasePath && adjacentBall.number == number + 1) || (!increasePath && adjacentBall.number == number - 1))
            {
                isEndPoint = false;

                adjacentBall.SearchNumberPath(ref longestPath, ref currentPath, increasePath);
            }
        }

        if (isEndPoint)
        {
            if (longestPath.Count < currentPath.Count)
            {
                longestPath = new List<GameObject>(currentPath);
            }
        }

        currentPath.Remove(gameObject);
    }

    public void StartFall()
    {
        enabled = true;
        state = BallState.Dropped;

        // 特殊情况：最后撒彩球的时候也要调用startfall，此时球都没有attach在grid上，所以需要检测一下
        if (_gameItem.isConnectedToGrid())
            _gameItem.DisconnectFromGrid();

        // 从ball layer移除，防止之后连接nearby balls
        gameObject.layer = LayerMask.NameToLayer("FallingBall");
        gameObject.tag = "Ball";
        if (gameObject.GetComponent<Rigidbody2D> () == null)
            gameObject.AddComponent<Rigidbody2D>();

        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        rb.mass = 1f;
        rb.isKinematic = false;
        rb.gravityScale = 1;
        rb.velocity = new Vector2(Random.Range(-ballFallXSpeedRange, ballFallXSpeedRange), 0f);
        ballFallRotationSpeed = Random.Range(-ballFallRotationSpeedRange, ballFallRotationSpeedRange);

        CircleCollider2D coll = gameObject.GetComponent<CircleCollider2D>();
        coll.sharedMaterial = fallingBallMaterial;
        coll.enabled = true;
        coll.isTrigger = false;
        coll.radius = mainscript.Instance.BallRealRadius; // 这里我们要将ball碰撞半径扩大，增加和蜘蛛碰撞效果
    }

    void PlayHitAnim()
    {
        Hashtable animTable = mainscript.Instance.animTable;
        animTable.Clear();
        animTable.Add(gameObject, gameObject);
        // start hit animation
        // TODO: 改进HitAnim，变成有慢动作的效果，并且增加newball本身的anim，删掉FixedUpdate()里强制设成meshPos
        // 现在的newball直接回到meshPos，太丑了
        PlayHitAnim(grid.pos, animTable);
    }

    public void PlayHitAnim(Vector3 newBallPos, Hashtable animTable)
    {
        // 对该球周围的所有球（该球自己除外），调用每个球的PlayHitAnimCorStart
        List<GameObject> adjacentGameItems = GridManager.Instance.GetAdjacentGameItems(gameObject);
        foreach (GameObject gameItem in adjacentGameItems) {
            if (gameItem.GetComponent<GameItem>().itemType == GameItem.ItemType.Ball)
            {
                if (!animTable.ContainsKey (gameItem) && gameItem != gameObject && animTable.Count < 20)
                    gameItem.GetComponent<Ball>().PlayHitAnimCorStart(newBallPos, animTable);
            }
        }
    }

    public void PlayHitAnimCorStart(Vector3 newBallPos, Hashtable animTable)
    {
        // 对该球先协程调用播放动画，然后递归调用PlayHitAnim(), 播放周围球动画，直到animTable满50
        if (!animStarted) {
            StartCoroutine (PlayHitAnimCor (newBallPos, animTable));
            PlayHitAnim (newBallPos, animTable);
        }
    }

    public IEnumerator PlayHitAnimCor(Vector3 newBallPos, Hashtable animTable)
    {
        animStarted = true;
        animTable.Add (gameObject, gameObject);
        if (tag == "chicken")
            yield break;
        yield return new WaitForFixedUpdate ();
        // TODO: 对transform.position随机扰动, 形成一个更随机的效果
        float dist = Vector3.Distance (transform.position, newBallPos);
        float force = 1 / dist + ballAnimForce;
        newBallPos = transform.position - newBallPos;
        if (transform.parent == null) {
            animStarted = false;
            yield break;
        }

        // 给动画施加一个随机扰动的角度
        float randAngle = (Random.value - 0.5f) * 80f;
        newBallPos = Quaternion.AngleAxis (transform.parent.parent.rotation.eulerAngles.z + randAngle, Vector3.back) * newBallPos;
        newBallPos = newBallPos.normalized;
        newBallPos = transform.localPosition + (newBallPos * force / 10);

        float startTime = Time.time;
        Vector3 startPos = transform.localPosition;
        // 该参数控制动画速度
        float speed = force * ballAnimSpeed;
        float distCovered = 0;
        while (distCovered < 1 && !float.IsNaN (newBallPos.x)) {
            distCovered = (Time.time - startTime) * speed;
            if (this == null)
                yield break;
            //   if( destroyed ) yield break;
            if (state == BallState.Dropped) {
                //           transform.localPosition = startPos;
                yield break;
            }
            transform.localPosition = Vector3.Lerp (startPos, newBallPos, distCovered);
            yield return new WaitForEndOfFrame ();
        }
        Vector3 lastPos = transform.localPosition;
        startTime = Time.time;
        distCovered = 0;
        while (distCovered < 1 && !float.IsNaN (newBallPos.x)) {
            distCovered = (Time.time - startTime) * speed;
            if (this == null)
                yield break;
            if (state == BallState.Dropped) {
                //      transform.localPosition = startPos;
                yield break;
            }
            transform.localPosition = Vector3.Lerp (lastPos, startPos, distCovered);
            yield return new WaitForEndOfFrame ();
        }
        transform.localPosition = startPos;
        animStarted = false;
    }

    void OnCollisionEnter2D (Collision2D coll)
    {
        OnTriggerEnter2D(coll.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameObject.name == "fireball")
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Border"))
                accumulatedCollisionTimes += 1;
            if (accumulatedCollisionTimes == 2)
            {
                //达到碰撞上限，毁掉，同时发射器可以发射啦～
                GameObject.Find("BallShooter").GetComponent<BallShooter>().isFreezing = false;
                accumulatedCollisionTimes = 0;
                mainscript.Instance.checkBall = null;
                Destroy(gameObject);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("FixedBall"))
            {
                Destroy(other.gameObject);
                mainscript.Instance.checkBall = gameObject;
            }
            return;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Border"))
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.hitBorder);

            // 圆形模式下topBorder依然起碰撞作用
            if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
            {
                return;
            }
                
            if (other.gameObject != mainscript.Instance.topBorder)
            {
                return;
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            return;
        }

        if (state != BallState.Flying)
        {
            return;
        }

        // Flying的球位置在线以下不碰撞
        if (other.gameObject.layer == LayerMask.NameToLayer("Pot"))
        {
            return;
        }

        StopBall();
    }

    void StopBall()
    {
        mainscript.Instance.lastStopBallPos = gameObject.transform.position;

        GameObject.Find("BallShooter").GetComponent<BallShooter>().isFreezing = false;
        state = BallState.Fixed;
        this.enabled = false;
        _gameItem.ConnectToGrid();

        mainscript.Instance.checkBall = gameObject;

        Vector2 ballVelocity = GetComponent<Rigidbody2D>().velocity;
        // 删掉RigidBody2D，彻底让mesh接管运动
        Destroy(GetComponent<Rigidbody2D>());
        // 设置circle collider
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        cc.offset = Vector2.zero;
        cc.isTrigger = true;

        //iTween.MoveTo(gameObject, iTween.Hash("position", grid.pos, "speed", speedBeforeColl.magnitude));
        transform.position = grid.pos;

        // 转动圆形关卡
        mainscript.Instance.platformController.Rotate(transform.position, ballVelocity);

        PlayHitAnim();
        mainscript.Instance.ballShooter.SetStageCollidersMode(BallShooter.StageCollidersMode.AimMode);
    }

    public void SplashDestroy()
    {
        Destroy (gameObject);
    }

    public void Explode(float delayedExplodeTime, int score)
    {
        if (grid.Row == 0)
        {
            MissionManager.Instance.GainTargetStar(grid);
        }

        GetComponent<GameItem>().DisconnectFromGrid();
        GetComponent<CircleCollider2D>().enabled = false;    //删掉CircleCollider，防止再碰撞检测
        gameObject.layer = LayerMask.NameToLayer("ExplodedBall");   // 从ball layer移除，防止之后connect nearball时候再连上

        StartCoroutine(ExplodeCor(delayedExplodeTime, score));
    }

    public void growUpPlaySound ()
    {
        Invoke ("growUpDelayed", 1 / (float)Random.Range (2, 10));
    }

    void playPop ()
    {
    }


    IEnumerator ExplodeCor(float delayedExplodeTime, int score)
    {
        // 延迟球爆炸
        yield return new WaitForSeconds(delayedExplodeTime);

        float startTime = Time.time;
        float endTime = Time.time + 0.1f;
        Vector3 tempPosition = transform.localScale;
        Vector3 targetPrepare = transform.localScale * 1.2f;

        // 让ball有个膨胀的效果
        while (!isPaused && endTime > Time.time)
        {
            transform.localScale = Vector3.Lerp (tempPosition, targetPrepare, (Time.time - startTime) * 10);
            // WaitForEndOfFrame的作用是等到渲染全部完成但没放到屏幕之前
            yield return new WaitForEndOfFrame();
        }
        // 播放完膨胀的动画之后让球及其高光都不再显示
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;

        // TODO: 播放爆炸的动画，注意：1. 爆炸之后要销毁 2. 爆炸应挂在当前grid下面

        // 播放分数动画在球爆炸之后
        ScoreManager.Instance.PopupComboScore(score, transform.position);

        Destroy (gameObject, 1);
    }

    public void ShowFirework ()
    {
        fireworks++;
        if (fireworks <= 2)
            SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.hit);

    }
}

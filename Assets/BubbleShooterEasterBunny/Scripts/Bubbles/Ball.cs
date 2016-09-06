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
        get { return _gameItem.grid; }
    }

    public BallState state;

    public float LaunchForce;

    //	 public OTSprite sprite;                    // This star's sprite class
    Vector3 forceVect;
    public float startTime;
    public float dropFadeTime;

    //	private OTSpriteBatch spriteBatch = null;
    float bottomBoarderY;  //低于此线就不能发射球
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

    // 初始化方法，在instantiate后手动调用
    public void Initialize()
    {
        // 初始化references
        _gameItem = gameObject.GetComponent<GameItem>();
        ballHighlightGO = transform.GetChild(0).gameObject;
        ballPicGO = transform.GetChild(1).gameObject;
    }

    // Use this for initialization
    void Start ()
    {
        isPaused = mainscript.Instance.isPaused;
        bottomBoarderY = GameObject.Find("BottomBorder").transform.position.y; //获取生死线的Y坐标
    }

    public void SetTypeAndColor(LevelData.ItemType itemType)
    {
        if (itemType == LevelData.ItemType.Animal)
        {
            _gameItem.itemType = GameItem.ItemType.Animal;
        }
        else
        {
            
        }

        color = (BallColor) itemType;
        gameObject.tag = "" + color;

        foreach (Sprite item in colorSprites)
        {
            if( item.name == "bubble_" + color )
            {
                ballPicGO.GetComponent<SpriteRenderer>().sprite = item;
            }
            else if (item.name == "bubble_" + color + "_highlight")
            {
                ballHighlightGO.GetComponent<SpriteRenderer>().sprite = item;
            }
        }

    }

    public void Fire()
    {
        // ClickOnGUI检查是否单击在gui上
        // newBall表示这是不是一个准备发射的ball
        if (!ClickOnGUI() &&
            state == BallState.ReadyToShoot && 
            !mainscript.Instance.gameOver && GameManager.Instance.GameStatus == GameStatus.Playing)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
            if (pos.y > bottomBoarderY && !mainscript.StopControl)
            {
                GetComponent<CircleCollider2D> ().enabled = true;

                startTime = Time.time;
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

    void Update()
    {
        if (state == BallState.Dropped)
        {
            ballPicGO.transform.Rotate(new Vector3(0f, 0f, ballFallRotationSpeed * Time.deltaTime));
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
        _gameItem.DisconnectFromGrid();

        // 从ball layer移除，防止之后连接nearby balls
        gameObject.layer = LayerMask.NameToLayer("FallingBall");
        gameObject.tag = "Ball";
        if (gameObject.GetComponent<Rigidbody2D> () == null)
            gameObject.AddComponent<Rigidbody2D>();

        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-ballFallXSpeedRange, ballFallXSpeedRange), 0f);
        ballFallRotationSpeed = Random.Range(-ballFallRotationSpeedRange, ballFallRotationSpeedRange);

        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        gameObject.GetComponent<CircleCollider2D>().isTrigger = false;
        gameObject.GetComponent<CircleCollider2D>().radius = mainscript.Instance.BallRealRadius; // 这里我们要将ball碰撞半径扩大，增加和蜘蛛碰撞效果
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
        List<GameObject> adjacentGameItems = mainscript.Instance.gridManager.GetAdjacentGameItems(gameObject);
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
    }

    public void SplashDestroy ()
    {
        Destroy (gameObject);
    }

    public void Explode()
    {
        StartCoroutine(ExplodeCor());
    }

    public void growUpPlaySound ()
    {
        Invoke ("growUpDelayed", 1 / (float)Random.Range (2, 10));
    }

    void playPop ()
    {
    }


    IEnumerator ExplodeCor()
    {
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

        Destroy (gameObject, 1);
    }

    public void ShowFirework ()
    {
        fireworks++;
        if (fireworks <= 2)
            SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.hit);

    }
}

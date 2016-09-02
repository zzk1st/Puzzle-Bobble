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
    public bool flying;
    public float startTime;
    public float dropFadeTime;

    //	private OTSpriteBatch spriteBatch = null;
    GameObject ballsNode;      // 所有ball的parent
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

    private GameObject fireTrail;

    // 初始化方法，在instantiate后手动调用
    public void Initialize()
    {
        // 初始化references
        _gameItem = gameObject.GetComponent<GameItem>();
        ballsNode = GameObject.Find("-Ball");
    }

    // Use this for initialization
    void Start ()
    {
        isPaused = mainscript.Instance.isPaused;
        bottomBoarderY = GameObject.Find("BottomBorder").transform.position.y; //获取生死线的Y坐标
    }

    public void SetTypeAndColor(LevelData.ItemType itemType)
    {
        if (itemType == LevelData.ItemType.chicken)
        {
            _gameItem.itemType = GameItem.ItemType.Animal;
        }
        else
        {
            
        }

        this.color = (BallColor) itemType;
        gameObject.tag = "" + color;
        foreach (Sprite item in colorSprites)
        {
            if( item.name == "ball_sprite_" + color )
            {
                GetComponent<SpriteRenderer>().sprite = item;
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

                flying = true;
                startTime = Time.time;
                // 取消circle collider的isTrigger, 以便触发ball和border的碰撞检测
                CircleCollider2D coll = GetComponent<CircleCollider2D>();
                coll.isTrigger = false;
                // 在这里给发射的ball赋予一个force，产生初速度
                Vector2 direction = pos - transform.position;
                GetComponent<Rigidbody2D>().AddForce(direction.normalized * LaunchForce, ForceMode2D.Force);

                // 生成球轨迹的prefab, 同时播放声音
                BallFX ballFX = mainscript.Instance.ballFXManager.ballFXs[color];
                fireTrail = (GameObject)Instantiate(ballFX.fireTrailPrefab, gameObject.transform.position, Quaternion.identity);
                fireTrail.transform.parent = transform;
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(ballFX.fireAudio);

                state = BallState.Flying;
            }
        }
    }

    bool ClickOnGUI()
    {
        UnityEngine.EventSystems.EventSystem ct = UnityEngine.EventSystems.EventSystem.current;

        if (ct.IsPointerOverGameObject ())
            return true;
        return false;
    }

    public GameObject findInArrayGameObject(List<GameObject> b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return obj;
        }
        return null;
    }


    public bool findInArray(List<GameObject> b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return true;
        }
        return false;
    }

    public List<GameObject> addFrom(List<GameObject> b, List<GameObject> b2)
    {
        foreach (GameObject obj in b) {
            if (!findInArray (b2, obj)) {
                b2.Add (obj);
            }
        }
        return b2;
    }

    public void checkNextNearestColor(List<GameObject> results)
    {
        foreach (GameObject nearbyBall in grid.GetAdjacentGameItems())
        {
            if (nearbyBall.tag == tag && !findInArray(results, nearbyBall))
            {
                results.Add(nearbyBall);
                nearbyBall.GetComponent<Ball>().checkNextNearestColor(results);
            }
        }
    }

    public void searchNumberPath(ref List<GameObject> longestPath, ref List<GameObject> currentPath, bool increasePath)
    {
        currentPath.Add(gameObject);
        bool isEndPoint = true;

        foreach(GameObject adjacentBallGO in grid.GetAdjacentGameItems())
        {
            Ball adjacentBall = adjacentBallGO.GetComponent<Ball>();
            if ((increasePath && adjacentBall.number == number + 1) || (!increasePath && adjacentBall.number == number - 1))
            {
                isEndPoint = false;

                adjacentBall.searchNumberPath(ref longestPath, ref currentPath, increasePath);
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
        enabled = false;
        state = BallState.Dropped;
        transform.SetParent(null);

        // 从ball layer移除，防止之后连接nearby balls
        gameObject.layer = LayerMask.NameToLayer("FallingBall");
        gameObject.tag = "Ball";
        if (gameObject.GetComponent<Rigidbody2D> () == null)
            gameObject.AddComponent<Rigidbody2D>();

        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        iTween.FadeTo(gameObject, 0f, dropFadeTime);

        Destroy(gameObject, dropFadeTime);
    }

    public bool checkNearestBall(List<GameObject> ballList)
    {
        // 算法：维护一个数组，将所有有嫌疑的ball都放到数组里，然后递归调用该方法
        //      一旦出现一个在边界中或者已在controlArray中的ball，表明目前怀疑组都是clean的，清除当前b array全部球
        //      否则，继续递归调用
        if (grid.Row == 0)
        {
            mainscript.Instance.controlArray = addFrom(ballList, Camera.main.GetComponent<mainscript> ().controlArray);
            ballList.Clear();
            return true;    /// don't destroy
        }

        if (findInArray(mainscript.Instance.controlArray, gameObject))
        {
            ballList.Clear();
            return true;
        } /// don't destroy
        /*int targetHash = gameObject.GetHashCode();
        bool hasSeen = false;
        foreach (GameObject ball in ballList)
        {
            if (ball.GetHashCode() == targetHash)
            {
                hasSeen = true;
            }
        }
        if (!hasSeen)*/
            ballList.Add(gameObject);
        List<GameObject> nearbyBalls = grid.GetAdjacentGameItems();
        foreach (GameObject nearbyBall in nearbyBalls)
        {
            if (nearbyBall.gameObject.layer == LayerMask.NameToLayer("FixedBall"))
            {
                if (!findInArray(ballList, nearbyBall.gameObject))
                {
                    if (nearbyBall.GetComponent<Ball>().checkNearestBall(ballList))
                        return true;
                }
            }
        }
        return false;

    }

    void pullToMesh()
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
        List<GameObject> fixedBalls = mainscript.Instance.gridManager.GetAdjacentGameItems(gameObject);
        foreach (GameObject ball in fixedBalls) {
            if (!animTable.ContainsKey (ball) && ball != gameObject && animTable.Count < 20)
                ball.GetComponent<Ball> ().PlayHitAnimCorStart(newBallPos, animTable);
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
        if (other.gameObject.name.Contains("ball"))
        {
            //当一个ball作为发射ball的时候，ball script是enabled的
            //一旦它碰到了其它ball（stopBall设成true），那么这个ball script就会被disable
            //所以判断一个ball script是不是enabled，就能知道这是不是个固定的ball
            // 注意script被disable之后，其变量仍然可用，函数依然可调用，只是callback不起作用了
            if (!other.gameObject.GetComponent<Ball>().enabled)
            {
                StopBall();
            }
        } 
        else if (other.gameObject.name == "TopBorder")
        {
            StopBall();
        }
    }

    void StopBall()
    {
        state = BallState.Fixed;

        transform.parent = ballsNode.transform;
        gameObject.layer = LayerMask.NameToLayer("FixedBall");
        this.enabled = false;

        flying = false;
        Vector2 speedBeforeColl = GetComponent<Rigidbody2D>().velocity;
        // 删掉RigidBody2D，彻底让mesh接管运动
        Destroy(GetComponent<Rigidbody2D>());

        // 设置circle collider
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        cc.offset = Vector2.zero;
        cc.isTrigger = true;

        Destroy(fireTrail, 0.5f);

        mainscript.Instance.gridManager.ConnectGameItemToGrid(gameObject);
        mainscript.Instance.platformController.UpdateLocalMinYFromSingleBall(this);

        mainscript.Instance.checkBall = gameObject;

        iTween.MoveTo(gameObject, iTween.Hash("position", grid.pos, "speed", speedBeforeColl.magnitude));

        pullToMesh();
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
        // 播放完膨胀的动画之后让球及其阴影都不再显示
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        BallFX ballFX = mainscript.Instance.ballFXManager.ballFXs[color];
        GameObject explosion = (GameObject)Instantiate (ballFX.explosionPrefab, gameObject.transform.position, Quaternion.identity);
        if (grid != null)
            explosion.transform.parent = grid.gameObject.transform;
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(ballFX.explosionAudio);

        Destroy (gameObject, 1);

    }

    public void ShowFirework ()
    {
        fireworks++;
        if (fireworks <= 2)
            SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.hit);

    }




}

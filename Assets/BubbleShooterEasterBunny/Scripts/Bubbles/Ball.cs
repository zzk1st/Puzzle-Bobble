using UnityEngine;
using System.Collections;
using System.Threading;
using InitScriptName;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Ball : MonoBehaviour
{
    public enum BallState {
        Waiting,
        ReadyToShoot,
        Flying,
        Fixed,
        Exploded,
        Dropped
    };

    public BallState state;

    public float LaunchForce;
    public Sprite[] sprites;
    public Sprite[] boosts;

    //	 public OTSprite sprite;                    // This star's sprite class
    Vector3 forceVect;
    public bool flying;
    public float startTime;
    public GameObject mesh;         //表示new ball发生碰撞后被attach到的mesh
    Vector2[] meshArray;
    float row;
    string str;
    public Vector3 targetPosition;
    public float dropFadeTime;

    public ArrayList nearbyBalls = new ArrayList();
    //	private OTSpriteBatch spriteBatch = null;
    GameObject ballsNode;      // 所有ball的parent
    float bottomBoarderY;  //低于此线就不能发射球
    bool isPaused;
    public AudioClip swish;
    public AudioClip pops;
    public AudioClip join;
    Vector3 meshPos;        // 表示当被发射的ball发生碰撞后，应该去的位置，而这个位置是mesh决定的
    public Vector3 LocalMeshPos;        // meshPos对应的grid local position，用于计算关卡最小y值
    bool rayTarget;
    Animation rabbit;
    private static int fireworks;
    private bool touchedTop;
    private bool animStarted;

    private float ballAnimForce = 0.15f;    // 播放碰撞动画时，给每个球施加的力，力越大位移越大
    private float ballAnimSpeed = 5f;       // 播放碰撞动画的速度，数越大播放越快

    // Use this for initialization
    void Start ()
    {
        rabbit = GameObject.Find ("Rabbit").gameObject.GetComponent<Animation>();
        ballsNode = GameObject.Find("-Ball");
        isPaused = mainscript.Instance.isPaused;
        bottomBoarderY = GameObject.Find("BottomBorder").transform.position.y; //获取生死线的Y坐标
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
                rabbit.Play ("rabbit_move");
                SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.swish [0]);
                GetComponent<CircleCollider2D> ().enabled = true;

                flying = true;
                startTime = Time.time;
                // 取消circle collider的isTrigger, 以便触发ball和border的碰撞检测
                CircleCollider2D coll = GetComponent<CircleCollider2D>();
                coll.isTrigger = false;
                // 在这里给发射的ball赋予一个force，产生初速度
                Vector2 direction = pos - transform.position;
                GetComponent<Rigidbody2D>().AddForce(direction.normalized * LaunchForce, ForceMode2D.Force);

                state = BallState.Flying;
            }
        }
    }

    public void ConnectToGrid(Grid grid)
    {
        grid.Busy = this.gameObject;
        meshPos = grid.gameObject.transform.position;
        LocalMeshPos = grid.gameObject.transform.localPosition;
        GetComponent<bouncer>().offset = grid.offset;

        mainscript.Instance.platformController.UpdateLocalMinYFromSingleBall(this);
    }

    public void DisconnectFromCurrentGrid()
    {
        if (mesh != null)
        {
            Grid grid = mesh.GetComponent<Grid>();
            grid.Busy = null;
        }
    }

    bool ClickOnGUI()
    {
        UnityEngine.EventSystems.EventSystem ct = UnityEngine.EventSystems.EventSystem.current;

        if (ct.IsPointerOverGameObject ())
            return true;
        return false;
    }

    public GameObject findInArrayGameObject(ArrayList b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return obj;
        }
        return null;
    }


    public bool findInArray(ArrayList b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return true;
        }
        return false;
    }

    public ArrayList addFrom(ArrayList b, ArrayList b2)
    {
        foreach (GameObject obj in b) {
            if (!findInArray (b2, obj)) {
                b2.Add (obj);
            }
        }
        return b2;
    }

    public void checkNextNearestColor(ArrayList results)
    {
        foreach (GameObject nearbyBall in nearbyBalls)
        {
            if (nearbyBall.tag == tag && !findInArray(results, nearbyBall))
            {
                results.Add(nearbyBall);
                nearbyBall.GetComponent<Ball>().checkNextNearestColor(results);
            }
        }
    }

    public void StartFall()
    {
        enabled = false;
        state = BallState.Dropped;
        transform.SetParent(null);
        DisconnectFromCurrentGrid();

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

    IEnumerator FlyToTarget()
    {
        Vector3 targetPos = new Vector3 (2.3f, 6, 0);
        if (mainscript.Instance.TargetCounter1 < mainscript.Instance.TotalTargets)
            mainscript.Instance.TargetCounter1++;

        AnimationCurve curveX = new AnimationCurve (new Keyframe (0, transform.position.x), new Keyframe (0.5f, targetPos.x));
        AnimationCurve curveY = new AnimationCurve (new Keyframe (0, transform.position.y), new Keyframe (0.5f, targetPos.y));
        curveY.AddKey (0.2f, transform.position.y - 1);
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        float distCovered = 0;
        while (distCovered < 0.6f) {
            distCovered = (Time.time - startTime);
            transform.position = new Vector3 (curveX.Evaluate (distCovered), curveY.Evaluate (distCovered), 0);
            transform.Rotate (Vector3.back * 10);
            yield return new WaitForEndOfFrame ();
        }
        Destroy (gameObject);

    }

    public bool checkNearestBall(ArrayList ballList)
    {
        // 算法：维护一个数组，将所有有嫌疑的ball都放到数组里，然后递归调用该方法
        //      一旦出现一个在边界中或者已在controlArray中的ball，表明目前怀疑组都是clean的，清除当前b array全部球
        //      否则，继续递归调用
        if (mainscript.Instance.TopBorder.transform.position.y - transform.position.y <= 0.5f)
        {
            Camera.main.GetComponent<mainscript> ().controlArray = addFrom(ballList, Camera.main.GetComponent<mainscript> ().controlArray);
            ballList.Clear ();
            return true;    /// don't destroy
        }

        if (findInArray(Camera.main.GetComponent<mainscript>().controlArray, gameObject))
        {
            ballList.Clear();
            return true;
        } /// don't destroy

        ballList.Add(gameObject);
        foreach (GameObject nearbyBall in nearbyBalls) {
            if (nearbyBall != gameObject && nearbyBall != null) {
                if (nearbyBall.gameObject.layer == LayerMask.NameToLayer("FixedBall"))
                {
                    if (!findInArray (ballList, nearbyBall.gameObject))
                    {
                        if (nearbyBall.GetComponent<Ball>().checkNearestBall(ballList))
                            return true;
                    }
                }
            }
        }
        return false;

    }

    public void connectNearbyBalls()
    {
        nearbyBalls.Clear();

        //连接周围的ball，结果记录在ball自己的nearBalls里（一个ArrayList）
        int layerMask = 1 << LayerMask.NameToLayer("FixedBall");
        Collider2D[] nearbyBallsColl = Physics2D.OverlapCircleAll(transform.position, 2.2f * CreatorBall.Instance.BallRealRadius, layerMask);

        foreach (Collider2D nearbyBallColl in nearbyBallsColl)
        {
            if (nearbyBallColl.gameObject != gameObject)
            {
                nearbyBalls.Add(nearbyBallColl.gameObject);
            }
        }
    }

    IEnumerator pullToMesh(Transform otherBall = null)
    {
        CreatorBall.Instance.EnableGridColliders();

        float searchRadius = CreatorBall.Instance.BallColliderRadius;
        // while循环用来把当前ball和grid连接起来
        bool foundMesh = false;
        while (!foundMesh)
        {
            Vector3 centerPoint = transform.position;
            // 注意在这里，系统根据球的位置试图找到所有的collider2D，实际是在寻找与之对应的mesh
            // 只寻找layer 10的，就是全部的mesh
            Collider2D[] meshesCollided = Physics2D.OverlapCircleAll(centerPoint, searchRadius, 1 << LayerMask.NameToLayer("Mesh"));
            foreach (Collider2D meshCollided in meshesCollided)
            {
                if (meshCollided.gameObject.GetComponent<Grid>().Busy == null)
                {
                    foundMesh = true;
                    ConnectToGrid(meshCollided.gameObject.GetComponent<Grid>());
                    transform.parent = ballsNode.transform;
                    // TODO: 找到一种更好的办法让击打的ball移动到meshPos
                    transform.position = meshPos;
                    break;
                }
            }

            if (!foundMesh)
                searchRadius += 0.2f;
        }

        // 将grid连接好之后，将当前ball和临近的ball都连接起来，
        // 在消除检测发生之前，我们只需要连接该球和周围球
        connectNearbyBalls();
        foreach (GameObject adjacentBall in nearbyBalls)
        {
            adjacentBall.GetComponent<Ball>().connectNearbyBalls();
        }

        if (foundMesh)
        {
            Hashtable animTable = mainscript.Instance.animTable;
            animTable.Clear();
            // start hit animation
            // TODO: 改进HitAnim，变成有慢动作的效果，并且增加newball本身的anim，删掉FixedUpdate()里强制设成meshPos
            // 现在的newball直接回到meshPos，太丑了
            PlayHitAnim (meshPos, animTable);
        }

        CreatorBall.Instance.OffGridColliders();

        yield return new WaitForEndOfFrame();
    }

    public void PlayHitAnim(Vector3 newBallPos, Hashtable animTable)
    {
        // 对该球周围的所有球（该球自己除外），调用每个球的PlayHitAnimCorStart
        int layerMask = 1 << LayerMask.NameToLayer ("FixedBall");
        Collider2D[] fixedBalls = Physics2D.OverlapCircleAll (transform.position, 0.5f, layerMask);
        // 该参数控制球受力大小
        foreach (Collider2D obj in fixedBalls) {
            if (!animTable.ContainsKey (obj.gameObject) && obj.gameObject != gameObject && animTable.Count < 20)
                obj.GetComponent<Ball> ().PlayHitAnimCorStart (newBallPos, animTable);
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
        if (other.gameObject.name.Contains ("ball") && flying)
        {
            //当一个ball作为发射ball的时候，ball script是enabled的
            //一旦它碰到了其它ball（stopBall设成true），那么这个ball script就会被disable
            //所以判断一个ball script是不是enabled，就能知道这是不是个固定的ball
            // 注意script被disable之后，其变量仍然可用，函数依然可调用，只是callback不起作用了
            if (!other.gameObject.GetComponent<Ball>().enabled)
            {
                StopBall(true, other.transform);
            }
        } 
        else if (other.gameObject.name == "TopBorder" && flying)
        {
            if (LevelData.mode == ModeGame.Vertical || LevelData.mode == ModeGame.Animals)
            {
                transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
                StopBall();
            }
        }
    }

    void StopBall(bool pulltoMesh = true, Transform otherBall = null)
    {
        state = BallState.Fixed;
        gameObject.layer = LayerMask.NameToLayer("FixedBall");
        Camera.main.GetComponent<mainscript> ().checkBall = gameObject;
        this.enabled = false;

        flying = false;
        // 设置circle collider
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        cc.offset = Vector2.zero;
        cc.isTrigger = true;
        // 删掉RigidBody2D，彻底让mesh接管运动
        Destroy(GetComponent<Rigidbody2D>());

        //推动其它ball
        //注意：将当前ball摆到正确位置的代码也在这里
        //if( pulltoMesh )
        StartCoroutine(pullToMesh(otherBall));
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
        // 播放完膨胀的动画之后让球不再显示
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        //      yield return new WaitForSeconds(0.01f );
        GameObject prefab = Resources.Load ("Particles/BubbleExplosion") as GameObject;

        GameObject explosion = (GameObject)Instantiate (prefab, gameObject.transform.position + Vector3.back * 20f, Quaternion.identity);
        if (mesh != null)
            explosion.transform.parent = mesh.transform;

        Destroy (gameObject, 1);

    }

    public void ShowFirework ()
    {
        fireworks++;
        if (fireworks <= 2)
            SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.hit);

    }




}

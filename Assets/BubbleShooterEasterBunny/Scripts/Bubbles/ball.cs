using UnityEngine;
using System.Collections;
using System.Threading;
using InitScriptName;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ball : MonoBehaviour
{
    public float LaunchForce;
    public Sprite[] sprites;
    public Sprite[] boosts;
    public bool isTarget;

    //	 public OTSprite sprite;                    // This star's sprite class
    Vector2 speed = // Star movement speed / second
        new Vector2 (250, 250);
    public Vector3 target;
    Vector2 worldPos;
    Vector3 forceVect;
    public bool setTarget;
    public float startTime;
    float duration = 1.0f;
    public GameObject mesh;         //表示new ball发生碰撞后被attach到的mesh
    Vector2[] meshArray;
    public bool findMesh;
    Vector3 dropTarget;
    float row;
    string str;
    public bool newBall;
    float mTouchOffsetX;
    float mTouchOffsetY;
    float xOffset;
    float yOffset;
    public Vector3 targetPosition;
    bool stopedBall;
    private bool destroyed;
    public bool NotSorting;
    ArrayList fireballArray = new ArrayList ();

    public bool Destroyed {
        get { return destroyed; }
        set {
            if (value) {
                GetComponent<CircleCollider2D> ().enabled = false;
                GetComponent<SpriteRenderer> ().enabled = false;

            }
            destroyed = value;
        }
    }

    public ArrayList nearBalls = new ArrayList ();
    //	private OTSpriteBatch spriteBatch = null;
    GameObject Meshes;      // 所有mesh的parent
    public int countNEarBalls;
    float topBorder;
    float leftBorder;
    float rightBorder;
    bool gameOver;
    bool isPaused;
    public AudioClip swish;
    public AudioClip pops;
    public AudioClip join;
    Vector3 meshPos;        // 表示当被发射的ball发生碰撞后，应该去的位置，而这个位置是mesh决定的
    public Vector3 LocalMeshPos;        // meshPos对应的grid local position，用于计算关卡最小y值
    bool dropedDown;
    bool rayTarget;
    public bool falling;
    Animation rabbit;
    private bool fireBall;
    private static int fireworks;
    private bool touchedTop;
    private bool touchedSide;
    private int fireBallLimit = 10;
    private bool launched;
    private bool animStarted;

    private float ballAnimForce = 0.15f;    // 播放碰撞动画时，给每个球施加的力，力越大位移越大
    private float ballAnimSpeed = 5f;       // 播放碰撞动画的速度，数越大播放越快

    // Use this for initialization
    void Start ()
    {
        rabbit = GameObject.Find ("Rabbit").gameObject.GetComponent<Animation> ();
        //  sprite = GetComponent<OTSprite>();
        //sprite.passive = true;
        //	sprite.onCollision = OnCollision;
        dropTarget = transform.position;
        //		spriteBatch = GameObject.Find("SpriteBatch").GetComponent<OTSpriteBatch>();    
        Meshes = GameObject.Find ("-Ball");
        // Add the custom tile action controller to this tile
        //      sprite.AddController(new MyActions(this));  

        topBorder = Camera.main.GetComponent<mainscript> ().topBorder;
        leftBorder = Camera.main.GetComponent<mainscript> ().leftBorder;
        rightBorder = Camera.main.GetComponent<mainscript> ().rightBorder;
        gameOver = Camera.main.GetComponent<mainscript> ().gameOver;
        isPaused = Camera.main.GetComponent<mainscript> ().isPaused;
        dropedDown = Camera.main.GetComponent<mainscript> ().dropingDown;
    }

    IEnumerator AllowLaunchBall ()
    {
        yield return new WaitForSeconds (2);
        mainscript.StopControl = false;

    }

    // Update is called once per frame
    void Update ()
    {
        // 发射ball
        if (Input.GetMouseButtonUp (0))
        {
            GameObject ball = gameObject;
            // ClickOnGUI检查是否单击在gui上
            // newBall表示这是不是一个准备发射的ball
            if (!ClickOnGUI (Input.mousePosition) && !launched && !ball.GetComponent<ball> ().setTarget && mainscript.Instance.newBall2 == null /*&& mainscript.Instance.newBall == null*/ && newBall && !Camera.main.GetComponent<mainscript> ().gameOver && (GamePlay.Instance.GameStatus == GameState.Playing || GamePlay.Instance.GameStatus == GameState.WaitForChicken)) {
                Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
                worldPos = pos;
                if (worldPos.y > -1.5f && !mainscript.StopControl) {
                    launched = true;
                    rabbit.Play ("rabbit_move");
                    SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.swish [0]);
                    mTouchOffsetX = (worldPos.x - ball.transform.position.x); //+ MathUtils.random(-10, 10);
                    mTouchOffsetY = (worldPos.y - ball.transform.position.y);
                    xOffset = (float)Mathf.Cos (Mathf.Atan2 (mTouchOffsetY, mTouchOffsetX));
                    yOffset = (float)Mathf.Sin (Mathf.Atan2 (mTouchOffsetY, mTouchOffsetX));
                    speed = new Vector2 (xOffset, yOffset);
                    if (!fireBall)
                        GetComponent<CircleCollider2D> ().enabled = true;
                    target = worldPos;

                    setTarget = true;
                    startTime = Time.time;
                    dropTarget = transform.position;
                    InitScript.Instance.BoostActivated = false;
                    mainscript.Instance.newBall = gameObject;
                    mainscript.Instance.newBall2 = gameObject;
                    // 取消circle collider的isTrigger, 以便触发ball和border的碰撞检测
                    CircleCollider2D coll = GetComponent<CircleCollider2D>();
                    coll.isTrigger = false;
                    // 在这里给发射的ball赋予一个force，产生初速度
                    GetComponent<Rigidbody2D> ().AddForce ((target - dropTarget).normalized * LaunchForce, ForceMode2D.Force);

                    //Debug.DrawLine( DrawLine.waypoints[0], target );
                    //Debug.Break();
                }
            }
        }

        // 也不知道这行干什么用的，但注释掉似乎完全不影响，似乎和fireball有关
        if (setTarget)
            triggerEnter ();

        if ((transform.position.y <= -10 || transform.position.y >= 5) && fireBall && !Destroyed) {
            mainscript.Instance.CheckFreeChicken ();
            setTarget = false;
            launched = false;
            DestroySingle (gameObject, 0.00001f);
            mainscript.Instance.checkBall = gameObject;
        }
    }

    public void ConnectToGrid(Grid grid)
    {
        grid.Busy = this.gameObject;
        meshPos = grid.gameObject.transform.position;
        LocalMeshPos = grid.gameObject.transform.localPosition;
        GetComponent<bouncer> ().offset = grid.offset;

        mainscript.Instance.UpdateLocalMinYFromSingleBall(this);
    }

    bool ClickOnGUI (Vector3 mousePos)
    {
        UnityEngine.EventSystems.EventSystem ct
           = UnityEngine.EventSystems.EventSystem.current;


        if (ct.IsPointerOverGameObject ())
            return true;
        return false;
    }

    public void SetBoost (BoostType boostType)
    {
        tag = "Ball";
        GetComponent<SpriteRenderer> ().sprite = boosts [(int)boostType - 1];
        if (boostType == BoostType.ColorBallBoost) {
        }
        if (boostType == BoostType.FireBallBoost) {
            GetComponent<SpriteRenderer> ().sortingOrder = 10;
            GetComponent<CircleCollider2D> ().enabled = false; 
            fireBall = true;
            fireballArray.Add (gameObject);
        }
    }

    void FixedUpdate ()
    {
        if (Camera.main.GetComponent<mainscript> ().gameOver)
            return;

        // stoppedBall是由pushToMesh()做出判断
        if (stopedBall) {
            // 如果stoppedBall了，就在这里强制将ball的位置更新成meshPos
            transform.position = meshPos;

            stopedBall = false;
            if (newBall) {
                //在前边条件下，如果又是个new ball，那么就把它变成一个固定的ball
                // disable ball script，设置layer等
                newBall = false;
                gameObject.layer = 9;
                Camera.main.GetComponent<mainscript> ().checkBall = gameObject;
                this.enabled = false;
            }

        }

    }

    public GameObject findInArrayGameObject (ArrayList b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return obj;
        }
        return null;
    }


    public bool findInArray (ArrayList b, GameObject destObj)
    {
        foreach (GameObject obj in b) {

            if (obj == destObj)
                return true;
        }
        return false;
    }

    public ArrayList addFrom (ArrayList b, ArrayList b2)
    {
        foreach (GameObject obj in b) {
            if (!findInArray (b2, obj)) {
                b2.Add (obj);
            }
        }
        return b2;
    }

    public void changeNearestColor ()
    {
        GameObject gm = GameObject.Find ("Creator");
        int ballLayer = LayerMask.NameToLayer("Ball");
        Collider2D[] fixedBalls = Physics2D.OverlapCircleAll (transform.position, 0.5f, 1 << ballLayer);
        foreach (Collider2D obj in fixedBalls) {
            gm.GetComponent<creatorBall> ().createBall (obj.transform.position);
            Destroy (obj.gameObject);
        }

    }


    public void checkNextNearestColor (ArrayList b, int counter)
    {
        //		Debug.Log(b.Count);
        Vector3 distEtalon = transform.localScale;
        //		GameObject[] meshes = GameObject.FindGameObjectsWithTag(tag);
        //		foreach(GameObject obj in meshes) {
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        Collider2D[] meshes = Physics2D.OverlapCircleAll (transform.position, 1.0f, layerMask);
        foreach (Collider2D obj1 in meshes) {
            if (obj1.gameObject.tag == tag) {
                GameObject obj = obj1.gameObject;
                float distTemp = Vector3.Distance (transform.position, obj.transform.position);
                if (distTemp <= 1.0f) {
                    if (!findInArray (b, obj)) {
                        counter++;
                        b.Add (obj);
                        obj.GetComponent<bouncer> ().checkNextNearestColor (b, counter);
                        //		destroy();
                        //obj.GetComponent<mesh>().checkNextNearestColor();
                        //		obj.GetComponent<mesh>().destroy();
                    }
                }
            }
        }
    }

    public void checkNearestColor ()
    {
        // 该方法用来查找是否有其它ball与之相连，形成三个或以上的ball，如果有则将其销毁
        int counter = 0;
        GameObject[] fixedBalls = GameObject.FindObjectsOfType (typeof(GameObject)) as GameObject[];			// change color tag of the rainbow
        foreach (GameObject obj in fixedBalls) {
            if (obj.layer == 9 && (obj.name.IndexOf ("Rainbow") > -1)) {
                obj.tag = tag;
            }
        }

        ArrayList b = new ArrayList ();
        b.Add (gameObject);
        Vector3 distEtalon = transform.localScale;
        GameObject[] meshes = GameObject.FindGameObjectsWithTag (tag);
        foreach (GameObject obj in meshes) {    													// detect the same color balls
            float distTemp = Vector3.Distance (transform.position, obj.transform.position);
            if (distTemp <= 0.9f && distTemp > 0) {
                b.Add (obj);
                obj.GetComponent<bouncer> ().checkNextNearestColor (b, counter);
            }
        }
        mainscript.Instance.countOfPreparedToDestroy = b.Count;
        if (b.Count >= 3) {
            mainscript.Instance.ComboCount++;
            // 在这里调用coroutine将其销毁
            destroy (b, 0.00001f);
            mainscript.Instance.CheckFreeChicken ();

            // 给整个关卡一个向上的force
            GameObject Meshes = GameObject.Find( "-Meshes" );
            Rigidbody2D rb = Meshes.GetComponent<Rigidbody2D>();
            rb.AddForce(Vector2.up * mainscript.Instance.StageBounceForce);
        }
        if (b.Count < 3) {
            Camera.main.GetComponent<mainscript> ().bounceCounter++;
            mainscript.Instance.ComboCount = 0;
        }

        b.Clear ();
        Camera.main.GetComponent<mainscript> ().dropingDown = false;
        FindLight (gameObject);

    }


    public void StartFall ()
    {
        enabled = false;

        if (mesh != null)
            mesh.GetComponent<Grid> ().Busy = null;
        if (gameObject == null)
            return;
        if (LevelData.mode == ModeGame.Vertical && isTarget) {
            Instantiate (Resources.Load ("Prefabs/TargetStar"), gameObject.transform.position, Quaternion.identity);
        } else if (LevelData.mode == ModeGame.Animals && isTarget) {
            StartCoroutine (FlyToTarget ());
        }
        setTarget = false;
        transform.SetParent (null);
        gameObject.layer = 13;
        gameObject.tag = "Ball";
        if (gameObject.GetComponent<Rigidbody2D> () == null)
            gameObject.AddComponent<Rigidbody2D> ();
        gameObject.GetComponent<Rigidbody2D> ().isKinematic = false;
        gameObject.GetComponent<Rigidbody2D> ().gravityScale = 1;
        gameObject.GetComponent<Rigidbody2D> ().fixedAngle = false;
        gameObject.GetComponent<Rigidbody2D> ().velocity = gameObject.GetComponent<Rigidbody2D> ().velocity + new Vector2 (Random.Range (-2, 2), 0);
        gameObject.GetComponent<CircleCollider2D> ().enabled = true;
        gameObject.GetComponent<CircleCollider2D> ().isTrigger = false;
        GetComponent<ball> ().falling = true;

    }

    IEnumerator FlyToTarget ()
    {
        Vector3 targetPos = new Vector3 (2.3f, 6, 0);
        if (mainscript.Instance.TargetCounter1 < mainscript.Instance.TotalTargets)
            mainscript.Instance.TargetCounter1++;

        AnimationCurve curveX = new AnimationCurve (new Keyframe (0, transform.position.x), new Keyframe (0.5f, targetPos.x));
        AnimationCurve curveY = new AnimationCurve (new Keyframe (0, transform.position.y), new Keyframe (0.5f, targetPos.y));
        curveY.AddKey (0.2f, transform.position.y - 1);
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        float speed = 0.2f;
        float distCovered = 0;
        while (distCovered < 0.6f) {
            distCovered = (Time.time - startTime);
            transform.position = new Vector3 (curveX.Evaluate (distCovered), curveY.Evaluate (distCovered), 0);
            transform.Rotate (Vector3.back * 10);
            yield return new WaitForEndOfFrame ();
        }
        Destroy (gameObject);

    }

    public bool checkNearestBall (ArrayList b)
    {
        // 算法：维护一个数组，将所有有嫌疑的ball都放到数组里，然后递归调用该方法
        //      一旦出现一个在边界中或者已在controlArray中的ball，表明目前怀疑组都是clean的，清除当前b array全部球
        //      否则，继续递归调用
        if ((mainscript.Instance.TopBorder.transform.position.y - transform.position.y <= 0.5f && LevelData.mode != ModeGame.Rounded) || (LevelData.mode == ModeGame.Rounded && tag == "chicken")) {
            Camera.main.GetComponent<mainscript> ().controlArray = addFrom (b, Camera.main.GetComponent<mainscript> ().controlArray);
            b.Clear ();
            return true;    /// don't destroy
        }
        if (findInArray (Camera.main.GetComponent<mainscript> ().controlArray, gameObject)) {
            b.Clear ();
            return true;
        } /// don't destroy
        b.Add (gameObject);
        foreach (GameObject obj in nearBalls) {
            if (obj != gameObject && obj != null) {
                if (obj.gameObject.layer == 9) {
                    //if(findInArray(Camera.main.GetComponent<mainscript>().controlArray, obj.gameObject)){b.Clear(); return true;} /// don't destroy
                    //else{
                    float distTemp = Vector3.Distance (transform.position, obj.transform.position);
                    if (distTemp <= 0.9f && distTemp > 0) {
                        if (!findInArray (b, obj.gameObject)) {
                            //print( gameObject + " " + distTemp );
                            Camera.main.GetComponent<mainscript> ().arraycounter++;
                            if (obj.GetComponent<ball> ().checkNearestBall (b))
                                return true;
                        }
                    }
                    //}
                }
            }
        }
        return false;

    }

    public void connectNearBalls ()
    {
        //连接周围的ball，结果记录在ball自己的nearBalls里（一个ArrayList）
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        Collider2D[] fixedBalls = Physics2D.OverlapCircleAll (transform.position, 0.5f, layerMask);
        nearBalls.Clear ();

        foreach (Collider2D obj in fixedBalls) {
            if (nearBalls.Count <= 7) {
                nearBalls.Add (obj.gameObject);
            }
        }
        countNEarBalls = nearBalls.Count;
    }

    IEnumerator pullToMesh (Transform otherBall = null)
    {
        //	AudioSource.PlayClipAtPoint(join, new Vector3(5, 1, 2));
        GameObject busyMesh = null;
        float searchRadius = 0.2f;
        while (findMesh) {
            Vector3 centerPoint = transform.position;
            // 注意在这里，系统根据球的位置试图找到所有的collider2D，实际是在寻找与之对应的mesh
            // 只寻找layer 10的，就是全部的mesh
            Collider2D[] fixedBalls1 = Physics2D.OverlapCircleAll (centerPoint, 0.1f, 1 << 10);

            foreach (Collider2D obj1 in fixedBalls1) {
                if (obj1.gameObject.GetComponent<Grid> () == null)
                    DestroySingle (gameObject, 0.00001f);
                else if (obj1.gameObject.GetComponent<Grid> ().Busy == null) {
                    findMesh = false;
                    stopedBall = true;
                    ConnectToGrid(obj1.gameObject.GetComponent<Grid>());
                }
            }
            if (findMesh) {
                Collider2D[] fixedBalls = Physics2D.OverlapCircleAll (centerPoint, searchRadius, 1 << 10);  //meshes
                foreach (Collider2D obj in fixedBalls) {
                    if (obj.gameObject.GetComponent<Grid> () == null)
                        DestroySingle (gameObject, 0.00001f);
                    else if (obj.gameObject.GetComponent<Grid> ().Busy == null) {
                        findMesh = false;
                        stopedBall = true;
                        ConnectToGrid(obj.gameObject.GetComponent<Grid>());
                    }
                }
            }
				
            transform.parent = Meshes.transform;
            // 删掉RigidBody2D，彻底让mesh接管运动
            Destroy (GetComponent<Rigidbody2D> ());
            //  rigidbody2D.isKinematic = true;
            yield return new WaitForFixedUpdate ();
            // StopCoroutine( "pullToMesh" );
            dropTarget = transform.position;

            if (findMesh)
                searchRadius += 0.2f;

            yield return new WaitForFixedUpdate ();
        }
        mainscript.Instance.connectNearBallsGlobal ();
        //   FindLight( gameObject );

        if (!findMesh)
        {
            Hashtable animTable = mainscript.Instance.animTable;
            animTable.Clear ();
            // start hit animation
            // TODO: 改进HitAnim，变成有慢动作的效果，并且增加newball本身的anim，删掉FixedUpdate()里强制设成meshPos
            // 现在的newball直接回到meshPos，太丑了
            PlayHitAnim (meshPos, animTable);
        }
        creatorBall.Instance.OffGridColliders ();

        yield return new WaitForSeconds (0.5f);

        // StartCoroutine( mainscript.Instance.destroyAloneBall() );
    }

    public void PlayHitAnim (Vector3 newBallPos, Hashtable animTable)
    {
        // 对该球周围的所有球（该球自己除外），调用每个球的PlayHitAnimCorStart
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        Collider2D[] fixedBalls = Physics2D.OverlapCircleAll (transform.position, 0.5f, layerMask);
        // 该参数控制球受力大小
        foreach (Collider2D obj in fixedBalls) {
            if (!animTable.ContainsKey (obj.gameObject) && obj.gameObject != gameObject && animTable.Count < 20)
                obj.GetComponent<ball> ().PlayHitAnimCorStart (newBallPos, animTable);
        }
    }

    public void PlayHitAnimCorStart (Vector3 newBallPos, Hashtable animTable)
    {
        // 对该球先协程调用播放动画，然后递归调用PlayHitAnim(), 播放周围球动画，直到animTable满50
        if (!animStarted) {
            StartCoroutine (PlayHitAnimCor (newBallPos, animTable));
            PlayHitAnim (newBallPos, animTable);
        }
    }

    public IEnumerator PlayHitAnimCor (Vector3 newBallPos, Hashtable animTable)
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
            if (falling) {
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
            if (falling) {
                //      transform.localPosition = startPos;
                yield break;
            }
            transform.localPosition = Vector3.Lerp (lastPos, startPos, distCovered);
            yield return new WaitForEndOfFrame ();
        }
        transform.localPosition = startPos;
        animStarted = false;
    }

    void OnTriggerStay2D (Collider2D other)
    {
        if (findMesh && other.gameObject.layer == 9) {
            //	StartCoroutine(pullToMesh());
        }
    }

    public void FindLight (GameObject activatedByBall)
    {
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        Collider2D[] fixedBalls = Physics2D.OverlapCircleAll (transform.position, 0.5f, layerMask);
        int i = 0;
        foreach (Collider2D obj in fixedBalls) {
            i++;
            if (i <= 10) {
                if ((obj.gameObject.tag == "light") && GamePlay.Instance.GameStatus == GameState.Playing) {
                    DestroySingle (obj.gameObject);
                    DestroySingle (activatedByBall);
                } else if ((obj.gameObject.tag == "cloud") && GamePlay.Instance.GameStatus == GameState.Playing) {
                    obj.GetComponent<ColorBallScript> ().ChangeRandomColor ();
                }

            }
        }
    }


    void OnCollisionEnter2D (Collision2D coll)
    {
        OnTriggerEnter2D (coll.collider);
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        // stop
        if (other.gameObject.name.Contains ("ball") && setTarget) {
            //当一个ball作为发射ball的时候，ball script是enabled的
            //一旦它碰到了其它ball（stopBall设成true），那么这个ball script就会被disable
            //所以判断一个ball script是不是enabled，就能知道这是不是个固定的ball
            // 注意script被disable之后，其变量仍然可用，函数依然可调用，只是callback不起作用了
            if (!other.gameObject.GetComponent<ball> ().enabled) {
                if ((other.gameObject.tag == "black_hole") && GamePlay.Instance.GameStatus == GameState.Playing) {
                    SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.black_hole);
                    DestroySingle (gameObject);
                }

                if (!fireBall)
                    StopBall (true, other.transform);
                else {
                    if (other.gameObject.tag.Contains ("animal") || other.gameObject.tag.Contains ("empty") || other.gameObject.tag.Contains ("chicken"))
                        return;
                    fireBallLimit--;
                    if (fireBallLimit > 0)
                        DestroySingle (other.gameObject, 0.000000000001f);
                    else {
                        StopBall ();
                        destroy (fireballArray, 0.000000000001f);

                    }


                }
                //           FindLight(gameObject);
            }
            //          }
        } else if (other.gameObject.name == "TopBorder" && setTarget) {
            if (LevelData.mode == ModeGame.Vertical || LevelData.mode == ModeGame.Animals) {
                if (!findMesh) {
                    transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
                    StopBall ();

                    if (fireBall) {
                        destroy (fireballArray, 0.000000000001f);
                    }
                }

            }
        }

    }

    void StopBall (bool pulltoMesh = true, Transform otherBall = null)
    {
        launched = true;
        mainscript.lastBall = gameObject.transform.position;
        creatorBall.Instance.EnableGridColliders ();
        target = Vector2.zero;
        setTarget = false;
        findMesh = true;
        GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        cc.offset = Vector2.zero;
        cc.isTrigger = true;

        if (GetComponent<SpriteRenderer> ().sprite == boosts [0]) {  //color ball boost
            DestroyAround ();
        }

        //推动其它ball
        //注意：将当前ball摆到正确位置的代码也在这里
        //if( pulltoMesh )
        StartCoroutine (pullToMesh (otherBall));
    }


    void DestroyAround ()
    {
        ArrayList b = new ArrayList ();
        b.Add (gameObject);
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        Collider2D[] meshes = Physics2D.OverlapCircleAll (transform.position, 1f, layerMask);
        foreach (Collider2D obj1 in meshes) {
            GameObject obj = obj1.gameObject;
            if (!findInArray (b, obj) && obj.tag != "chicken" && !obj.tag.Contains ("animal") && !obj.tag.Contains ("empty")) {
                b.Add (obj);
            }
        }
        if (b.Count >= 0) {
            mainscript.Instance.ComboCount++;
            destroy (b, 0.001f);
        }

    }

    void DestroyLine ()
    {

        ArrayList b = new ArrayList ();
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        RaycastHit2D[] fixedBalls = Physics2D.LinecastAll (transform.position + Vector3.left * 10, transform.position + Vector3.right * 10, layerMask);
        foreach (RaycastHit2D item in fixedBalls) {
            if (!findInArray (b, item.collider.gameObject)) {
                b.Add (item.collider.gameObject);
            }
        }


        if (b.Count >= 0) {
            mainscript.Instance.ComboCount++;
            mainscript.Instance.destroy (b);
        }

        mainscript.Instance.StartCoroutine (mainscript.Instance.destroyAloneBall ());
    }


    public void CheckBallCrossedBorder ()
    {
        if (Physics2D.OverlapCircle (transform.position, 0.1f, 1 << 14) != null || Physics2D.OverlapCircle (transform.position, 0.1f, 1 << 17) != null) {
            DestroySingle (gameObject, 0.00001f);
        }

    }

    void triggerEnter ()
    {
        //// check if we collided with a left block and adjust our speed and rotation accordingly
        if (transform.position.x <= leftBorder && target.x < 0 && !touchedSide && fireBall) {
            //  touchedSide = true;
            Invoke ("CanceltouchedSide", 0.1f);
            target = new Vector2 (target.x * -1, target.y);
            GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D> ().velocity.x * -1, GetComponent<Rigidbody2D> ().velocity.y);
        }
        // check if we collided with a right block and adjust our speed and rotation accordingly
        if (transform.position.x >= rightBorder && target.x > 0 && !touchedSide && fireBall) {
            //  touchedSide = true;
            Invoke ("CanceltouchedSide", 0.1f);
            target = new Vector2 (target.x * -1, target.y);
            GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D> ().velocity.x * -1, GetComponent<Rigidbody2D> ().velocity.y);
        }
//             check if we collided with a right block and adjust our speed and rotation accordingly
        if (transform.position.y >= topBorder && target.y > 0 && LevelData.mode == ModeGame.Rounded && !touchedTop) {
            touchedTop = true;
            // target = new Vector2( target.x, -target.y );
            GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D> ().velocity.x, GetComponent<Rigidbody2D> ().velocity.y * -1);
            //         print( target.y );
        }
    }

    void CanceltouchedSide ()
    {
        touchedSide = false;

    }

    public void destroy (ArrayList b, float speed = 0.1f)
    {
        StartCoroutine (DestroyCor (b, speed));
    }

    IEnumerator DestroyCor (ArrayList b, float speed = 0.1f)
    {
        ArrayList l = new ArrayList ();
        foreach (GameObject obj in b) {
            l.Add (obj);
        }

        Camera.main.GetComponent<mainscript> ().bounceCounter = 0;
        int scoreCounter = 0;
        int rate = 0;
        int soundPool = 0;
        foreach (GameObject obj in l) {
            if (obj == null)
                continue;
            if (obj.name.IndexOf ("ball") == 0)
                obj.layer = 0;
            //GameObject obj2 = findInArrayGameObject( b, obj );
            //if(obj2 != null)
            // 让ball爆炸
            obj.GetComponent<ball> ().growUp ();
            soundPool++;
            GetComponent<Collider2D> ().enabled = false;
            if (scoreCounter > 3) {
                rate += 10;
                scoreCounter += rate;
            }
            scoreCounter += 10;
            if (b.Count > 10 && Random.Range (0, 10) > 5)
                mainscript.Instance.perfect.SetActive (true);
            obj.GetComponent<ball> ().Destroyed = true;
            //		Destroy(obj);

            //  Camera.main.GetComponent<mainscript>().explode( obj.gameObject );
            if (b.Count < 10 || soundPool % 20 == 0)
                yield return new WaitForSeconds (speed);

            //			Destroy(obj);
        }
        mainscript.Instance.PopupScore (scoreCounter, transform.position);
        //   StartCoroutine( mainscript.Instance.destroyAloneBall() );
        mainscript.Instance.UpdateLocalMinYFromAllFixedBalls();

    }

    void DestroySingle (GameObject obj, float speed = 0.1f)
    {
        Camera.main.GetComponent<mainscript> ().bounceCounter = 0;
        int scoreCounter = 0;
        int rate = 0;
        int soundPool = 0;
        if (obj.name.IndexOf ("ball") == 0)
            obj.layer = 0;
        obj.GetComponent<ball> ().growUp ();
        soundPool++;

        if (obj.tag == "light") {
            SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.spark);
            obj.GetComponent<ball> ().DestroyLine ();
        }

        if (scoreCounter > 3) {
            rate += 10;
            scoreCounter += rate;
        }
        scoreCounter += 10;
        obj.GetComponent<ball> ().Destroyed = true;
        mainscript.Instance.PopupScore (scoreCounter, transform.position);

    }

    public void SplashDestroy ()
    {
        if (setTarget)
            mainscript.Instance.newBall2 = null;
        Destroy (gameObject);
    }

    public void destroy ()
    {
        growUpPlaySound ();
        destroy (gameObject);
    }

    public void destroy (GameObject obj)
    {
        if (obj.name.IndexOf ("ball") == 0)
            obj.layer = 0;

        Camera.main.GetComponent<mainscript> ().bounceCounter = 0;
        //	collider.enabled = false;
        obj.GetComponent<ball> ().destroyed = true;
        //	Destroy(obj);
        //obj.GetComponent<ball>().growUpPlaySound();
        obj.GetComponent<ball> ().growUp ();
        //	Invoke("playPop",1/(float)Random.Range(2,10));
        Camera.main.GetComponent<mainscript> ().explode (obj.gameObject);
    }

    public void growUp ()
    {
        StartCoroutine (explode ());
    }

    public void growUpPlaySound ()
    {
        Invoke ("growUpDelayed", 1 / (float)Random.Range (2, 10));
    }

    public void growUpDelayed ()
    {
        StartCoroutine (explode ());
    }

    void playPop ()
    {
    }


    IEnumerator explode ()
    {

        float startTime = Time.time;
        float endTime = Time.time + 0.1f;
        Vector3 tempPosition = transform.localScale;
        Vector3 targetPrepare = transform.localScale * 1.2f;

        GetComponent<CircleCollider2D> ().enabled = false;


        // 让ball有个膨胀的效果
        while (!isPaused && endTime > Time.time) {
            //transform.position  += targetPrepare * Time.deltaTime;
            transform.localScale = Vector3.Lerp (tempPosition, targetPrepare, (Time.time - startTime) * 10);
            //	transform.position  = targetPrepare ;
            // WaitForEndOfFrame的作用是等到渲染全部完成但没放到屏幕之前
            yield return new WaitForEndOfFrame ();
        }
        //      yield return new WaitForSeconds(0.01f );
        GameObject prefab = Resources.Load ("Particles/BubbleExplosion") as GameObject;

        GameObject explosion = (GameObject)Instantiate (prefab, gameObject.transform.position + Vector3.back * 20f, Quaternion.identity);
        if (mesh != null)
            explosion.transform.parent = mesh.transform;
        //   if( !isPaused )
        CheckNearCloud ();

        if (LevelData.mode == ModeGame.Vertical && isTarget) {
            Instantiate (Resources.Load ("Prefabs/TargetStar"), gameObject.transform.position, Quaternion.identity);
        } else if (LevelData.mode == ModeGame.Animals && isTarget) {
            // Instantiate( Resources.Load( "Prefabs/TargetStar" ), gameObject.transform.position, Quaternion.identity );
        }
        Destroy (gameObject, 1);

    }

    void CheckNearCloud ()
    {
        int layerMask = 1 << LayerMask.NameToLayer ("Ball");
        Collider2D[] meshes = Physics2D.OverlapCircleAll (transform.position, 1f, layerMask);
        foreach (Collider2D obj1 in meshes) {
            if (obj1.gameObject.tag == "cloud") {
                GameObject obj = obj1.gameObject;
                float distTemp = Vector3.Distance (transform.position, obj.transform.position);
                if (distTemp <= 1f) {
                    obj.GetComponent<ColorBallScript> ().ChangeRandomColor ();
                }
            }
        }

    }

    public void ShowFirework ()
    {
        fireworks++;
        if (fireworks <= 2)
            SoundBase.Instance.GetComponent<AudioSource> ().PlayOneShot (SoundBase.Instance.hit);

    }




}

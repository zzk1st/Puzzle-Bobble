using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class CreatorBall : MonoBehaviour
{
    public static CreatorBall Instance;
    public GameObject ball_hd;
    public GameObject ball_ld;
    public GameObject thePrefab;        // box
    public float InitialMoveUpSpeed;
    public float BallColliderRadius;
    public float BallRealRadius;
    GameObject ball;
    public static int columns = 11;
    public static int rows = 70;
    int lastRow;
    float offsetStep = 0.33f;
    //private OTSpriteBatch spriteBatch = null;  
    GameObject Meshes;
    [HideInInspector]
    public List<GameObject> squares = new List<GameObject>();       // 存储了所有用来做mesh的box
    int[] map;
    private int maxCols;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        ball = ball_hd;
        thePrefab.transform.localScale = new Vector3( 0.67f, 0.58f, 1 );
        Meshes = GameObject.Find( "-Ball" );
        // LevelData.LoadDataFromXML( mainscript.Instance.currentLevel );
        LoadLevel();
        //LevelData.LoadDataFromLocal(mainscript.Instance.currentLevel);
        if( LevelData.mode == ModeGame.Vertical || LevelData.mode == ModeGame.Animals )
            MoveLevelUp();
        else
        {
            // GameObject.Find( "TopBorder" ).transform.position += Vector3.down * 3.5f;
            GameObject.Find( "TopBorder" ).transform.parent = null;
            GameObject.Find( "TopBorder" ).GetComponent<SpriteRenderer>().enabled = false;
            GameObject ob = GameObject.Find( "-Grids" );
            ob.transform.position += Vector3.up * 2f;
            GameManager.Instance.PreTutorial();
        }

        mainscript.Instance.gridManager.CreateGrids(rows, columns);
        LoadMap( LevelData.map );
    }

    public void LoadLevel()
    {
        mainscript.Instance.currentLevel = PlayerPrefs.GetInt("OpenLevel");// TargetHolder.level;
        if (mainscript.Instance.currentLevel == 0)
            mainscript.Instance.currentLevel = 1;
        LoadDataFromLocal(mainscript.Instance.currentLevel);

    }


    public bool LoadDataFromLocal(int currentLevel)
    {
        //Read data from text file
        TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        if (mapText == null)
        {
            mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        }
        ProcessGameDataFromString(mapText.text);
        return true;
    }

    void ProcessGameDataFromString(string mapText)
    {
        string[] lines = mapText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        LevelData.colorsDict.Clear();
        int mapLine = 0;
        int key = 0;
        foreach (string line in lines)
        {
            if (line.StartsWith("MODE "))
            {
                string modeString = line.Replace("MODE", string.Empty).Trim();
                LevelData.mode = (ModeGame)int.Parse(modeString);
            }
            else if (line.StartsWith("SIZE "))
            {
                string blocksString = line.Replace("SIZE", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                maxCols = int.Parse(sizes[0]);
                //maxRows = int.Parse(sizes[1]);
            }
            else if (line.StartsWith("LIMIT "))
            {
                string blocksString = line.Replace("LIMIT", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                //limitType = (LIMIT)int.Parse(sizes[0]);
                // 注意：sizes[1]本来是用于限制每关球数量，我们取消了
            }
            else if (line.StartsWith("COLOR LIMIT "))
            {
                string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
                LevelData.colors = int.Parse(blocksString);
            }
            else if (line.StartsWith("STARS "))
            {
                string blocksString = line.Replace("STARS", string.Empty).Trim();
                string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < 3; ++i)
                {
                    LevelData.stars[i] = int.Parse(blocksNumbers[i]);
                }
            }
            else
            { //Maps
              //Split lines again to get map numbers
                string[] st = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < st.Length; i++)
                {
                    int value =  int.Parse(st[i][0].ToString());
                    if (!LevelData.colorsDict.ContainsValue((BallColor)value) && value > 0 && value < (int)BallColor.random)
                    {
                        LevelData.colorsDict.Add(key, (BallColor)value);
                        key++;

                    }

                    LevelData.map[mapLine * maxCols + i] = int.Parse(st[i][0].ToString());
                }
                mapLine++;
            }
        }
        //random colors
        if (LevelData.colorsDict.Count == 0)
        {
            //add constant colors 
            LevelData.colorsDict.Add(0, BallColor.yellow);
            LevelData.colorsDict.Add(1, BallColor.red);

            //add random colors
            List<BallColor> randomList = new List<BallColor>();
            randomList.Add(BallColor.blue);
            randomList.Add(BallColor.green);
            //if (LevelData.mode != ModeGame.Rounded)
                randomList.Add(BallColor.violet);
            for (int i = 0; i < LevelData.colors - 2; i++)
            {
                BallColor randCol = BallColor.yellow;
                while (LevelData.colorsDict.ContainsValue(randCol))
                {
                    randCol = randomList[UnityEngine.Random.Range(0, randomList.Count)];
                }
                LevelData.colorsDict.Add(2 + i, randCol);

            }

        }

    }

    public void LoadMap( int[] pMap )
    {
        map = pMap;
        //int key = -1;
        int roww = 0;
        for( int i = 0; i < rows; i++ )
        {
            for( int j = 0; j < columns; j++ )
            {
                int mapValue = map[i * columns + j];
                if( mapValue > 0  )
                {
                    roww = i;
                    if (LevelData.mode == ModeGame.Rounded) roww = i +4;
                    createBall(mainscript.Instance.gridManager.GetGrid(roww, j).transform.position, (BallColor)mapValue, false, i );
                }
                else if( mapValue == 0 && LevelData.mode == ModeGame.Vertical && i == 0 )
                {
                    //Instantiate( Resources.Load( "Prefabs/TargetStar" ), GetSquare( i, j ).transform.position, Quaternion.identity );
                }
            }
        }
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
        Transform bubbles = GameObject.Find( "-Ball" ).transform;
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

    public void MoveLevelDown()
    {
        StartCoroutine( MoveUpDownCor( true ) );
    }

    private bool BubbleBelowLine()
    {
        throw new System.NotImplementedException();
    }

    public void createRow( int j )
    {
        float offset = 0;
        for( int i = 0; i < columns; i++ )
        {
            if( j % 2 == 0 ) offset = 0; else offset = offsetStep;
            Vector3 v = new Vector3( transform.position.x + i * thePrefab.transform.localScale.x + offset, transform.position.y - j * thePrefab.transform.localScale.y, transform.position.z );
            createBall( v );
        }
    }

    public GameObject createBall( Vector3 vec, BallColor color = BallColor.random, bool newball = false, int row = 1 )
    {
        GameObject b = null;
        List<BallColor> colors = new List<BallColor>();

        for( int i = 1; i < System.Enum.GetValues( typeof( BallColor ) ).Length; i++ )
        {
            colors.Add( (BallColor)i );
        }

        if( color == BallColor.random )
            color = (BallColor)LevelData.colorsDict[UnityEngine.Random.Range( 0, LevelData.colorsDict.Count )];
		if( newball && mainscript.colorsDict.Count > 0 )
        {
            if( GameManager.Instance.GameStatus == GameStatus.Playing )
            {
                mainscript.Instance.GetColorsInGame();
                color = (BallColor)mainscript.colorsDict[UnityEngine.Random.Range( 0, mainscript.colorsDict.Count )];
            }
            else
                color = (BallColor)LevelData.colorsDict[UnityEngine.Random.Range( 0, LevelData.colorsDict.Count )];

        }



        b = Instantiate( ball, transform.position, transform.rotation ) as GameObject;
        b.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        b.GetComponent<CircleCollider2D>().radius = BallColliderRadius;
        b.GetComponent<Ball>().SetColor(color);

        b.tag = "" + color;

        GameObject[] fixedBalls = GameObject.FindObjectsOfType( typeof( GameObject ) ) as GameObject[];
        b.name = b.name + fixedBalls.Length.ToString();
        // Rigidbody2D在createBall里程序化的被加入
        if( newball )
        {
            b.gameObject.layer = LayerMask.NameToLayer("NewBall");
            b.transform.parent = Camera.main.transform;
            Rigidbody2D rig = b.AddComponent<Rigidbody2D>();
            b.GetComponent<CircleCollider2D>().enabled = false;
            rig.gravityScale = 0;
            if( GameManager.Instance.GameStatus == GameStatus.Playing )
                b.GetComponent<Animation>().Play();
        }
        else
        {
            b.GetComponent<Ball>().state = Ball.BallState.Fixed;
            // 只有当不是newball的时候，parent才设置成meshes，newball的parent随后会在stopball里设置
            b.transform.parent = Meshes.transform;
            b.GetComponent<Ball>().enabled = false;
            b.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            mainscript.Instance.gridManager.ConnectBallToGrid(b);
            mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
        }

        return b.gameObject;
    }
}

using UnityEngine;
using System.Collections;

public class GameItemFactory : MonoBehaviour {
    static public GameItemFactory Instance;

    public GameObject ballPrefab;

    void Awake()
    {
        Instance = this;
    }

    public GameObject createGameItemFromMap(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject result = null;

        switch(itemType)
        {
        case LevelData.ItemType.empty:
            break;
        case LevelData.ItemType.blue:
        case LevelData.ItemType.green:
        case LevelData.ItemType.red:
        case LevelData.ItemType.violet:
        case LevelData.ItemType.yellow:
        case LevelData.ItemType.random:
            result = createBall(vec, itemType, false);
            break;
        case LevelData.ItemType.Animal:
            result = createAnimal(vec, itemType);
            break;
        default:
            Debug.Log("ERROR: unknown itemType, itemType=" + itemType);
            break;
        }

        return result;
    }

    public GameObject createBall(Vector3 vec, LevelData.ItemType itemType = LevelData.ItemType.random, bool newball = false)
    {
        GameObject ball = null;

        if( itemType == LevelData.ItemType.random)
            itemType = (LevelData.ItemType)LevelData.allColors[UnityEngine.Random.Range(1, LevelData.allColors.Count)];

        if( newball && mainscript.curStageColors.Count > 0 )
        {
            if( GameManager.Instance.GameStatus == GameStatus.Playing )
            {
                mainscript.Instance.GetColorsInGame();
                itemType = (LevelData.ItemType)mainscript.curStageColors[UnityEngine.Random.Range(1, mainscript.curStageColors.Count)];
            }
            else
                itemType = (LevelData.ItemType)LevelData.allColors[UnityEngine.Random.Range(1, LevelData.allColors.Count)];

        }

        ball = Instantiate(ballPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        ball.GetComponent<Ball>().Initialize();

        ball.GetComponent<CircleCollider2D>().radius = mainscript.Instance.BallColliderRadius;
        ball.GetComponent<Ball>().SetTypeAndColor(itemType);
        ball.GetComponent<Ball>().number = UnityEngine.Random.Range(1, 6);

        GameObject[] fixedBalls = GameObject.FindObjectsOfType( typeof( GameObject ) ) as GameObject[];
        ball.name = ball.name + fixedBalls.Length.ToString();

        // Rigidbody2D在createBall里程序化的被加入
        if( newball )
        {
            ball.gameObject.layer = LayerMask.NameToLayer("NewBall");
            ball.transform.parent = Camera.main.transform;
            Rigidbody2D rig = ball.AddComponent<Rigidbody2D>();
            ball.GetComponent<CircleCollider2D>().enabled = false;
            rig.gravityScale = 0;
            if( GameManager.Instance.GameStatus == GameStatus.Playing )
                ball.GetComponent<Animation>().Play();
        }
        else
        {
            ball.GetComponent<Ball>().state = Ball.BallState.Fixed;
            // 只有当不是newball的时候，parent才设置成meshes，newball的parent随后会在stopball里设置
            ball.transform.parent = mainscript.Instance.gameItemsNode.transform;
            ball.GetComponent<Ball>().enabled = false;
            ball.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            mainscript.Instance.gridManager.ConnectGameItemToGrid(ball);
            mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
        }

        return ball.gameObject;
    }

    public GameObject createAnimal(Vector3 vec, LevelData.ItemType itemType)
    {
        return null;
    }
}

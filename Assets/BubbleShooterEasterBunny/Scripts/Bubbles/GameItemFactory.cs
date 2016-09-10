using UnityEngine;
using System.Collections;

public class GameItemFactory : MonoBehaviour {
    static public GameItemFactory Instance;

    public GameObject ballPrefab;
    public GameObject centerItemPrefab;
    public GameObject animalPrefab;

    void Awake()
    {
        Instance = this;
    }

    public GameObject CreateGameItemFromMap(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject result = null;

        switch(itemType)
        {
        case LevelData.ItemType.Empty:
            break;
        case LevelData.ItemType.Blue:
        case LevelData.ItemType.Green:
        case LevelData.ItemType.Red:
        case LevelData.ItemType.Violet:
        case LevelData.ItemType.Yellow:
        case LevelData.ItemType.Random:
            result = CreateFixedBall(vec, itemType);
            break;
        case LevelData.ItemType.CenterItem:
            result = CreateCenterItem(vec, itemType);
            break;
        case LevelData.ItemType.Animal:
            result = CreateAnimal(vec, itemType);
            break;
        default:
            Debug.Log("ERROR: unknown itemType, itemType=" + itemType);
            break;
        }

        return result;
    }

    public GameObject CreateNewBall(Vector3 vec, LevelData.ItemType itemType = LevelData.ItemType.Random)
    {
        GameObject ball = null;

        // 获取当前关卡颜色，并生成随机颜色
        if (itemType == LevelData.ItemType.Random)
        {
            mainscript.Instance.GetColorsInGame();
            itemType = (LevelData.ItemType)mainscript.curStageColors[UnityEngine.Random.Range(0, mainscript.curStageColors.Count)];
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
        ball.gameObject.layer = LayerMask.NameToLayer("NewBall");
        ball.transform.parent = Camera.main.transform;
        Rigidbody2D rig = ball.AddComponent<Rigidbody2D>();
        ball.GetComponent<CircleCollider2D>().enabled = false;
        rig.gravityScale = 0;
        if( GameManager.Instance.gameStatus == GameStatus.Playing )
            ball.GetComponent<Animation>().Play();

        return ball;
    }

    public GameObject CreateFixedBall(Vector3 vec, LevelData.ItemType itemType = LevelData.ItemType.Random)
    {
        GameObject ball = null;

        if( itemType == LevelData.ItemType.Random)
            itemType = (LevelData.ItemType)LevelData.allColors[UnityEngine.Random.Range(1, LevelData.allColors.Count)];

        ball = Instantiate(ballPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        ball.GetComponent<Ball>().Initialize();

        // 设置collider, state和其他基本属性
        ball.GetComponent<CircleCollider2D>().radius = mainscript.Instance.LineColliderRadius;
        ball.GetComponent<CircleCollider2D>().offset = Vector2.zero;
        ball.GetComponent<Ball>().SetTypeAndColor(itemType);
        ball.GetComponent<Ball>().number = UnityEngine.Random.Range(1, 6);
        ball.GetComponent<Ball>().state = Ball.BallState.Fixed;
        ball.GetComponent<Ball>().enabled = false;

        // 设置名字
        GameObject[] fixedBalls = GameObject.FindObjectsOfType( typeof( GameObject ) ) as GameObject[];
        ball.name = ball.name + fixedBalls.Length.ToString();

        ball.GetComponent<GameItem>().ConnectToGrid();

        return ball;
    }

    public GameObject CreateCenterItem(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject centerItem = Instantiate(centerItemPrefab, transform.position, transform.rotation) as GameObject;
        centerItem.transform.position = vec;
        centerItem.GetComponent<GameItem>().ConnectToGrid();
        centerItem.GetComponent<CenterItem>().Initialize();
        return centerItem;
    }

    public GameObject CreateAnimal(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject animal = Instantiate(animalPrefab, transform.position, transform.rotation) as GameObject;
        animal.transform.position = vec;
        animal.GetComponent<GameItem>().ConnectToGrid();
        animal.GetComponent<Animal>().Initialize();
        return animal;
    }
}

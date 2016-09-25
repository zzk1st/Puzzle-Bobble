using UnityEngine;
using System.Collections;

public enum BoostType
{
    FiveBallsBoost = 0,
    MagicBallBoost,
    RainbowBallBoost,
    FireBallBoost,
    None
}

public class GameItemFactory : MonoBehaviour {
    static public GameItemFactory Instance;

    public GameObject ballPrefab;
    public GameObject rainbowBallPrefab;
    public GameObject fireBallPrefab;
    public GameObject centerItemPrefab;
    public GameObject animalSinglePrefab;
    public GameObject animalTrianglePrefab;
    public GameObject animalHexagonPrefab;
    public GameObject bossPlacePrefab;

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
        case LevelData.ItemType.Occupied:
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
        case LevelData.ItemType.AnimalSingle:
        case LevelData.ItemType.AnimalTriangle:
        case LevelData.ItemType.AnimalHexagon:
            result = CreateAnimal(vec, itemType);
            break;
        case LevelData.ItemType.BossPlace:
            result = CreateBossPlace(vec, itemType);
            break;
        default:
            Debug.Log("ERROR: unknown itemType, itemType=" + itemType);
            break;
        }

        return result;
    }

    public GameObject CreateBoost(BoostType boostType, Vector3 pos)
    {
        switch(boostType)
        {
        case BoostType.RainbowBallBoost:
            return CreateRainbowBallBoost(pos);
        case BoostType.FireBallBoost:
            return CreateFireBallBoost(pos);
        }

        throw new System.AccessViolationException("未知特殊道具！");
    }

    GameObject CreateRainbowBallBoost(Vector3 vec)
    {
        GameObject ball = null;

        ball = Instantiate(rainbowBallPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        ball.GetComponent<RainbowBallBoost>().Initialize();

        return ball;
    }

    GameObject CreateFireBallBoost(Vector3 vec)
    {
        GameObject ball = null;

        ball = Instantiate(fireBallPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        ball.GetComponent<FireBallBoost>().Initialize();

        return ball;
    }

    public GameObject CreateNewBall(Vector3 vec, bool playAnimation)
    {
        GameObject ball = null;

        LevelData.ItemType itemType = LevelData.ItemType.Empty;
        if (GameManager.Instance.gameStatus == GameStatus.Win)
        {
            itemType = mainscript.Instance.levelData.ballColors[Random.Range(0, mainscript.Instance.levelData.ballColors.Count)];
        }
        else
        {
            itemType = (LevelData.ItemType) mainscript.Instance.GetRandomCurStageColor();
        }

        ball = Instantiate(ballPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        ball.GetComponent<Ball>().Initialize();

        ball.GetComponent<CircleCollider2D>().radius = mainscript.Instance.BallColliderRadius;
        ball.GetComponent<CircleCollider2D>().isTrigger = false;
        ball.GetComponent<Ball>().SetTypeAndColor(itemType);
        ball.GetComponent<Ball>().number = UnityEngine.Random.Range(1, 6);

        GameObject[] fixedBalls = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        ball.name = ball.name + fixedBalls.Length.ToString();

        // Rigidbody2D在createBall里程序化的被加入
        ball.gameObject.layer = LayerMask.NameToLayer("NewBall");
        ball.transform.parent = Camera.main.transform;
        Rigidbody2D rig = ball.AddComponent<Rigidbody2D>();
        ball.GetComponent<CircleCollider2D>().enabled = false;
        rig.gravityScale = 0;

        if(playAnimation)
        {
            ball.GetComponent<Animation>().Play();
        }

        return ball;
    }

    public GameObject CreateFixedBall(Vector3 vec, LevelData.ItemType itemType = LevelData.ItemType.Random)
    {
        GameObject ball = null;

        if( itemType == LevelData.ItemType.Random)
            itemType = (LevelData.ItemType) mainscript.Instance.levelData.ballColors[UnityEngine.Random.Range(0, mainscript.Instance.levelData.ballColors.Count)];

        ball = Instantiate(ballPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3( vec.x, vec.y, ball.transform.position.z );
        ball.GetComponent<Ball>().Initialize();

        // 设置collider, state和其他基本属性
        ball.GetComponent<CircleCollider2D>().radius = mainscript.Instance.LineColliderRadius;
        ball.GetComponent<CircleCollider2D>().offset = Vector2.zero;
        ball.GetComponent<CircleCollider2D>().isTrigger = true;
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

    GameObject CreateCenterItem(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject centerItem = Instantiate(centerItemPrefab, vec, transform.rotation) as GameObject;
        centerItem.GetComponent<CenterItem>().Initialize();
        return centerItem;
    }

    GameObject CreateAnimal(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject animalPrefab;
        switch(itemType)
        {
        case LevelData.ItemType.AnimalSingle:
            animalPrefab = animalSinglePrefab;
            break;
        case LevelData.ItemType.AnimalTriangle:
            animalPrefab = animalTrianglePrefab;
            break;
        case LevelData.ItemType.AnimalHexagon:
            animalPrefab = animalHexagonPrefab;
            break;
        default:
            throw new System.AccessViolationException("未知的AnimalType!");
        }

        GameObject animal = Instantiate(animalPrefab, vec, transform.rotation) as GameObject;
        animal.GetComponent<Animal>().Initialize();
        return animal;
    }

    GameObject CreateBossPlace(Vector3 vec, LevelData.ItemType itemType)
    {
        GameObject bossPlace = Instantiate(bossPlacePrefab, vec, transform.rotation) as GameObject;
        bossPlace.GetComponent<BossPlace>().Initialize();
        return bossPlace;
    }
}

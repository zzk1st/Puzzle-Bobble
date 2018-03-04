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

public class GameItemFactory : MonoBehaviour
{
    static public GameItemFactory Instance;

    public GameObject ballPrefab;
    public GameObject ballSmokePrefab;
    public GameObject rainbowBallPrefab;
    public GameObject fireBallPrefab;
    public GameObject magicBallPrefab;
    public GameObject centerItemPrefab;
    public GameObject animalSinglePrefab;
    public GameObject animalTrianglePrefab;
    public GameObject animalHexagonPrefab;
    public GameObject bossPlacePrefab;
    public GameObject gameLogoPrefab;

    void Awake()
    {
        Instance = this;
    }

    public GameObject CreateGameItemFromMap(Vector3 vec, LevelGameItem levelGameItem)
    {
        GameObject result = null;

        switch (levelGameItem.type)
        {
            case LevelItemType.Empty:
            case LevelItemType.Occupied:
                break;
            case LevelItemType.Blue:
            case LevelItemType.Green:
            case LevelItemType.Red:
            case LevelItemType.Violet:
            case LevelItemType.Yellow:
            case LevelItemType.Random:
                result = CreateFixedBall(vec, levelGameItem);
                break;
            case LevelItemType.CenterItem:
                result = CreateCenterItem(vec, levelGameItem);
                break;
            case LevelItemType.AnimalSingle:
            case LevelItemType.AnimalTriangle:
            case LevelItemType.AnimalHexagon:
                result = CreateAnimal(vec, levelGameItem);
                break;
            case LevelItemType.BossPlace:
                result = CreateBossPlace(vec, levelGameItem);
                break;
            case LevelItemType.GameLogo:
                result = CreateGameLogo(vec, levelGameItem);
                break;
            default:
                Debug.Log("ERROR: unknown itemType, itemType=" + levelGameItem);
                break;
        }

        return result;
    }

    public GameObject CreateBoost(BoostType boostType, Vector3 pos)
    {
        switch (boostType)
        {
            case BoostType.RainbowBallBoost:
                return CreateRainbowBallBoost(pos);
            case BoostType.FireBallBoost:
                return CreateFireBallBoost(pos);
            case BoostType.MagicBallBoost:
                return CreateMagicBallBoost(pos);
        }

        throw new System.AccessViolationException("未知特殊道具！");
    }

    GameObject CreateRainbowBallBoost(Vector3 vec)
    {
        GameObject ball = null;

        ball = Instantiate(rainbowBallPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3(vec.x, vec.y, ball.transform.position.z);
        ball.GetComponent<RainbowBallBoost>().Initialize();

        return ball;
    }

    GameObject CreateFireBallBoost(Vector3 vec)
    {
        GameObject ball = null;

        ball = Instantiate(fireBallPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3(vec.x, vec.y, ball.transform.position.z);
        ball.GetComponent<FireBallBoost>().Initialize();

        return ball;
    }

    GameObject CreateMagicBallBoost(Vector3 vec)
    {
        GameObject ball = null;

        ball = Instantiate(magicBallPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3(vec.x, vec.y, ball.transform.position.z);
        return ball;
    }

    public GameObject CreateNewBall(Vector3 vec, bool playAnimation)
    {
        GameObject ball = null;

        LevelGameItem levelGameItem = new LevelGameItem(LevelItemType.Empty);
        // 如果已经赢了，或者是开场，就生成随机彩球
        if (GameManager.Instance.gameStatus == GameStatus.Win ||
            GameManager.Instance.gameMode == GameMode.Opening)
        {
            levelGameItem.type = mainscript.Instance.levelData.ballColors[Random.Range(0, mainscript.Instance.levelData.ballColors.Count)];
        }
        else
        {
            levelGameItem.type = (LevelItemType)mainscript.Instance.GetRandomCurStageColor();
        }

        ball = Instantiate(ballPrefab, transform.position, transform.rotation) as GameObject;
        ball.transform.position = new Vector3(vec.x, vec.y, ball.transform.position.z);
        ball.GetComponent<Ball>().Initialize(levelGameItem, true);

        if (playAnimation)
        {
            ball.GetComponent<Animation>().Play();
        }

        return ball;
    }

    public GameObject CreateFixedBall(Vector3 vec, LevelGameItem levelGameItem)
    {
        if (levelGameItem.type == LevelItemType.Random)
            levelGameItem.type = (LevelItemType)mainscript.Instance.levelData.ballColors[UnityEngine.Random.Range(0, mainscript.Instance.levelData.ballColors.Count)];

        GameObject ballGO = Instantiate(ballPrefab, transform.position, transform.rotation) as GameObject;
        ballGO.transform.position = new Vector3(vec.x, vec.y, ballGO.transform.position.z);

        ballGO.GetComponent<Ball>().Initialize(levelGameItem);
        return ballGO;
    }

    GameObject CreateCenterItem(Vector3 vec, LevelGameItem levelGameItem)
    {
        GameObject centerItem = Instantiate(centerItemPrefab, vec, transform.rotation) as GameObject;
        centerItem.GetComponent<CenterItem>().Initialize();
        return centerItem;
    }

    GameObject CreateAnimal(Vector3 vec, LevelGameItem levelGameItem)
    {
        GameObject animalPrefab;
        switch (levelGameItem.type)
        {
            case LevelItemType.AnimalSingle:
                animalPrefab = animalSinglePrefab;
                break;
            case LevelItemType.AnimalTriangle:
                animalPrefab = animalTrianglePrefab;
                break;
            case LevelItemType.AnimalHexagon:
                animalPrefab = animalHexagonPrefab;
                break;
            default:
                throw new System.AccessViolationException("未知的AnimalType!");
        }

        GameObject animal = Instantiate(animalPrefab, vec, transform.rotation) as GameObject;
        animal.GetComponent<Animal>().Initialize();
        animal.GetComponent<Animal>().SetSprite(levelGameItem.type);
        return animal;
    }

    GameObject CreateBossPlace(Vector3 vec, LevelGameItem levelGameItem)
    {
        GameObject bossPlace = Instantiate(bossPlacePrefab, vec, transform.rotation) as GameObject;
        bossPlace.GetComponent<BossPlace>().Initialize();
        return bossPlace;
    }

    GameObject CreateGameLogo(Vector3 vec, LevelGameItem levelGameItem)
    {
        GameObject gameLogo = Instantiate(gameLogoPrefab, vec, transform.rotation) as GameObject;
        gameLogo.GetComponent<GameLogo>().Initialize();
        return gameLogo;
    }
}

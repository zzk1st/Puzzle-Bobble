using UnityEngine;
using System.Collections;

public class SingleLevel : MonoBehaviour {

    private GameObject missionTypeGO;
    private GameObject levelNumberGO;
    private GameObject starsGO;

    public int levelNumber;
    public MissionType missionType;
    public int starCount;
    public bool isUnlocked;

    public void Initialize(MissionType type, int stars)
    {
        missionTypeGO = transform.FindChild("MissionType").gameObject;
        levelNumberGO = transform.FindChild("LevelNumber").gameObject;
        starsGO = transform.FindChild("Stars").gameObject;
        missionType = type;
        starCount = stars;

        isUnlocked = PlayerPrefs.GetInt("Score" + (levelNumber-1)) > 0 || levelNumber == 1;

        // set level number text
        levelNumberGO.GetComponent<TextMesh>().text = levelNumber.ToString();

        // set mission type image
        switch(missionType)
        {
        case MissionType.EliminateBalls:
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = Resources.Load<UnityEngine.Sprite>("Textures/balls/ball_blue");
            break;
        case MissionType.RescueGhost:
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = Resources.Load<UnityEngine.Sprite>("Textures/balls/ball_green");
            break;
        case MissionType.SaveAnimals:
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = Resources.Load<UnityEngine.Sprite>("Textures/balls/ball_red");
            break;
        case MissionType.BossBattle:
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = Resources.Load<UnityEngine.Sprite>("Textures/balls/ball_yellow");
            break;
        default:
            throw new System.AccessViolationException("未知关卡类型" + missionType);
        }

        if (!isUnlocked)
        {
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = Resources.Load<UnityEngine.Sprite>("Textures/balls/ball_random");
        }

        // set stars
        for (int i = 1; i <= 3; i++)
        {
            GameObject curStar = starsGO.transform.FindChild("Star" + i).gameObject;
            if (i <= starCount)
            {
                curStar.SetActive(true);
            }
            else
            {
                curStar.SetActive(false);
            }
        }
    }
}

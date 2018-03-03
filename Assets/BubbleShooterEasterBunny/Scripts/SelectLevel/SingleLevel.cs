using UnityEngine;
using System.Collections;

public class SingleLevel : MonoBehaviour {
    private GameObject missionTypeGO;
    private GameObject levelNumberGO;
    private GameObject starsGO;

    public Sprite[] activeMissionTypeTextures;
    public Sprite[] inactiveMissionTypeTextures;

    public int levelNumber;
    public MissionType missionType;
    public int starCount;
    public bool isUnlocked;

    public void Initialize(MissionType type, int stars)
    {
        missionTypeGO = transform.Find("MissionType").gameObject;
        levelNumberGO = transform.Find("LevelNumber").gameObject;
        starsGO = transform.Find("Stars").gameObject;

        missionType = type;
        starCount = stars;
        //DEBUG!!!!!!!!
        //isUnlocked = PlayerPrefs.GetInt("Score" + (levelNumber-1)) > 0 || levelNumber == 1;
        isUnlocked = true;
        levelNumberGO.GetComponent<TextMesh>().text = levelNumber.ToString();
        if (isUnlocked)
        {
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = activeMissionTypeTextures[(int) missionType];
        }
        else
        {
            missionTypeGO.GetComponent<SpriteRenderer>().sprite = inactiveMissionTypeTextures[(int) missionType];
        }

        // set stars
        for (int i = 1; i <= 3; i++)
        {
            GameObject curStar = starsGO.transform.Find("Star" + i).gameObject;
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

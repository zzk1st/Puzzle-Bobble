using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour {
    public static MissionManager Instance;
	// Use this for initialization
	void Start ()
    {
        Instance = this;
	}

    private int _currentMissionPoints = 0;
    public  int currentMissionPoints
    {
        get { return _currentMissionPoints; }
    }

    private int _stageMissionPoints;
    public  int stageMissionPoints
    {
        get { return _stageMissionPoints; }
    }

    public GameObject missionPointCounter;
    public GameObject targetStarPrefab;

    public void Initialize()
    {
        LevelData levelData = mainscript.Instance.levelData;
        _stageMissionPoints = levelData.missionPoints;
    }

    public void GainAnimalPoint()
    {
        if (mainscript.Instance.levelData.missionType == MissionType.SaveAnimals)
        {
            _currentMissionPoints++;
            UpdateMissionPointCounter();

            checkWin();
        }
    }

    public void GainBossPoint()
    {
        if (mainscript.Instance.levelData.missionType == MissionType.BossBattle)
        {
            _currentMissionPoints++;
            UpdateMissionPointCounter();

            checkWin();
        }
    }

    public void GainTargetStar(Grid grid)
    {
        if (mainscript.Instance.levelData.missionType == MissionType.EliminateBalls)
        {
            _currentMissionPoints++;
            if (grid.Row == 0)
            {
                Instantiate(targetStarPrefab, grid.transform.position, grid.transform.rotation);
                GameObject movingTargetStar = Instantiate(targetStarPrefab, grid.transform.position, grid.transform.rotation) as GameObject;
                movingTargetStar.GetComponent<TargetStar>().fly();
            }

            checkWin();
        }
    }

    public void DecreaseTargetStar(Grid grid)
    {
        if (mainscript.Instance.levelData.missionType == MissionType.EliminateBalls && _currentMissionPoints > 0)
        {
            _currentMissionPoints--;
            UpdateMissionPointCounter();
        }
    }

    public void GainCenterItem()
    {
        if (mainscript.Instance.levelData.missionType == MissionType.RescueGhost)
        {
            _currentMissionPoints++;
            UpdateMissionPointCounter();

            checkWin();
        }
    }

    public void UpdateMissionPointCounter()
    {
        missionPointCounter.GetComponent<Text>().text = currentMissionPoints.ToString() + "/" + stageMissionPoints.ToString();
    }

    void checkWin()
    {
        if (UIManager.Instance.gameStatus != GameStatus.Playing)
        {
            return;
        }

        if (currentMissionPoints >= stageMissionPoints)
        {
            UIManager.Instance.Win();
        }
    }
}

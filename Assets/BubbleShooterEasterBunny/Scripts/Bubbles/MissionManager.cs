using UnityEngine;
using System.Collections;

public class MissionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
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

    public void Initialize()
    {
        LevelData levelData = mainscript.Instance.levelData;
        _stageMissionPoints = levelData.missionPoints;
    }
}

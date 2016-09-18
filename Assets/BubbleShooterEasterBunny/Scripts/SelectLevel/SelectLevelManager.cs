﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class SelectLevelManager : MonoBehaviour {
    public static SelectLevelManager Instance;
    private static string levelMissionTypesFilename = "LevelMissionTypes";
    public GameObject levels;

    void Awake()
    {
        Instance = this;
    }

	// Use this for initialization
	void Start ()
    {
        Initialize();
	}
	
    void Initialize()
    {
        TextAsset missionTypesText = Resources.Load("Levels/" + levelMissionTypesFilename) as TextAsset;
        if (missionTypesText == null)
        {
            throw new System.MissingFieldException("找不到文件" + levelMissionTypesFilename);
        }

        string[] lines = missionTypesText.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach(string line in lines)
        {
            string[] numbers = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (numbers.Length != 2)
            {
                throw new System.FieldAccessException("missionTypes文件必须有两个field，错误line=" + line);
            }

            SingleLevel level = getSingleLevel(int.Parse(numbers[0]));
            if (level != null)
            {
                MissionType missionType = (MissionType) int.Parse(numbers[1]);
                int starCount = PlayerPrefs.GetInt(string.Format( "Level.{0:000}.StarsCount", level.levelNumber), 0);
                level.Initialize(missionType, starCount);
            }
        }
    }

    SingleLevel getSingleLevel(int levelNumber)
    {
        foreach(Transform levelTransform in levels.transform)
        {
            SingleLevel level = levelTransform.gameObject.GetComponent<SingleLevel>();
            if (level.levelNumber == levelNumber)
            {
                return level;
            }
        }

        // 正常情况下应该enable这个exception
        // 目前我们为了调试方便暂且只返回null
        //throw new System.AccessViolationException("在scene中找不到missiontypes文件中提到的关卡，未知levelNumber=" + levelNumber);
        return null;
    }

    public void StartLevel(GameObject selectedLevelGO)
    {
        SingleLevel selectedLevel = selectedLevelGO.GetComponent<SingleLevel>();

        if (selectedLevel.isUnlocked)
        {
            PlayerPrefs.SetInt("OpenLevel", selectedLevel.levelNumber);
            PlayerPrefs.Save();

            SceneManager.LoadScene("game");
        }
    }
}

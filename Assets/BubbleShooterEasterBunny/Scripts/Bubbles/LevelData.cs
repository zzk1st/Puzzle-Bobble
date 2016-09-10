using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Xml;

// 游戏关卡移动模式，分为垂直和圆形两种
public enum StageMoveMode
{
    Vertical=0,
    Rounded,
}

public enum Target
{
    Top = 0,
    Chicken
}

public enum MissionType
{
    EliminateBalls=0,
    RescueGhost,
    SaveAnimals,
    BossBattle
}

public class LevelData
{
    public enum ItemType
    {
        Empty = 0,
        Blue,
        Green,
        Red,
        Violet,
        Yellow,
        Random,
        CenterItem,
        Animal,
        Boss
    }

    public static int VerticalModeMaxRows = 71;
    public static int VerticalModeMaxCols = 11;

    public static int RoundedModeMaxRows = 11;
    public static int RoundedModeMaxCols = 11;

    // animal在圆形模式下的row和col
    public static int CenterItemRow = RoundedModeMaxRows / 2;
    public static int CenterItemCol = RoundedModeMaxCols / 2;

    // 没找到更好的解决方法，为了让编辑器能复用levelData，目前把所有成员变量设为public
    public int currentLevel;
    public int[] map = new int[VerticalModeMaxRows * VerticalModeMaxCols];
    public int rowCount;
    public int colCount;

    //List of mission in this map
    public StageMoveMode stageMoveMode;
    public MissionType missionType;
    public int missionPoints;
    public int limitAmount = 40;

    public List<ItemType> allColors = new List<ItemType>();
    public int allowedColorCount;
    public int[] starScores = new int[3];
    private int[] itemTypeCounts = new int[System.Enum.GetValues(typeof(ItemType)).Length];

    public Target GetTarget(int levelNumber)
    {
        LoadLevel(levelNumber);
        return (Target)stageMoveMode;
    }

    void ProcessGameDataFromString(string mapText)
    {
        string[] lines = mapText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        allColors.Clear();
        map = new int[VerticalModeMaxRows * VerticalModeMaxCols];

        int mapLine = 0;
        foreach (string line in lines)
        {
            if (line.StartsWith("MISSIONTYPE "))
            {
                string modeString = line.Replace("MISSIONTYPE", string.Empty).Trim();
                missionType = (MissionType)int.Parse(modeString);
                stageMoveMode = missionType == MissionType.RescueGhost ? StageMoveMode.Rounded : StageMoveMode.Vertical;
            }
            else if (line.StartsWith("SIZE "))
            {
                string blocksString = line.Replace("SIZE", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                colCount = int.Parse(sizes[0]);
                rowCount = int.Parse(sizes[1]);
            }
            else if (line.StartsWith("LIMIT "))
            {
                string blocksString = line.Replace("LIMIT", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                limitAmount = int.Parse(sizes[1]);
                //limitType = (LIMIT)int.Parse(sizes[0]);
                // TODO: 加一个打击球的颜色列表
            }
            else if (line.StartsWith("COLOR LIMIT "))
            {
                string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
                allowedColorCount = int.Parse(blocksString);
            }
            else if (line.StartsWith("STARS "))
            {
                string blocksString = line.Replace("STARS", string.Empty).Trim();
                string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < 3; ++i)
                {
                    starScores[i] = int.Parse(blocksNumbers[i]);
                }
            }
            else
            { //Maps
              //Split lines again to get map numbers
                string[] st = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < st.Length; i++)
                {
                    int value = int.Parse(st[i][0].ToString());
                    itemTypeCounts[value]++;
                    int ballColorCount = Enum.GetNames(typeof(BallColor)).Length;
                    if (!allColors.Contains((ItemType)value) && value > 0 && value <= ballColorCount)
                    {
                        allColors.Add((ItemType)value);
                    }

                    map[mapLine * colCount + i] = int.Parse(st[i][0].ToString());
                }
                mapLine++;
            }
        }

        // 根据文件创建该level全部颜色表
        if (allColors.Count == 0)
        {
            for (int i = 1; i <= allowedColorCount; ++i)
            {
                allColors.Add((ItemType) i);
            }
        }
    }

    void CalculateMissionPoints()
    {
        switch(missionType)
        {
        case MissionType.EliminateBalls:
            missionPoints = 6;
            break;
        case MissionType.RescueGhost:
            missionPoints = 1;
            break;
        case MissionType.SaveAnimals:
            missionPoints = itemTypeCounts[(int)ItemType.Animal];
            break;
        case MissionType.BossBattle:
            missionPoints = itemTypeCounts[(int)ItemType.Boss];
            break;
        }
    }

    public bool LoadLevel(int curLevel)
    {
        currentLevel = curLevel;
        //Read data from text file
        TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        if (mapText == null)
        {
            return false;
        }
        ProcessGameDataFromString(mapText.text);
        CalculateMissionPoints();
        return true;
    }

    public void SaveLevel()
    {
        string saveString = "";
        //Create save string
        saveString += "MISSIONTYPE " + (int)missionType;
        saveString += "\n";
        saveString += "SIZE " + colCount + "/" + rowCount;
        saveString += "\n";
        saveString += "LIMIT " + "0" + "/" + limitAmount;
        saveString += "\n";
        saveString += "COLOR LIMIT " + allowedColorCount;
        saveString += "\n";
        saveString += "STARS " + starScores[0] + "/" + starScores[1] + "/" + starScores[2];
        saveString += "\n";

        //set map data
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                saveString += (int)map[row * colCount + col];
                //if this column not yet end of row, add space between them
                if (col < (colCount - 1))
                    saveString += " ";
            }
            //if this row is not yet end of row, add new line symbol between rows
            if (row < (rowCount - 1))
                saveString += "\n";
        }

        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
        {
            //Write to file
            string activeDir = Application.dataPath + @"/BubbleShooterEasterBunny/Resources/Levels/";
            string newPath = System.IO.Path.Combine(activeDir, currentLevel + ".txt");
            StreamWriter sw = new StreamWriter(newPath);
            sw.Write(saveString);
            sw.Close();
        }
    }
}



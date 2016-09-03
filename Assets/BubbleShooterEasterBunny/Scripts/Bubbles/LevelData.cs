﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Xml;

public enum GameMode
{
    Vertical=0,
    Rounded,
    Animals,
}

public enum Target
{
    Top = 0,
    Chicken
}


public class LevelData
{
    public enum ItemType
    {
        empty = 0,
        blue,
        green,
        red,
        violet,
        yellow,
        random,
        Animal
    }

    public static int MaxRows = 70;
    public static int MaxCols = 11;
    public int[] map = new int[MaxRows * MaxCols];
    public int rowCount;
    public int colCount;

    //List of mission in this map
    public static GameMode mode = GameMode.Vertical;
    private static float limitAmount = 40;

    public static float LimitAmount
    {
        get { return LevelData.limitAmount; }
        set 
        { 
            LevelData.limitAmount = value;
            if( value < 0 ) LevelData.limitAmount = 0;
        }
    }
    private static bool startReadData;
    public static List<ItemType> allColors = new List<ItemType>();
    static int key;
    public static int colors;
    public static int[] stars = new int[3];

    public Target GetTarget(int levelNumber)
    {
        LoadLevel(levelNumber);
        return (Target)LevelData.mode;
    }

    /*
    public static bool LoadLevel(int currentLevel)
    {
        //Read data from text file
        TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        if (mapText == null)
        {
            mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        }
        ProcesDataFromString(mapText.text);
        return true;
    }

    static void ProcesDataFromString(string mapText)
    {
        string[] lines = mapText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        LevelData.allColors.Clear();
        foreach (string line in lines)
        {
            if (line.StartsWith("MODE "))
            {
                string modeString = line.Replace("MODE", string.Empty).Trim();
                LevelData.mode = (GameMode)int.Parse(modeString);
            }
        }
    }
    */

    public bool LoadLevel(int currentLevel)
    {
        //Read data from text file
        TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        if (mapText == null)
        {
            mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        }
        ProcessGameDataFromString(mapText.text);
        return true;
    }

    void ProcessGameDataFromString(string mapText)
    {
        string[] lines = mapText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        LevelData.allColors.Clear();
        int mapLine = 0;
        foreach (string line in lines)
        {
            if (line.StartsWith("MODE "))
            {
                string modeString = line.Replace("MODE", string.Empty).Trim();
                LevelData.mode = (GameMode)int.Parse(modeString);
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
                //limitType = (LIMIT)int.Parse(sizes[0]);
                // TODO: 加一个打击球的颜色列表
            }
            else if (line.StartsWith("COLOR LIMIT "))
            {
                string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
                LevelData.colors = int.Parse(blocksString);
            }
            else if (line.StartsWith("STARS "))
            {
                string blocksString = line.Replace("STARS", string.Empty).Trim();
                string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < 3; ++i)
                {
                    LevelData.stars[i] = int.Parse(blocksNumbers[i]);
                }
            }
            else
            { //Maps
              //Split lines again to get map numbers
                string[] st = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < st.Length; i++)
                {
                    int value =  int.Parse(st[i][0].ToString());
                    int ballColorCount = Enum.GetNames(typeof(BallColor)).Length;
                    if (!LevelData.allColors.Contains((LevelData.ItemType)value) && value > 0 && value <= ballColorCount)
                    {
                        LevelData.allColors.Add((LevelData.ItemType)value);
                    }

                    map[mapLine * colCount + i] = int.Parse(st[i][0].ToString());
                }
                mapLine++;
            }
        }

        // 根据文件创建该level全部颜色表
        if (LevelData.allColors.Count == 0)
        {
            for (int i = 1; i <= LevelData.colors; ++i)
            {
                LevelData.allColors.Add((LevelData.ItemType) i);
            }
        }
    }
}



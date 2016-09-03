using UnityEngine;
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

    public static LevelData Instance;

    public static int[] map = new int[11 * 70];

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

    public static Target GetTarget(int levelNumber)
    {
        LoadLevel(levelNumber);
        return (Target)LevelData.mode;
    }

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
}



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class CreatorBall : MonoBehaviour
{
    private static int columns = 11;
    private static int rows = 70;
    //private OTSpriteBatch spriteBatch = null;  
    [HideInInspector]
    public List<GameObject> squares = new List<GameObject>();       // 存储了所有用来做mesh的box
    int[] map;
    private int maxCols;

    // Use this for initialization
    void Start()
    {
        mainscript.Instance.gridManager.CreateGrids(rows, columns);

        LoadLevel();
        LoadMap( LevelData.map );
    }

    public void LoadLevel()
    {
        mainscript.Instance.currentLevel = PlayerPrefs.GetInt("OpenLevel");// TargetHolder.level;
        if (mainscript.Instance.currentLevel == 0)
            mainscript.Instance.currentLevel = 1;
        LoadDataFromLocal(mainscript.Instance.currentLevel);

    }

    public bool LoadDataFromLocal(int currentLevel)
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
                LevelData.mode = (ModeGame)int.Parse(modeString);
            }
            else if (line.StartsWith("SIZE "))
            {
                string blocksString = line.Replace("SIZE", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                maxCols = int.Parse(sizes[0]);
                //maxRows = int.Parse(sizes[1]);
            }
            else if (line.StartsWith("LIMIT "))
            {
                string blocksString = line.Replace("LIMIT", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                //limitType = (LIMIT)int.Parse(sizes[0]);
                // TODO: 把限制球的数量加回来
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

                    LevelData.map[mapLine * maxCols + i] = int.Parse(st[i][0].ToString());
                }
                mapLine++;
            }
        }
        //random colors
        if (LevelData.allColors.Count == 0)
        {
            for (int i = 1; i <= LevelData.colors; ++i)
            {
                LevelData.allColors.Add((LevelData.ItemType) i);
            }
        }
    }

    public void LoadMap(int[] pMap)
    {
        map = pMap;
        //int key = -1;
        int roww = 0;
        for( int i = 0; i < rows; i++ )
        {
            for( int j = 0; j < columns; j++ )
            {
                int mapValue = map[i * columns + j];
                if( mapValue > 0  )
                {
                    roww = i;
                    if (LevelData.mode == ModeGame.Rounded) roww = i +4;
                    LevelData.ItemType type = (LevelData.ItemType)mapValue;
                    if (type != LevelData.ItemType.empty)
                    {
                        GameItemFactory.Instance.createGameItemFromMap(mainscript.Instance.gridManager.GetGrid(roww, j).transform.position, (LevelData.ItemType)mapValue);
                    }
                }
                else if( mapValue == 0 && LevelData.mode == ModeGame.Vertical && i == 0 )
                {
                    //Instantiate( Resources.Load( "Prefabs/TargetStar" ), GetSquare( i, j ).transform.position, Quaternion.identity );
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class StageLoader
{
    // Use this for initialization
    public static void Load()
    {
        mainscript.Instance.levelData.LoadLevel(mainscript.Instance.currentLevel);
        if (mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
        {
            GridManager.Instance.CreateGrids(LevelData.VerticalModeMaxRows, LevelData.VerticalModeMaxCols, mainscript.Instance.levelData.stageMoveMode);
        }
        else
        {
            GridManager.Instance.CreateGrids(LevelData.RoundedModeMaxRows, LevelData.RoundedModeMaxCols, mainscript.Instance.levelData.stageMoveMode);
        }

        mainscript.Instance.currentLevel = PlayerPrefs.GetInt("OpenLevel");// TargetHolder.level;
        if (mainscript.Instance.currentLevel == 0)
            mainscript.Instance.currentLevel = 1;
        LoadMap();

        MissionManager.Instance.Initialize();
        GameManager.Instance.Demo();
        mainscript.Instance.platformController.StartGameMoveUp();
        // TODO: 写一个iniitalizeBorders(), 负责border的初始化创建，注意要创建bottom border
    }

    static void LoadMap()
    {
        LevelData levelData = mainscript.Instance.levelData;

        for( int row = 0; row < levelData.rowCount; row++ )
        {
            for( int col = 0; col < levelData.colCount; col++ )
            {
                LevelData.ItemType mapValue = levelData.MapData(row, col);
                if(mapValue != LevelData.ItemType.Empty)
                {
                    GameItemFactory.Instance.CreateGameItemFromMap(GridManager.Instance.Grid(row, col).transform.position, mapValue);
                }
                else if(mapValue == LevelData.ItemType.Empty && levelData.stageMoveMode == StageMoveMode.Vertical && row == 0)
                {
                    //Instantiate( Resources.Load( "Prefabs/TargetStar" ), GetSquare( i, j ).transform.position, Quaternion.identity );
                }
            }
        }
    }
}

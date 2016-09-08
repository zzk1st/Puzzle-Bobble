using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class CreatorBall : MonoBehaviour
{
    // Use this for initialization
    void Start()
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

        GameManager.Instance.Demo();
        mainscript.Instance.platformController.StartGameMoveUp();
        // TODO: 写一个iniitalizeBorders(), 负责border的初始化创建，注意要创建bottom border
    }

    public void LoadMap()
    {
        LevelData levelData = mainscript.Instance.levelData;

        for( int row = 0; row < levelData.rowCount; row++ )
        {
            for( int col = 0; col < levelData.colCount; col++ )
            {
                int mapValue = levelData.map[row * levelData.colCount + col];
                if( mapValue > 0  )
                {
                    //if (levelData.gameMode == GameMode.Rounded) row = i +4;
                    LevelData.ItemType type = (LevelData.ItemType)mapValue;
                    if (type != LevelData.ItemType.empty)
                    {
                        GameItemFactory.Instance.CreateGameItemFromMap(GridManager.Instance.Grid(row, col).transform.position, (LevelData.ItemType)mapValue);
                    }
                }
                else if( mapValue == 0 && levelData.stageMoveMode == StageMoveMode.Vertical && row == 0 )
                {
                    //Instantiate( Resources.Load( "Prefabs/TargetStar" ), GetSquare( i, j ).transform.position, Quaternion.identity );
                }
            }
        }
    }
}

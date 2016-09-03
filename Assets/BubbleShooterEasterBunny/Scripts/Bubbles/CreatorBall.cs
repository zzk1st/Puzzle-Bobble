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
        mainscript.Instance.gridManager.CreateGrids(LevelData.MaxRows, LevelData.MaxCols);

        mainscript.Instance.currentLevel = PlayerPrefs.GetInt("OpenLevel");// TargetHolder.level;
        if (mainscript.Instance.currentLevel == 0)
            mainscript.Instance.currentLevel = 1;

        mainscript.Instance.levelData.LoadLevel(mainscript.Instance.currentLevel);
        LoadMap();
    }

    public void LoadMap()
    {
        LevelData levelData = mainscript.Instance.levelData;
        int row = 0;

        for( int i = 0; i < levelData.rowCount; i++ )
        {
            for( int j = 0; j < levelData.colCount; j++ )
            {
                int mapValue = levelData.map[i * levelData.colCount + j];
                if( mapValue > 0  )
                {
                    row = i;
                    //if (levelData.gameMode == GameMode.Rounded) row = i +4;
                    LevelData.ItemType type = (LevelData.ItemType)mapValue;
                    if (type != LevelData.ItemType.empty)
                    {
                        GameItemFactory.Instance.createGameItemFromMap(mainscript.Instance.gridManager.grid(row, j).transform.position, (LevelData.ItemType)mapValue);
                    }
                }
                else if( mapValue == 0 && levelData.gameMode == GameMode.Vertical && i == 0 )
                {
                    //Instantiate( Resources.Load( "Prefabs/TargetStar" ), GetSquare( i, j ).transform.position, Quaternion.identity );
                }
            }
        }
    }
}

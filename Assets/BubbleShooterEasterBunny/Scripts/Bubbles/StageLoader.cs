using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

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
        Profiler.BeginSample("Stage Load");
        LoadSceneFromLevelData();
        Profiler.EndSample();

        MissionManager.Instance.Initialize();
        UIManager.Instance.StageMovingUp();
        mainscript.Instance.platformController.StartGameMove();
        // TODO: 写一个iniitalizeBorders(), 负责border的初始化创建，注意要创建bottom border
    }

    static void LoadSceneFromLevelData()
    {
        LevelData levelData = mainscript.Instance.levelData;
        List<GameObject> bossPlaces = new List<GameObject>();

        for( int row = 0; row < levelData.rowCount; row++ )
        {
            for( int col = 0; col < levelData.colCount; col++ )
            {
                LevelGameItem levelGameItem = levelData.MapData(row, col);
                if(levelGameItem.type != LevelItemType.Empty)
                {
                    GameObject go = GameItemFactory.Instance.CreateGameItemFromMap(GridManager.Instance.Grid(row, col).transform.position, levelGameItem);
                    if (levelGameItem.type == LevelItemType.BossPlace)
                    {
                        bossPlaces.Add(go);
                    }
                }
                else if(levelGameItem.type == LevelItemType.Empty && levelData.stageMoveMode == StageMoveMode.Vertical && row == 0)
                {
                    //Instantiate( Resources.Load( "Prefabs/TargetStar" ), GetSquare( i, j ).transform.position, Quaternion.identity );
                }
            }
        }

        // 在第一个bossplace开始生成防御颜色之前，更新当前关卡颜色
        mainscript.Instance.UpdateColorsInGame();

        if (levelData.missionType == MissionType.BossBattle)
        {
            BossManager.Instance.Iniaizlize(bossPlaces);
        }
    }
}

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
        CoreManager.Instance.levelData.LoadLevel(CoreManager.Instance.currentLevel);

        if (CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
        {
            GridManager.Instance.CreateGrids(LevelData.VerticalModeMaxRows, LevelData.VerticalModeMaxCols, CoreManager.Instance.levelData.stageMoveMode);
        }
        else
        {
            GridManager.Instance.CreateGrids(LevelData.RoundedModeMaxRows, LevelData.RoundedModeMaxCols, CoreManager.Instance.levelData.stageMoveMode);
        }

        CoreManager.Instance.currentLevel = PlayerPrefs.GetInt("OpenLevel");// TargetHolder.level;
        if (CoreManager.Instance.currentLevel == 0)
            CoreManager.Instance.currentLevel = 1;
        Profiler.BeginSample("Stage Load");
        LoadSceneFromLevelData();
        Profiler.EndSample();

        MissionManager.Instance.Initialize();
        UIManager.Instance.StageMovingUp();
        CoreManager.Instance.platformController.StartGameMove();
        // TODO: 写一个iniitalizeBorders(), 负责border的初始化创建，注意要创建bottom border
    }

    static void LoadSceneFromLevelData()
    {
        LevelData levelData = CoreManager.Instance.levelData;
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
        CoreManager.Instance.UpdateColorsInGame();

        if (levelData.missionType == MissionType.BossBattle)
        {
            BossManager.Instance.Iniaizlize(bossPlaces);
        }
    }
}

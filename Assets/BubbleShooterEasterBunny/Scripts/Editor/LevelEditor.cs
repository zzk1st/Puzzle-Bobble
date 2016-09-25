using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class LevelEditor : EditorWindow
{
    private static LevelEditor window;

    LevelData levelData = new LevelData();

    private Texture[] ballTex;
    int levelNumber = 1;
    private Vector2 scrollViewVector;
    private LevelData.ItemType brush;

    [MenuItem("Window/Level editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (LevelEditor)EditorWindow.GetWindow(typeof(LevelEditor));
        window.Show();
    }

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LevelEditor));

    }

    void OnFocus()
    {
        LoadDataFromLocal(levelNumber);
        LevelEditorBase lm = GameObject.Find("LevelEditorBase").GetComponent<LevelEditorBase>();
        ballTex = new Texture[lm.sprites.Length];
        for (int i = 0; i < lm.sprites.Length; i++)
        {
            ballTex[i] = lm.sprites[i].texture;
        }
    }

    void OnGUI()
    {
        if (levelNumber < 1)
            levelNumber = 1;

        scrollViewVector = GUI.BeginScrollView(new Rect(25, 45, position.width - 30, position.height), scrollViewVector, new Rect(0, 0, 400, 2000));

        GUILevelSelector();
        GUILayout.Space(10);

        GUIMissionType();
        GUILayout.Space(10);

        GUILimit();
        GUILayout.Space(10);


        GUIColorLimit();
        GUILayout.Space(10);

        GUIStars();
        GUILayout.Space(10);

        GUIBlocks();
        GUILayout.Space(20);

        GUIGameField();

        GUI.EndScrollView();
    }


    #region leveleditor

    void GUILevelSelector()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level editor", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
        //if (GUILayout.Button("Test level", new GUILayoutOption[] { GUILayout.Width(150) }))
        //{
        //    PlayerPrefs.SetInt("OpenLevelTest", levelNumber);
        //    PlayerPrefs.SetInt("OpenLevel", levelNumber);
        //    PlayerPrefs.Save();

        //    EditorApplication.isPlaying = true;


        //}
        GUILayout.EndHorizontal();

        //     myString = EditorGUILayout.TextField("Text Field", myString);
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(50) });
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        if (GUILayout.Button("<<", new GUILayoutOption[] { GUILayout.Width(50) }))
        {
            PreviousLevel();
        }
        string changeLvl = GUILayout.TextField(" " + levelNumber, new GUILayoutOption[] { GUILayout.Width(50) });
        try
        {
            if (int.Parse(changeLvl) != levelNumber)
            {
                if (LoadDataFromLocal(int.Parse(changeLvl)))
                    levelNumber = int.Parse(changeLvl);

            }
        }
        catch (Exception)
        {

            throw;
        }

        if (GUILayout.Button(">>", new GUILayoutOption[] { GUILayout.Width(50) }))
        {
            NextLevel();
        }

        if (GUILayout.Button("New level", new GUILayoutOption[] { GUILayout.Width(100) }))
        {
            AddLevel();
        }


        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Space(60);

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

    }

    void AddLevel()
    {
        levelNumber = GetLastLevel() + 1;
    }

    void NextLevel()
    {
        levelNumber++;
        if (!LoadDataFromLocal(levelNumber))
            levelNumber--;
    }

    void PreviousLevel()
    {
        levelNumber--;
        if (levelNumber < 1)
            levelNumber = 1;
        if (!LoadDataFromLocal(levelNumber))
            levelNumber++;
    }

    #endregion

    void GUILimit()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(60);

        GUILayout.Label("Limit:", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(50) });
        levelData.limitAmount = EditorGUILayout.IntField(levelData.limitAmount, new GUILayoutOption[] { GUILayout.Width(50) });
        GUILayout.EndHorizontal();
    }

    void GUIColorLimit()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(60);

        GUILayout.Label("Color limit:", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(100) });
        levelData.allowedColorCount = (int)GUILayout.HorizontalSlider(levelData.allowedColorCount, 3, 5, new GUILayoutOption[] { GUILayout.Width(100) });
        levelData.allowedColorCount = EditorGUILayout.IntField("", levelData.allowedColorCount, new GUILayoutOption[] { GUILayout.Width(50) });

        GUILayout.EndHorizontal();
    }

    void GUIStars()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();

        GUILayout.Label("Stars:", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.Label("Star1", new GUILayoutOption[] { GUILayout.Width(100) });
        GUILayout.Label("Star2", new GUILayoutOption[] { GUILayout.Width(100) });
        GUILayout.Label("Star3", new GUILayoutOption[] { GUILayout.Width(100) });
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        int s = 0;
        s = EditorGUILayout.IntField("", levelData.starScores[0], new GUILayoutOption[] { GUILayout.Width(100) });
        if (s != levelData.starScores[0])
        {
            levelData.starScores[0] = s;
        }
        s = EditorGUILayout.IntField("", levelData.starScores[1], new GUILayoutOption[] { GUILayout.Width(100) });
        if (s != levelData.starScores[1])
        {
            levelData.starScores[1] = s;
        }
        s = EditorGUILayout.IntField("", levelData.starScores[2], new GUILayoutOption[] { GUILayout.Width(100) });
        if (s != levelData.starScores[2])
        {
            levelData.starScores[2] = s;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void GUIMissionType()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        GUILayout.Label("Target:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        levelData.missionType = (MissionType) EditorGUILayout.EnumPopup(levelData.missionType, GUILayout.Width(100));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }


    void GUIBlocks()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();


        GUILayout.BeginVertical();
        GUILayout.Label("Tools:", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        if (GUILayout.Button("Clear", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
        {
            for (int i = 0; i < levelData.map.Length; i++)
            {
                levelData.map[i] = LevelData.ItemType.Empty;
            }
        }
        if (GUILayout.Button("Save", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
        {
            SaveLevel();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(30);
        GUILayout.EndVertical();


        GUILayout.Label("Balls:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();

        for (int i = 0; i < System.Enum.GetValues(typeof(LevelData.ItemType)).Length; i++)
        {
            // Occupied只由大型GameItem的放置和消除决定，不能手动设置
            if (i != (int) LevelData.ItemType.Occupied)
            {
                if (GUILayout.Button(ballTex[i], new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
                {
                    if ((LevelData.ItemType)i != LevelData.ItemType.CenterItem)
                    {
                        brush = (LevelData.ItemType)i;
                    }
                    else
                    {
                        levelData.missionType = MissionType.RescueGhost;
                        checkAndBrushMap(LevelData.ItemType.CenterItem, LevelData.CenterItemRow, LevelData.CenterItemCol);
                    }
                }
            }
        }

        if (GUILayout.Button("  ", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
        {
            brush = LevelData.ItemType.Empty;
        }
        //   GUILayout.Label(" - empty", EditorStyles.boldLabel);


        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

    }

    void GUIGameField()
    {
        GUILayout.BeginVertical();
        bool offset = false;
        for (int row = 0; row < levelData.rowCount; row++)
        {
            GUILayout.BeginHorizontal();
            if (offset)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);

            }
            for (int col = 0; col < levelData.colCount; col++)
            {
                var imageButton = new object();
                if (levelData.MapData(row, col) == LevelData.ItemType.Empty)
                {
                    imageButton = "X";
                }
                else if (levelData.MapData(row, col) != LevelData.ItemType.Empty)
                {
                    imageButton = ballTex[(int)levelData.MapData(row, col)];
                }

                if (GUILayout.Button(imageButton as Texture, new GUILayoutOption[] {
                    GUILayout.Width (50),
                    GUILayout.Height (50)
                }))
                {
                    BrushOrRemoveItem(row, col);
                }
            }
            GUILayout.EndHorizontal();
            if (offset)
            {
                GUILayout.EndHorizontal();

            }


            offset = !offset;
        }
        GUILayout.EndVertical();
    }

    void BrushOrRemoveItem(int row, int col)
    {
        if (levelData.MapData(row, col) != LevelData.ItemType.Occupied)   // 凡事Occupied的地方一律由gameitem的center决定
        {
            if (brush == LevelData.ItemType.Empty)
            {
                RemoveItemFromMap(row, col);
            }
            else
            {
                checkAndBrushMap(brush, row, col);
            }
        }
    }


    int GetLastLevel()
    {
        TextAsset mapText = null;
        for (int i = levelNumber; i < 50000; i++)
        {
            mapText = Resources.Load("Levels/" + i) as TextAsset;
            if (mapText == null)
            {
                return i - 1;
            }
        }
        return 0;
    }

    void SaveLevel()
    {
        levelData.SaveLevel();
        AssetDatabase.Refresh();
    }

    public bool LoadDataFromLocal(int currentLevel)
    {
        return levelData.LoadLevel(currentLevel);
    }

    // 检测如果在row,col放置该item, 其shap是否出界，是否占其他已存在item
    bool checkAndBrushMap(LevelData.ItemType itemType, int row, int col)
    {
        GameItemShapeType shapeType = levelData.ShapeType(itemType);
        List<GridCoord> gridCoords = GameItemShapes.Instance.ShapeGridCoords(shapeType, row, col);
        foreach(GridCoord gridCoord in gridCoords)
        {
            if (gridCoord.row < 0 || gridCoord.row >= levelData.rowCount ||
                gridCoord.col < 0 || gridCoord.col >= levelData.colCount)
            {
                return false;
            }

            if (levelData.MapData(gridCoord.row, gridCoord.col) != LevelData.ItemType.Empty)
            {
                return false;
            }
        }

        foreach(GridCoord gridCoord in gridCoords)
        {
            levelData.map[gridCoord.row * levelData.colCount + gridCoord.col] = LevelData.ItemType.Occupied;
        }
        levelData.map[row * levelData.colCount + col] = itemType;   // 最后把大型gameItem的中心刷上

        return true;
    }

    // 检测如果在row,col放置该item, 其shap是否出界，是否占其他已存在item
    bool RemoveItemFromMap(int row, int col)
    {
        LevelData.ItemType itemType = levelData.MapData(row, col);
        if (itemType == LevelData.ItemType.Occupied)
        {
            return false;
        }

        GameItemShapeType shapeType = levelData.ShapeType(itemType);
        List<GridCoord> gridCoords = GameItemShapes.Instance.ShapeGridCoords(shapeType, row, col);

        foreach(GridCoord gridCoord in gridCoords)
        {
            levelData.map[gridCoord.row * levelData.colCount + gridCoord.col] = LevelData.ItemType.Empty;
        }

        return true;
    }
}

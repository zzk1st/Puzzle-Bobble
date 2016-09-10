using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;

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
        SaveLevel();
        levelNumber = GetLastLevel() + 1;
        SaveLevel();
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
        int oldLimit = levelData.limitAmount;
        levelData.limitAmount = EditorGUILayout.IntField(levelData.limitAmount, new GUILayoutOption[] { GUILayout.Width(50) });
        GUILayout.EndHorizontal();

        if (oldLimit != levelData.limitAmount)
            SaveLevel();
    }

    void GUIColorLimit()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(60);

        int saveInt = levelData.allowedColorCount;
        GUILayout.Label("Color limit:", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(100) });
        levelData.allowedColorCount = (int)GUILayout.HorizontalSlider(levelData.allowedColorCount, 3, 5, new GUILayoutOption[] { GUILayout.Width(100) });
        levelData.allowedColorCount = EditorGUILayout.IntField("", levelData.allowedColorCount, new GUILayoutOption[] { GUILayout.Width(50) });

        GUILayout.EndHorizontal();

        if (saveInt != levelData.allowedColorCount)
        {
            SaveLevel();
        }

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
            SaveLevel();
        }
        s = EditorGUILayout.IntField("", levelData.starScores[1], new GUILayoutOption[] { GUILayout.Width(100) });
        if (s != levelData.starScores[1])
        {
            levelData.starScores[1] = s;
            SaveLevel();
        }
        s = EditorGUILayout.IntField("", levelData.starScores[2], new GUILayoutOption[] { GUILayout.Width(100) });
        if (s != levelData.starScores[2])
        {
            levelData.starScores[2] = s;
            SaveLevel();
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
        MissionType missionType = levelData.missionType;
        levelData.missionType = (MissionType) EditorGUILayout.EnumPopup(levelData.missionType, GUILayout.Width(100));
        GUILayout.EndVertical();
        if (missionType != levelData.missionType)
        {
            SaveLevel();
        }
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
        if (GUILayout.Button("Clear", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
        {
            for (int i = 0; i < levelData.map.Length; i++)
            {
                levelData.map[i] = 0;
            }
            SaveLevel();
        }
        GUILayout.EndVertical();


        GUILayout.Label("Balls:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();

        for (int i = 0; i <= System.Enum.GetValues(typeof(LevelData.ItemType)).Length-1; i++)
        {
            if (GUILayout.Button(ballTex[i], new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
            {
                if ((LevelData.ItemType)i != LevelData.ItemType.CenterItem)
                    brush = (LevelData.ItemType)i;
                else
                {
                    levelData.missionType = MissionType.RescueGhost;
                    levelData.map[LevelData.CenterItemRow * levelData.colCount + LevelData.CenterItemCol] = (int) LevelData.ItemType.CenterItem;
                    SaveLevel();
                }
            }
        }

        if (GUILayout.Button("  ", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
        {
            brush = 0;
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
                if (levelData.map[row * levelData.colCount + col] == 0)
                {
                    imageButton = "X";
                }
                else if (levelData.map[row * levelData.colCount + col] != 0)
                {
                    imageButton = ballTex[(int)levelData.map[row * levelData.colCount + col]];
                }

                if (GUILayout.Button(imageButton as Texture, new GUILayoutOption[] {
                    GUILayout.Width (50),
                    GUILayout.Height (50)
                }))
                {
                    SetType(col, row);
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

    void SetType(int col, int row)
    {
        levelData.map[row * levelData.colCount + col] = (int) brush;
        SaveLevel();
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
}

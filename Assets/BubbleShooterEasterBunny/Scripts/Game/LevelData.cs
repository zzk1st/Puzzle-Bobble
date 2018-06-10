using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Xml;

// 游戏关卡移动模式，分为垂直和圆形两种
public enum StageMoveMode
{
	Vertical = 0,
	Rounded,
}

public enum Target
{
	Top = 0,
	Chicken
}

public enum MissionType
{
	EliminateBalls = 0,
	RescueGhost,
	SaveAnimals,
	BossBattle
}

public enum LevelItemType
{
	Empty = 0,
	Blue,
	Green,
	Red,
	Violet,
	Yellow,
	Random,
	CenterItem,		// 圆形模式下位于关卡中央的物体
	AnimalSingle,
	AnimalTriangle,
	AnimalHexagon,
	BossPlace,
	Occupied, 		// 这个item表示该区域是某个大型gameItem的一部分，但并不是其中心
	GameLogo,		// 表示游戏一开始的logo
}

public enum BallCoverType
{
	None = 0,
	Smoke,
	Ice
}

public class LevelGameItem
{
	public LevelGameItem (LevelItemType type, BallCoverType ballCoverType = BallCoverType.None)
	{
		this.type = type;
		this.ballCoverType = ballCoverType;
	}

	public LevelGameItem (string str)
	{
		string[] fields = str.Trim ().Split (',');
		if (fields.Length > 2) {
			throw new System.AccessViolationException ("LevelData game item doesn't have 2 fields!");
		} else if (fields.Length == 2) {
			this.type = (LevelItemType)int.Parse (fields [0]);
			this.ballCoverType = (BallCoverType)Enum.Parse(typeof(BallCoverType), fields [1]);
		} else {
			this.type = (LevelItemType)int.Parse (str);
			this.ballCoverType = BallCoverType.None;
		}
	}

	public string ToMapData ()
	{
		if (ballCoverType != BallCoverType.None) {
			return ((int)type).ToString () + "," + ballCoverType.ToString ();
		} else {
			return ((int)type).ToString ();
		}
	}

	public LevelItemType type;
	public BallCoverType ballCoverType;
}

public class LevelData
{
	public Dictionary<LevelItemType, GameItemShapeType> itemShapeTypes = new Dictionary<LevelItemType, GameItemShapeType> {
		{ LevelItemType.Empty, GameItemShapeType.Single },
		{ LevelItemType.Blue, GameItemShapeType.Single },
		{ LevelItemType.Green, GameItemShapeType.Single },
		{ LevelItemType.Red, GameItemShapeType.Single },
		{ LevelItemType.Violet, GameItemShapeType.Single },
		{ LevelItemType.Yellow, GameItemShapeType.Single },
		{ LevelItemType.Random, GameItemShapeType.Single },
		{ LevelItemType.CenterItem, GameItemShapeType.Single },
		{ LevelItemType.AnimalSingle, GameItemShapeType.Single },
		{ LevelItemType.AnimalTriangle, GameItemShapeType.Triangle },
		{ LevelItemType.AnimalHexagon, GameItemShapeType.Hexagon },
		{ LevelItemType.BossPlace, GameItemShapeType.Hexagon },
		{ LevelItemType.GameLogo, GameItemShapeType.Hexagon }
	};

	public static int VerticalModeMaxRows = 71;
	public static int VerticalModeMaxCols = 11;

	public static int RoundedModeMaxRows = 11;
	public static int RoundedModeMaxCols = 11;

	// animal在圆形模式下的row和col
	public static int CenterItemRow = RoundedModeMaxRows / 2;
	public static int CenterItemCol = RoundedModeMaxCols / 2;

	// 没找到更好的解决方法，为了让编辑器能复用levelData，目前把所有成员变量设为public
	public int currentLevel;
	public LevelGameItem[] map = new LevelGameItem[VerticalModeMaxRows * VerticalModeMaxCols];
	public int rowCount;
	public int colCount;

	//List of mission in this map
	public StageMoveMode stageMoveMode;
	public MissionType missionType;
	public int missionPoints;
	public int limitAmount = 40;

	public List<LevelItemType> ballColors = new List<LevelItemType> ();
	public int allowedColorCount;
	public int[] starScores = new int[3];
    public int freeBoostScore;
	private int[] itemTypeCounts = new int[System.Enum.GetValues (typeof(LevelItemType)).Length];

	public Target GetTarget (int levelNumber)
	{
		LoadLevel (levelNumber);
		return (Target)stageMoveMode;
	}

	public LevelGameItem MapData (int row, int col)
	{
		return map [row * colCount + col];
	}

	void ProcessGameDataFromString (string mapText)
	{
		string[] lines = mapText.Split (new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
		ballColors.Clear ();
		map = new LevelGameItem[VerticalModeMaxRows * VerticalModeMaxCols];

		int mapLine = 0;
		foreach (string line in lines) {
            if (line.StartsWith ("MISSIONTYPE ")) {
                string modeString = line.Replace ("MISSIONTYPE", string.Empty).Trim ();
                missionType = (MissionType)int.Parse (modeString);
                stageMoveMode = missionType == MissionType.RescueGhost ? StageMoveMode.Rounded : StageMoveMode.Vertical;
            } else if (line.StartsWith ("SIZE ")) {
                string blocksString = line.Replace ("SIZE", string.Empty).Trim ();
                string[] sizes = blocksString.Split (new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                colCount = int.Parse (sizes [0]);
                rowCount = int.Parse (sizes [1]);
            } else if (line.StartsWith ("LIMIT ")) {
                string blocksString = line.Replace ("LIMIT", string.Empty).Trim ();
                string[] sizes = blocksString.Split (new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                limitAmount = int.Parse (sizes [1]);
                //limitType = (LIMIT)int.Parse(sizes[0]);
                // TODO: 加一个打击球的颜色列表
            } else if (line.StartsWith ("COLOR LIMIT ")) {
                string blocksString = line.Replace ("COLOR LIMIT", string.Empty).Trim ();
                allowedColorCount = int.Parse (blocksString);
            } else if (line.StartsWith ("STARS ")) {
                string blocksString = line.Replace ("STARS", string.Empty).Trim ();
                string[] blocksNumbers = blocksString.Split (new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < 3; ++i) {
                    starScores [i] = int.Parse (blocksNumbers [i]);
                }
            } else if (line.StartsWith ("FREE BOOST SCORE ")) {
                string blocksString = line.Replace ("FREE BOOST SCORE", string.Empty).Trim ();
                freeBoostScore = int.Parse (blocksString);
                
			} else { //Maps
				//Split lines again to get map numbers
				string[] st = line.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < st.Length; i++) {
					map [mapLine * colCount + i] = new LevelGameItem (st [i]);
					AddLevelItemTypeStats ((int)map [mapLine * colCount + i].type);
				}
				mapLine++;
			}
		}

		// 根据文件创建该level全部颜色表
		if (ballColors.Count == 0) {
			for (int i = 1; i <= allowedColorCount; ++i) {
				ballColors.Add ((LevelItemType)i);
			}
		}
	}

	void AddLevelItemTypeStats (int itemType)
	{
		itemTypeCounts [itemType]++;
		int ballColorCount = Enum.GetNames (typeof(BallColor)).Length;
		if (!ballColors.Contains ((LevelItemType)itemType) && itemType > 0 && itemType <= ballColorCount) {
			ballColors.Add ((LevelItemType)itemType);
		}
	}

	void CalculateMissionPoints ()
	{
		switch (missionType) {
		case MissionType.EliminateBalls:
			missionPoints = 6;
			break;
		case MissionType.RescueGhost:
			missionPoints = 1;
			break;
		case MissionType.SaveAnimals:
			missionPoints = itemTypeCounts [(int)LevelItemType.AnimalSingle] +
			itemTypeCounts [(int)LevelItemType.AnimalTriangle] +
			itemTypeCounts [(int)LevelItemType.AnimalHexagon];
			break;
		case MissionType.BossBattle:
			missionPoints = itemTypeCounts [(int)LevelItemType.BossPlace];
			break;
		}
	}

	public bool LoadLevel (int curLevel)
	{
		currentLevel = curLevel;
		//Read data from text file
		TextAsset mapText = Resources.Load ("Levels/" + currentLevel) as TextAsset;
		if (mapText == null) {
			return false;
		}
		ProcessGameDataFromString (mapText.text);
		CalculateMissionPoints ();
		return true;
	}

	public void SaveLevel ()
	{
		string saveString = "";
		//Create save string
		saveString += "MISSIONTYPE " + (int)missionType;
		saveString += "\n";
		saveString += "SIZE " + colCount + "/" + rowCount;
		saveString += "\n";
		saveString += "LIMIT " + "0" + "/" + limitAmount;
		saveString += "\n";
		saveString += "COLOR LIMIT " + allowedColorCount;
		saveString += "\n";
		saveString += "STARS " + starScores [0] + "/" + starScores [1] + "/" + starScores [2];
		saveString += "\n";
        saveString += "FREE BOOST SCORE " + freeBoostScore;
        saveString += "\n";

		//set map data
		for (int row = 0; row < rowCount; row++) {
			for (int col = 0; col < colCount; col++) {
				saveString += map [row * colCount + col].ToMapData ();
				//if this column not yet end of row, add space between them
				if (col < (colCount - 1))
					saveString += " ";
			}
			//if this row is not yet end of row, add new line symbol between rows
			if (row < (rowCount - 1))
				saveString += "\n";
		}

		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) {
			//Write to file
			string activeDir = Application.dataPath + @"/Resources/Levels/";
			string newPath = System.IO.Path.Combine (activeDir, currentLevel + ".txt");
			StreamWriter sw = new StreamWriter (newPath);
			sw.Write (saveString);
			sw.Close ();
		}
	}

	public GameItemShapeType ShapeType (LevelItemType itemType)
	{
		return itemShapeTypes [itemType];
	}
}



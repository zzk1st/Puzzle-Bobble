using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 代替原先的SelectLevelManager
/// </summary>
public class NewSelectLevelManager : MonoBehaviour
{
	public static NewSelectLevelManager Instance;
	private static string levelMissionTypesFilename = "LevelMissionTypes";
	public GameObject[] levels;
	//	public GameObject levelHighlight;
	//自动翻页到当前达到的关卡
	private GameObject selectedLevel;

	void Awake ()
	{
		Instance = this;
	}

	void Start ()
	{
		Initialize ();
	}

	void Initialize ()
	{
		TextAsset missionTypesText = Resources.Load ("Levels/" + levelMissionTypesFilename) as TextAsset;
		if (missionTypesText == null) {
			throw new System.MissingFieldException ("找不到文件" + levelMissionTypesFilename);
		}

		string[] lines = missionTypesText.text.Split (new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

		foreach (string line in lines) {
			string[] numbers = line.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			if (numbers.Length != 2) {
				throw new System.FieldAccessException ("missionTypes文件必须有两个field，错误line=" + line);
			}

			NewSingleLevel level = GetSingleLevel (int.Parse (numbers [0]));
//			print ("haha2:" + numbers [0].ToString ());
			if (level != null) {
				MissionType missionType = (MissionType)int.Parse (numbers [1]);
				int starCount = PlayerPrefs.GetInt (string.Format ("Level.{0:000}.StarsCount", level.levelNumber), 0);
				print ("level:" + level.levelNumber + ",starCount:" + starCount);
				level.Initialize (missionType, starCount);
			}
		}

		// 在这里设置一下关卡数，方便game使用
		PlayerPrefs.SetInt ("LevelCount", GetLevelCount ());
//		SetSelectedLevel (FindOpenLevel ());
	}

	//	GameObject FindOpenLevel ()
	//	{
	//		int selectedLevelNumber = PlayerPrefs.GetInt ("OpenLevel");
	//		return levels.transform.Find ("SingleLevel" + selectedLevelNumber.ToString ()).gameObject;
	//	}

	//	void SetSelectedLevel (GameObject level)
	//	{
	//		if (level != null) {
	//			selectedLevel = level;
	//			levelHighlight.transform.position = selectedLevel.transform.position;
	//			levelHighlight.SetActive (true);
	//		} else {
	//			throw new System.AccessViolationException ("找不到OpenLevel对应的SingleLevel GameObject!");
	//		}
	//
	//		MoveCameraToSelectedLevel ();
	//	}

	void MoveCameraToSelectedLevel ()
	{
		Camera.main.GetComponent<SelectLevelCamera> ().MoveTo (selectedLevel.transform.position);
	}

	NewSingleLevel GetSingleLevel (int levelNumber)
	{
//		foreach (Transform levelTransform in levels.transform) {
//			SingleLevel level = levelTransform.gameObject.GetComponent<SingleLevel> ();
//			if (level.levelNumber == levelNumber) {
//				return level;
//			}
//		}

		try {
			return levels [levelNumber - 1].GetComponent<NewSingleLevel> ();
		} catch (Exception ex) {
			
		}

		// 正常情况下应该enable这个exception
		// 目前我们为了调试方便暂且只返回null
		//throw new System.AccessViolationException("在scene中找不到missiontypes文件中提到的关卡，未知levelNumber=" + levelNumber);
		return null;
	}

	int GetLevelCount ()
	{
		return levels.Length;
	}

	public void StartLevel (GameObject selectedLevelGO)
	{
		NewSingleLevel selectedLevel = selectedLevelGO.GetComponent<NewSingleLevel> ();

		if (selectedLevel.isUnlocked) {
			PlayerPrefs.SetInt ("OpenLevel", selectedLevel.levelNumber);
			PlayerPrefs.Save ();

			SceneManager.LoadScene ("game");
		}
	}
}

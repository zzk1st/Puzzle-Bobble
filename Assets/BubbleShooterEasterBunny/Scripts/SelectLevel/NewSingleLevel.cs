using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 代替原先的SingleLevel,SelectLevelManager还沿用了
/// </summary>
public class NewSingleLevel : MonoBehaviour
{
	private GameObject missionTypeGO;
	private GameObject levelNumberGO;
	private GameObject starsGO;
	private GameObject imageLock;
	private Button btnStartGame;

	public Sprite[] missionTypeTextures;
	//	public Sprite[] inactiveMissionTypeTextures;

	public int levelNumber;
	public MissionType missionType;
	public int starCount;
	public bool isUnlocked;

	public void Initialize (MissionType type, int stars)
	{
		missionTypeGO = transform.Find ("Image MissionType").gameObject;
		levelNumberGO = transform.Find ("Text LevelNumber").gameObject;
		starsGO = transform.Find ("Stars").gameObject;

		missionType = type;
		starCount = stars;

		//关卡数和关卡类型
		levelNumberGO.GetComponent<Text> ().text = levelNumber.ToString ();
		missionTypeGO.GetComponent<Image> ().sprite = missionTypeTextures [(int)missionType];

		// set stars
		for (int i = 1; i <= 3; i++) {
			GameObject curStar = starsGO.transform.Find ("Image Star " + i).gameObject;
			if (i <= starCount) {
				
				curStar.SetActive (true);
			} else {
				curStar.SetActive (false);
			}
		}

		btnStartGame = transform.GetComponent<Button> ();
		btnStartGame.onClick.AddListener (() => {
			NewSelectLevelManager.Instance.StartLevel (gameObject);
		});
			
		imageLock = transform.Find ("Image Lock").gameObject;
		//DEBUG!!!!!!!!
		//isUnlocked = PlayerPrefs.GetInt("Score" + (levelNumber-1)) > 0 || levelNumber == 1;
		//关卡lock状态判断还未做读取处理
		//		isUnlocked = true;
		if (isUnlocked) {
			imageLock.SetActive (false);
			btnStartGame.interactable = true;
		} else {
			imageLock.SetActive (true);
			btnStartGame.interactable = false;
		}

	}
}

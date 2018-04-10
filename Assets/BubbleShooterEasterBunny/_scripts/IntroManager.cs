using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IntroManager : MonoBehaviour
{
	public Button btnStart;
	public Button btnSetting;
	public GameObject panelSetting;
	public Button btnCloseSetting;

	public static IntroManager Instance{ private set; get; }



	void Awake ()
	{
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy (gameObject);
		}
	}

	void Start ()
	{
		btnStart.onClick.AddListener (() => {
			//TODO
		});

		btnSetting.onClick.AddListener (() => {
			OpenSlideLayout ();
		});

		btnCloseSetting.onClick.AddListener (() => {
			CloseSlideLayout ();
		});
	}

	//	void Update ()
	//	{
	//
	//	}

	//-----------------------------------------------------------------

	void OpenSlideLayout ()
	{
		panelSetting.GetComponentInChildren<DOTweenAnimation> ().DORestartAllById ("slide");
	}

	void CloseSlideLayout ()
	{
		foreach (var item in panelSetting.GetComponentsInChildren<DOTweenAnimation>()) {
			item.DOPlayBackwards ();
		}
	}
}

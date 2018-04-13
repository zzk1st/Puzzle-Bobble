using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class LIFESAddCounter : MonoBehaviour {
	Text text;
	static float TimeLeft;
	float TotalTimeForRestLife = 15f * 60;  //8 minutes for restore life
	bool startTimer;
	DateTime templateTime;
	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
		
	}
	
	bool CheckPassedTime(){
		print(PlayerPrefsManager.DateOfExit );
		if(PlayerPrefsManager.DateOfExit == "" || PlayerPrefsManager.DateOfExit == default(DateTime).ToString()) PlayerPrefsManager.DateOfExit = DateTime.Now.ToString();

		DateTime dateOfExit = DateTime.Parse( PlayerPrefsManager.DateOfExit);

		if(DateTime.Now.Subtract( dateOfExit).TotalSeconds > TotalTimeForRestLife * (PlayerPrefsManager.CapOfLife - PlayerPrefsManager.Lifes)){
			//			Debug.Log(dateOfExit + " " + MainMenu.today);
			PlayerPrefsManager.Instance.RestoreLifes();
			PlayerPrefsManager.RestLifeTimer = 0;
			return false;    ///we dont need lifes
		}
		else{
			TimeCount((float) DateTime.Now.Subtract( dateOfExit).TotalSeconds);
			//			Debug.Log(MainMenu.today.Subtract( dateOfExit).TotalSeconds/60/15 +" " + dateOfExit );
			return true;     ///we need lifes
		}
	}
	
	void TimeCount( float tick){
		if(PlayerPrefsManager.RestLifeTimer <= 0) ResetTimer();
		
		PlayerPrefsManager.RestLifeTimer -= tick;
		if(PlayerPrefsManager.RestLifeTimer<=1  && PlayerPrefsManager.Lifes < PlayerPrefsManager.CapOfLife){ PlayerPrefsManager.Instance.AddLife(1);  ResetTimer();}
		//		}
	}
	
	void ResetTimer(){
		PlayerPrefsManager.RestLifeTimer =  TotalTimeForRestLife;
	}
	
	// Update is called once per frame
	void Update () {
		if(!startTimer && DateTime.Now.Subtract( DateTime.Now).Days == 0){
			PlayerPrefsManager.DateOfRestLife = DateTime.Now;
			if(PlayerPrefsManager.Lifes < PlayerPrefsManager.CapOfLife){
				if(CheckPassedTime())
					startTimer = true;
				//	StartCoroutine(TimeCount());
			}
		}
		
		if(startTimer)
			TimeCount(Time.deltaTime);
		
		if(gameObject.activeSelf){
			if(PlayerPrefsManager.Lifes < PlayerPrefsManager.CapOfLife){
				int minutes = Mathf.FloorToInt(PlayerPrefsManager.RestLifeTimer / 60F);
				int seconds = Mathf.FloorToInt(PlayerPrefsManager.RestLifeTimer - minutes * 60);
				
				text.enabled = true;
				text.text = "" + string.Format("{0:00}:{1:00}", minutes, seconds);
				PlayerPrefsManager.timeForReps = text.text;
				//				//	text.text = "+1 in \n " + Mathf.FloorToInt( MainMenu.RestLifeTimer/60f) + ":" + Mathf.RoundToInt( (MainMenu.RestLifeTimer/60f - Mathf.FloorToInt( MainMenu.RestLifeTimer/60f))*60f);
			}
			else{
				text.text = "FULL";
			}
		}
	}
	
	void OnApplicationPause(bool pauseStatus) {
		if(pauseStatus){ 
			//	StopCoroutine("TimeCount");
			PlayerPrefsManager.DateOfExit = DateTime.Now.ToString();
			//			PlayerPrefs.SetString("DateOfExit",DateTime.Now.ToString());
			//			PlayerPrefs.Save();
		}
		else{
			startTimer = false;
			//MainMenu.today = DateTime.Now; 
			//		MainMenu.DateOfExit = PlayerPrefs.GetString("DateOfExit");
		}
	}

	void OnEnable(){
		startTimer = false;
	}

	void OnDisable(){
		PlayerPrefsManager.DateOfExit = DateTime.Now.ToString();
	}
}

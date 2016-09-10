﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using InitScriptName;
using UnityEngine.SceneManagement;

public class Counter : MonoBehaviour {
  //  UILabel label;
    Text label;
	// Use this for initialization
	void Start () {
        label = GetComponent<Text>(); 
	}
	
	// Update is called once per frame
	void Update () {
        if (name == "Moves")
        {
            label.text = "" + mainscript.Instance.levelData.limitAmount;
            if (mainscript.Instance.levelData.limitAmount <= 5 && GameManager.Instance.gameStatus == GameStatus.Playing)
            {
                label.color = Color.red;
                if (!GetComponent<Animation>().isPlaying)
                {
                    GetComponent<Animation>().Play();
                    SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.alert);
                }
            }
        }

        if ( name == "Scores" || name == "Score" )
        {
            label.text = "" + ScoreManager.Instance.Score;
        }
        if( name == "Level" )
        {
            label.text = "" + PlayerPrefs.GetInt("OpenLevel");
        }
        if( name == "Target" )
        {
            if(mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
                label.text = "" + Mathf.Clamp( mainscript.Instance.TargetCounter1, 0, 6 ) + "/6";
            else if(mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
                label.text = "" + Mathf.Clamp(mainscript.Instance.TargetCounter1, 0, 1)+ "/1";
        }

        if( name == "Lifes" )
        {
            label.text = "" + InitScript.Instance.GetLife();
        }

        if( name == "Gems" )
        {
            label.text = "" + InitScript.Gems;
        }
        if( name == "5BallsBoost" )
        {
            label.text = "" + GetPlus(InitScript.Instance.FiveBallsBoost);
        }
        if( name == "ColorBallBoost" )
        {
            label.text = "" + GetPlus(InitScript.Instance.ColorBallBoost);
        }
        if( name == "FireBallBoost" )
        {
            label.text = "" + GetPlus(InitScript.Instance.FireBallBoost);
        }
        if( name == "TargetDescription" )
        {
            label.text = "" + GetTarget( );
        }
	}

    string GetPlus(int boostCount)
    {
        if( boostCount > 0 ) return ""+ boostCount;
        else return "+";
    }

    string GetTarget()
    {
        if( SceneManager.GetActiveScene().name == "map" )
        {
            if( InitScript.Instance.currentTarget == Target.Top ) return "Clear the top";
            else if( InitScript.Instance.currentTarget == Target.Chicken ) return "Rescue the chicken";

        }
        else
        {
            if( mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Vertical ) return "Clear the top";
            else if( mainscript.Instance.levelData.stageMoveMode == StageMoveMode.Rounded ) return "Rescue the chicken";

        }
        return "";
    }
}

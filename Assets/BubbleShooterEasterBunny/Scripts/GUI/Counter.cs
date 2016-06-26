using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using InitScriptName;
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
            label.text = "" + LevelData.LimitAmount;
            if( LevelData.LimitAmount <= 5 && GamePlay.Instance.GameStatus == GameState.Playing )
            {
                label.color = Color.red;
                if( !GetComponent<Animation>().isPlaying )
                {
                    GetComponent<Animation>().Play();
                    SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.alert );
                }
            }
        }
        if( name == "Scores" || name == "Score" )
        {
            label.text = "" + mainscript.Score;
        }
        if( name == "Level" )
        {
            label.text = "" + PlayerPrefs.GetInt("OpenLevel");
        }
        if( name == "Target" )
        {
            if(LevelData.mode == ModeGame.Vertical)
                label.text = "" + Mathf.Clamp( mainscript.Instance.TargetCounter1, 0, 6 ) + "/6";
            else if(LevelData.mode == ModeGame.Rounded)
                label.text = "" + Mathf.Clamp(mainscript.Instance.TargetCounter1, 0, 1)+ "/1";
            else if( LevelData.mode == ModeGame.Animals )
                label.text = "" + mainscript.Instance.TargetCounter1 + "/" + mainscript.Instance.TotalTargets;
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
        if( Application.loadedLevelName == "map" )
        {
            if( InitScript.Instance.currentTarget == Target.Top ) return "Clear the top";
            else if( InitScript.Instance.currentTarget == Target.Chicken ) return "Rescue the chicken";

        }
        else
        {
            if( LevelData.mode == ModeGame.Vertical ) return "Clear the top";
            else if( LevelData.mode == ModeGame.Rounded ) return "Rescue the chicken";

        }
        return "";
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
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
            if (CoreManager.Instance.levelData.limitAmount >= 0)
                label.text = "" + CoreManager.Instance.levelData.limitAmount;
            else
                label.text = "0";
            if (CoreManager.Instance.levelData.limitAmount < 5 && UIManager.Instance.gameStatus == GameStatus.Playing)
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
        if (name == "PotScore1")
        {
            label.text = "" + GameObject.Find("Pot1").GetComponent<Pot>().score * ScoreManager.Instance.DoubleScore;
        }
        if (name == "PotScore2")
        {
            label.text = "" + GameObject.Find("Pot2").GetComponent<Pot>().score * ScoreManager.Instance.DoubleScore;
        }
        if (name == "PotScore3")
        {
            label.text = "" + GameObject.Find("Pot3").GetComponent<Pot>().score * ScoreManager.Instance.DoubleScore;
        }
        if ( name == "Level" )
        {
            label.text = "" + PlayerPrefs.GetInt("OpenLevel");
        }
        if( name == "Target" )
        {
            if(CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Vertical)
                label.text = "" + Mathf.Clamp( CoreManager.Instance.TargetCounter1, 0, 6 ) + "/6";
            else if(CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Rounded)
                label.text = "" + Mathf.Clamp(CoreManager.Instance.TargetCounter1, 0, 1)+ "/1";
        }

        if( name == "Lifes" )
        {
            label.text = "" + PlayerPrefsManager.Instance.GetLife();
        }

        if( name == "Gems" )
        {
            label.text = "" + PlayerPrefsManager.Gems;
        }
        if( name == "MagicBallBoost" )
        {
            label.text = "" + GetPlus(PlayerPrefsManager.Instance.MagicBallBoost);
        }
        if( name == "ColorBallBoost" )
        {
            label.text = "" + GetPlus(PlayerPrefsManager.Instance.ColorBallBoost);
        }
        if( name == "FireBallBoost" )
        {
            label.text = "" + GetPlus(PlayerPrefsManager.Instance.FireBallBoost);
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
            if( PlayerPrefsManager.Instance.currentTarget == Target.Top ) return "Clear the top";
            else if( PlayerPrefsManager.Instance.currentTarget == Target.Chicken ) return "Rescue the chicken";

        }
        else
        {
            if( CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Vertical ) return "Clear the top";
            else if( CoreManager.Instance.levelData.stageMoveMode == StageMoveMode.Rounded ) return "Rescue the chicken";

        }
        return "";
    }
}

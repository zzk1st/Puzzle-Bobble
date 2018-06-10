using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostManager : MonoBehaviour {

    public static BoostManager Instance;

    public GameObject freeBoostCrystal;
    public GameObject boostBarOnlyMask;

    private bool freeBoostRewarded;
    private bool selectingFreeBoost;

    void Awake () {
        Instance = this;
    }

    public void MagicBallBoost()
    {
        if (UIManager.Instance.gameStatus != GameStatus.Playing)
            return;

        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (selectingFreeBoost) 
        {
            selectFreeBoostComplete ();
            PlayerPrefsManager.Instance.SpendBoost(BoostType.MagicBallBoost);
        }
        else if (PlayerPrefsManager.Instance.MagicBallBoost > 0)
        {
            PlayerPrefsManager.Instance.SpendBoost(BoostType.MagicBallBoost);
        }
        else
        {
            OpenBoostShop(BoostType.MagicBallBoost);
        }
    }

    public void ColorBallBoost()
    {
        if (UIManager.Instance.gameStatus != GameStatus.Playing)
            return;
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (selectingFreeBoost) 
        {
            selectFreeBoostComplete ();
            PlayerPrefsManager.Instance.SpendBoost(BoostType.RainbowBallBoost);
        }
        else if (PlayerPrefsManager.Instance.ColorBallBoost > 0)
        {
            if (UIManager.Instance.gameStatus == GameStatus.Playing)
                PlayerPrefsManager.Instance.SpendBoost(BoostType.RainbowBallBoost);
        }
        else
        {
            OpenBoostShop(BoostType.RainbowBallBoost);
        }

    }

    public void FireBallBoost()
    {
        if (UIManager.Instance.gameStatus != GameStatus.Playing)
            return;
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);

        if (selectingFreeBoost) 
        {
            selectFreeBoostComplete ();
            PlayerPrefsManager.Instance.SpendBoost(BoostType.FireBallBoost);
        }
        else if (PlayerPrefsManager.Instance.FireBallBoost > 0)
        {
            if (UIManager.Instance.gameStatus == GameStatus.Playing)
                PlayerPrefsManager.Instance.SpendBoost(BoostType.FireBallBoost);
        }
        else
        {
            OpenBoostShop(BoostType.FireBallBoost);
        }

    }

    public void OpenBoostShop(BoostType boostType)
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        GameObject.Find("Canvas").transform.Find("NewBoostShop").gameObject.GetComponent<NewBoostShop>().SetBoost(boostType);
    }

    /*
    public void BuyBoost(BoostType boostType, int price)
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (InitScript.Gems >= price)
        {
            InitScript.Instance.BuyBoost(boostType, 1, price);
            InitScript.Instance.SpendBoost(boostType);
            CloseMenu();
        }
        else
        {
            BuyGems();
        }
    }
    */

    public void SetFreeBoostCrystal (int score) {
        if (score == 0) {
            // reset the score
            freeBoostRewarded = false;
        } 

        float freeBoostScore = CoreManager.Instance.levelData.freeBoostScore;
        float curScore = Mathf.Min (score, freeBoostScore);
        float ratio = curScore / freeBoostScore * 100.0f;
        freeBoostCrystal.GetComponent<CustomProgress> ().mValue = ratio;
        if (ratio >= 100.0f && freeBoostRewarded == false) {
            freeBoostRewarded = true;
            StartBoostSelectMode ();
        }
    }

    void StartBoostSelectMode () {
        boostBarOnlyMask.SetActive (true);
        selectingFreeBoost = true;
    }

    void selectFreeBoostComplete () {
        selectingFreeBoost = false;
        boostBarOnlyMask.SetActive (false);
    }
}

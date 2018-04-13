using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InitScriptName;

public class BoostBarView : MonoBehaviour {

    public void MagicBallBoost()
    {
        if (UIManager.Instance.gameStatus != GameStatus.Playing)
            return;
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (InitScript.Instance.MagicBallBoost > 0)
        {
            if (UIManager.Instance.gameStatus == GameStatus.Playing)
                InitScript.Instance.SpendBoost(BoostType.MagicBallBoost);
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
        if (InitScript.Instance.ColorBallBoost > 0)
        {
            if (UIManager.Instance.gameStatus == GameStatus.Playing)
                InitScript.Instance.SpendBoost(BoostType.RainbowBallBoost);
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
        if (InitScript.Instance.FireBallBoost > 0)
        {
            if (UIManager.Instance.gameStatus == GameStatus.Playing)
                InitScript.Instance.SpendBoost(BoostType.FireBallBoost);
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
}

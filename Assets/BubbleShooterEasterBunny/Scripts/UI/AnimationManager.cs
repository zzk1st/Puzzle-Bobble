using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using InitScriptName;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class AnimationManager : MonoBehaviour
{
    public bool PlayOnEnable = true;
    bool WaitForPickupFriends;

    bool WaitForAksFriends;
    System.Collections.Generic.Dictionary<string, string> parameters;

    void OnEnable()
    {
        if (PlayOnEnable)
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.swish[0]);
        }
        if (name == "MenuPlay")
        {
            for (int i = 1; i <= 3; i++)
            {
                transform.Find("Image").Find("Star" + i).gameObject.SetActive(false);
            }
            int stars = PlayerPrefs.GetInt(string.Format("Level.{0:000}.StarsCount", PlayerPrefs.GetInt("OpenLevel")), 0);
            if (stars > 0)
            {
                for (int i = 1; i <= stars; i++)
                {
                    transform.Find("Image").Find("Star" + i).gameObject.SetActive(true);
                }

            }
            else
            {
                for (int i = 1; i <= 3; i++)
                {
                    transform.Find("Image").Find("Star" + i).gameObject.SetActive(false);
                }

            }

        }

        if (name == "Settings" || name == "MenuPause")
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
                transform.Find("Image/Sound/SoundOff").gameObject.SetActive(true);
            else
                transform.Find("Image/Sound/SoundOff").gameObject.SetActive(false);

            if (PlayerPrefs.GetInt("Music") == 0)
                transform.Find("Image/Music/MusicOff").gameObject.SetActive(true);
            else
                transform.Find("Image/Music/MusicOff").gameObject.SetActive(false);

        }

    }

    void OnDisable()
    {
        //if( PlayOnEnable )
        //{
        //    if( !GetComponent<SequencePlayer>().sequenceArray[0].isPlaying )
        //        GetComponent<SequencePlayer>().sequenceArray[0].Play
        //}
    }

    public void OnFinished()
    {
        if (name == "MenuComplete")
        {
            StartCoroutine(MenuComplete());
            StartCoroutine(MenuCompleteScoring());
        }
        if (name == "MenuPlay")
        {
            InitScript.Instance.currentTarget = mainscript.Instance.levelData.GetTarget(PlayerPrefs.GetInt("OpenLevel"));
        }
    }

    IEnumerator MenuComplete()
    {
        for (int i = 1; i <= mainscript.Instance.stars; i++)
        {
            //  SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.scoringStar );
            transform.Find("Image").Find("Star" + i).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.hit);
        }
    }

    IEnumerator MenuCompleteScoring()
    {
        Text scores = transform.Find("Image").Find("Scores").GetComponent<Text>();
        for (int i = 0; i <= ScoreManager.Instance.Score; i += 500)
        {
            scores.text = "" + i;
            // SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.scoring );
            yield return new WaitForSeconds(0.00001f);
        }
        scores.text = "" + ScoreManager.Instance.Score;
    }


    public void PlaySoundButton()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);

    }

    public IEnumerator Close()
    {
        yield return new WaitForSeconds(0.5f);
    }

    public void CloseMenu()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (gameObject.name == "MenuPreGameOver")
        {
            ShowGameOver();
        }
        else if (gameObject.name == "MenuComplete" || gameObject.name == "MenuGameOver")
        {
            SceneManager.LoadScene("map2");
        }

        if (SceneManager.GetActiveScene().name == "game")
        {
            if (UIManager.Instance.gameStatus == GameStatus.Pause)
            {
                UIManager.Instance.Resume();

            }
        }
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.swish[1]);

        gameObject.SetActive(false);
    }

    public void Play()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (gameObject.name == "MenuPreGameOver")
        {
            if (InitScript.Gems >= 12)
            {
                InitScript.Instance.SpendGems(12);
                gameObject.SetActive(false);

            }
            else
            {
                BuyGems();
            }
        }
        else if (gameObject.name == "MenuGameOver")
        {
            SceneManager.LoadScene("map2");
        }
        else if (gameObject.name == "MenuPlay")
        {
            if (InitScript.Lifes > 0)
            {
                InitScript.Instance.SpendLife(1);

                SceneManager.LoadScene("game");
            }
            else
            {
                BuyLifeShop();
            }

        }
        else if (gameObject.name == "PlayMain")
        {
            SceneManager.LoadScene("map2");
        }
    }

    public void PlayTutorial()
    {
//        SoundBase.Instance.audio.PlayOneShot( SoundBase.Instance.click );
        UIManager.Instance.Play();
//        CloseMenu();
    }

    public void NextLevel()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        //进入下一关
        if (PlayerPrefs.GetInt("OpenLevel") <= PlayerPrefs.GetInt("LevelCount"))
        {
            PlayerPrefs.SetInt("OpenLevel", PlayerPrefs.GetInt("OpenLevel") + 1);
        }

        SceneManager.LoadScene("map2");
    }

    public void BuyGems()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        GameObject.Find("Canvas").transform.Find("GemsShop").gameObject.SetActive(true);
    }

    public void Buy(GameObject pack)
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (pack.name == "Pack1")
        {
            InitScript.waitedPurchaseGems = int.Parse(pack.transform.Find("Count").GetComponent<Text>().text.Replace("x ", ""));
#if UNITY_WEBPLAYER
            InitScript.Instance.PurchaseSucceded();
            CloseMenu();
            return;
#endif
            INAPP.Instance.purchaseProduct("pack1");
            //	INAPP.Instance.purchaseProduct("android.test.refunded");
        }

        if (pack.name == "Pack2")
        {
            InitScript.waitedPurchaseGems = int.Parse(pack.transform.Find("Count").GetComponent<Text>().text.Replace("x ", ""));
#if UNITY_WEBPLAYER
            InitScript.Instance.PurchaseSucceded();
            CloseMenu();
            return;
#endif
            INAPP.Instance.purchaseProduct("pack2");
            //	INAPP.Instance.purchaseProduct("android.test.refunded");
        }
        if (pack.name == "Pack3")
        {
            InitScript.waitedPurchaseGems = int.Parse(pack.transform.Find("Count").GetComponent<Text>().text.Replace("x ", ""));
#if UNITY_WEBPLAYER
            InitScript.Instance.PurchaseSucceded();
            CloseMenu();
            return;
#endif
            INAPP.Instance.purchaseProduct("pack3");
            //	INAPP.Instance.purchaseProduct("android.test.refunded");
        }
        if (pack.name == "Pack4")
        {
            InitScript.waitedPurchaseGems = int.Parse(pack.transform.Find("Count").GetComponent<Text>().text.Replace("x ", ""));
#if UNITY_WEBPLAYER
            InitScript.Instance.PurchaseSucceded();
            CloseMenu();
            return;
#endif
            INAPP.Instance.purchaseProduct("pack4");
            //	INAPP.Instance.purchaseProduct("android.test.refunded");
        }
        CloseMenu();

    }

    public void BuyLifeShop()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (InitScript.Lifes < InitScript.CapOfLife)
            GameObject.Find("Canvas").transform.Find("LiveShop").gameObject.SetActive(true);

    }

    public void BuyLife(GameObject button)
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (InitScript.Gems >= int.Parse(button.transform.Find("Price").GetComponent<Text>().text))
        {
            InitScript.Instance.SpendGems(int.Parse(button.transform.Find("Price").GetComponent<Text>().text));
            InitScript.Instance.RestoreLifes();
            CloseMenu();
        }
        else
        {
            GameObject.Find("Canvas").transform.Find("GemsShop").gameObject.SetActive(true);
        }

    }



    void ShowGameOver()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.gameOver);

        GameObject.Find("Canvas").transform.Find("MenuGameOver").gameObject.SetActive(true);
        gameObject.SetActive(false);

    }

    #region Settings

    public void ShowSettings(GameObject menuSettings)
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if (!menuSettings.activeSelf)
        {
            menuSettings.SetActive(true);
            //           menuSettings.GetComponent<SequencePlayer>().Play();
        }
        else
            menuSettings.SetActive(false);
    }

    public void SoundOff(GameObject Off)
    {
        if (!Off.activeSelf)
        {
            SoundBase.Instance.GetComponent<AudioSource>().volume = 0;
            InitScript.sound = false;

            Off.SetActive(true);
        }
        else
        {
            SoundBase.Instance.GetComponent<AudioSource>().volume = 1;
            InitScript.sound = true;

            Off.SetActive(false);

        }
        PlayerPrefs.SetInt("Sound", (int)SoundBase.Instance.GetComponent<AudioSource>().volume);
        PlayerPrefs.Save();

    }

    public void MusicOff(GameObject Off)
    {
        if (!Off.activeSelf)
        {
            GameObject.Find("Music").GetComponent<AudioSource>().volume = 0;
            InitScript.music = false;

            Off.SetActive(true);
        }
        else
        {
            GameObject.Find("Music").GetComponent<AudioSource>().volume = 1;
            InitScript.music = true;

            Off.SetActive(false);

        }
        PlayerPrefs.SetInt("Music", (int)GameObject.Find("Music").GetComponent<AudioSource>().volume);
        PlayerPrefs.Save();

    }

    public void Quit()
    {
        if (SceneManager.GetActiveScene().name == "game")
            SceneManager.LoadScene("map2");
        else
            Application.Quit();
    }



    #endregion



}

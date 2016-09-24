using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using InitScriptName;

public class NewBoostShop : MonoBehaviour {
    public Sprite[] icons;
    public string[] titles;
    public string[] descriptions;
    public int[] prices;

    public Image icon;
    public Text title;
    public Text description;
    public Text price;

    private BoostType curBoostType;

    public void SetBoost(BoostType boostType)
    {
        curBoostType = boostType;
        int index = (int) curBoostType;

        icon.sprite = icons[index];
        title.text = titles[index];
        description.text = descriptions[index];
        price.text = prices[index].ToString();

        GameManager.Instance.Pause();
        gameObject.SetActive( true );
    }

    public void BuyBoost()
    {
        int boostPrice = int.Parse(price.text);
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.click);
        if( InitScript.Gems >= boostPrice)
        {
            InitScript.Instance.BuyBoost(curBoostType, 1, boostPrice);
            InitScript.Instance.SpendBoost(curBoostType);
            CloseMenu();
        }
        else
        {
            BuyGems();
        }
    }

    public void CloseMenu()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.click );

        if( GameManager.Instance.gameStatus == GameStatus.Pause )
        {
            GameManager.Instance.Resume();
        }

        gameObject.SetActive( false );
    }

    void BuyGems()
    {
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot( SoundBase.Instance.click );
        GameObject.Find( "Canvas" ).transform.Find( "GemsShop" ).gameObject.SetActive( true );
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Animal : MonoBehaviour {

    private GameItem _gameItem;

    private GameObject animalShell;
    private GameObject animalBody;
    private Animator escapeAnim;
    public float escape_time = 1.8f;

    public Vector3 flyingMiddlePoint;

    private int idleStateHash = Animator.StringToHash("Base.Idle");
    private int playHash = Animator.StringToHash("Play");

    private GameObject targetImage;

    public void Initialize()
    {
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.startFallFunc = StartFall;
        animalShell = transform.GetChild(0).gameObject;
        animalBody = transform.GetChild(1).gameObject;
        _gameItem.ConnectToGrid();
        flyingMiddlePoint = new Vector3(0, 3, 0);
        escapeAnim = transform.FindChild("AnimalEscape").GetComponent<Animator>();
    }

    public void SetSprite(LevelItemType itemType)
    {
        if (itemType == LevelItemType.AnimalSingle)
        {
            int idx = Random.Range(0, mainscript.Instance.animalSingleSprites.Length);
            animalShell.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalSingleShellSprite;
            animalBody.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalSingleSprites[idx];
            return;
        }
        if (itemType == LevelItemType.AnimalHexagon)
        {
            int idx = Random.Range(0, mainscript.Instance.animalHexSprites.Length);
            animalShell.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalHexShellSprite;
            animalBody.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalHexSprites[idx];
            return;
        }
    }

    public void Fly()
    {
        targetImage = GameObject.Find("MissionTypeImage");
        mainscript.Instance.black_back.SetActive(true);

        Destroy(animalShell);
        playEscapeAnim();
        // 我们要让这个star显示在UI层
        animalBody.GetComponent<SpriteRenderer>().sortingLayerName = "UI layer";
        animalBody.GetComponent<SpriteRenderer>().sortingOrder = 10;
        Vector3 cur_scale = animalBody.transform.localScale;

        Vector3[] paths = new Vector3[3];
        paths[0] = transform.position;
        paths[1] = flyingMiddlePoint;
        paths[2] = targetImage.transform.position;

        
        Hashtable args_move2 = new Hashtable();
        args_move2.Add("easeType", iTween.EaseType.easeInOutQuad);
        args_move2.Add("path", paths);
        args_move2.Add("time", escape_time);
        args_move2.Add("delay", 0.5f);

        Hashtable args_scale = new Hashtable();
        args_scale.Add("time", escape_time);
        args_scale.Add("x", cur_scale.x * 2f);
        args_scale.Add("y", cur_scale.y * 2f);
        args_scale.Add("delay", 0.5f);
        iTween.ScaleTo(animalBody, args_scale);
        iTween.MoveTo(animalBody, args_move2);
        iTween.FadeTo(animalBody, iTween.Hash("alpha", 0.0f, "delay", escape_time+0.5f, "onComplete", "onFlyingComplete", "onCompleteTarget", gameObject));
        

    }

    void playEscapeAnim()
    {
        //if (escapeAnim.GetCurrentAnimatorStateInfo(0).fullPathHash == idleStateHash)
        {
            escapeAnim.GetComponent<SpriteRenderer>().sortingLayerName = "UI layer";
            escapeAnim.GetComponent<SpriteRenderer>().sortingOrder = 10;
            escapeAnim.SetTrigger(playHash);
        }
    }

    void onFlyingComplete()
    {
        mainscript.Instance.black_back.SetActive(false);
        Destroy(gameObject);
    }

    void StartFall()
    {
        // TODO: 球掉落，动物动画删除
        MissionManager.Instance.GainAnimalPoint();
        _gameItem.DisconnectFromGrid();

        // Fly then destroy
        Fly();
    }
}

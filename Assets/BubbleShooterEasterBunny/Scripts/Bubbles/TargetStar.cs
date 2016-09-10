using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TargetStar : MonoBehaviour
{
    private GameObject targetCountGO;
    private GameObject targetImageGO;

    public Vector3 flyingMiddlePoint;

    public void fly()
    {
        targetCountGO = GameObject.Find("TargetCount");
        targetImageGO = GameObject.Find("TargetImage");

        // 我们要让这个star显示在UI层
        GetComponent<SpriteRenderer>().sortingLayerName = "UI layer";
        GetComponent<SpriteRenderer>().sortingOrder = 10;

        Vector3[] paths = new Vector3[3];
        paths[0] = transform.position;
        paths[1] = flyingMiddlePoint;
        paths[2] = targetImageGO.transform.position;

        Hashtable args = new Hashtable();
        args.Add("easeType", iTween.EaseType.easeInOutQuad);
        args.Add("path", paths);
        args.Add("speed", 50f);
        args.Add("onComplete", "onFlyingComplete");
        iTween.MoveTo(gameObject, args);
    }

    void onFlyingComplete()
    {
        Destroy(gameObject);
        // TODO: 以后mission manager负责更新
        targetCountGO.GetComponent<Text>().text = GameManager.Instance.emptyTopGrid.ToString() + "/6";
    }
}

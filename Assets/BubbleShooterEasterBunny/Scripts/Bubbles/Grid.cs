using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private GameObject busy;

    // 该属性表示一个grid连着的ball
    public GameObject Busy
    {
        get { return busy; }
        set
        {
            if( value != null )
            {
                if( value.GetComponent<Ball>() != null )
                {
                    value.GetComponent<Ball>().mesh = gameObject;
                }

            }

            busy = value;
        }
    }

    GameObject[] meshes;
    bool destroyed;
    public float offset;    // 表示该行的每个ball针对行首的便宜（因为蜂窝状排列导致）
    bool triggerball;
    public GameObject boxFirst;     // 即将发射的ball的box
    public GameObject boxSecond;    // 等待的ball的box
    public static bool waitForAnim;

    // Use this for initialization
    void Start()
    {
    }
}

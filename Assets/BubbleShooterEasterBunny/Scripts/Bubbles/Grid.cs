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
    bool triggerball;
    public GameObject boxFirst;     // 即将发射的ball的box
    public GameObject boxSecond;    // 等待的ball的box
    public static bool waitForAnim;

    // Use this for initialization
    void Start()
    {
    }
}

using UnityEngine;
using System.Collections;

public class MoveStage : MonoBehaviour {
    void OnCollisionEnter2D(Collision2D coll)
    {
        // 该代码用来探测是否某个stage碰到了某个它不想碰的东西
        Debug.Log("Something happened!!!!!!!!!!");
    }
}

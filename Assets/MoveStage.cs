using UnityEngine;
using System.Collections;
using System;

public class MoveStage : MonoBehaviour {
    void OnCollisionEnter2D(Collision2D coll)
    {
        // 该代码用来探测是否某个stage碰到了某个它不想碰的东西
        Debug.Log(String.Format("Unexpected collision! coll1.name={0}, coll2.name={1}", 
                                coll.contacts[0].collider.name,
                                coll.contacts[0].otherCollider.name)
                 );
    }
}

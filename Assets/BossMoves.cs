using UnityEngine;
using System.Collections;

public class BossMoves : MonoBehaviour {
    public GameObject bossAnimGO;

    void Awake()
    {
        bossAnimGO = transform.FindChild("BossAnim");
    }

	// Use this for initialization
	void Start ()
    {
        // 先让boss移动到屏幕外
        transform.position = new Vector3(-5.5f, 0f, 0f);
	}

    public void MoveToBossPlace(BossPlace place)
    {
        if (place.transform.position.x > transform.position)
        {
            
        }
        bossAnimGO.GetComponent<Animator>().SetTrigger("Escape");
    }
	
}

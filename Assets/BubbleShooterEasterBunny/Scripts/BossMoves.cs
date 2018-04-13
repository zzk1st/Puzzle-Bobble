using UnityEngine;
using System.Collections;

public class BossMoves : MonoBehaviour {
    GameObject bossAnimGO;
    private Vector3 initialScale;

	// Use this for initialization
	void Start ()
    {
        bossAnimGO = transform.Find("BossAnimNode").Find("BossAnim").gameObject;
        // 先让boss移动到屏幕外
        transform.position = new Vector3(-5.5f, 0f, 0f);
        initialScale = transform.localScale;
	}

    public void MoveToBossPlace(BossPlace targetPlace)
    {
        transform.parent = transform.root;
        if (targetPlace.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z);
        }
        else
        {
            transform.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z);
        }
        bossAnimGO.GetComponent<Animator>().SetTrigger("Escape");

        StartCoroutine(MoveToBossPlaceCor(targetPlace));
    }
	
    IEnumerator MoveToBossPlaceCor(BossPlace targetPlace)
    {
        float initDis = Vector3.Distance(targetPlace.gameObject.transform.position, gameObject.transform.position);
        float dis = initDis;

        float speed = 0.1f;

        while (dis > 0.1f)
        {
            Vector3 bossPos = gameObject.transform.position;
            Vector3 placePos = targetPlace.gameObject.transform.position;
            Vector3 normalDir = Vector3.Normalize(placePos - bossPos);

            gameObject.transform.position += normalDir * speed;
            dis = Vector3.Distance(placePos, bossPos);
            yield return new WaitForEndOfFrame();
        }

        gameObject.transform.position = targetPlace.gameObject.transform.position;
        OnMoveToBossPlaceComplete(targetPlace);
    }

    void OnMoveToBossPlaceComplete(BossPlace targetPlace)
    {
        targetPlace.GetComponent<BossPlace>().SetAlive();

        transform.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z);
        transform.parent = targetPlace.transform;
        bossAnimGO.GetComponent<Animator>().SetTrigger("Idle");
        if (UIManager.Instance.gameStatus == GameStatus.BossArriving)
        {
            UIManager.Instance.Play();
        }
        else
        {
            UIManager.Instance.Resume();
        }
    }
}

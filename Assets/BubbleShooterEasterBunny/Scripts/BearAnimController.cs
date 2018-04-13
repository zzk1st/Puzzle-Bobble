using UnityEngine;
using System.Collections;

public class BearAnimController : MonoBehaviour {

    private Animator m_Anim;
    private float timer = 0.0f;

    // Use this for initialization
    void Start () {
        m_Anim = GetComponent<Animator>();
        m_Anim.SetInteger("GameStatus", 0);
        m_Anim.SetBool("Scored", false);
	}
	
	// Update is called once per frame
	void Update () {
        if (mainscript.Instance.scored)
        {
            m_Anim.SetBool("Scored", true);
            timer += Time.deltaTime;
            if (timer > 3.0f)
            {
                m_Anim.SetBool("Scored", false);
                mainscript.Instance.scored = false;
                timer = 0.0f;
            }
        }

        if (UIManager.Instance.gameStatus == GameStatus.GameOver)
        {
            m_Anim.SetInteger("GameStatus", 1);
        }

        if (UIManager.Instance.gameStatus == GameStatus.Win)
        {
            m_Anim.SetInteger("GameStatus", 2);
        }
    }
}

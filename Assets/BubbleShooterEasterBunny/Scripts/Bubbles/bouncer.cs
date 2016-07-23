using UnityEngine;
using System.Collections;

public class bouncer : MonoBehaviour
{
    Vector3 tempPosition;
    Vector3 targetPrepare;
    bool isPaused;
    public bool startBounce;
    float startTime;
    public float offset;

    // Use this for initialization
    void Start()
    {
        isPaused = Camera.main.GetComponent<mainscript>().isPaused;
        targetPrepare = transform.position;
    }

    public void BounceToCatapult(Vector3 vector3)
    {
        vector3 = new Vector3(vector3.x, vector3.y, gameObject.transform.position.z);
        tempPosition = transform.position;
        targetPrepare = vector3;
        startBounce = true;
        startTime = Time.time;
        iTween.MoveTo(gameObject, iTween.Hash("position", vector3, "time", 0.3, "easetype", iTween.EaseType.linear, "onComplete", "OnBounceToCatapultComplete"));
        Grid.waitForAnim = false;
    }

    void OnBounceToCatapultComplete()
    {
        mainscript.Instance.ballShooter.OnBounceToCatapultComplete();
    }

    void OnSwapBallsComplete()
    {
        mainscript.Instance.ballShooter.OnSwapBallsComplete();
    }
}

using UnityEngine;
using System.Collections;

public class bouncer : MonoBehaviour
{
    public void BounceToCatapult(Vector3 vector3)
    {
        vector3 = new Vector3(vector3.x, vector3.y, gameObject.transform.position.z);
        iTween.MoveTo(gameObject, iTween.Hash("position", vector3, "time", 0.3, "easetype", iTween.EaseType.linear, "onComplete", "OnBounceToCatapultComplete"));
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

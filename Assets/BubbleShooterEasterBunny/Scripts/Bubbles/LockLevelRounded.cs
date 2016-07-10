using UnityEngine;
using System.Collections;

public class LockLevelRounded : MonoBehaviour {
    Vector3 staticPos;
    public static LockLevelRounded Instance;
    Vector3 dir;
    Vector3 ballPos;
    float angle;
    Quaternion newRot;
    private bool addForce;
	// Use this for initialization
	void Start () {
        Instance = this;
        staticPos = transform.position;
        newRot = Quaternion.identity;
	}

    void Update()
    {
        if( transform.rotation != newRot )
            transform.rotation = Quaternion.Lerp( transform.rotation, newRot, Time.deltaTime );
    }

}

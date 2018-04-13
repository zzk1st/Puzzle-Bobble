using UnityEngine;
using System.Collections;

public class ButtonX : MonoBehaviour {
	// Use this for initialization
	void Start () {
	}

    void OnMouseDown()
    {
        CoreManager.Instance.ballShooter.SwapBalls();
    }
}

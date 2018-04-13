using UnityEngine;
using System.Collections;

public class ParallaxMove : MonoBehaviour {
    public float moveSpeed;
    private float initialY;
	// Use this for initialization
	void Start()
    {
        initialY = transform.position.y;
	}
	
	// Update is called once per frame
	void Update()
    {
        Vector3 pos = transform.position;
        float newY = Camera.main.transform.position.y * moveSpeed + initialY;
        transform.position = new Vector3(pos.x, newY, pos.z);
	}
}

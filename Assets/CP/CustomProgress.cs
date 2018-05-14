using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomProgress : MonoBehaviour
{
	[Range (0f, 100f)]
	public float mValue;
	public Transform mfiller;
	public float mSpeed;
	//这两个高度值在不同的工程中可能需要调整
	private float fillerAt0 = -126f;
	private float fillerAt100 = -26f;

	//	void Start ()
	//	{
	//
	//	}

	void Update ()
	{
		mfiller.Rotate (0f, 0f, mSpeed);
		mfiller.parent.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, ((fillerAt100 - fillerAt0) / 100 * mValue) + fillerAt0);
	}
}

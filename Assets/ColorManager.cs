using UnityEngine;
using System.Collections;

public class ColorManager : MonoBehaviour {
    public static ColorManager Instance;

    public Color[] ballColors;
    public Color[] ballBackgroundColors;

	// Use this for initialization
	void Awake()
    {
        Instance = this;
	}
}

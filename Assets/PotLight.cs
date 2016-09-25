using UnityEngine;
using System.Collections;

public class PotLight : MonoBehaviour
{
    private bool lightFlashing;
    public Animation anim;

    public void Flash()
    {
        if (!lightFlashing)
        {
            anim.Play();
        }
    }

    public void OnAnimFinished()
    {
        lightFlashing = false;
    }
}

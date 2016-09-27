using UnityEngine;
using System.Collections;

public class SeqAnimPlayer : MonoBehaviour
{
    public Animation anim;
    public void Flash()
    {
        if (!anim.isPlaying)
        {
            anim.Play();
        }
    }
}

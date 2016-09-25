using UnityEngine;
using System.Collections;

public class SoundSeq : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audios;
    private int curPlayingIndex;
    private float lastPlayTime;

    void Start()
    {
        curPlayingIndex = 0;
        lastPlayTime = Time.time;
    }

    public void Play()
    {
        if (Time.time - lastPlayTime > 3f)
        {
            lastPlayTime = Time.time;
            curPlayingIndex = 0;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.clip = audios[curPlayingIndex];
            audioSource.Play();

            if (curPlayingIndex < audios.Length - 1)
            {
                curPlayingIndex++;
            }
        }
    }
}

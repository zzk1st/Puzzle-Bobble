using UnityEngine;
using System.Collections;

public class AnimNextState : MonoBehaviour
{
    void NextState()
    {
        GetComponent<Animator>().SetTrigger("NextState");
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TotalScoreCounter : MonoBehaviour {

    private Queue<int> incomingScores = new Queue<int>();
    private bool readyForNextPop = false;

    void Update()
    {
        if (readyForNextPop && incomingScores.Count > 0)
        {
            int newScore = incomingScores.Dequeue();
            GetComponent<Text>().text = newScore.ToString();
            readyForNextPop = false;
            GetComponent<Animation>().Play();
        }
    }

    public void updateScore(int newScore)
    {
        incomingScores.Enqueue(newScore);
    }

    public void onPopScoreComplete()
    {
        readyForNextPop = true;
    }
}

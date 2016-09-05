using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BugManager : MonoBehaviour {
    static public BugManager Instance;

    public GameObject bug_hd;
    public GameObject bug_ld;
    GameObject bug;

	// Use this for initialization
	void Start ()
    {
        Instance = this;

        bug = bug_hd;
        ShowBugs();
	}
	
    void ShowBugs()
    {
        int effset = 1;
        for (int i = 0; i < 2; i++)
        {
            effset *= -1;
            CreateBug(new Vector3(10 * effset, -3, 0));

        }

    }

    public void CreateBug(Vector3 pos, int value = 1)
    {
        Transform spiders = GameObject.Find("Spiders").transform;
        List<Bug> listFreePlaces = new List<Bug>();
        foreach (Transform item in spiders)
        {
            if (item.childCount > 0) listFreePlaces.Add(item.GetChild(0).GetComponent<Bug>());
        }

        if (listFreePlaces.Count < 6)
            Instantiate(bug, pos, Quaternion.identity);
        else
        {
            listFreePlaces.Clear();
            foreach (Transform item in spiders)
            {
                if (item.childCount > 0)
                {
                    if (item.GetChild(0).GetComponent<Bug>().color == 0) listFreePlaces.Add(item.GetChild(0).GetComponent<Bug>());
                }
            }
            if (listFreePlaces.Count > 0)
                listFreePlaces[UnityEngine.Random.Range(0, listFreePlaces.Count)].ChangeColor(1);
        }
    }

    public void DestroyBugs()
    {
        Transform spiders = GameObject.Find("Spiders").transform;
        List<Bug> listFreePlaces = new List<Bug>();
        for (int i = 0; i < 2; i++)
        {
            listFreePlaces.Clear();
            foreach (Transform item in spiders)
            {
                if (item.childCount > 0) listFreePlaces.Add(item.GetChild(0).GetComponent<Bug>());
            }
            if (listFreePlaces.Count > 0)
                listFreePlaces[UnityEngine.Random.Range(0, listFreePlaces.Count)].MoveOut();

        }

    }
}

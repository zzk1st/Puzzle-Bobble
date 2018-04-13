using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    private List<GameObject> bossPlaces;
    public GameObject bossGO;
    // Use this for initialization
    void Awake()
    {
        Instance = this;
    }

    public void Iniaizlize(List<GameObject> bp)
    {
        bossPlaces = bp;
        bossPlaces.Last().GetComponent<BossPlace>().SetAlive();
    }

    public void UpdateBossPlaceHitColor()
    {
        if (bossPlaces.Count > 0)
        {
            bossPlaces.Last().GetComponent<BossPlace>().UpdateHitColor();
        }
    }

    public void RemoveLastBossPlace()
    {
        bossPlaces.Remove(bossPlaces.Last());
        mainscript.Instance.FindAndDestroyDetachedGameItems();
    }

    public void BossMoveToLastPlace()
    {
        if (bossPlaces.Count > 0)
        {
            UIManager.Instance.Demo();
            bossGO.GetComponent<BossMoves>().MoveToBossPlace(bossPlaces.Last().GetComponent<BossPlace>());
        }
    }

    public void GameStartBossMove()
    {
        bossGO.GetComponent<BossMoves>().MoveToBossPlace(bossPlaces.Last().GetComponent<BossPlace>());
    }
}

using UnityEngine;
using System.Collections;

public class CenterItem : MonoBehaviour {
    private GameItem _gameItem;

    public void Initialize()
    {
        _gameItem = GetComponent<GameItem>();
        mainscript.Instance.onBallsDestroyed += new mainscript.DestroyBallsHandler(onBallsDestroyed);
    }

    void onBallsDestroyed()
    {
        bool adjacentGridsEmpty = true;
        foreach(Grid adjacentGrid in _gameItem.centerGrid.AdjacentGrids)
        {
            if (adjacentGrid.AttachedGameItem != null)
            {
                adjacentGridsEmpty = false;
                break;
            }
        }

        if (adjacentGridsEmpty)
        {
            MissionManager.Instance.GainCenterItem();
        }
    }
}

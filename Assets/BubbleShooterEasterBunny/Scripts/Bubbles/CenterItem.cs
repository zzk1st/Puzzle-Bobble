using UnityEngine;
using System.Collections;

public class CenterItem : MonoBehaviour {
    private GameItem _gameItem;

    public void Initialize()
    {
        _gameItem = GetComponent<GameItem>();
        _gameItem.ConnectToGrid();
        mainscript.Instance.onBallsDestroyed += OnBallsDestroyed;
    }

    void OnDestroy()
    {
        mainscript.Instance.onBallsDestroyed -= OnBallsDestroyed;
    }

    void OnBallsDestroyed()
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

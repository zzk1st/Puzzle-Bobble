using UnityEngine;
using System.Collections;

public class Animal : MonoBehaviour {

    private GameItem _gameItem;

    public void Initialize()
    {
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.startFallFunc = StartFall;
        _gameItem.ConnectToGrid();
    }

    void StartFall()
    {
        // TODO: 球掉落，动物动画删除
        MissionManager.Instance.GainAnimalPoint();
        _gameItem.DisconnectFromGrid();

        Destroy(gameObject);
    }
}

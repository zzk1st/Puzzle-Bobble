using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogo : MonoBehaviour
{
    private GameItem _gameItem;

	public void Initialize()
	{
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.ConnectToGrid();
	}
}

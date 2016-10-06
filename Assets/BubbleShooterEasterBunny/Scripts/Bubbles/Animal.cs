using UnityEngine;
using System.Collections;

public class Animal : MonoBehaviour {

    private GameItem _gameItem;

    private GameObject animalShell;
    private GameObject animalBody;

    public void Initialize()
    {
        _gameItem = gameObject.GetComponent<GameItem>();
        _gameItem.startFallFunc = StartFall;
        animalShell = transform.GetChild(0).gameObject;
        animalBody = transform.GetChild(1).gameObject;
        _gameItem.ConnectToGrid();
    }

    public void SetSprite(LevelData.ItemType itemType)
    {
        if (itemType == LevelData.ItemType.AnimalSingle)
        {
            int idx = Random.Range(0, mainscript.Instance.animalSingleSprites.Length);
            animalShell.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalSingleShellSprite;
            animalBody.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalSingleSprites[idx];
            return;
        }
        if (itemType == LevelData.ItemType.AnimalHexagon)
        {
            int idx = Random.Range(0, mainscript.Instance.animalHexSprites.Length);
            animalShell.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalHexShellSprite;
            animalBody.GetComponent<SpriteRenderer>().sprite = mainscript.Instance.animalHexSprites[idx];
            return;
        }
    }

    void StartFall()
    {
        // TODO: 球掉落，动物动画删除
        MissionManager.Instance.GainAnimalPoint();
        _gameItem.DisconnectFromGrid();

        Destroy(gameObject);
    }
}

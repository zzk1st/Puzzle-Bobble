using UnityEngine;
using System.Collections;

/// <summary>
/// 包含了游戏里在stage里的一切物体（球，道具，动物等）的通用属性／方法
/// 具体包括：
///     对Grid的管理
///     对GameItem类型的管理
/// 碰撞由各个特殊component自己检测，因为很可能不同道具使用不同的collider
/// </summary>
public class GameItem : MonoBehaviour {
    public enum ItemType
    {
        Ball,
        Animal
    }

    public ItemType itemType;
    [HideInInspector]
    public Grid grid;

    public void ConnectToGrid()
    {
        gameObject.transform.parent = mainscript.Instance.gameItemsNode.transform;
        gameObject.layer = LayerMask.NameToLayer("FixedBall");
        mainscript.Instance.gridManager.ConnectGameItemToGrid(gameObject);
        mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
    }

    public void DisconnectFromGrid()
    {
        gameObject.transform.parent = transform.root;
        gameObject.layer = LayerMask.NameToLayer("Default");
        mainscript.Instance.gridManager.DisconnectGameItemToGrid(gameObject);
        mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
    }
}

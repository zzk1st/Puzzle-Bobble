using UnityEngine;
using System.Collections;

public delegate void StartFallDelegate();

/// <summary>
/// 包含了游戏里在stage里的一切物体（球，道具，动物等）的通用属性／方法
/// 这些通用方法不是通过虚函数，而是通过delegate实现
/// 具体实现每个组件只要在Initialize()里给delegate赋值就行
/// 
/// 具体包括：
///     对Grid的管理
///     对GameItem类型的管理
///     对各个delegate的管理
/// 碰撞由各个特殊component自己检测，因为很可能不同道具使用不同的collider
/// </summary>
public class GameItem : MonoBehaviour {
    public enum ItemType
    {
        Ball,
        CenterItem,
        Animal
    }

    public ItemType itemType;
    [HideInInspector]
    public Grid grid;

    public StartFallDelegate startFallFunc;

    public void ConnectToGrid()
    {
        gameObject.transform.parent = mainscript.Instance.gameItemsNode.transform;
        gameObject.layer = LayerMask.NameToLayer("FixedBall");
        GridManager.Instance.ConnectGameItemToGrid(gameObject);
        mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
    }

    public bool isConnectedToGrid()
    {
        return grid != null;
    }

    public void DisconnectFromGrid()
    {
        gameObject.transform.parent = transform.root;
        gameObject.layer = LayerMask.NameToLayer("Default");
        GridManager.Instance.DisconnectGameItemToGrid(gameObject);
        mainscript.Instance.platformController.UpdateLocalMinYFromAllFixedBalls();
    }

    public void StartFall()
    {
        if (startFallFunc != null)
        {
            startFallFunc();
        }
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// 包含了游戏里在stage里的一切物体（球，道具，动物等）的通用属性／方法
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
}

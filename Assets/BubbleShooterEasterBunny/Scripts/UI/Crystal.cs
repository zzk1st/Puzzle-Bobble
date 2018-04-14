using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour {
    public Texture2D crystal;
    public Vector2 pos;     // 左下角坐标
    public Vector2 size;    // 完全渲染的尺寸
    public float percent;   // 绘制的百分比

    void OnDrawGizmos () {
        // 不知道为什么，不好使
        //Gizmos.DrawGUITexture (new Rect (pos, size), crystal);
    }

    void OnGUI () {
        // 注意：左上角是(0, 0)
        Vector2 curPos = new Vector2 (pos.x, pos.y + size.y - size.y * percent);
        Vector2 curSize = new Vector2 (size.x, size.y * percent);
        GUI.DrawTextureWithTexCoords(new Rect (curPos, curSize), crystal, new Rect(0.0f, 0.0f, 1.0f, percent));
    }
}

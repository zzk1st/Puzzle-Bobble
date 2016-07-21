using UnityEngine;
using System.Collections;

public class BallDirIndicatorRotation : MonoBehaviour
{

    public int rotationOffset = 90;
    float bottomBoarderY;  //为了美观 把这个indicator永远指向高于此线的方向

    void Start()
    {
        bottomBoarderY = GameObject.Find("BottomBorder").transform.position.y; //获取生死线的Y坐标
    }

    // Update is called once per frame
    void Update()
    {
        // Get the direction from cursor to ball direction indicator sprite
        // and compute/apply the rotation accordingly
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //当指向线下方的时候 强制指回上方
        if (mousePosition.y < bottomBoarderY)
        {
            mousePosition.y = bottomBoarderY;
        }
        Vector3 difference = mousePosition - transform.position;
        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
    }
}

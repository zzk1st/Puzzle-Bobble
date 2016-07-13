using UnityEngine;
using System.Collections;

public class BallDirIndicatorRotation : MonoBehaviour
{

    public int rotationOffset = 90;

    // Update is called once per frame
    void Update()
    {
        // Get the direction from cursor to ball direction indicator sprite
        // and compute/apply the rotation accordingly
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
    }
}

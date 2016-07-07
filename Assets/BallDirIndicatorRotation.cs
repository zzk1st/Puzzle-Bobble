using UnityEngine;
using System.Collections;

public class BallDirIndicatorRotation : MonoBehaviour
{

    public int rotationOffset = 90;
    Color col;
    SpriteRenderer renderer;

    void Start()
    {
        renderer = GameObject.Find("dir_indicator").GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy != null)
        {
            col = mainscript.Instance.boxCatapult.GetComponent<Grid>().Busy.GetComponent<SpriteRenderer>().sprite.texture.GetPixelBilinear(0.1f, 0.6f);
            col.a = 1;
            renderer.color = col;
        }
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();

        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
    }
}

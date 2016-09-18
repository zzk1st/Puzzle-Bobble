using UnityEngine;
using System.Collections;

public class SelectLevelCamera : MonoBehaviour {
    private Vector3 Origin; // place where mouse is first pressed
    private Vector3 Diff; // place where mouse is first pressed
    private float cameraSizeX, cameraSizeY;
    private Bounds backgroundBounds;

    private float mouseDownTime;
    static private float clickDeltaTime = 0.2f;

    public GameObject background;

    void Start()
    {
        //cameraSizeY = Camera.main.orthographicSize;
        //cameraSizeX = cameraSizeY * Screen.width / Screen.height;
        backgroundBounds = background.GetComponent<SpriteRenderer>().bounds;
        cameraSizeX = backgroundBounds.extents.x;
        cameraSizeY = cameraSizeX * Screen.height / Screen.width;
        Camera.main.orthographicSize = cameraSizeY;
    }

    void LateUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Origin = MousePos();
            mouseDownTime = Time.time;
        }
        else if (Input.GetMouseButton(0))
        {
            Diff = Origin - MousePos();
            transform.position += Diff;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - mouseDownTime < clickDeltaTime)
            {
                SelectGameObject(MousePos());
            }
        }
        else
        {
            transform.position += Diff;
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, backgroundBounds.center.x - backgroundBounds.extents.x + cameraSizeX, backgroundBounds.center.x + backgroundBounds.extents.x - cameraSizeX),
                                         Mathf.Clamp(transform.position.y, backgroundBounds.center.y - backgroundBounds.extents.y + cameraSizeY, backgroundBounds.center.y + backgroundBounds.extents.y - cameraSizeY),
            0);
    }
    // return the position of the mouse in world coordinates (helper method)
    Vector3 MousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void SelectGameObject(Vector3 selectPos)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(selectPos, Vector2.zero);
        if (hitInfo && hitInfo.transform.tag == "SingleLevel")       
        {
            SelectLevelManager.Instance.StartLevel(hitInfo.transform.gameObject);
        }
    }
}

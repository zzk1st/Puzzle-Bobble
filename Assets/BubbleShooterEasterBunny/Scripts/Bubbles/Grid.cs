using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private GameObject attachedBall;
    private List<Grid> adjacentGrids = new List<Grid>();
    public int Row;
    public int Col;

    // 该属性表示一个grid连着的ball
    public GameObject AttachedBall
    {
        get { return attachedBall; }
        set
        {
            if( value != null )
            {
                if( value.GetComponent<Ball>() != null )
                {
                    value.GetComponent<Ball>().mesh = gameObject;
                }

            }

            attachedBall = value;
        }
    }

    public List<GameObject> GetAdjacentBalls()
    {
        List<GameObject> result = new List<GameObject>();

        foreach(Grid grid in adjacentGrids)
        {
            if (grid.attachedBall != null)
            {
                result.Add(grid.attachedBall);
            }
        }

        return result;
    }

    public List<Grid> AdjacentGrids
    {
        get { return adjacentGrids; }
    }

    public Vector3 pos
    {
        get { return gameObject.transform.position; }
    }

    public Vector3 localPos
    {
        get { return gameObject.transform.localPosition; }
    }

    //public GameObject boxFirst;     // 即将发射的ball的box
    //public GameObject boxSecond;    // 等待的ball的box

    // Use this for initialization
    void Start()
    {
    }

    public void ConnectBall(GameObject ball)
    {
        if (attachedBall != null)
        {
            throw new System.AccessViolationException("尝试链接一个已经链接的grid!");
        }

        attachedBall = ball;
        ball.GetComponent<Ball>().grid = this;
    }

    public void DisonnectBall()
    {
        if (attachedBall.GetComponent<Ball>().grid != this)
        {
            throw new System.AccessViolationException("Disconnect grid不是本grid！");
        }

        attachedBall.GetComponent<Ball>().grid = null;
        attachedBall = null;
    }

    public void connectAdjacentGrids()
    {
        int gridLayer = LayerMask.NameToLayer("Grid");
        Collider2D[] adjacentGridColls = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x, 1 << gridLayer);
        adjacentGrids.Clear();
        foreach(Collider2D coll in adjacentGridColls)
        {
            if (coll.gameObject != gameObject)
            {
                adjacentGrids.Add(coll.gameObject.GetComponent<Grid>());
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3 scale = transform.localScale;
        scale -= new Vector3(0.1f, 0.1f, 0.1f);

        if (attachedBall != null)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.gray;
        }

        Gizmos.DrawWireCube(transform.position, scale);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 scale = transform.localScale;
        scale -= new Vector3(0.1f, 0.1f, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, scale);
    }
}

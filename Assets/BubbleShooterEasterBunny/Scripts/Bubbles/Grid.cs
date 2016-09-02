using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private GameObject attachedGameItem;
    private List<Grid> adjacentGrids = new List<Grid>();
    public int Row;
    public int Col;

    // 该属性表示一个grid连着的gameItem
    public GameObject AttachedGameItem
    {
        get { return attachedGameItem; }
        set
        {
            attachedGameItem = value;
        }
    }

    public List<GameObject> GetAdjacentGameItems()
    {
        List<GameObject> result = new List<GameObject>();

        foreach(Grid grid in adjacentGrids)
        {
            if (grid.attachedGameItem != null)
            {
                result.Add(grid.attachedGameItem);
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

    // Use this for initialization
    void Start()
    {
    }

    public void ConnectGameItem(GameObject gameItem)
    {
        if (attachedGameItem != null)
        {
            throw new System.AccessViolationException("尝试链接一个已经链接的grid!");
        }

        attachedGameItem = gameItem;
        gameItem.GetComponent<GameItem>().grid = this;
    }

    public void DisonnectGameItem()
    {
        if (attachedGameItem.GetComponent<GameItem>().grid != this)
        {
            throw new System.AccessViolationException("Disconnect grid不是本grid！");
        }

        attachedGameItem.GetComponent<GameItem>().grid = null;
        attachedGameItem = null;
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

        if (attachedGameItem != null)
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

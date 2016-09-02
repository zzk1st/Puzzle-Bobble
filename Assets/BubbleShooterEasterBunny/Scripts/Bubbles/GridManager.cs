using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {

    public GameObject gridPrefab;

    private int rowCount;
    private int colCount;
    // TODO: 这个应该从creator计算？
    private float offsetStep = 0.33f;
    private List<GameObject> grids = new List<GameObject>();


    public void CreateGrids(int rows, int cols)
    {
        rowCount = rows;
        colCount = cols;
        // mesh没有用六边形，而只用了长方形，这样判断起来效率更高
        GameObject gridsNode = GameObject.Find( "-Grids" );
        float offset = 0;
        for( int j = 0; j < rowCount + 1; j++ )
        {
            for( int i = 0; i < colCount; i++ )
            {
                if( j % 2 == 0 )
                {
                    offset = 0;
                }
                else
                {
                    offset = offsetStep;
                }

                GameObject newGrid = Instantiate(gridPrefab, transform.position, transform.rotation ) as GameObject;
                Vector3 v = new Vector3(transform.position.x + i * newGrid.transform.localScale.x + offset - colCount / 2f * newGrid.transform.localScale.x,
                                        transform.position.y - j * newGrid.transform.localScale.y,
                                        transform.position.z);

                newGrid.transform.parent = gridsNode.transform;
                newGrid.transform.localPosition = v;
                newGrid.GetComponent<Grid>().Row = j;
                newGrid.GetComponent<Grid>().Col = i;

                //Debug.Log(String.Format("row={0}, col={1}, LocalPosition={2}, WorldPosition={3}", j, i, b.transform.localPosition, b.transform.position));
                GameObject[] existingGrids = GameObject.FindGameObjectsWithTag( "Grid" );
                newGrid.name = newGrid.name + existingGrids.Length.ToString();
                grids.Add(newGrid);
            }
        }

        DisableGridColliders();
        ConnectAllAdjacentGrids();
    }

    private Grid FindClosestGridFromGameItem(GameObject gameItem)
    {
        float curMinDistance = 9999f;
        Grid resultGrid = null;

        int gridLayer = LayerMask.NameToLayer("Grid");
        Collider2D[] colls = Physics2D.OverlapCircleAll(gameItem.transform.position, CreatorBall.Instance.BallRealRadius, 1 << gridLayer);

        foreach(Collider2D coll in colls)
        {
            Grid grid = coll.gameObject.GetComponent<Grid>();
            if (grid.AttachedGameItem == null)
            {
                float gridDistance = Vector2.Distance(grid.pos, gameItem.transform.position);
                if ( gridDistance < curMinDistance)
                {
                    curMinDistance = gridDistance;
                    resultGrid = grid;
                }
            }
        }

        return resultGrid;
    }

    public void ConnectGameItemToGrid(GameObject gameItem)
    {
        EnableGridColliders();

        Grid closestGrid = FindClosestGridFromGameItem(gameItem);
        closestGrid.GetComponent<Grid>().ConnectGameItem(gameItem);

        DisableGridColliders();
    }

    public void DisconnectGameItemToGrid(GameObject gameItem)
    {
        gameItem.GetComponent<GameItem>().grid.DisonnectGameItem();
    }

    private void ConnectAllAdjacentGrids()
    {
        EnableGridColliders();
        foreach(GameObject grid in grids)
        {
            grid.GetComponent<Grid>().connectAdjacentGrids();
        }
        DisableGridColliders();
    }

    private void EnableGridColliders()
    {
        foreach( GameObject item in grids )
        {
            item.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void DisableGridColliders()
    {
        foreach( GameObject item in grids )
        {
            item.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public List<GameObject> GetAdjacentGameItems(GameObject gameItem)
    {
        return gameItem.GetComponent<GameItem>().grid.GetAdjacentGameItems();
    }

    public GameObject GetGrid(int row, int col)
    {
        return grids[row * colCount + col];
    }

	// Use this for initialization
	void Start()
    {
	
	}
	
}

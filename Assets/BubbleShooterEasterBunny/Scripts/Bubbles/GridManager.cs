using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
	public static GridManager Instance;

	public GameObject gridPrefab;

	public int rowCount;
	public int colCount;
	// TODO: 这个应该从creator计算？
	private float offsetStep = 0.33f;
	private List<GameObject> grids = new List<GameObject> ();
	public List<Grid> controlGrids = new List<Grid> ();
	// 用来找到没有连到顶部或者动物的grids

	void Start ()
	{
		Instance = this;
	}

	public void CreateGrids (int rows, int cols, StageMoveMode stageMoveMode)
	{
		rowCount = rows;
		colCount = cols;
		float rowOffset, colOffset;
		if (stageMoveMode == StageMoveMode.Vertical) {
			// 注意2和2f是不一样的！
			rowOffset = 0f;
			colOffset = -colCount / 2 * gridPrefab.transform.localScale.x;
		} else {
			// 注意2和2f是不一样的！
			rowOffset = rowCount / 2 * gridPrefab.transform.localScale.y;
			colOffset = -colCount / 2f * gridPrefab.transform.localScale.x;
		}

		GameObject gridsNode = GameObject.Find ("-Grids");
		float offset = 0f;
		for (int row = 0; row < rowCount; row++) {
			for (int col = 0; col < colCount; col++) {
				if (row % 2 == 0) {
					offset = 0f;
				} else {
					offset = offsetStep;
				}

				GameObject newGrid = Instantiate (gridPrefab, transform.position, transform.rotation) as GameObject;
				newGrid.transform.parent = gridsNode.transform;
				Vector3 v = new Vector3 (transform.position.x + col * newGrid.transform.localScale.x + offset + colOffset,
					                        transform.position.y + (-row) * newGrid.transform.localScale.y + rowOffset,
					                        transform.position.z);
				newGrid.transform.localPosition = v;
				newGrid.GetComponent<Grid> ().Row = row;
				newGrid.GetComponent<Grid> ().Col = col;

				//Debug.Log(String.Format("row={0}, col={1}, LocalPosition={2}, WorldPosition={3}", j, i, b.transform.localPosition, b.transform.position));
				GameObject[] existingGrids = GameObject.FindGameObjectsWithTag ("Grid");
				newGrid.name = newGrid.name + existingGrids.Length.ToString ();
				grids.Add (newGrid);
			}
		}

		DisableGridColliders ();
		ConnectAllAdjacentGrids ();
	}

	private Grid FindClosestGridFromGameItem (GameObject gameItem)
	{
		float curMinDistance = 9999f;
		Grid resultGrid = null;

		int gridLayer = LayerMask.NameToLayer ("Grid");
		Collider2D[] colls = Physics2D.OverlapCircleAll (gameItem.transform.position, mainscript.Instance.BallRealRadius, 1 << gridLayer);

		foreach (Collider2D coll in colls) {
			Grid grid = coll.gameObject.GetComponent<Grid> ();
			if (grid.AttachedGameItem == null) {
				float gridDistance = Vector2.Distance (grid.pos, gameItem.transform.position);
				if (gridDistance < curMinDistance) {
					curMinDistance = gridDistance;
					resultGrid = grid;
				}
			} else {
				// 碰到其他球是正常的，我们只要找到空的grid就行
				// 现在问题是可能会填上多个grid，之后遇到相应bug可能要看看是不是因为这个
				//Debug.Log(string.Format("ERROR: trying to attach a gameItem to a grid that already has a gameItem, newGameItem={0}, attachedGameItem={1}", gameItem.name, grid.AttachedGameItem.name));
			}
		}

		return resultGrid;
	}

	public void ConnectGameItemToGrid (GameObject gameItemGO)
	{
		EnableGridColliders ();

		GameItem gameItem = gameItemGO.GetComponent<GameItem> ();
		Grid gridFound = FindClosestGridFromGameItem (gameItemGO);
		GameItemShapeType shapeType = gameItem.shapeType;
		List<GridCoord> shapeGridCoords = GameItemShapes.Instance.ShapeGridCoords (shapeType, gridFound.Row, gridFound.Col);

		// 我们不用检查边界，因为unity会自己抛出异常
		foreach (GridCoord gridCoord in shapeGridCoords) {
			Grid grid = Grid (gridCoord.row, gridCoord.col).GetComponent<Grid> ();
			if (grid.AttachedGameItem != null) {
				throw new System.AccessViolationException ("尝试链接一个已经链接的grid!");
			} else {
				grid.AttachedGameItem = gameItemGO;
			}
		}

		gameItem.centerGrid = gridFound;

		DisableGridColliders ();
	}

	public void DisconnectGameItemToGrid (GameObject gameItemGO)
	{
		GameItem gameItem = gameItemGO.GetComponent<GameItem> ();
		GameItemShapeType shapeType = gameItem.shapeType;
		List<GridCoord> shapeGridCoords = GameItemShapes.Instance.ShapeGridCoords (shapeType, gameItem.centerGrid.Row, gameItem.centerGrid.Col);

		// 我们不用检查边界，因为unity会自己抛出异常
		foreach (GridCoord gridCoord in shapeGridCoords) {
			Grid grid = Grid (gridCoord.row, gridCoord.col).GetComponent<Grid> ();
			if (grid.AttachedGameItem != gameItemGO) {
				throw new System.AccessViolationException ("Disconnect grid不是本grid！");
			} else {
				grid.AttachedGameItem = null;
			}
		}

		gameItem.centerGrid = null;
	}

	private void ConnectAllAdjacentGrids ()
	{
		EnableGridColliders ();
		foreach (GameObject grid in grids) {
			grid.GetComponent<Grid> ().ConnectAdjacentGrids ();
		}
		DisableGridColliders ();
	}

	private void EnableGridColliders ()
	{
		foreach (GameObject item in grids) {
			item.GetComponent<BoxCollider2D> ().enabled = true;
		}
	}

	private void DisableGridColliders ()
	{
		foreach (GameObject item in grids) {
			item.GetComponent<BoxCollider2D> ().enabled = false;
		}
	}

	public List<GameObject> GetAdjacentGameItems (GameObject gameItem)
	{
		return gameItem.GetComponent<GameItem> ().centerGrid.GetAdjacentGameItems ();
	}

	public GameObject Grid (int row, int col)
	{
		return grids [row * colCount + col];
	}

	public List<GameObject> FindDetachedGameItems ()
	{
		controlGrids.Clear ();
		List<Grid> gridsDetached = new List<Grid> ();
		foreach (GameObject gridGO in grids) {
			Grid grid = gridGO.GetComponent<Grid> ();
			if (grid.AttachedGameItem != null) {
				if (!controlGrids.Contains (grid)) {
					List<Grid> resultGrids = new List<Grid> ();
					grid.CheckNearbyDetachedGrids (resultGrids);
					gridsDetached.AddRange (resultGrids);
				}
			}
		}

		List<GameObject> results = new List<GameObject> ();
		foreach (Grid grid in gridsDetached) {
			if (!results.Contains (grid.AttachedGameItem)) {
				results.Add (grid.AttachedGameItem);
			}
		}

		return results;
	}
}

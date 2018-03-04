using System;
using System.Collections.Generic;

public struct GridCoord
{
    public GridCoord(int r, int c)
    {
        row = r;
        col = c;
    }

    public int row;
    public int col;
}

public enum GameItemShapeType
{
    Single,
    Triangle,
    Hexagon,
}

/// <summary>
/// Game item shapes: 用来定义一系列形状
/// </summary>
public class GameItemShapes
{
    public static GameItemShapes Instance = new GameItemShapes();

    private List<GridCoord> single = new List<GridCoord>
    {
        new GridCoord(0, 0)
    };

    private List<GridCoord> triangleOdd = new List<GridCoord>
    {
        new GridCoord(0, 0),
        new GridCoord(1, 0),
        new GridCoord(1, 1)
    };

    private List<GridCoord> triangleEven = new List<GridCoord>()
    {
        new GridCoord(0, 0),
        new GridCoord(1, -1),
        new GridCoord(1, 0)
    };

    private List<GridCoord> hexagonOdd = new List<GridCoord>()
    {
        new GridCoord(0, 0),
        new GridCoord(-1, 0),
        new GridCoord(-1, 1),
        new GridCoord(0, -1),
        new GridCoord(0, 1),
        new GridCoord(1, 0),
        new GridCoord(1, 1)
    };

    private  List<GridCoord> hexagonEven = new List<GridCoord>()
    {
        new GridCoord(0, 0),
        new GridCoord(-1, -1),
        new GridCoord(-1, 0),
        new GridCoord(0, -1),
        new GridCoord(0, 1),
        new GridCoord(1, -1),
        new GridCoord(1, 0)
    };

    private Dictionary<GameItemShapeType, List<GridCoord>[]> _gameItemShapes = new Dictionary<GameItemShapeType, List<GridCoord>[]>();

    private GameItemShapes()
    {
        // 单独一个格
        {
            List<GridCoord>[] gcList = new List<GridCoord>[2];
            gcList[0] = single;
            gcList[1] = single;
            _gameItemShapes[GameItemShapeType.Single] = gcList;
        }
        // 正三角形格子
        {
            List<GridCoord>[] gcList = new List<GridCoord>[2];
            gcList[0] = triangleOdd;
            gcList[1] = triangleEven;
            _gameItemShapes[GameItemShapeType.Triangle] = gcList;
        }
        // 六边形格子
        {
            List<GridCoord>[] gcList = new List<GridCoord>[2];
            gcList[0] = hexagonOdd;
            gcList[1] = hexagonEven;
            _gameItemShapes[GameItemShapeType.Hexagon] = gcList;
        }
    }

    public List<GridCoord> ShapeGridCoords(GameItemShapeType shapeType, int centerRow, int centerCol)
    {
        List<GridCoord> result;
        if (centerRow % 2 == 0) // 注意偶数排和奇数排的坐标是不同的
        {
            result = new List<GridCoord>(_gameItemShapes[shapeType][1]);
        }
        else
        {
            result = new List<GridCoord>(_gameItemShapes[shapeType][0]);
        }

        for (int i = 0; i < result.Count; i++)
        {
            result[i] = new GridCoord(result[i].row + centerRow, result[i].col + centerCol);
        }

        return result;
    }
}


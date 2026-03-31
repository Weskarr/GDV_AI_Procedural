using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DungeonCell
{
    private Vector2Int _gridPosition;
    private WallEnum _walls; //bit Encoded

    #region Getters & Setters

    public Vector2Int GridPosition
    {
        get => _gridPosition;
        set => _gridPosition = value;
    }

    public WallEnum Walls
    {
        get => _walls;
        set => _walls = value;
    }

    #endregion

    public void RemoveWall(WallEnum wallToRemove)
    {
        _walls = (_walls & ~wallToRemove);
    }

    public int GetNumWalls()
    {
        int numWalls = 0;
        if (((_walls & WallEnum.Down) != 0)) { numWalls++; }
        if (((_walls & WallEnum.Up) != 0)) { numWalls++; }
        if (((_walls & WallEnum.Left) != 0)) { numWalls++; }
        if (((_walls & WallEnum.Right) != 0)) { numWalls++; }
        return numWalls;
    }

    public bool HasWall(WallEnum wallDirection)
    {
        return (_walls & wallDirection) != 0;
    }
    
    public List<DungeonCell> GetNeighbours(DungeonCell[,] grid)
    {
        List<DungeonCell> result = new List<DungeonCell>();
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                int cellX = this._gridPosition.x + x;
                int cellY = this._gridPosition.y + y;
                if (cellX < 0 || cellX >= grid.GetLength(0) || cellY < 0 || cellY >= grid.GetLength(1) || Mathf.Abs(x) == Mathf.Abs(y))
                {
                    continue;
                }
                DungeonCell canditateCell = grid[cellX, cellY];
                result.Add(canditateCell);
            }
        }
        return result;
    }
}



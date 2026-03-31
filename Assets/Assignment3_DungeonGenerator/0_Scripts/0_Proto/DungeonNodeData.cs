// This is the Node Data class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is!
// (Didn't leave it as is, did some re-org. Btw this is a data container now so no calculative logic here!)
using UnityEngine;

public class DungeonNodeData
{
    private DungeonCell _cell; // Cell on which this node sits.
    private Vector2Int _position; // Position on the grid.
    private DungeonNodeData _parent; // Parent Node of this node.
    private float _gScore; // Current Travelled Distance.
    private float _hScore; // Distance estimated based on Heuristic.
    private float _fScore; // FScore = GScore + HScore.

    #region Getters & Setters

    public DungeonCell Cell
    {
        get => _cell;
        set => _cell = value;
    }

    public Vector2Int Position
    {
        get => _position;
        set => _position = value;
    }

    public DungeonNodeData Parent
    {
        get => _parent;
        set => _parent = value;
    }

    public float GScore
    {
        get => _gScore;
        set => _gScore = value;
    }

    public float HScore
    {
        get => _hScore;
        set => _hScore = value;
    }

    public float FScore
    {
        get => _fScore;
        set => _fScore = value;
    }

    #endregion

    public DungeonNodeData(DungeonCell cell, Vector2Int position, DungeonNodeData parent, float gScore, float hScore, float fScore)
    {
        this._cell = cell;
        this._position = position;
        this._parent = parent;
        this._gScore = gScore;
        this._hScore = hScore;
        this._fScore = fScore;
    }
}
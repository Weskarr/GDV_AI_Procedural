// This is a Grid Data Class, with precalculated astar variables of the grid to save some min-max performance, this is a data container so no logic here!
public class DungeonGridData
{
    private int _size; // Total grid cell count.
    private int _width; // Width of the grid.
    private int _height; // Height of the grid.

    #region Getters & Setters

    public int Size
    {
        get => _size;
        //set => _size = value;
    }

    public int Width
    {
        get => _width;
        //set => _width = value;
    }

    public int Height
    {
        get => _height;
        //set => _height = value;
    }

    #endregion

    public DungeonGridData(int size, int width, int height)
    {
        this._size = size;
        this._width = width;
        this._height = height;
    }
}
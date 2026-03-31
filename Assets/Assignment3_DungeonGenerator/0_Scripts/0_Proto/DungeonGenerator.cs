using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField, Range(4, 10)] private int _dungeonWidth = 10;
    [SerializeField, Range(4, 10)] private int _dungeonHeight = 10;
    [SerializeField, Range(0, 100)] private float _internalWallPercentage = 65f; // For afterward removal, clamped 0–100%.
    [SerializeField] private int _trappedRoomsAmount = 3; // For a *deadly* challenge.
    [SerializeField] private int _lootRoomsAmount = 3; // A kind of a reward.
    [SerializeField] private int _dwellerRoomsAmount = 2; // A place where an dungeon dweller calls home.
    [SerializeField] private int _randomWalkSteps = 5; // For random walk algorithm.
    [SerializeField] private int _dungeonSeed = 987654321; // For reproducibility.

    [Header("Prefab")]
    [SerializeField] private int _gridScale = 10; // Should be based on the room prefab.
    [SerializeField] private DungeonRoom _roomPrefab;

    [Header("Step Iterator Debug")] // Mostly for debugging or showcasing.
    [SerializeField] private int _iteratorStep = 0;
    [SerializeField] private bool _iteratorRestarts = false;
    [SerializeField] private bool _iteratorFinishedOnce = false;

    #region Hidden Private Variables

    private DungeonCell[,] _cellGrid; // For fast cell lookup by grid position.
    private DungeonRoom[,] _roomGrid; // For fast room lookup by grid position.

    private List<DungeonRoom> _allRooms = new(); // For fast room lookup by index.
    private List<DungeonRoom> _startToEndRoute = new(); // First is the "Start", between is the "Path", last is the "End".
    private List<DungeonRoom> _trappedRooms = new(); // All the trapped rooms.
    private List<DungeonRoom> _lootRooms = new(); // All the reward rooms.
    private List<DungeonRoom> _dwellerRooms = new(); // An bunch of dungeon gremlins that have unique behaviours.

    private System.Random _dungeonSeedRng; // Rng holder, to ensure the seed creates the same result.

    private InputHandler MyInputHandler => InputHandler.Instance; // My input handler. 

    #endregion

    // ------------------------------------------------------------------------------------------------------------------------------------

    // Setters & Getters:

    #region Setters

    // Empty for now no need, perhaps later.

    #endregion

    #region Getters

    public DungeonCell[,] CellGrid
    {
        get => _cellGrid;
        //set => _cellGrid = value;
    }

    public int GridScale
    {
        get => _gridScale;
        //set => _gridScale = value;
    }

    #endregion

    // Input Management:

    #region Start Input

    private void Start()
    {
        MyInputHandler.OnEnterStarted += StepIterator;
        MyInputHandler.OnSpaceStarted += InstantGeneration;
    }

    #endregion

    // Generation Aproaches:

    #region Iterative

    private void StepIterator()
    {
        // Only restart when allowed.
        if (!_iteratorRestarts && _iteratorFinishedOnce)
            return;

        // Apply each step carefully.
        switch (_iteratorStep)
        {
            // Try to despawn dungeon and reset, also just reiterate afterwards.
            case 0:
                DespawnDungeon();
                break;

            // Create the cell grid, also spawn the rooms ontop with order displayed.
            case 1:
                CreateCellGrid();
                CreateRooms();
                ApplySpawnOrdersPerRoom();
                DisplaySpawnOrderPerRoom();
                break;

            // Ensure accessibility to each room. 
            case 2:
                EnsureAccesibility();
                UpdateAllRoomVisuals();
                break;

            // Optional removal of a percentage of internal walls.
            case 3:
                RemoveInternalWalls();
                UpdateAllRoomVisuals();
                break;

            // Calculate the seed costs per room.
            case 4:
                ApplySeedCostsPerRoom();
                DisplaySeedCostPerRoom();
                break;

            // Set an start for random walk.
            case 5:
                ClearAllRoomDisplays();
                SetAnStartRoom();
                DisplayStartRoomAsStart();
                break;

            // Walk a route via the random path algorithm.
            case 6:
                RandomWalkAnPathFromStart();
                DisplayAllPathRoomsAsPath();
                break;

            // Set an end for random walk.
            case 7:
                SetAnEndRoom();
                DisplayEndRoomAsEnd();
                break;

            // Assign a set amount of traps to unassigned rooms.
            case 8:
                SpawnTrapRooms();
                DisplayTrappedRooms();
                break;

            // Assign a set amount of loot to unassigned rooms.
            case 9:
                SpawnLootRooms();
                DisplayLootRooms();
                break;

            // Assign a set amount of dwellers to unassigned rooms.
            case 10:
                SpawnDwellerRooms();
                DisplayDwellerRooms();
                break;

            // Clear all displays.
            case 11:
                ClearAllRoomDisplays();
                // Spawn ceilings as a easy fog of war alternative?
                //SpawnCeilingPerRoom(); // To complex for now. 
                break;

            // Generator is finished.
            default:
                _iteratorFinishedOnce = true;
                _iteratorStep = -1;
                break;
        }

        // Increment step for next iteration.
        _iteratorStep++;
    }

    #endregion // 

    #region Instant

    private void InstantGeneration()
    {
        // Despawn Fully
        DespawnDungeon();

        // Grid & Room Spawning
        CreateCellGrid();
        CreateRooms();

        // Modify Walls
        EnsureAccesibility();
        RemoveInternalWalls();

        // Apply to Rooms
        ApplySpawnOrdersPerRoom();
        ApplySeedCostsPerRoom();

        // Random Walk
        SetAnStartRoom();
        RandomWalkAnPathFromStart();
        SetAnEndRoom();

        // Special Rooms
        SpawnTrapRooms();
        SpawnLootRooms();
        SpawnDwellerRooms();

        // Display in Rooms
        ClearAllRoomDisplays();
        DisplayStartRoomAsStart();
        DisplayAllPathRoomsAsPath();
        DisplayEndRoomAsEnd();
        DisplayTrappedRooms();
        DisplayLootRooms();
        DisplayDwellerRooms();

        // Final Visual Update
        UpdateAllRoomVisuals();
    }

    #endregion

    // Modular Pieces:

    #region Despawn Fully

    private void DespawnDungeon()
    {
        // Reset seed and grid.
        _dungeonSeedRng = new System.Random(_dungeonSeed);
        _roomGrid = new DungeonRoom[_dungeonWidth, _dungeonHeight];

        // Clear lists.
        _allRooms.Clear();
        _startToEndRoute.Clear();
        _trappedRooms.Clear();
        _lootRooms.Clear();
        _dwellerRooms.Clear();

        // Destroy children.
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion

    #region Grid & Room Creation

    private void CreateCellGrid()
    {
        _cellGrid = new DungeonCell[_dungeonWidth, _dungeonHeight];
        _cellGrid.Initialize();

        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonHeight; y++)
            {
                _cellGrid[x, y] = new DungeonCell();
                _cellGrid[x, y].GridPosition = new Vector2Int(x, y);

                // Start fully closed
                _cellGrid[x, y].Walls = WallEnum.Down | WallEnum.Left | WallEnum.Right | WallEnum.Up;
            }
        }
    }

    private void CreateRooms()
    {
        for (int y = _dungeonHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < _dungeonWidth; x++)
            {
                DungeonRoom room = Instantiate(
                    _roomPrefab,
                    new Vector3(x * _gridScale, 0, y * _gridScale),
                    Quaternion.identity,
                    transform
                );

                room.SpawnFloor();
                room.SpawnSides(_cellGrid[x, y], _dungeonWidth, _dungeonHeight);
                room.GridPosition = new Vector2Int(x, y);
                room.GivenRoomType = RoomTypeEnum.Unassigned;

                _roomGrid[x, y] = room;
                _allRooms.Add(room);
            }
        }
    }

    #endregion

    #region Modify Walls

    private void EnsureAccesibility()
    {
        Stack<DungeonCell> cellStack = new();
        List<DungeonCell> visitedCells = new();

        DungeonCell startCell = _cellGrid[0, 0];
        cellStack.Push(startCell);
        visitedCells.Add(startCell);

        while (cellStack.Count > 0)
        {
            DungeonCell currentCell = cellStack.Pop();
            List<DungeonCell> neighbours = GetUnvisitedNeighbours(currentCell, visitedCells, cellStack);

            if (neighbours.Count > 1)
                cellStack.Push(currentCell);

            if (neighbours.Count != 0)
            {
                DungeonCell randomNeighbour = neighbours[_dungeonSeedRng.Next(0, neighbours.Count)];
                RemoveWallBetweenCells(currentCell, randomNeighbour);
                visitedCells.Add(randomNeighbour);
                cellStack.Push(randomNeighbour);
            }
        }
    }

    private void RemoveInternalWalls()
    {
        // Locate and add all internal walls exactly once.
        List<(DungeonCell a, DungeonCell b)> internalWalls = new();
        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonHeight; y++)
            {
                DungeonCell cell = _cellGrid[x, y];

                // Right neighbor.
                if (x < _dungeonWidth - 1)
                    internalWalls.Add((cell, _cellGrid[x + 1, y]));

                // Up neighbor.
                if (y < _dungeonHeight - 1)
                    internalWalls.Add((cell, _cellGrid[x, y + 1]));
            }
        }

        // Shuffle the list randomly. (https://discussions.unity.com/t/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code/535113/6)
        // Might not be the best approach, but easiest atm. ^^
        internalWalls = internalWalls.OrderBy(_ => _dungeonSeedRng.Next()).ToList();

        // Calculate how many walls to remove.
        int totalWalls = internalWalls.Count;
        float fractionToKeep = _internalWallPercentage / 100f;
        int wallsToKeep = Mathf.RoundToInt(totalWalls * (_internalWallPercentage / 100f));

        // Remove all the left over walls
        for (int i = wallsToKeep; i < totalWalls; i++)
            RemoveWallBetweenCells(internalWalls[i].a, internalWalls[i].b);
    }

    #endregion

    #region Apply to Rooms

    private void ApplySpawnOrdersPerRoom()
    {
        int order = 0;
        foreach (DungeonRoom room in _allRooms)
            room.GivenSpawnOrder = order++;
    }

    private void ApplySeedCostsPerRoom()
    {
        foreach (DungeonRoom room in _allRooms)
            room.GivenSeedCost = _dungeonSeedRng.Next(0, 10000);
    }

    #endregion

    #region Random Walk Algorithm

    private void SetAnStartRoom()
    {
        DungeonRoom start = FindBestMapCandidate(RoomTypeEnum.Unassigned);
        start.GivenRoomType = RoomTypeEnum.Start;
        _startToEndRoute.Add(start);
    }

    private void RandomWalkAnPathFromStart()
    {
        // Current.
        DungeonRoom last = _startToEndRoute[0];

        // Create a path.
        for (int i = 0; i < _randomWalkSteps; i++)
        {
            DungeonRoom next = FindBestNeighbourCandidate(last, RoomTypeEnum.Unassigned);

            // if dead end, stop prematurely.
            if (next == null)
                break;

            // We found a new path.
            next.GivenRoomType = RoomTypeEnum.Path;


            // Replace last.
            last = next;

            // Also add to route.
            _startToEndRoute.Add(next);
        }
    }

    private void SetAnEndRoom()
    {
        // Last one simply becomes the end.
        DungeonRoom end = _startToEndRoute[^1];
        end.GivenRoomType = RoomTypeEnum.End;
    }

    #endregion

    #region Special Rooms

    private void SpawnTrapRooms()
    {
        // Add Traps to set amount of rooms.
        for (int i = 0; i < _trappedRoomsAmount; i++)
        {
            DungeonRoom trap = FindBestMapCandidate(RoomTypeEnum.Unassigned);

            // Stop early if there are no unassigned rooms left.
            if (trap == null)
                break;

            trap.GivenRoomType = RoomTypeEnum.Trap;
            _trappedRooms.Add(trap);
        }


    }

    private void SpawnLootRooms()
    {
        // Add loot to set amount of rooms.
        for (int i = 0; i < _lootRoomsAmount; i++)
        {
            DungeonRoom loot = FindBestMapCandidate(RoomTypeEnum.Unassigned);

            // Stop early if there are no unassigned rooms left.
            if (loot == null)
                break;

            loot.GivenRoomType = RoomTypeEnum.Loot;
            _lootRooms.Add(loot);
        }
    }

    private void SpawnDwellerRooms()
    {
        // Add dwellers to set amount of rooms.
        for (int i = 0; i < _dwellerRoomsAmount; i++)
        {
            DungeonRoom loot = FindBestMapCandidate(RoomTypeEnum.Unassigned);

            // Stop early if there are no unassigned rooms left.
            if (loot == null)
                break;

            loot.GivenRoomType = RoomTypeEnum.Dweller;
            _dwellerRooms.Add(loot);
        }
    }

    #endregion

    #region Ceiling Fog of War

    private void SpawnCeilingPerRoom()
    {
        foreach (DungeonRoom room in _allRooms)
            room.SpawnCeiling();

        _startToEndRoute[0].DespawnCeiling();
    }

    #endregion

    #region Display in Rooms

    private void ClearAllRoomDisplays()
    {
        foreach (DungeonRoom room in _allRooms)
            room.DisplayText("");
    }

    private void DisplaySpawnOrderPerRoom()
    {
        foreach (DungeonRoom room in _allRooms)
            room.DisplayText(room.GivenSpawnOrder.ToString());
    }

    private void DisplaySeedCostPerRoom()
    {
        foreach (DungeonRoom room in _allRooms)
            room.DisplayText(room.GivenSeedCost.ToString());
    }

    private void DisplayStartRoomAsStart()
    {
        _startToEndRoute[0].DisplayText("START");
    }

    private void DisplayAllPathRoomsAsPath()
    {
        // Skip first and last, between is what we want.
        for (int i = 1; i < _startToEndRoute.Count - 1; i++)
            _startToEndRoute[i].DisplayText($"PATH\n{i - 1}");
    }

    private void DisplayEndRoomAsEnd()
    {
        _startToEndRoute[^1].DisplayText("END");
    }

    private void DisplayTrappedRooms() 
    {
        for (int i = 0; i < _trappedRooms.Count; i++)
        {
            _trappedRooms[i].DisplayText($"TRAP\n{i}");
        }
    }

    private void DisplayLootRooms()
    {
        for (int i = 0; i < _lootRooms.Count; i++)
        {
            _lootRooms[i].DisplayText($"LOOT\n{i}");
        }
    }

    private void DisplayDwellerRooms()
    {
        for (int i = 0; i < _dwellerRooms.Count; i++)
        {
            _dwellerRooms[i].DisplayText($"DWELLER\n{i}");
        }
    }

    #endregion

    #region Extra Helpers

    private DungeonRoom FindBestMapCandidate(RoomTypeEnum ofType)
    {
        // Safety check.
        if (_allRooms.Count == 0)
            return null;

        // Temporary storage.
        DungeonRoom bestCandidate = null;

        // Reverse cycle all rooms to find the best one.
        for (int i = _allRooms.Count - 1; i >= 0; i--)
        {
            // Find and temporary store this potential candidate from the list.
            DungeonRoom candidate = _allRooms[i];

            // Filer out all other types.
            if (candidate.GivenRoomType != ofType)
                continue;

            // compare cost, if higher then replace the best candidate since it's closer.
            if (bestCandidate == null || candidate.GivenSeedCost >= bestCandidate.GivenSeedCost)
            {
                bestCandidate = candidate;
            }
        }

        // Result.
        return bestCandidate;
    }

    private DungeonRoom FindBestNeighbourCandidate(DungeonRoom origin, RoomTypeEnum ofType)
    {
        Vector2Int pos = origin.GridPosition;
        DungeonRoom bestCandidate = null;

        // 4-directional neighbours.
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            int x = pos.x + dir.x;
            int y = pos.y + dir.y;

            // Bounds check
            if (x < 0 || x >= _dungeonWidth || y < 0 || y >= _dungeonHeight)
                continue;

            DungeonRoom candidate = _roomGrid[x, y];

            if (candidate == null || candidate.GivenRoomType != ofType)
                continue;

            // --- Wall check ---
            DungeonCell originCell = _cellGrid[pos.x, pos.y];
            DungeonCell candidateCell = _cellGrid[x, y];

            // Continue if a wall is between them.
            if (dir == Vector2Int.up && originCell.HasWall(WallEnum.Up) ||
                dir == Vector2Int.down && originCell.HasWall(WallEnum.Down) ||
                dir == Vector2Int.left && originCell.HasWall(WallEnum.Left) ||
                dir == Vector2Int.right && originCell.HasWall(WallEnum.Right))
                continue;

            // Pick best. (highest cost!)
            if (bestCandidate == null || candidate.GivenSeedCost > bestCandidate.GivenSeedCost)
            {
                bestCandidate = candidate;
            }
            else if (candidate.GivenSeedCost == bestCandidate.GivenSeedCost)
            {
                // Lower spawn order gets priority. (Tie-breaker!)
                if (candidate.GivenSpawnOrder < bestCandidate.GivenSpawnOrder)
                    bestCandidate = candidate;
            }
        }

        return bestCandidate;
    }

    private void UpdateAllRoomVisuals()
    {
        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonHeight; y++)
            {
                DungeonRoom room = _roomGrid[x, y];

                // Clear old visuals and then rebuild from data.
                room.DespawnSides();
                room.SpawnSides(_cellGrid[x, y], _dungeonWidth, _dungeonHeight);
            }
        }
    }

    private List<DungeonCell> GetUnvisitedNeighbours(DungeonCell cell, List<DungeonCell> visitedCells, Stack<DungeonCell> cellstack)
    {
        List<DungeonCell> result = new List<DungeonCell>();
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                int cellX = cell.GridPosition.x + x;
                int cellY = cell.GridPosition.y + y;
                if (cellX < 0 || cellX >= _dungeonWidth || cellY < 0 || cellY >= _dungeonHeight || Mathf.Abs(x) == Mathf.Abs(y))
                {
                    continue;
                }
                DungeonCell canditateCell = _cellGrid[cellX, cellY];
                if (!visitedCells.Contains(canditateCell) && !cellstack.Contains(canditateCell))
                {
                    result.Add(canditateCell);
                }
            }
        }

        return result;
    }

    private bool RemoveWallBetweenCells(DungeonCell cellOne, DungeonCell cellTwo)
    {
        int numWallCellOne = cellOne.GetNumWalls();
        Vector2Int dirVector = cellTwo.GridPosition - cellOne.GridPosition;
        if (dirVector.x != 0)
        {
            cellOne.RemoveWall(dirVector.x > 0 ? WallEnum.Right : WallEnum.Left);
            cellTwo.RemoveWall(dirVector.x > 0 ? WallEnum.Left : WallEnum.Right);
        }
        if (dirVector.y != 0)
        {
            cellOne.RemoveWall(dirVector.y > 0 ? WallEnum.Up : WallEnum.Down);
            cellTwo.RemoveWall(dirVector.y > 0 ? WallEnum.Down : WallEnum.Up);
        }

        //Is a wall succesfully removed?
        if (numWallCellOne != cellOne.GetNumWalls())
            return true;

        return false;
    }

    #endregion
}

using TMPro;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [Header("Room Attributes")]
    private float _givenSeedCost = 0;
    private float _givenSpawnOrder = 0;
    private RoomTypeEnum _givenRoomType;
    private Vector2Int _gridPosition;

    [Header("UI feedback")]
    [SerializeField] private TextMeshProUGUI _personalTextDisplay;

    [Header("Preset Prefabs")]
    [SerializeField] private GameObject _internalWallPrefab;
    [SerializeField] private GameObject _externalWallPrefab;
    [SerializeField] private GameObject _doorPrefab;
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _ceilingPrefab;

    [Header("Current Children Slots Debug")]
    [SerializeField] private GameObject _currentFloorObject;
    [SerializeField] private GameObject _currentDownObject;
    [SerializeField] private GameObject _currentUpObject;
    [SerializeField] private GameObject _currentLeftObject;
    [SerializeField] private GameObject _currentRightObject;
    [SerializeField] private GameObject _currentCeilingObject;

    #region Getter & Setters

    public float GivenSeedCost
    {
        get => _givenSeedCost;
        set => _givenSeedCost = value;
    }

    public float GivenSpawnOrder
    {
        get => _givenSpawnOrder;
        set => _givenSpawnOrder = value;
    }

    public RoomTypeEnum GivenRoomType
    {
        get => _givenRoomType;
        set => _givenRoomType = value;
    }

    public Vector2Int GridPosition
    {
        get => _gridPosition;
        set => _gridPosition = value;
    }

    #endregion

    public void DisplayText(string newText)
    {
        _personalTextDisplay.text = newText;
    }

    public void SpawnFloor()
    {
        _currentFloorObject = Instantiate(_floorPrefab, transform.position, Quaternion.identity, transform);
    }

    public void SpawnCeiling()
    {
        _currentCeilingObject = Instantiate(_ceilingPrefab, transform.position, Quaternion.identity, transform);
    }

    public void DespawnFloor()
    {
        if (_currentFloorObject == null)
            return;

        Destroy(_currentFloorObject);
        _currentFloorObject = null;
    }

    public void DespawnCeiling()
    {
        if (_currentCeilingObject == null)
            return;

        Destroy(_currentCeilingObject);
        _currentCeilingObject = null;
    }

    public void DespawnSides()
    {
        DespawnAnSide(ref _currentDownObject);
        DespawnAnSide(ref _currentUpObject);
        DespawnAnSide(ref _currentLeftObject);
        DespawnAnSide(ref _currentRightObject);
    }

    private void DespawnAnSide(ref GameObject obj)
    {
        if (obj == null) return;

        Destroy(obj);
        obj = null;
    }

    public void SpawnSides(DungeonCell cell, int width, int height)
    {
        Vector2Int pos = cell.GridPosition;

        bool isBottomEdge = pos.y == 0;
        bool isTopEdge = pos.y == height - 1;
        bool isLeftEdge = pos.x == 0;
        bool isRightEdge = pos.x == width - 1;

        SpawnAnSide(ref _currentDownObject, cell.HasWall(WallEnum.Down), isBottomEdge, new Vector3(0, 0, -1));
        SpawnAnSide(ref _currentUpObject, cell.HasWall(WallEnum.Up), isTopEdge, new Vector3(0, 0, 1));
        SpawnAnSide(ref _currentLeftObject, cell.HasWall(WallEnum.Left), isLeftEdge, new Vector3(-1, 0, 0));
        SpawnAnSide(ref _currentRightObject, cell.HasWall(WallEnum.Right), isRightEdge, new Vector3(1, 0, 0));
    }

    private void SpawnAnSide(
        ref GameObject wallSlot,
        bool hasWall,
        bool isEdge,
        Vector3 direction
    )
    {
        if (wallSlot != null) return;

        if (hasWall)
        {
            GameObject prefab = isEdge ? _externalWallPrefab : _internalWallPrefab;

            wallSlot = Instantiate(
                prefab,
                transform.position,
                Quaternion.LookRotation(direction),
                transform
            );
        }
        else
        {
            wallSlot = Instantiate(
                _doorPrefab,
                transform.position,
                Quaternion.LookRotation(direction),
                transform
            );
        }
    }
}

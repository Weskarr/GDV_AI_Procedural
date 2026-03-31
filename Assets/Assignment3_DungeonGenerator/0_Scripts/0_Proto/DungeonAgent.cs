using System.Collections.Generic;
using UnityEngine;

public class DungeonAgent : MonoBehaviour
{
    [SerializeField] private MoveOptionsEnum _moveButton = MoveOptionsEnum.LeftMouse;
    [SerializeField] private float _moveSpeed = 3;
    private DungeonAstar _astar = new();
    private List<Vector2Int> _path = new();
    private Plane _ground = new(Vector3.up, 0f);
    private MeshRenderer _renderer;
    private GameObject _targetVisual;
    private DungeonGenerator _dungeon;
    private LineRenderer _line;

    // Minor improvement for rotation.
    [SerializeField] private float _lookSpeed = 1f;
    [SerializeField] private bool _onlyMoveWhenFacing = true;
    private Vector3 _currentLookAtPosition;
    private bool _isFacingCorrectly = true;
    private bool _reachedDestination = false;

    // Changed to newer Input System
    private InputHandler MyInputHandler => InputHandler.Instance;

    // Switched from awake to start, better for the InputHandler.
    private void Start()
    {
        SubscribeCorrectMoveButton();

        _dungeon = FindAnyObjectByType<DungeonGenerator>();
        _renderer = GetComponentInChildren<MeshRenderer>();
        _targetVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _targetVisual.transform.localScale = new Vector3(1f, 1f, 1f);
        _targetVisual.GetComponent<MeshRenderer>().material.color = _renderer.material.color;
        _line = GetComponent<LineRenderer>();
        _line.material.color = _renderer.material.color;
    }

    private void SubscribeCorrectMoveButton()
    {
        switch (_moveButton)
        {
            case MoveOptionsEnum.None:
                // Do nothing.
                break;

            case MoveOptionsEnum.LeftMouse:
                MyInputHandler.OnLeftMouseCanceled += AgentSetNewMoveGoal;
                break;

            case MoveOptionsEnum.RightMouse:
                MyInputHandler.OnRightMouseCanceled += AgentSetNewMoveGoal;
                break;
        }
    }

    private void FindPathToTarget(Vector2Int startPos, Vector2Int endPos, DungeonCell[,] grid)
    {
        _path = _astar.FindPathToTarget(startPos, endPos, grid);
        DrawPath();
    }

    private void DrawPath()
    {
        if (_path != null && _path.Count > 0)
        {
            _line.positionCount = _path.Count;
            for (int i = 0; i < _path.Count; i++)
            {
                _line.SetPosition(i, Vector2IntToVector3(_path[i], 0.1f));
            }
        }
    }


    //Move to clicked position
    private void Update()
    {
        // Singular block.
        if (_reachedDestination)
            return;

        // Looking Logic.
        if (_currentLookAtPosition.sqrMagnitude > 0.1f || _isFacingCorrectly == false)
            AgentLook();

        // Moving Logic.
        if (_path != null && _path.Count > 0)
            AgentMove();

        // Is finished check.
        if (_path != null && _path.Count == 0 && _isFacingCorrectly)
            _reachedDestination = true;
    }

    private void AgentLook()
    {
        _isFacingCorrectly = false;
        Quaternion targetRot = Quaternion.LookRotation(_currentLookAtPosition);

        if (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                360f * Time.deltaTime * _lookSpeed
            );
        }
        else
        {
            _isFacingCorrectly = true;
        }
    }

    private void AgentMove()
    {
        if (transform.position != Vector2IntToVector3(_path[0]))
        {
            _currentLookAtPosition = Vector2IntToVector3(_path[0]) - transform.position;

            if (_onlyMoveWhenFacing && !_isFacingCorrectly)
                return; // Return since not facing in the right direction to move, if enabled.

            transform.position = Vector3.MoveTowards(transform.position, Vector2IntToVector3(_path[0]), _moveSpeed * Time.deltaTime);
        }
        else
        {
            _path.RemoveAt(0);
            DrawPath();
        }
    }

    private void AgentSetNewMoveGoal()
    {
        // Only set a new goal once the current one is finished, might causes weird issues otherwise.
        if (!_reachedDestination)
            return;

        Ray ray = Camera.main.ScreenPointToRay(MyInputHandler.GetMousePosition);

        if (!_ground.Raycast(ray, out float dist))
            return; // Just ignore invalid clicks.

        Vector3 worldPos = ray.GetPoint(dist);
        Vector2Int targetPos = Vector3ToVector2Int(worldPos);
        Vector2Int thisPos = Vector3ToVector2Int(transform.position);

        _targetVisual.transform.position = Vector2IntToVector3(targetPos);

        FindPathToTarget(
            thisPos,
            targetPos,
            _dungeon.CellGrid
        );

        Debug.Log("Click From: " + this.gameObject.name + $" ({targetPos})");

        // Time to Pathfind!
        _reachedDestination = false;
    }

    /*
    // No longer needed, integrated into Agent Move for simplicity.
    public Vector3 MouseToWorld()
    {
        Ray ray = Camera.main.ScreenPointToRay(MyInputHandler.GetMousePosition);

        float distToGround = -1f;
        ground.Raycast(ray, out distToGround);
        Vector3 worldPos = ray.GetPoint(distToGround);

        return worldPos;
    }
    */

    private Vector2Int Vector3ToVector2Int(Vector3 pos)
    {
        int scale = _dungeon.GridScale;
        Vector3 origin = _dungeon.transform.position;

        return new Vector2Int(
            Mathf.FloorToInt((pos.x - origin.x + scale * 0.5f) / scale),
            Mathf.FloorToInt((pos.z - origin.z + scale * 0.5f) / scale)
        );
    }
    private Vector3 Vector2IntToVector3(Vector2Int pos, float YPos = 0)
    {
        int scale = _dungeon.GridScale;
        Vector3 origin = _dungeon.transform.position;

        return new Vector3(
            origin.x + pos.x * scale,
            YPos,
            origin.z + pos.y * scale
        );
    }
    private void OnDrawGizmos()
    {
        if (_path != null && _path.Count > 0)
        {
            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.color = _renderer.material.color;
                Gizmos.DrawLine(Vector2IntToVector3(_path[i], 0.5f), Vector2IntToVector3(_path[i + 1], 0.5f));
            }
        }
    }
}

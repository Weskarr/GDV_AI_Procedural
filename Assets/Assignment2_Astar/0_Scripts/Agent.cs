using System.Collections.Generic;
using UnityEngine;

public enum MoveButtonOptions
{
    None = 0,
    LeftMouse = 1,
    RightMouse = 2
}

public class Agent : MonoBehaviour
{

    public MoveButtonOptions moveButton = MoveButtonOptions.LeftMouse;
    public float moveSpeed = 3;
    private Astar Astar = new Astar();
    private List<Vector2Int> path = new List<Vector2Int>();
    private Plane ground = new Plane(Vector3.up, 0f);
    private MeshRenderer renderer;
    private GameObject targetVisual;
    private MazeGeneration maze;
    private LineRenderer line;

    // Minor improvement for rotation.
    private Vector3 CurrentLookAtPosition;
    public float lookSpeed = 1f;
    public bool onlyMoveWhenFacing = true;
    private bool isFacingCorrectly = true;
    private bool reachedDestination = false;

    // Changed to newer Input System
    private InputHandler MyInputHandler => InputHandler.Instance;

    // Switched from awake to start, better for the InputHandler.
    private void Start()
    {
        SubscribeCorrectMoveButton();

        maze = FindObjectOfType<MazeGeneration>();
        renderer = GetComponentInChildren<MeshRenderer>();
        targetVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        targetVisual.GetComponent<MeshRenderer>().material.color = renderer.material.color;
        line = GetComponent<LineRenderer>();
        line.material.color = renderer.material.color;
        line.material.color = renderer.material.color;
    }

    private void SubscribeCorrectMoveButton()
    {
        switch (moveButton)
        {
            case MoveButtonOptions.None:
                // Do nothing.
                break;

            case MoveButtonOptions.LeftMouse:
                MyInputHandler.OnLeftMouseCanceled += AgentSetNewMoveGoal;
                break;

            case MoveButtonOptions.RightMouse:
                MyInputHandler.OnRightMouseCanceled += AgentSetNewMoveGoal;
                break;
        }
    }

    public void FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        path = Astar.FindPathToTarget(startPos, endPos, grid);
        DrawPath();
    }

    private void DrawPath()
    {
        if (path != null && path.Count > 0)
        {
            line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                line.SetPosition(i, Vector2IntToVector3(path[i], 0.1f));
            }
        }
    }


    //Move to clicked position
    public void Update()
    {
        // Singular block.
        if (reachedDestination)
            return;

        // Looking Logic.
        if (CurrentLookAtPosition.sqrMagnitude > 0.1f || isFacingCorrectly == false)
            AgentLook();

        // Moving Logic.
        if (path != null && path.Count > 0)
            AgentMove();

        // Is finished check.
        if (path != null && path.Count == 0 && isFacingCorrectly)
            reachedDestination = true;
    }

    private void AgentLook()
    {
        Debug.Log("Look Logic");

        isFacingCorrectly = false;
        Quaternion targetRot = Quaternion.LookRotation(CurrentLookAtPosition);

        if (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                360f * Time.deltaTime * lookSpeed
            );
        }
        else
        {
            isFacingCorrectly = true;
        }
    }

    private void AgentMove()
    {
        if (transform.position != Vector2IntToVector3(path[0]))
        {
            CurrentLookAtPosition = Vector2IntToVector3(path[0]) - transform.position;

            if (onlyMoveWhenFacing && !isFacingCorrectly)
                return; // Return since not facing in the right direction to move, if enabled.

            transform.position = Vector3.MoveTowards(transform.position, Vector2IntToVector3(path[0]), moveSpeed * Time.deltaTime);
        }
        else
        {
            path.RemoveAt(0);
            DrawPath();
        }
    }

    private void AgentSetNewMoveGoal()
    {
        Ray ray = Camera.main.ScreenPointToRay(MyInputHandler.GetMousePosition);

        if (!ground.Raycast(ray, out float dist))
            return; // Just ignore invalid clicks.

        Vector3 worldPos = ray.GetPoint(dist);
        Vector2Int targetPos = Vector3ToVector2Int(worldPos);
        Vector2Int thisPos = Vector3ToVector2Int(transform.position);

        targetVisual.transform.position = Vector2IntToVector3(targetPos);

        FindPathToTarget(
            thisPos,
            targetPos,
            maze.grid
        );

        Debug.Log("Click From: " + this.gameObject.name + $" ({targetPos})");

        // Time to Pathfind!
        reachedDestination = false;
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
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
    }
    private Vector3 Vector2IntToVector3(Vector2Int pos, float YPos = 0)
    {
        return new Vector3(Mathf.RoundToInt(pos.x), YPos, Mathf.RoundToInt(pos.y));
    }
    private void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.color = renderer.material.color;
                Gizmos.DrawLine(Vector2IntToVector3(path[i], 0.5f), Vector2IntToVector3(path[i + 1], 0.5f));
            }

        }
    }
}

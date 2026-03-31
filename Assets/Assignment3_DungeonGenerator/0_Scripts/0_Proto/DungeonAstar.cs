
using System.Collections.Generic;
using UnityEngine;

// [Astar] Steps:
// 0. Set destination not reached..
// 1. Start node is primary..                                                   (Add the start node to the open list.)
// 2. Identify neighbours of the primary node..                                 (Get the neighbours from cell, but skip closed neighbours!)
// 3. Eliminate inaccessible neighbours..                                       (Walls, blocked tiles, or invalid nodes)
// 4. Calculate the cost from start to each neighbour..                         (G Score: this is cumulative! [Start -> … -> current -> neighbour])
// 5. Calculate the heuristic distance from each neighbour to the end..         (H Score: estimated distance! [Manhattan, Euclidean, etc.])
// 6. Calculate total cost..                                                    (F score: This is what determines the most worthy contender! [f = g + h])
// 7. Pick the node with the lowest f from the open list..                      (Check the full open list.]
// 8. Move this node to the closed list and set it as primary..                 (Prevents reprocessing.)
// 9. If this node is the end node..                                            (Stop searching and reconstruct the path by following parents backward.)
// 10. If the open list is empty and the end was not reached..                  (No path is possible.)
// 11. Return the reconstructed path to the agent..                             (Agent moves from start -> end using the path.)

public class DungeonAstar
{
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, DungeonCell[,] grid)
    {
        #region Initial Safety Checks

        // Safety check One:
        if (startPos == null || endPos == null || grid == null)
        {
            Debug.LogWarning("Receiving nulls, not allowed to travel!");
            return null;
        }

        // Safety check two:
        if (startPos == endPos)
        {
            Debug.Log("Start and end is the same, no need to travel!");
            return null;
        }

        // Safety check three:
        if (grid.Length == 0)
        {
            Debug.Log("Grid length is 0, so there is no grid!");
            return null;
        }

        #endregion

        // Data container for grid related variables.
        DungeonGridData gridData = PrecalculateGridData(grid);

        // Open & closed node group data holders.
        DungeonNodeGroupData closedNodeGroupData = new();
        DungeonNodeGroupData openNodeGroupData = new();

        // For knowning when the end is reached.
        bool destinationReached = false;

        // Temporary node variables.
        DungeonNodeData primaryNode;

        // Reconstructed path.
        List<Vector2Int> reconstructedPath = new();

        // Clamp end position, yet to determine if reachable though.
        endPos = ClampPositionInsideGrid(endPos, gridData);

        // Initialise start node and add to open node group.
        primaryNode = InitialStartNode(grid[startPos.x, startPos.y], startPos, openNodeGroupData);

        // Actual Astar pathfinding loop.
        while (!destinationReached)
        {
            #region Safety Breaks

            // Had a few crashes hence, rather quit early intead.

            // Safety breaks one:
            if (primaryNode == null)
            {
                Debug.LogError("Primary is null?");
                break;
            }

            // Safety breaks two:
            int nodeCount = openNodeGroupData.CountNodes() + closedNodeGroupData.CountNodes();
            if (nodeCount > gridData.Size)
            {
                Debug.LogError($"Lists exceed maximum of {nodeCount} / {gridData.Size}?");
                break;
            }

            #endregion

            // Swap primary from open to closed groups.
            openNodeGroupData.RemoveNode(primaryNode);
            closedNodeGroupData.AddNode(primaryNode);

            // Attempt to expand into primary neighbours.
            AttemptToAddPrimaryNeighbours(primaryNode, openNodeGroupData, closedNodeGroupData, gridData, grid, endPos);

            // Go to the next primary node.
            primaryNode = TryGetLowestNodeScoreF(openNodeGroupData);

            // Destination reached check. (Includes clamped end destination)
            if (primaryNode != null && primaryNode.Position == endPos)
            {
                // Destination found.
                reconstructedPath = ReconstructPathFromNode(primaryNode);
                destinationReached = true;
            }

            // Forced alternative check. (Incase if the whole grid is explored, force alternative)
            if (openNodeGroupData.CountNodes() == 0 && !destinationReached)
            {
                // Find best alternative candidate. (Assuming closed is not null, since start is included)
                primaryNode = TryGetLowestNodeScoreH(closedNodeGroupData);

                // Alternative closest found. (Back-up after clamp)
                reconstructedPath = ReconstructPathFromNode(primaryNode);
                destinationReached = true;
            }
        }

        return reconstructedPath;
    }

    private DungeonNodeData InitialStartNode(DungeonCell cell, Vector2Int position, DungeonNodeGroupData group)
    {
        DungeonNodeData node = new(cell, position, null, 0, float.MaxValue, float.MaxValue);
        group.AddNode(node);
        return node;
    }

    private Vector2Int ClampPositionInsideGrid(Vector2Int position, DungeonGridData gridData)
    {
        if (!(position.x >= 0 && position.x < gridData.Width && position.y >= 0 && position.y < gridData.Height))
        {
            position.x = Mathf.Clamp(position.x, 0, gridData.Width - 1);
            position.y = Mathf.Clamp(position.y, 0, gridData.Height - 1);
        }
        return position;
    }

    private DungeonGridData PrecalculateGridData(DungeonCell[,] grid)
    {
        // Precalculate grid variables.
        int size = grid.Length;
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        DungeonGridData data = new(size, width, height);
        return data;
    }

    private List<Vector2Int> GetAvailableNeighboursOfGivenNode(DungeonNodeData node, DungeonGridData gridData)
    {
        List<Vector2Int> neighbours = new();

        // For UP neighbour.
        Vector2Int upNeighbour = node.Position + new Vector2Int(0, 1);
        if (!node.Cell.HasWall(WallEnum.Up) && upNeighbour.y < gridData.Height)
            neighbours.Add(upNeighbour);

        // For DOWN neighbour.
        Vector2Int downNeighbour = node.Position + new Vector2Int(0, -1);
        if (!node.Cell.HasWall(WallEnum.Down) && downNeighbour.y >= 0)
            neighbours.Add(downNeighbour);

        // For RIGHT neighbour.
        Vector2Int rightNeighbour = node.Position + new Vector2Int(1, 0);
        if (!node.Cell.HasWall(WallEnum.Right) && rightNeighbour.x < gridData.Width)
            neighbours.Add(rightNeighbour);

        // For LEFT neighbour.
        Vector2Int leftNeighbour = node.Position + new Vector2Int(-1, 0);
        if (!node.Cell.HasWall(WallEnum.Left) && leftNeighbour.x >= 0)
            neighbours.Add(leftNeighbour);

        return neighbours;
    }

    private void AttemptToAddPrimaryNeighbours(DungeonNodeData primary, DungeonNodeGroupData openGroup, DungeonNodeGroupData closedGroup, DungeonGridData gridData, DungeonCell[,] grid, Vector2Int end)
    {
        List<Vector2Int> neighbouringVectors = GetAvailableNeighboursOfGivenNode(primary, gridData);
        for (int i = 0; i < neighbouringVectors.Count; i++)
        {
            // Get the neighbour position.
            Vector2Int position = neighbouringVectors[i];

            // Check if the position has already been used previously.
            if (closedGroup.ContainsViaPosition(position) ||
                openGroup.ContainsViaPosition(position))
                continue;

            // Appoximation from cell position to start position.
            float Gscore = primary.GScore + 1;

            // Known cost so far from cell position to start.
            float Hscore = Vector2Int.Distance(position, end);

            // Combined cost used for primary node order.
            float Fscore = Gscore + Hscore;

            // Find cell on grid.
            DungeonCell cell = grid[position.x, position.y];

            // Create new pathfinding node.
            DungeonNodeData tempNode = new DungeonNodeData(cell, position, primary, Gscore, Hscore, Fscore);

            // Add the pathfinding node to open group.
            openGroup.AddNode(tempNode);
        }
    }

    private DungeonNodeData TryGetLowestNodeScoreF(DungeonNodeGroupData group)
    {
        DungeonNodeData lowestNode = null;
        float lowest = float.MaxValue;
        int count = group.CountNodes();
        for (int i = 0; i < count; i++)
        {
            DungeonNodeData temp = group.GetNodeDataAtIndex(i);
            float current = temp.FScore;
            if (current < lowest)
            {
                lowest = current;
                lowestNode = temp;
            }
        }
        return lowestNode;
    }

    private DungeonNodeData TryGetLowestNodeScoreH(DungeonNodeGroupData group)
    {
        DungeonNodeData lowestNode = null;
        float lowest = float.MaxValue;
        int count = group.CountNodes();
        for (int i = 0; i < count; i++)
        {
            DungeonNodeData temp = group.GetNodeDataAtIndex(i);
            float current = temp.HScore;
            if (current < lowest)
            {
                lowest = current;
                lowestNode = temp;
            }
        }
        return lowestNode;
    }

    private List<Vector2Int> ReconstructPathFromNode(DungeonNodeData node)
    {
        List<Vector2Int> path = new();
        DungeonNodeData pathNode = node;
        while (pathNode != null)
        {
            path.Add(pathNode.Position);
            pathNode = pathNode.Parent;
        }
        path.Reverse();
        return path;
    }
}

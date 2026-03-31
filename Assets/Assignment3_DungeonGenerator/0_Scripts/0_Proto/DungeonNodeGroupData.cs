// This is a Node Group Data Class, acts as an kind of blackboard for tracking a group of nodes.
using System.Collections.Generic;
using UnityEngine;

public class DungeonNodeGroupData
{
    private List<DungeonNodeData> _nodeList = new();
    private HashSet<Vector2Int> _nodeHash = new();

    public bool ContainsViaNode(DungeonNodeData node)
    {
        if (_nodeList.Contains(node))
            return true;
        else
            return false;
    }

    public bool ContainsViaPosition(Vector2Int position)
    {
        if (_nodeHash.Contains(position))
            return true;
        else
            return false;
    }

    public DungeonNodeData GetNodeDataAtIndex(int index)
    {
        return _nodeList[index];
    }

    public int CountNodes()
    {
        return _nodeList.Count;
    }

    public void AddNode(DungeonNodeData nodeData)
    {
        _nodeList.Add(nodeData);
        _nodeHash.Add(nodeData.Position);
    }

    public void RemoveNode(DungeonNodeData nodeData)
    {
        _nodeList.Remove(nodeData);
        _nodeHash.Remove(nodeData.Position);
    }
}
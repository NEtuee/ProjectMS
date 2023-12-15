using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AkaneSequencerGraph;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SequencerGraphFile", menuName = "SequencerGraphFile")]
public sealed class AkaneSequencerGraphData : ScriptableObject
{
    public int InstanceID
    {
        get
        {
            if (_instanceID == 0)
            {
                _instanceID = GetInstanceID();
            }

            return _instanceID;
        }
    }

    public PhaseNodeData InitializePhaseNodeData = new PhaseNodeData(Guid.NewGuid().ToString());
    public PhaseNodeData UpdatePhaseNodeData = new PhaseNodeData(Guid.NewGuid().ToString());
    public PhaseNodeData EndPhaseNodeData = new PhaseNodeData(Guid.NewGuid().ToString());

    public List<EventNodeData> InitializePhaseNodeDataList = new List<EventNodeData>();
    public List<EventNodeData> UpdatePhaseNodeDataList = new List<EventNodeData>();
    public List<EventNodeData> EndPhaseNodeDataList = new List<EventNodeData>();

    public List<TaskNodeData> TaskNodeDataList = new List<TaskNodeData>();

    public List<EventNodeData> TaskChildNodeDataList = new List<EventNodeData>();

    public List<EdgeSaveData> EdgeList = new List<EdgeSaveData>();

    public int HashCode => InstanceID.GetHashCode();
    
    [SerializeField] private int _instanceID;
    
    public override int GetHashCode()
    {
        return HashCode;
    }

    public static PhaseNodeData ParsePhaseNodeToData(ReservedPhaseNode node)
    {
        var nodeData = new PhaseNodeData(node.Guid);
        nodeData.SetPosition(node.GetPosition());

        return nodeData;
    }

    public static EventNodeData ParseEventNodeToData(EventNode node)
    {
        var nodeData = new EventNodeData();
        nodeData.Guid = node.Guid;
        nodeData.TypeName = node.GetType().ToString();
        nodeData.SetPosition(node.GetPosition());

        if (node is TaskCallNode taskCallNode)
        {
            nodeData.Xml = taskCallNode.TaskKey;
        }
        else
        {
            nodeData.Xml = node.GetResultContext();
        }

        return nodeData;
    }
    
    public static TaskNodeData ParseTaskNodeToData(TaskEventNode node)
    {
        var nodeData = new TaskNodeData();
        nodeData.Guid = node.Guid;
        nodeData.TaskKey = node.TaskKey;
        nodeData.ProcessEnum = node.ProcessTypeInt;
        nodeData.SetPosition(node.GetPosition());

        return nodeData;
    }
}

[Serializable]
public class PhaseNodeData
{
    public string Guid;
    
    public float X;
    public float Y;
    public float Width;
    public float Height;
    
    public PhaseNodeData(string guid)
    {
        Guid = guid;
    }

    public void SetPosition(Rect rect)
    {
        X = rect.x;
        Y = rect.y;
        Width = rect.width;
        Height = rect.height;
    }

    public Rect GetPosition()
    {
        return new Rect
        {
            x = X,
            y = Y,
            width = Width,
            height = Height
        };
    }
}

[Serializable]
public class EventNodeData
{
    public string Guid;
    public string TypeName;
    public string Xml;
    
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public void SetPosition(Rect rect)
    {
        X = rect.x;
        Y = rect.y;
        Width = rect.width;
        Height = rect.height;
    }

    public Rect GetPosition()
    {
        return new Rect
        {
            x = X,
            y = Y,
            width = Width,
            height = Height
        };
    }
}

[Serializable]
public class TaskNodeData
{
    public string Guid;
    public string TaskKey;
    public int ProcessEnum;
    
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public void SetPosition(Rect rect)
    {
        X = rect.x;
        Y = rect.y;
        Width = rect.width;
        Height = rect.height;
    }

    public Rect GetPosition()
    {
        return new Rect
        {
            x = X,
            y = Y,
            width = Width,
            height = Height
        };
    }
}

[Serializable]
public class EdgeSaveData
{
    public string From;
    public string To;

    public EdgeSaveData(string fromGuid, string toGuid)
    {
        From = fromGuid;
        To = toGuid;
    }
}

[Serializable]
public class SerializableRect
{
    private float x;
    private float y;
    private float width;
    private float height;
    
    public SerializableRect(Rect rect)
    {
        x = rect.x;
        y = rect.y;
        width = rect.width;
        height = rect.height;
		
    }
    
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}, {3}]", x, y, width, height);
    }
    
    public static implicit operator Rect(SerializableRect vRect)
    {
        return new Rect(vRect.x, vRect.y, vRect.width, vRect.height);
    }
    
    public static implicit operator SerializableRect(Rect vRect)
    {
        return new SerializableRect(vRect);
    }
}

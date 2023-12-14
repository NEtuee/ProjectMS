using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AkaneSequencerGraph;
using NUnit.Framework;
using UnityEngine;

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

    [SerializeField] public InitializePhaseNode InitializePhase;
    [SerializeField] public UpdatePhaseNode UpdatePhase;
    [SerializeField] public EndPhaseNode EndPhase;
    
    [SerializeField] public List<AkaneSequenceNode> InitalizePhaseNodeList = new List<AkaneSequenceNode>();
    [SerializeField] public List<AkaneSequenceNode> UpdatePhaseNodeList = new List<AkaneSequenceNode>();
    [SerializeField] public List<AkaneSequenceNode> EndPhaseNodeList = new List<AkaneSequenceNode>();

    [SerializeField] public List<AkaneSequenceNode> TaskNodeList = new List<AkaneSequenceNode>();

    public List<EdgeSaveData> EdgeList = new List<EdgeSaveData>();

    public int HashCode => InstanceID.GetHashCode();
    
    [SerializeField] private int _instanceID;
    
    public override int GetHashCode()
    {
        return HashCode;
    }
}

[Serializable]
public sealed class EdgeSaveData
{
    public string From;
    public string To;

    public EdgeSaveData(string fromGuid, string toGuid)
    {
        From = fromGuid;
        To = toGuid;
    }
}

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

    public InitializePhaseNode InitializePhase;
    public UpdatePhaseNode UpdatePhase;
    public EndPhaseNode EndPhase;
    
    public List<AkaneSequenceNode> InitalizePhaseNodeList = new List<AkaneSequenceNode>();
    public List<AkaneSequenceNode> UpdatePhaseNodeList = new List<AkaneSequenceNode>();
    public List<AkaneSequenceNode> EndPhaseNodeList = new List<AkaneSequenceNode>();

    public List<EdgeSaveData> EdgeList = new List<EdgeSaveData>();

    public int HashCode => InstanceID.GetHashCode();
    
    private int _instanceID;
    
    public override int GetHashCode()
    {
        return HashCode;
    }

    public void Save()
    {
        // GenerateNewSequencerGraphData();
        //
        // var jsonString = JsonUtility.ToJson(_data, true);
        //
        // if (FilePath == null)
        // {
        //     Debug.LogError("Failed to save SequencerGraphData");
        //     return;
        // }
        //
        // using StreamWriter streamWriter = new StreamWriter(FilePath);
        // streamWriter.Write(jsonString);
    }

    public void Load()
    {
        // using StreamReader streamReader = new StreamReader(FilePath);
        // var jsonString = streamReader.ReadToEnd();
        // _data = JsonUtility.FromJson<Data>(jsonString);
        //
        // _dataName = _data.Name;
    }

    private void GenerateNewSequencerGraphData()
    {
        //_data.Name = _dataName;
    }

    private void OnEnable()
    {
        //_dataName = name;
    }
    
    [SerializeField]
    public sealed class Data
    {
        public string Name;
    }
}

[SerializeField]
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

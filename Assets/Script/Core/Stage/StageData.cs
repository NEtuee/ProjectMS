using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StagePointCharacterSpawnData
{
    public string   _characterKey = "";
    public bool     _flip = false;
    public Vector3  _localPosition = Vector3.zero;
}

[System.Serializable]
public class StagePointData
{
    public Vector3      _stagePoint = Vector3.zero;
    public float        _maxLimitedDistance = 0f;
    public string[]     _onEnterSequencerPath = null;
    public string[]     _onExitSequencerPath = null;

    public StagePointCharacterSpawnData[] _characterSpawnData = null;

    public StagePointData(Vector3 point) {_stagePoint = point;}
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Object/Stage Data", order = int.MaxValue)]
public class StageData : ScriptableObject
{
    public string _stageName;
    public string _backgroundPrefabPath = "";

    public List<StagePointData> _stagePointData = new List<StagePointData>();
}


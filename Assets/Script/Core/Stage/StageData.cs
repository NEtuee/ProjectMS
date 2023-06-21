using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StagePointData
{
    public Vector3 _stagePoint;
    public float _maxLimitedDistance;

    public StagePointData(Vector3 point) {_stagePoint = point;}
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Object/Stage Data", order = int.MaxValue)]
public class StageData : ScriptableObject
{
    public string _stageName;
    public List<StagePointData> _stagePointData = new List<StagePointData>();
}


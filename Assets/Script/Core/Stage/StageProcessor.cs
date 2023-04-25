using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageProcessor : MonoBehaviour
{
    public List<StagePoint> _stagePointList = new List<StagePoint>();

    private List<StageLine> _stageLineList = new List<StageLine>();


    public void addPoint(Vector3 initialPosition)
    {
        _stagePointList.Add(new StagePoint(initialPosition));

        if(_stagePointList.Count == 1)
            return;

        addLine(_stagePointList[_stagePointList.Count - 2], _stagePointList[_stagePointList.Count - 1]);
    }

    private void addLine(StagePoint start, StagePoint end)
    {
        StageLine line = new StageLine();
        line._startPoint = start;
        line._endPoint = end;

        _stageLineList.Add(line);
    }
}


public class StagePoint
{
    public Vector3 _stagePoint;
    public StageLine _linkedLine;

    public StagePoint(Vector3 point) {_stagePoint = point;}
}

public class StageLine
{
    public StagePoint _startPoint;
    public StagePoint _endPoint;
}
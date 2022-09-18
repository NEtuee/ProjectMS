using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoHelper : MonoBehaviour
{
    public static GizmoHelper instance;
    class GizmoCircleData
    {
        public Vector3 _center;
        public float _radius;
        public int _accuracy;

        public float _timer;

        public Color _color;
        
    };

    class GizmoPolygonData
    {
        public Vector3[] _verticies;

        public float _timer;
        public Color _color;
    }

    class GizmoLineData
    {
        public Vector3 _start;
        public Vector3 _end;

        public float _timer;
        public Color _color;
    }

    private List<GizmoCircleData> _circleRequestData = new List<GizmoCircleData>();
    private List<GizmoPolygonData> _polygonData = new List<GizmoPolygonData>();
    private List<GizmoLineData> _lineData = new List<GizmoLineData>();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        for(int index = 0; index < _lineData.Count; ++index)
        {
            _lineData[index]._timer -= Time.deltaTime;
        }

        for(int index = 0; index < _circleRequestData.Count; ++index)
        {
            _circleRequestData[index]._timer -= Time.deltaTime;
        }

        for(int index = 0; index < _polygonData.Count; ++index)
        {
            _polygonData[index]._timer -= Time.deltaTime;
        }
    }

    public void drawCircle(Vector3 center, float radius, int accuracy, Color color, float time = 0f)
    {
        GizmoCircleData circleData = new GizmoCircleData();
        circleData._center = center;
        circleData._radius = radius;
        circleData._accuracy = accuracy;
        circleData._color = color;
        circleData._timer = time;

        _circleRequestData.Add(circleData);
    }

    public void drawPolygon(Vector3[] verticies, Color color, float time = 0f)
    {
        if(verticies.Length < 3)
        {
            DebugUtil.assert(false,"is not polygon");
            return;
        }

        GizmoPolygonData polygonData = new GizmoPolygonData();
        polygonData._verticies = verticies;
        polygonData._color = color;
        polygonData._timer = time;

        _polygonData.Add(polygonData);
    }

    public void drawLine(Vector3 start, Vector3 end, Color color, float time = 0f)
    {
        GizmoLineData lineData = new GizmoLineData();
        lineData._start = start;
        lineData._end = end;
        lineData._color = color;
        lineData._timer = time;

        _lineData.Add(lineData);
    }

    private void drawLine()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _lineData.Count;)
        {
            GizmoLineData item = _lineData[index];

            Gizmos.color = item._color;
            Gizmos.DrawLine(item._start, item._end);

            if(item._timer <= 0f)
                _lineData.RemoveAt(index);
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void drawPolygon()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _polygonData.Count;)
        {
            GizmoPolygonData item = _polygonData[index];

            Gizmos.color = item._color;
            for(int i = 0; i < item._verticies.Length - 1; ++i)
            {
                Gizmos.DrawLine(item._verticies[i], item._verticies[i + 1]);
            }

            Gizmos.DrawLine(item._verticies[item._verticies.Length - 1], item._verticies[0]);

            if(item._timer <= 0f)
                _polygonData.RemoveAt(index);
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void drawCircle()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _circleRequestData.Count;)
        {
            GizmoCircleData item = _circleRequestData[index];
            Gizmos.color = item._color;

            float accur = 360f / (float)item._accuracy;
            for(int i = 0; i < 36; ++i)
            {
                float x = Mathf.Cos(10f * i * Mathf.Deg2Rad);
                float y = Mathf.Sin(10f * i * Mathf.Deg2Rad);

                float x2 = Mathf.Cos(10f * (i + 1) * Mathf.Deg2Rad);
                float y2 = Mathf.Sin(10f * (i + 1) * Mathf.Deg2Rad);

                Gizmos.DrawLine(new Vector3(x,y) * item._radius + item._center,new Vector3(x2,y2) * item._radius + item._center);
            }

            if(item._timer <= 0f)
                _circleRequestData.RemoveAt(index);
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void OnDrawGizmos()
    {
        drawCircle();
        drawLine();
        drawPolygon();
    }
}

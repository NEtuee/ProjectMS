using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoHelper : MonoBehaviour
{
    public static GizmoHelper instance;
    struct GizmoCircleData
    {
        public Vector3 _center;
        public float _radius;
        public int _accuracy;

        public Color _color;
        
    };

    struct GizmoPolygonData
    {
        public Vector3[] _verticies;

        public Color _color;
    }

    struct GizmoLineData
    {
        public Vector3 _start;
        public Vector3 _end;

        public Color _color;
    }

    private Stack<GizmoCircleData> _circleRequestData = new Stack<GizmoCircleData>();
    private Stack<GizmoPolygonData> _polygonData = new Stack<GizmoPolygonData>();
    private Stack<GizmoLineData> _lineData = new Stack<GizmoLineData>();

    private void Awake()
    {
        instance = this;
    }

    public void drawCircle(Vector3 center, float radius, int accuracy, Color color)
    {
        GizmoCircleData circleData = new GizmoCircleData();
        circleData._center = center;
        circleData._radius = radius;
        circleData._accuracy = accuracy;
        circleData._color = color;

        _circleRequestData.Push(circleData);
    }

    public void drawPolygon(Vector3[] verticies, Color color)
    {
        if(verticies.Length < 3)
        {
            DebugUtil.assert(false,"is not polygon");
            return;
        }

        GizmoPolygonData polygonData = new GizmoPolygonData();
        polygonData._verticies = verticies;
        polygonData._color = color;

        _polygonData.Push(polygonData);
    }

    public void drawLine(Vector3 start, Vector3 end, Color color)
    {
        GizmoLineData lineData = new GizmoLineData();
        lineData._start = start;
        lineData._end = end;
        lineData._color = color;

        _lineData.Push(lineData);
    }

    private void drawLine()
    {
        Color color = Gizmos.color;
        foreach(var item in _lineData)
        {
            Gizmos.color = item._color;
            Gizmos.DrawLine(item._start, item._end);
        }

        Gizmos.color = color;
        _lineData.Clear();
    }

    private void drawPolygon()
    {
        Color color = Gizmos.color;
        foreach(var item in _polygonData)
        {
            Gizmos.color = item._color;
            for(int i = 0; i < item._verticies.Length - 1; ++i)
            {
                Gizmos.DrawLine(item._verticies[i], item._verticies[i + 1]);
            }

            Gizmos.DrawLine(item._verticies[item._verticies.Length - 1], item._verticies[0]);
        }

        Gizmos.color = color;
        _polygonData.Clear();
    }

    private void drawCircle()
    {
        Color color = Gizmos.color;
        foreach(var item in _circleRequestData)
        {
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
        }

        Gizmos.color = color;
        _circleRequestData.Clear();
    }

    private void OnDrawGizmos()
    {
        drawCircle();
        drawLine();
        drawPolygon();
    }
}

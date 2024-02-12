using UnityEngine;

public class CrosshairUIBinder : UIObjectBinder
{
    public GameObject HeadObject;
    public GameObject SubMarker;

    public float PlayerRadius = 5f;
    public float LimitSpace = 0.9f;
    public GameObject[] DottedLine = new GameObject[4];
    public SpriteRenderer[] CounterDot = new SpriteRenderer[4];

    public SpriteRenderer MainCusor;
    public SpriteRenderer SubCusor;

    public Color IdleColor;
    public Color SubIdleColor;
    public Color DetectColor;
    public Color DetectIdleColor;

    [Range(0f, 1f)]
    public float LerpTime = 1f;
    [Range(0f, 1f)]
    public float ColorLerpTime = 0.5f;
    [Range(0f, 0.5f)]
    public float DetectHighlightTime = 0.1f;

    [Header("Debug")]
    public bool DebugDetect;
    
    public override bool CheckValidLink(out string reason)
    {
        if (HeadObject == null)
        {
            reason = "크로스헤어 스프라이트 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (SubMarker == null)
        {
            reason = "크로스헤어 스프라이트 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (MainCusor == null)
        {
            reason = "크로스헤어 스프라이트 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (SubCusor == null)
        {
            reason = "크로스헤어 스프라이트 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }

        if (DottedLine == null || DottedLine.Length != 4)
        {
            reason = "크로스헤어 절취선 오브젝트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (CounterDot == null || CounterDot.Length != 4)
        {
            reason = "크로스헤어 카운팅 점 오브젝트가 연결되어 있지 않습니다";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}

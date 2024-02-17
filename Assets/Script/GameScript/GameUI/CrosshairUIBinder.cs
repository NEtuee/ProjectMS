using UnityEngine;
using UnityEngine.Serialization;

public class CrosshairUIBinder : UIObjectBinder
{
    public GameObject HeadObject;
    public GameObject SubMarker;
    public GameObject MainBaisc;
    public GameObject MainKick;
    public GameObject MainSuper;
    public GameObject[] SubCursorDashPointObjects;
    public GameObject SubCursorDashPointRoot;
    public GameObject SubCursorDashPointKick;

    public float PlayerRadius = 5f;
    
    public SpriteRenderer MainCursor;

    public Color IdleColor;
    public Color DetectColor;
    public Color DetectIdleColor;

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
        
        if (MainCursor == null)
        {
            reason = "크로스헤어 스프라이트 컴포넌트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (MainBaisc == null)
        {
            reason = "크로스헤어 베이직 오브젝트 가 연결되어 있지 않습니다";
            return false;
        }
        
        if (MainKick == null)
        {
            reason = "크로스헤어 킥 오브젝트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (MainSuper == null)
        {
            reason = "크로스헤어 슈퍼 오브젝트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (SubCursorDashPointRoot == null)
        {
            reason = "크로스헤어 킥 오브젝트가 연결되어 있지 않습니다";
            return false;
        }
        
        if (SubCursorDashPointKick == null)
        {
            reason = "크로스헤어 슈퍼 오브젝트가 연결되어 있지 않습니다";
            return false;
        }

        if (SubCursorDashPointObjects == null || SubCursorDashPointObjects.Length != 4)
        {
            reason = "크로스헤어 서브 커서 대쉬 포인트 오브젝트가 연결되어 있지 않음";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}

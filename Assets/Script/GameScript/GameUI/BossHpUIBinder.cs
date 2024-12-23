using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHpUIBinder : UIObjectBinder
{
    public CanvasGroup CanvasGroup;
    public Transform Root;
    public Transform _decoRoot;
    public Transform _gaugeRoot;
    public Image InGauge;
    public Image OutGauge;
    public LocalizationText _localizationText;
    public Image _portrait;

    public float _inGaugePadding = 0f;

    public override bool CheckValidLink(out string reason)
    {
        if (CanvasGroup == null)
        {
            reason = "보스 hp ui 에 캔버스 그룹이 없음";
            return false;
        }
        
        if (Root == null)
        {
            reason = "보스 hp ui 에 루트 오브젝트가 없음";
            return false;
        }
        
        if (InGauge == null)
        {
            reason = "보스 hp ui 에 인게이지 이미지가 없음";
            return false;
        }
        
        if (OutGauge == null)
        {
            reason = "보스 hp ui 에 아웃게이지 이미지가 없음";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}

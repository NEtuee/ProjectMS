using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtteckRayEffect : MonoBehaviour
{
    public string _targetAttackPreset = "";
    public LineRenderer _targetLine;

    public void setAttackData()
    {
        AttackPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AttackPreset") as AttackPreset;
        AttackPresetData presetData = preset.getPresetData(_targetAttackPreset);
        if(presetData == null)
        {
            DebugUtil.assert(false, "failed to load attack preset: {0}",_targetAttackPreset);
            return;
        }

        _targetLine.startWidth = presetData._attackRayRadius * 2f;
        _targetLine.endWidth = presetData._attackRayRadius * 2f;

        _targetLine.SetPosition(1, new Vector3(presetData._attackRadius,0f,0f));
    }
}

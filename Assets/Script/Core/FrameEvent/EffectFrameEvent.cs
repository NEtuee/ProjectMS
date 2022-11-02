using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class ActionFrameEvent_Effect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Effect;}

    public string _effectPath = "";

    private float _framePerSecond = 1f;

    private float _spawnAngle = 0f;

    private bool _random = false;
    private bool _toTarget = false;

    private Vector3 _spawnOffset = Vector3.zero;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;
        
        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData._effectPath = _effectPath;
        requestData._startFrame = 0f;
        requestData._endFrame = -1f;
        requestData._framePerSecond = _framePerSecond;
        requestData._position = centerPosition 
                + (Quaternion.Euler(0f,0f,Vector3.SignedAngle(Vector3.right, executeEntity.getDirection(), Vector3.forward)) 
                * _spawnOffset);

        requestData._angle = _random ? Random.Range(0f,360f) : _spawnAngle;

        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
                _effectPath = attributes[i].Value;
            // else if(attributes[i].Name == "StartFrame")
            //     _aniStartFrame = float.Parse(attributes[i].Value);
            // else if(attributes[i].Name == "EndFrame")
            //     _aniEndFrame = float.Parse(attributes[i].Value);
            else if(attributes[i].Name == "FramePerSecond")
                _framePerSecond = float.Parse(attributes[i].Value);
            else if(attributes[i].Name == "Offset")
            {
                string[] vector = attributes[i].Value.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert(false, "invalid vector3 data: {0}",attributes[i].Value);
                    return;
                }

                _spawnOffset.x = float.Parse(vector[0]);
                _spawnOffset.y = float.Parse(vector[1]);
                _spawnOffset.z = float.Parse(vector[2]);
            }
            else if(attributes[i].Name == "Angle")
            {
                if(attributes[i].Value == "Random")
                {
                    _random = true;
                }
                else
                {
                    _spawnAngle = float.Parse(attributes[i].Value);
                }
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }

        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential");
    }
}
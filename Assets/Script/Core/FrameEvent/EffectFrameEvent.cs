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
    private Vector2 _randomValue = Vector2.zero;

    private bool _followEntity = false;
    private bool _toTarget = false;

    private Vector3 _spawnOffset = Vector3.zero;

    private bool _usePhysics = false;
    private bool _useFlip = false;
    private PhysicsBodyDescription _physicsBodyDesc = PhysicsBodyDescription._empty;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;
        
        Quaternion directionAngle = Quaternion.Euler(0f,0f,Vector3.SignedAngle(Vector3.right, executeEntity.getDirection(), Vector3.forward));

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData._effectPath = _effectPath;
        requestData._startFrame = 0f;
        requestData._endFrame = -1f;
        requestData._framePerSecond = _framePerSecond;
        requestData._position = centerPosition + (directionAngle  * _spawnOffset);
        requestData._usePhysics = _usePhysics;
        requestData._rotation = directionAngle;

        GameEntityBase requester = (GameEntityBase)executeEntity;
        if(_useFlip && executeEntity is GameEntityBase == true)
        {
            requestData._useFlip = requester.getFlipState().xFlip;
        }
        
        PhysicsBodyDescription physicsBody = _physicsBodyDesc;
        if(_usePhysics)
        {
            float angle = MathEx.directionToAngle(executeEntity.getDirection());
            if(_useFlip && requester.getFlipState().xFlip)
            {
                angle -= 180f;
                angle *= -1f;
            }

            physicsBody._velocity = Quaternion.Euler(0f,0f, angle) * physicsBody._velocity;
        }

        requestData._physicsBodyDesc = physicsBody;

        if(_followEntity == true)
        {
            requestData._angle = executeEntity.getSpriteRendererTransform().rotation.eulerAngles.z;
        }
        else if(_random == true)
        {
            requestData._angle = Random.Range(_randomValue.x,_randomValue.y);
        }
        else
        {
            requestData._angle = _spawnAngle;
        }

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
                if(attributes[i].Value.Contains("Random_"))
                {
                    string data = attributes[i].Value.Replace("Random_","");
                    string[] randomData = data.Split('^');
                    if(randomData == null || randomData.Length != 2)
                    {
                        DebugUtil.assert(false, "invalid float2 data: {0}, {1}",attributes[i].Name, attributes[i].Value);
                        return;
                    }
                    
                    _randomValue = new Vector2(float.Parse(randomData[0]),float.Parse(randomData[1]));
                    _random = true;
                }
                else if(attributes[i].Value == "FollowEntity")
                {
                    _followEntity = true;
                }
                else
                {
                    float angleValue = 0f;
                    if(float.TryParse(attributes[i].Value,out angleValue) == false)
                    {
                        DebugUtil.assert(false, "invalid float data: {0}, {1}",attributes[i].Name, attributes[i].Value);
                        return;
                    }

                    _spawnAngle = angleValue;
                }
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UseFlip")
            {
                _useFlip = bool.Parse(attributes[i].Value);
            }

        }

        if(node.HasChildNodes)
        {
            XmlNodeList childNodes = node.ChildNodes;
            for(int i = 0; i < childNodes.Count; ++i)
            {
                string attrName = childNodes[i].Name;
                string attrValue = childNodes[i].Value;

                if(attrName == "Physics")
                {
                    _usePhysics = true;
                    XmlAttributeCollection physicsAttributes = childNodes[i].Attributes;
                    for(int j = 0; j < physicsAttributes.Count; ++j)
                    {
                        if(physicsAttributes[j].Name == "UseGravity")
                        {
                            _physicsBodyDesc._useGravity = bool.Parse(physicsAttributes[i].Value);
                        }
                        else if(physicsAttributes[j].Name == "Velocity")
                        {
                            string[] floatList = physicsAttributes[j].Value.Split('^');
                            if(floatList == null || floatList.Length != 2)
                            {
                                DebugUtil.assert(false, "invalid float3 data: {0}, {1}",physicsAttributes[j].Name, physicsAttributes[j].Value);
                                return;
                            }

                            _physicsBodyDesc._velocity = new Vector3(float.Parse(floatList[0]),float.Parse(floatList[1]),0f);
                        }
                        else if(physicsAttributes[j].Name == "Friction")
                        {
                            _physicsBodyDesc._friction = float.Parse(physicsAttributes[j].Value);
                        }
                        else if(physicsAttributes[j].Name == "Torque")
                        {
                            _physicsBodyDesc._torque = StringDataUtil.readFloat(physicsAttributes[j].Value);
                        }
                        else if(physicsAttributes[j].Name == "AngularFriction")
                        {
                            _physicsBodyDesc._angularFriction = StringDataUtil.readFloat(physicsAttributes[j].Value);
                        }
                        else
                        {
                            DebugUtil.assert(false,"invalid physics attribute data: {0}", physicsAttributes[j].Name);
                        }
                    }

                }
            }

        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential");
    }
}
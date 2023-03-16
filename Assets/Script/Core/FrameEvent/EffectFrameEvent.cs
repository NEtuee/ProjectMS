using System.Xml;
using System.Collections.Generic;
using UnityEngine;

enum AngleDirectionType
{
    Normal,
    AttackPoint,
}

public class ActionFrameEvent_ProjectilePathEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ProjectilePathEffect;}

    public Material                         _trailMaterial;
    
    public string                           _projectileGraphName = "";
    private ProjectileGraphShotInfoData     _shotInfo;
    private ActionFrameEvent_Projectile.ShotInfoUseType _useType = ActionFrameEvent_Projectile.ShotInfoUseType.UseDefault;
    private DirectionType                   _directionType = DirectionType.Count;
    private SetTargetType                   _setTargetType = SetTargetType.SetTargetType_Self;

    private Vector3[]                       _pathPredictionArray = null;
    private int                             _predictionAccuracy = 0;

    private float                           _lifeTime = 0f;

    private EffectUpdateType                _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public override void initialize()
    {
        base.initialize();
    }

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(ProjectileManager._instance == null)
        {
            DebugUtil.assert(false, "projectile execute order error!!");
            return true;
        }

        if(executeEntity is GameEntityBase == false)
            return true;

        float defaultAngle = ActionFrameEvent_Projectile.getDefaultAngle(executeEntity as GameEntityBase, _directionType);

        ProjectileGraphShotInfoData shotInfo;
        if(ActionFrameEvent_Projectile.getShotInfo(_projectileGraphName,_useType,defaultAngle,ref _shotInfo,out shotInfo) == false)
            return true;

        Vector3 spawnPosition = ActionFrameEvent_Projectile.getSpawnPosition(_setTargetType, executeEntity, targetEntity);
        predictionPath(ref shotInfo, spawnPosition);

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.TrailEffect;
        requestData._lifeTime = _lifeTime;
        requestData._trailWidth = ProjectileManager._instance.getProjectileGraphData(_projectileGraphName)._collisionRadius * 2f;
        requestData._trailMaterial = _trailMaterial;
        requestData._trailPositionData = _pathPredictionArray;

        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    private void predictionPath(ref ProjectileGraphShotInfoData shotInfo, Vector3 position)
    {
        float currentVelocity = shotInfo._deafaultVelocity;
        float currentAngle = shotInfo._defaultAngle;

        float stepDeltaTime = shotInfo._lifeTime / (float)_predictionAccuracy;
        Vector3 evaluatePosition = position;

        int stepIndex = 0;
        _pathPredictionArray[stepIndex] = evaluatePosition;

        while(++stepIndex >= _predictionAccuracy)
        {
            currentVelocity += shotInfo._acceleration * stepDeltaTime;

            if(shotInfo._friction != 0f)
                currentVelocity = MathEx.convergence0(currentVelocity,shotInfo._friction * stepDeltaTime);
            if(shotInfo._angularAcceleration != 0f)
                currentAngle += shotInfo._angularAcceleration * stepDeltaTime;

            evaluatePosition += (currentVelocity * stepDeltaTime) * (UnityEngine.Quaternion.Euler(0f,0f,currentAngle) * UnityEngine.Vector3.right);

            _pathPredictionArray[stepIndex] = evaluatePosition;
        }

    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        _shotInfo.clear();
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;
            if(attrName == "MaterialPath")
            {
                _trailMaterial = ResourceContainerEx.Instance().GetMaterial(attrValue);
            }
            else if(attrName == "Accuracy")
            {
                _predictionAccuracy = int.Parse(attrValue);
                _pathPredictionArray = new Vector3[_predictionAccuracy];
            }
            else if(attrName == "LifeTime")
            {
                _lifeTime = float.Parse(attrValue);
            }
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attrName == "GraphName")
            {
                _projectileGraphName = attrValue;
            }
            if(attrName == "Velocity")
            {
                _shotInfo._deafaultVelocity = float.Parse(attrValue);
            }
            else if(attrName == "Acceleration")
            {
                _shotInfo._acceleration = float.Parse(attrValue);
            }
            else if(attrName == "Friction")
            {
                _shotInfo._friction = float.Parse(attrValue);
            }
            else if(attrName == "Angle")
            {
                if(attrValue.Contains("Random"))
                {
                    string randomData = attrValue.Replace("Random_","");
                    string[] randomValue = randomData.Split('^');
                    if(randomValue == null || randomValue.Length != 2)
                    {
                        DebugUtil.assert(false, "invalid random angle attrubute: {0}", attrValue);
                        continue;
                    }
                    _shotInfo._useRandomAngle = true;
                    _shotInfo._randomAngle = new Vector2(float.Parse(randomValue[0]),float.Parse(randomValue[1]));
                }
                else
                {
                    _shotInfo._defaultAngle = float.Parse(attrValue);
                }
            }
            else if(attrName == "AngularAcceleration")
            {
                _shotInfo._angularAcceleration = float.Parse(attrValue);
            }
            else if(attrName == "LifeTime")
            {
                _shotInfo._lifeTime = float.Parse(attrValue);
            }
            else if(attrName == "DirectionType")
            {
                _directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), attrValue);
            }
            else if(attrName == "ShotInfoUseType")
            {
                _useType = (ActionFrameEvent_Projectile.ShotInfoUseType)System.Enum.Parse(typeof(ActionFrameEvent_Projectile.ShotInfoUseType), attrValue);
            }
            else if(attrName == "SpawnTargetType")
            {
                if(attrValue == "Self")
                    _setTargetType = SetTargetType.SetTargetType_Self;
                else if(attrValue == "Target")
                    _setTargetType = SetTargetType.SetTargetType_Target;
                else if(attrValue == "AITarget")
                    _setTargetType = SetTargetType.SetTargetType_AITarget;
                else
                {
                    DebugUtil.assert(false,"invalid targetType: {0}", attrValue);
                }
            }
        }
        if(_projectileGraphName == "")
            DebugUtil.assert(false, "projectile graph is essential");
    }
}

public class ActionFrameEvent_TimelineEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TimelineEffect;}

    public string               _effectPath = "";
    private bool                _toTarget = false;
    private bool                _attach = false;

    private Vector3             _spawnOffset = Vector3.zero;
    private Quaternion          _effectRotation = Quaternion.identity;
    private EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    private AngleDirectionType  _angleDirectionType = AngleDirectionType.Normal;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + _spawnOffset;
        requestData._rotation = getAngleByType(executeEntity, requestData._position);
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.TimelineEffect;

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }


        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    public Quaternion getAngleByType(ObjectBase executeEntity, Vector3 effectPosition)
    {
        switch(_angleDirectionType)
        {
            case AngleDirectionType.Normal:
                return Quaternion.identity;
            case AngleDirectionType.AttackPoint:
            {
                if(executeEntity is GameEntityBase == false)
                    return Quaternion.identity;

                Vector3 direction = effectPosition - (executeEntity as GameEntityBase).getAttackPoint();
                direction.Normalize();

                return Quaternion.Euler(0f,0f,MathEx.directionToAngle(direction));
            }
        }

        return Quaternion.identity;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
            {
                _effectPath = attributes[i].Value;
            }
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
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "AngleType")
            {
                _angleDirectionType = (AngleDirectionType)System.Enum.Parse(typeof(AngleDirectionType), attributes[i].Value);
            }
        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential");
    }
}

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

    private bool _attach = false;

    private Vector3 _spawnOffset = Vector3.zero;

    private bool _usePhysics = false;
    private bool _useFlip = false;
    private bool _castShadow = false;
    private PhysicsBodyDescription _physicsBodyDesc = PhysicsBodyDescription._empty;
    private EffectUpdateType _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;
        
        Quaternion directionAngle = Quaternion.Euler(0f,0f,Vector3.SignedAngle(Vector3.right, executeEntity.getDirection(), Vector3.forward));

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._startFrame = 0f;
        requestData._endFrame = -1f;
        requestData._framePerSecond = _framePerSecond;
        requestData._position = centerPosition + (directionAngle  * _spawnOffset);
        requestData._usePhysics = _usePhysics;
        requestData._rotation = directionAngle;
        requestData._effectType = EffectType.SpriteEffect;
        requestData._updateType = _effectUpdateType;
        requestData._castShadow = _castShadow;

        if(_attach)
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;

        if(_useFlip && executeEntity is GameEntityBase == true)
        {
            GameEntityBase requester = (GameEntityBase)executeEntity;
            requestData._useFlip = requester.getFlipState().xFlip;
        }
        
        PhysicsBodyDescription physicsBody = _physicsBodyDesc;
        if(_usePhysics)
        {
            GameEntityBase requester = (GameEntityBase)executeEntity;
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
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "CastShadow")
            {
                _castShadow = bool.Parse(attributes[i].Value);
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
                            string[] floatList = physicsAttributes[j].Value.Split(' ');
                            if(floatList == null || floatList.Length != 2)
                            {
                                DebugUtil.assert(false, "invalid float3 data: {0}, {1}",physicsAttributes[j].Name, physicsAttributes[j].Value);
                                return;
                            }
                            
                            _physicsBodyDesc._velocity = new Vector3(StringDataUtil.readFloat(floatList[0]),StringDataUtil.readFloat(floatList[1]),0f);
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
            DebugUtil.assert(false, "effect path is essential [Line: {0}]", XMLScriptConverter.getLineFromXMLNode(node));
    }
}
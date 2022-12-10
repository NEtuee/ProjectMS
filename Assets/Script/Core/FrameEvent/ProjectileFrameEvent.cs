using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class ActionFrameEvent_Projectile : ActionFrameEventBase
{
    public enum ShotInfoUseType
    {
        UseDefault,
        Overlap,
        Add,
    };

    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Projectile;}

    public string                           _projectileGraphName = "";

    private ProjectileGraphShotInfoData     _shotInfo;
    private ShotInfoUseType                 _useType = ShotInfoUseType.UseDefault;
    private DirectionType                   _directionType = DirectionType.Count;

    private SetTargetType                   _setTargetType = SetTargetType.SetTargetType_Self;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(ProjectileManager._instance == null)
        {
            DebugUtil.assert(false, "projectile execute order error!!");
            return true;
        }

        if(executeEntity is GameEntityBase == false)
            return true;

        ProjectileGraphBaseData baseData = ProjectileManager._instance.getProjectileGraphData(_projectileGraphName);
        if(baseData == null)
            return true;


        ProjectileGraphShotInfoData shotInfo = baseData._defaultProjectileShotInfoData;

        switch(_useType)
        {
            case ShotInfoUseType.UseDefault:
            break;
            case ShotInfoUseType.Overlap:
            shotInfo = _shotInfo;
            break;
            case ShotInfoUseType.Add:
            shotInfo += _shotInfo;
            break;
        }

        Vector3 direction = Vector3.zero;
        if(_directionType != DirectionType.Count)
            direction = ((GameEntityBase)executeEntity).getDirectionFromType(_directionType);

        shotInfo._defaultAngle += Quaternion.Euler(0f,0f,Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg).eulerAngles.z;

        Vector3 spawnPosition = Vector3.zero;
        switch(_setTargetType)
        {
            case SetTargetType.SetTargetType_Self:
            spawnPosition = executeEntity.transform.position;
            break;
            case SetTargetType.SetTargetType_Target:
            spawnPosition = targetEntity.transform.position;
            break;
            case SetTargetType.SetTargetType_AITarget:
            GameEntityBase aiTarget = ((GameEntityBase)executeEntity).getCurrentTargetEntity();
            spawnPosition = aiTarget == null ? executeEntity.transform.position : aiTarget.transform.position;
            break;
        }


        ProjectileManager._instance.spawnProjectile(_projectileGraphName,ref shotInfo,spawnPosition,executeEntity._searchIdentifier);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        _shotInfo.clear();

        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "GraphName")
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
                _useType = (ShotInfoUseType)System.Enum.Parse(typeof(ShotInfoUseType), attrValue);
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
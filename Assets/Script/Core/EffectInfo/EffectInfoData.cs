using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectInfoDataBase
{
    public abstract EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity);
    protected Quaternion getAngleByType(ObjectBase executeEntity, Vector3 effectPosition, AngleDirectionType angleDirectionType)
    {
        switch(angleDirectionType)
        {
            case AngleDirectionType.identity:
                return Quaternion.identity;
            case AngleDirectionType.Direction:
                return Quaternion.Euler(0f,0f,MathEx.directionToAngle(executeEntity.getDirection()));
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
}

public class ParticleEffectInfoData : EffectInfoDataBase
{
    public string              _key = "";
    public string              _effectPath = "";
    public bool                _toTarget = false;
    public bool                _attach = false;
    public bool                _followDirection = false;
    public bool                _castShadow = false;
    public float               _lifeTime = 0f;

    public Vector3             _spawnOffset = Vector3.zero;
    public Quaternion          _effectRotation = Quaternion.identity;
    public EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;

    public override EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity)
    {
        Vector3 centerPosition;
        if(_toTarget && targetEntity == null)
        {
            DebugUtil.assert(false,"대상이 존재하는 상황에서만 사용할 수 있는 이펙트 입니다. [EffectInfo: {0}]",_key);
            return null;
        }

        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + _spawnOffset;
        requestData._rotation = getAngleByType(executeEntity, requestData._position, _angleDirectionType);
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.ParticleEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }

        return requestData;
    }
}

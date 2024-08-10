using System.Collections;
using System.Collections.Generic;
using System.IO;
using FMODUnity;
using Unity.Mathematics;
using UnityEngine;

public enum EffectInfoDataType
{
    SpriteEffect,
    ParticleEffect,
}

public class EffectInfoDataList : SerializableDataType
{
    public Dictionary<string,EffectInfoDataBase[]> _effectInfoDataDic = new Dictionary<string, EffectInfoDataBase[]>();
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_effectInfoDataDic.Count);
        foreach(var item in _effectInfoDataDic)
        {
            binaryWriter.Write(item.Key);
            BinaryHelper.writeArray(ref binaryWriter, item.Value);
        }
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        for(int i = 0; i < count; ++i)
        {
            string key = binaryReader.ReadString();
            int dataCount = binaryReader.ReadInt32();

            EffectInfoDataBase[] effectInfoDataList = new EffectInfoDataBase[dataCount];
            for(int j = 0; j < dataCount; ++j)
            {
                effectInfoDataList[j] = EffectInfoDataBase.buildEffectInfoData(ref binaryReader);
            }

            _effectInfoDataDic.Add(key,effectInfoDataList);
        }
    }
}

public abstract class EffectInfoDataBase : SerializableDataType
{
    public string              _key = "";
    public string              _effectPath = "";
    public bool                _toTarget = false;
    public bool                _attach = false;
    public bool                _followDirection = false;
    public bool                _castShadow = false;
    public bool                _dependentAction = false;
    public bool                _followCamera = false;
    public bool                _decal = false;
    public FloatEx             _angleOffset = new FloatEx();
    public float               _lifeTime = 0f;

    public CommonMaterial      _attackMaterial = CommonMaterial.Empty;
    public CommonMaterial      _defenceMaterial = CommonMaterial.Empty;

    public Vector3             _spawnOffset = Vector3.zero;
    public Quaternion          _effectRotation = Quaternion.identity;
    public EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;

    public abstract EffectInfoDataType getEffectInfoDataType();
    
    public abstract EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity);
    
    public virtual bool compareMaterial(CommonMaterial attackMaterial, CommonMaterial defenceMaterial)
    {
        if(_attackMaterial == CommonMaterial.Empty && _defenceMaterial != CommonMaterial.Empty)
            return _defenceMaterial == defenceMaterial;
        else if(_attackMaterial != CommonMaterial.Empty && _defenceMaterial == CommonMaterial.Empty)
            return _attackMaterial == defenceMaterial;
        else if(_attackMaterial == CommonMaterial.Empty && _defenceMaterial == CommonMaterial.Empty)
            return true;
            
        return compareMaterialExactly(attackMaterial, defenceMaterial);
    }

    public virtual bool compareMaterialExactly(CommonMaterial attackMaterial, CommonMaterial defenceMaterial) 
    {
        return _attackMaterial == attackMaterial && _defenceMaterial == defenceMaterial;
    }

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
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        BinaryHelper.writeEnum<EffectInfoDataType>(ref binaryWriter, getEffectInfoDataType());
        binaryWriter.Write(_key);
        binaryWriter.Write(_effectPath);
        binaryWriter.Write(_toTarget);
        binaryWriter.Write(_attach);
        binaryWriter.Write(_followDirection);
        binaryWriter.Write(_castShadow);
        binaryWriter.Write(_dependentAction);
        binaryWriter.Write(_followCamera);
        binaryWriter.Write(_decal);
        _angleOffset.serialize(ref binaryWriter);
        binaryWriter.Write(_lifeTime);
        BinaryHelper.writeEnum<CommonMaterial>(ref binaryWriter, _attackMaterial);
        BinaryHelper.writeEnum<CommonMaterial>(ref binaryWriter, _defenceMaterial);
        BinaryHelper.writeVector3(ref binaryWriter, _spawnOffset);
        BinaryHelper.writeQuaternion(ref binaryWriter, _effectRotation);
        BinaryHelper.writeEnum<EffectUpdateType>(ref binaryWriter, _effectUpdateType);
        BinaryHelper.writeEnum<AngleDirectionType>(ref binaryWriter, _angleDirectionType);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _key = binaryReader.ReadString();
        _effectPath = binaryReader.ReadString();
        _toTarget = binaryReader.ReadBoolean();
        _attach = binaryReader.ReadBoolean();
        _followDirection = binaryReader.ReadBoolean();
        _castShadow = binaryReader.ReadBoolean();
        _dependentAction = binaryReader.ReadBoolean();
        _followCamera = binaryReader.ReadBoolean();
        _decal = binaryReader.ReadBoolean();
        _angleOffset.deserialize(ref binaryReader);
        _lifeTime = binaryReader.ReadSingle();
        _attackMaterial = BinaryHelper.readEnum<CommonMaterial>(ref binaryReader);
        _defenceMaterial = BinaryHelper.readEnum<CommonMaterial>(ref binaryReader);
        _spawnOffset = BinaryHelper.readVector3(ref binaryReader);
        _effectRotation = BinaryHelper.readQuaternion(ref binaryReader);
        _effectUpdateType = BinaryHelper.readEnum<EffectUpdateType>(ref binaryReader);
        _angleDirectionType = BinaryHelper.readEnum<AngleDirectionType>(ref binaryReader);
    }

    public static EffectInfoDataBase buildEffectInfoData(ref BinaryReader binaryReader)
    {
        EffectInfoDataType effectInfoDataType = BinaryHelper.readEnum<EffectInfoDataType>(ref binaryReader);
        EffectInfoDataBase effectInfoDataBase = getEffectInfoDataBase(effectInfoDataType);

        effectInfoDataBase.deserialize(ref binaryReader);

        return effectInfoDataBase;
    }

    public static EffectInfoDataBase getEffectInfoDataBase(EffectInfoDataType effectInfoDataType)
    {
        switch(effectInfoDataType)
        {
            case EffectInfoDataType.SpriteEffect:
                return new SpriteEffectInfoData();
            case EffectInfoDataType.ParticleEffect:
                return new ParticleEffectInfoData();
        }

        return null;
    }
}

public class ParticleEffectInfoData : EffectInfoDataBase
{
    public override EffectInfoDataType getEffectInfoDataType() => EffectInfoDataType.ParticleEffect;

    public override EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity)
    {
        Vector3 centerPosition = Vector3.zero;
        if(_toTarget && targetEntity == null)
        {
            DebugUtil.assert(false,"대상이 존재하는 상황에서만 사용할 수 있는 이펙트 입니다. [EffectInfo: {0}] [ToTarget: {1}]",_key,_toTarget);
            return null;
        }

        if(_toTarget && targetEntity != null)
            centerPosition = targetEntity.transform.position;
        else if(executeEntity != null)
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + requestData._rotation * _spawnOffset;
        requestData._rotation = executeEntity == null ? Quaternion.identity : getAngleByType(executeEntity, requestData._position, _angleDirectionType);
        requestData._rotationOffset = Quaternion.Euler(0f,0f,_angleOffset.getValue());
        requestData._rotation *= requestData._rotationOffset;
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.ParticleEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;
        requestData._dependentAction = _dependentAction;
        requestData._followCamera = _followCamera;
        requestData._decal = _decal;

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }

        return requestData;
    }

}

public class SpriteEffectInfoData : EffectInfoDataBase
{
    public override EffectInfoDataType getEffectInfoDataType() => EffectInfoDataType.SpriteEffect;

    public override EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity)
    {
        Vector3 centerPosition = Vector3.zero;
        if(_toTarget && targetEntity == null)
        {
            DebugUtil.assert(false,"대상이 존재하는 상황에서만 사용할 수 있는 이펙트 입니다. [EffectInfo: {0}] [ToTarget: {1}]",_key,_toTarget);
            return null;
        }

        if(_toTarget && targetEntity != null)
            centerPosition = targetEntity.transform.position;
        else if(executeEntity != null)
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + requestData._rotation * _spawnOffset;
        requestData._rotation = executeEntity == null ? Quaternion.identity : getAngleByType(executeEntity, requestData._position, _angleDirectionType);
        requestData._rotationOffset = Quaternion.Euler(0f,0f,_angleOffset.getValue());
        requestData._rotation *= requestData._rotationOffset;
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.SpriteEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;
        requestData._dependentAction = _dependentAction;
        requestData._followCamera = _followCamera;
        requestData._decal = _decal;
        requestData._animationCustomPreset = ResourceContainerEx.Instance().GetAnimationCustomPreset(_effectPath);

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }

        return requestData;
    }
}

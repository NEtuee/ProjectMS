using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class AnimationPlayDataInfo : SerializableDataType
{
    public AnimationPlayDataInfo(){}

    //따로 때내어야 함
    public ActionFrameEventBase[]       _frameEventData = null;
    public ActionFrameEventBase[]       _timeEventData = null;

    public MultiSelectAnimationData[]   _multiSelectAnimationData = null;

    public AnimationTranslationPresetData _translationPresetData = null;
    public AnimationRotationPresetData  _rotationPresetData = null;
    public AnimationScalePresetData     _scalePresetData = null;
    public AnimationCustomPresetData    _customPresetData = null;
    public AnimationCustomPreset        _customPreset = null;

    public int                          _multiSelectAnimationDataCount = 0;

    public string                       _path = "";
    public float                        _framePerSec = -1f;
    public float                        _actionTime = -1f;
    public float                        _startFrame = -1f;
    public float                        _endFrame = -1f;

    public float                        _duration = -1f;

    public int                          _animationLoopCount = 0;
    public int                          _frameEventDataCount = -1;
    public int                          _timeEventDataCount = -1;
    public int                          _angleBaseAnimationSpriteCount = -1;

    public bool                         _isLoop = false;
    public bool                         _isAngleBaseAnimation = false;
    public bool                         _multiSelectConditionUpdateOnce = false;


    public FlipState                    _flipState;

#if UNITY_EDITOR
    public int                          _lineNumber;

    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_multiSelectAnimationDataCount);
        binaryWriter.Write(_path);
        binaryWriter.Write(_framePerSec);
        binaryWriter.Write(_actionTime);
        binaryWriter.Write(_startFrame);
        binaryWriter.Write(_endFrame);
        binaryWriter.Write(_duration);
        binaryWriter.Write(_animationLoopCount);
        binaryWriter.Write(_frameEventDataCount);
        binaryWriter.Write(_timeEventDataCount);
        binaryWriter.Write(_angleBaseAnimationSpriteCount);
        binaryWriter.Write(_isLoop);
        binaryWriter.Write(_isAngleBaseAnimation);
        binaryWriter.Write(_multiSelectConditionUpdateOnce);
        _flipState.serialize(ref binaryWriter);

        binaryWriter.Write(_customPreset != null);

        if(_frameEventData != null)
        {
            for(int i = 0; i < _frameEventData.Length; ++i)
            {
                _frameEventData[i].serialize(ref binaryWriter);
            }
        }

        if(_timeEventData != null)
        {
            for(int i = 0; i < _timeEventData.Length; ++i)
            {
                _timeEventData[i].serialize(ref binaryWriter);
            }
        }

        if(_multiSelectAnimationData != null)
        {
            for(int i = 0; i < _multiSelectAnimationData.Length; ++i)
            {
                _multiSelectAnimationData[i].serialize(ref binaryWriter);
            }
        }

        binaryWriter.Write(_translationPresetData == null ? "" : _translationPresetData.getName());
        binaryWriter.Write(_rotationPresetData == null ? "" : _rotationPresetData.getName());
        binaryWriter.Write(_scalePresetData == null ? "" : _scalePresetData.getName());
    }
#endif

    public override void deserialize(ref BinaryReader binaryReader)
    {
        _multiSelectAnimationDataCount = binaryReader.ReadInt32();
        _path = binaryReader.ReadString();
        _framePerSec = binaryReader.ReadSingle();
        _actionTime = binaryReader.ReadSingle();
        _startFrame = binaryReader.ReadSingle();
        _endFrame = binaryReader.ReadSingle();
        _duration = binaryReader.ReadSingle();
        _animationLoopCount = binaryReader.ReadInt32();
        _frameEventDataCount = binaryReader.ReadInt32();
        _timeEventDataCount = binaryReader.ReadInt32();
        _angleBaseAnimationSpriteCount = binaryReader.ReadInt32();
        _isLoop = binaryReader.ReadBoolean();
        _isAngleBaseAnimation = binaryReader.ReadBoolean();
        _multiSelectConditionUpdateOnce = binaryReader.ReadBoolean();
        _flipState.deserialize(ref binaryReader);

        bool hasPreset = binaryReader.ReadBoolean();

        if(_frameEventDataCount != -1)
        {
            _frameEventData = new ActionFrameEventBase[_frameEventDataCount];
            for(int i = 0; i < _frameEventDataCount; ++i)
            {
                _frameEventData[i] = ActionFrameEventBase.buildFrameEvent(ref binaryReader);
            }
        }

        if(_timeEventDataCount != -1)
        {
            _timeEventData = new ActionFrameEventBase[_timeEventDataCount];
            for(int i = 0; i < _timeEventDataCount; ++i)
            {
                _timeEventData[i] = ActionFrameEventBase.buildFrameEvent(ref binaryReader);
            }
        }

        if(_multiSelectAnimationDataCount != -1)
        {
            _multiSelectAnimationData = new MultiSelectAnimationData[_multiSelectAnimationDataCount];
            for(int i = 0; i < _multiSelectAnimationDataCount; ++i)
            {
                _multiSelectAnimationData[i] = new MultiSelectAnimationData();
                _multiSelectAnimationData[i].deserialize(ref binaryReader);
            }
        }

        if(hasPreset)
        {
            ScriptableObject[] scriptableObjects = ResourceContainerEx.Instance().GetScriptableObjects(_path);
            AnimationCustomPreset animationCustomPreset = (scriptableObjects[0] as AnimationCustomPreset);
            _customPresetData = animationCustomPreset._animationCustomPresetData;
            _customPreset = animationCustomPreset;

            if(animationCustomPreset._rotationPresetName != "")
            {
                AnimationRotationPreset rotationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationRotationPreset") as AnimationRotationPreset;
                _rotationPresetData = rotationPreset.getPresetData(animationCustomPreset._rotationPresetName);
            }

            if(animationCustomPreset._scalePresetName != "")
            {
                AnimationScalePreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationScalePreset") as AnimationScalePreset;
                _scalePresetData = scalePreset.getPresetData(animationCustomPreset._scalePresetName);
            }

            if(animationCustomPreset._translationPresetName != "")
            {
                AnimationTranslationPreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationTranslationPreset") as AnimationTranslationPreset;
                _translationPresetData = scalePreset.getPresetData(animationCustomPreset._translationPresetName);
            }
        }

        string translationPresetName = binaryReader.ReadString();
        string rotationPresetName = binaryReader.ReadString();
        string scalePresetName = binaryReader.ReadString();
        
        if(rotationPresetName != "")
        {
            AnimationRotationPreset rotationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationRotationPreset") as AnimationRotationPreset;
            _rotationPresetData = rotationPreset.getPresetData(rotationPresetName);
        }

        if(scalePresetName != "")
        {
            AnimationScalePreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationScalePreset") as AnimationScalePreset;
            _scalePresetData = scalePreset.getPresetData(scalePresetName);
        }

        if(translationPresetName != "")
        {
            AnimationTranslationPreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset/AnimationTranslationPreset") as AnimationTranslationPreset;
            _translationPresetData = scalePreset.getPresetData(translationPresetName);
        }
    }
}

public class MultiSelectAnimationData
{
    public string                           _path = "";
    public ActionGraphConditionCompareData  _actionConditionData = null;
#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_path);
        binaryWriter.Write(_actionConditionData != null);
        if(_actionConditionData != null)
            _actionConditionData.serialize(ref binaryWriter);
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        _path = binaryReader.ReadString();
        bool needCondition = binaryReader.ReadBoolean();
        if(needCondition)
        {
            _actionConditionData = new ActionGraphConditionCompareData();
            _actionConditionData.deserialize(ref binaryReader);
        }
    }

}

public struct FlipState
{
    public bool xFlip;
    public bool yFlip;
#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(xFlip);
        binaryWriter.Write(yFlip);
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        xFlip = binaryReader.ReadBoolean();
        yFlip = binaryReader.ReadBoolean();
    }
}

public struct FrameEventProcessDescription
{
    public ActionFrameEventBase     _targetFrameEvent;
    public ObjectBase               _executeObject;
    public float                    _endTime;
    public bool                     _isTimeBase;

    public void processFrameEvent()
    {
        if(_executeObject is GameEntityBase && _targetFrameEvent.checkCondition(_executeObject as GameEntityBase) == false)
            return;

        _targetFrameEvent.onExecute(_executeObject);
    }

    public void exitFrameEvent(bool isForceEnd)
    {
        _targetFrameEvent.onExit(_executeObject,isForceEnd);
    }
}

public class AnimationPlayer
{
    private AnimationTimeProcessor _animationTimeProcessor = new AnimationTimeProcessor();
    private AnimationPlayDataInfo _currentAnimationPlayData;

    private string _currentAnimationName;
    private Sprite[] _currentAnimationSprites;

    private bool _multiSelectAnimationUpdated = false;

    private int _currentAnimationFrameEventIndex;
    private int _currentFrameEventIndex;
    private int _currentTimeEventIndex;

    private System.Action _onAnimationEnd = null;

    private List<FrameEventProcessDescription> _frameEventProcessList = new List<FrameEventProcessDescription>();

    public bool isValid()
    {
        return true;//_currentAnimationPlayData != null;
    }

    public void initialize()
    {
        _animationTimeProcessor.initialize();
        _currentAnimationPlayData = null;
        _currentAnimationSprites = null;
        _currentAnimationName = "";
        _currentAnimationFrameEventIndex = 0;
        _currentFrameEventIndex = 0;
        _currentTimeEventIndex = 0;
        _onAnimationEnd = null;
        _frameEventProcessList.Clear();
    }

    public bool progress(float deltaTime, ObjectBase targetEntity)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"잘못된 플레이 데이터 입니다. 통보 요망");
            return false;
        }

        bool isEnd = _animationTimeProcessor.updateTime(deltaTime);
        if(isEnd == false && _animationTimeProcessor.isLoopedThisFrame())
        {
            _currentFrameEventIndex = 0;
            _currentAnimationFrameEventIndex = 0;
        }
            
        if(targetEntity != null)
        {
            processFrameEventContinue();
            processFrameEvent(_currentAnimationPlayData, targetEntity);
        }
        
        if(_animationTimeProcessor.isEndThisFrame())
        {
            if(_onAnimationEnd != null)
            {
                _onAnimationEnd.Invoke();
                _onAnimationEnd = null;
            }
        }
        
        return _animationTimeProcessor.isEnd();
    } 

    public void setOnAnimationEnd(System.Action onAnimationEnd)
    {
        _onAnimationEnd = onAnimationEnd;
    }

    public void Release()
    {
        
    }

    public bool isEndThisFrame() {return _animationTimeProcessor.isEndThisFrame();}

    public void processMultiSelectAnimation(ActionGraph actionGraph)
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._multiSelectAnimationDataCount == 0)
            return;

        if(_currentAnimationPlayData._multiSelectConditionUpdateOnce && _multiSelectAnimationUpdated)
            return;

        _multiSelectAnimationUpdated = true;
        for(int i = 0; i < _currentAnimationPlayData._multiSelectAnimationDataCount; ++i)
        {
            if(actionGraph.processActionCondition(_currentAnimationPlayData._multiSelectAnimationData[i]._actionConditionData) == true)
            {
                _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(_currentAnimationPlayData._multiSelectAnimationData[i]._path);
                return;
            }
        }

        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(_currentAnimationPlayData._path);
    }
    

    public void processFrameEventContinue()
    {
        for(int i = 0; i < _frameEventProcessList.Count;)
        {
            _frameEventProcessList[i].processFrameEvent();

            if(_frameEventProcessList[i]._endTime <= _animationTimeProcessor.getAnimationTotalPlayTime() || 
               MathEx.equals(_frameEventProcessList[i]._endTime, _animationTimeProcessor.getAnimationTotalPlayTime(), float.Epsilon))
            {
                _frameEventProcessList[i].exitFrameEvent(false);
                _frameEventProcessList.RemoveAt(i);
            }
            else
                ++i;
        }
    }
    
    public void processFrameEvent(AnimationPlayDataInfo playData, ObjectBase targetEntity)
    {
        if(playData == null)
            return;

        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        if(playData._customPresetData != null && playData._customPresetData._effectFrameEvent != null && targetEntity is GameEntityBase)
        {
            for(int i = _currentAnimationFrameEventIndex; i < playData._customPresetData._effectFrameEvent.Length; ++i)
            {
                string effectEvent = playData._customPresetData._effectFrameEvent[i];
                if(MathEx.equals((float)i, currentFrame,float.Epsilon) == true || (float)i < currentFrame)
                {
                    EffectInfoManager.Instance().requestEffect(effectEvent,targetEntity as GameEntityBase, null,CommonMaterial.Empty);
                    _currentAnimationFrameEventIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        for(int i = _currentFrameEventIndex; i < playData._frameEventDataCount; ++i)
        {
            if (playData._frameEventData == null || playData._frameEventData.Length == 0)
                break;

            ActionFrameEventBase frameEvent = playData._frameEventData[i];
            if(MathEx.equals(frameEvent._startFrame, currentFrame,float.Epsilon) == true || frameEvent._startFrame < currentFrame)
            {
                if(targetEntity is GameEntityBase && frameEvent.checkCondition(targetEntity as GameEntityBase) == false)
                {
                    _currentFrameEventIndex++;
                    continue;
                }
                
                frameEvent.initialize(targetEntity);
                if(frameEvent.onExecute(targetEntity) == true && frameEvent._endFrame > frameEvent._startFrame)
                {
                    FrameEventProcessDescription desc;
                    desc._executeObject = targetEntity;
                    desc._endTime = _animationTimeProcessor.frameToTime(frameEvent._endFrame);
                    desc._targetFrameEvent = frameEvent;
                    desc._isTimeBase = false;

                    _frameEventProcessList.Add(desc);
                }
                else
                {
                    frameEvent.onExit(targetEntity, false);
                }

                _currentFrameEventIndex++;
            }
            else
            {
                break;
            }
        }

        float currentTotalTime = _animationTimeProcessor.getAnimationTotalPlayTime();
        for(int i = _currentTimeEventIndex; i < playData._timeEventDataCount; ++i)
        {
            ActionFrameEventBase frameEvent = playData._timeEventData[i];
            if(MathEx.equals(frameEvent._startFrame, currentTotalTime,float.Epsilon) == true || frameEvent._startFrame < currentTotalTime)
            {
                if(targetEntity is GameEntityBase && frameEvent.checkCondition(targetEntity as GameEntityBase) == false)
                {
                    _currentTimeEventIndex++;
                    continue;
                }
                
                frameEvent.initialize(targetEntity);
                if(frameEvent.onExecute(targetEntity) == true && frameEvent._endFrame > frameEvent._startFrame)
                {
                    FrameEventProcessDescription desc;
                    desc._executeObject = targetEntity;
                    desc._endTime = frameEvent._endFrame;
                    desc._targetFrameEvent = frameEvent;
                    desc._isTimeBase = true;

                    _frameEventProcessList.Add(desc);
                }
                else
                {
                    frameEvent.onExit(targetEntity, false);
                }

                _currentTimeEventIndex++;
            }
            else
            {
                break;
            }
        }
    }

    private void setCurrentFrameEventIndex(AnimationPlayDataInfo playData)
    {
        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        for(int i = 0; i < playData._frameEventDataCount; ++i)
        {
            _currentFrameEventIndex = i;
            if(playData._frameEventData[i]._startFrame >= currentFrame)
                break;
        }

        float currentTime = _animationTimeProcessor.getAnimationTotalPlayTime();
        for(int i = 0; i < playData._timeEventDataCount; ++i)
        {
            _currentTimeEventIndex = i;
            if(playData._timeEventData[i]._startFrame >= currentTime)
                break;
        }

        _currentAnimationFrameEventIndex = 0;
    }

    public void changeAnimation(AnimationPlayDataInfo playData)
    {
        _currentAnimationPlayData = playData;
        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(playData._path);

        DebugUtil.assert(_currentAnimationSprites != null, "애니메이션 스프라이트 배열이 null입니다. 해당 경로에 스프라이트가 존재 하나요? : {0}",playData._path);
        _multiSelectAnimationUpdated = false;

        _currentAnimationName = playData._path;

        float startFrame = playData._startFrame;
        startFrame = startFrame == -1f ? 0f : startFrame;

        float endFrame = playData._endFrame;
        endFrame = endFrame == -1f ? (float)_currentAnimationSprites.Length : endFrame;

        float framePerSecond = playData._framePerSec;

        if(framePerSecond == -1f && playData._actionTime != -1f)
            framePerSecond = ((float)(endFrame - startFrame) / playData._actionTime) * (playData._animationLoopCount > 0 ? (float)playData._animationLoopCount : 1.0f);

        if(playData._isAngleBaseAnimation)
        {
            endFrame = playData._angleBaseAnimationSpriteCount;
            endFrame = endFrame == -1f ? 1f : endFrame;
        }
        
        if(framePerSecond == -1f)
        {
            DebugUtil.assert(false, "애니메이션 FPS가 -1 입니다. 코드버그인듯? 통보 요망");
            framePerSecond = 1f;
        }

        _animationTimeProcessor.initialize();
        _animationTimeProcessor.setFrame(startFrame,endFrame, framePerSecond);
        _animationTimeProcessor.setLoop(playData._isLoop);
        _animationTimeProcessor.setLoopCount(playData._animationLoopCount);
        _animationTimeProcessor.setActionDuration(playData._duration);
        _animationTimeProcessor.setFrameToTime(startFrame);
        _animationTimeProcessor.setAnimationSpeed(1f);

        if(playData._customPresetData != null)
        {
            DebugUtil.assert(playData._customPresetData.getTotalDuration() > 0,"CustomPreset의 TotalDuration이 0이거나 음수 입니다. 데이터를 잘못 만든듯? : [Path : {0}]",playData._path);
            _animationTimeProcessor.setCustomPresetData(playData._customPresetData);
        }

        for(int i = 0; i < _frameEventProcessList.Count; ++i)
        {
            _frameEventProcessList[i].exitFrameEvent(true);
        }
        _frameEventProcessList.Clear();

        setCurrentFrameEventIndex(playData);
    }

    public void changeAnimationByCustomPreset(string path)
    {
        ScriptableObject[] scriptableObjects = ResourceContainerEx.Instance().GetScriptableObjects(path);
        if(scriptableObjects == null)
            return;

        if(scriptableObjects == null || (scriptableObjects[0] is AnimationCustomPreset) == false)
            return;

        AnimationCustomPreset animationCustomPreset = (scriptableObjects[0] as AnimationCustomPreset);

        changeAnimationByCustomPreset(path, animationCustomPreset);
    }

    public void changeAnimationByCustomPreset(string path, AnimationCustomPreset customPreset)
    {
        _currentAnimationPlayData = null;
        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(path);
        DebugUtil.assert(_currentAnimationSprites != null, "애니메이션 스프라이트 배열이 null입니다. 해당 경로에 스프라이트가 존재 하나요? : {0}",path);

        _currentAnimationName = path;

        _animationTimeProcessor.initialize();
        //_animationTimeProcessor.setActionDuration(customPreset._animationCustomPresetData.getTotalDuration());
        _animationTimeProcessor.setAnimationSpeed(1f);

        _animationTimeProcessor.setCustomPresetData(customPreset._animationCustomPresetData);

        for(int i = 0; i < _frameEventProcessList.Count; ++i)
        {
            _frameEventProcessList[i].exitFrameEvent(true);
        }
        _frameEventProcessList.Clear();
        _currentFrameEventIndex = -1;
        _currentAnimationFrameEventIndex = 0;
        _currentTimeEventIndex = 0;
        
    }

    public int angleToSectorNumberByAngleBaseSpriteCount(float angleDegree)
    {
        angleDegree = MathEx.clamp360Degree(angleDegree);
        float baseAngle = 360f / (float)_currentAnimationSprites.Length;
        if(angleDegree < baseAngle * 0.5f || angleDegree >= 360f - baseAngle * 0.5f)
            return 0;

        return (int)((angleDegree + baseAngle * 0.5f) / baseAngle);
    }

    public int angleToSectorNumberByCount(float angleDegree, int count)
    {
        angleDegree = MathEx.clamp360Degree(angleDegree);
        float baseAngle = 360f / (float)count;
        if(angleDegree < baseAngle * 0.5f || angleDegree > 360f - baseAngle * 0.5f)
            return 0;

        return (int)((angleDegree + baseAngle * 0.5f) / baseAngle);
    }

    public bool isEnd() {return _animationTimeProcessor.isEnd();}

    public string getCurrentAnimationName() {return _currentAnimationName;}

    public void setAnimationSpeed(float speed) {_animationTimeProcessor.setAnimationSpeed(speed);}

    public float getCurrentFrame() {return _animationTimeProcessor.getCurrentFrame();}
    public float getCurrentAnimationTime() {return _animationTimeProcessor.getCurrentAnimationTime();}
    public float getCurrentAnimationDuration() {return _animationTimeProcessor.getAnimationDuration();}

    public int getCurrentIndex() {return _animationTimeProcessor.getCurrentIndex();}
    public int getEndIndex() {return _animationTimeProcessor.getEndIndex();}

    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc() {return _animationTimeProcessor.getMoveValuePerFrameFromTimeDesc();}
    public AnimationTimeProcessor getTimeProcessor(){return _animationTimeProcessor;}

    public bool getCurrentAnimationTranslation(out Vector3 outTranslation)
    {
        if (_currentAnimationPlayData == null || _currentAnimationPlayData._translationPresetData == null)
        {
            outTranslation = Vector3.zero;
            return false;
        }

        Vector2 currentTranslation = _currentAnimationPlayData._translationPresetData.evaulate(_animationTimeProcessor.getCurrentAnimationNormalizedTime());
        outTranslation = new Vector3(currentTranslation.x, currentTranslation.y, 0f);
        return true;
    }

    public Vector3 getAnimationTranslationPerFrame()
    {
        if (_currentAnimationPlayData == null || _currentAnimationPlayData._translationPresetData == null)
            return Vector3.one;

        Vector2 translation = _currentAnimationPlayData._translationPresetData.getTranslationValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc());
        return new Vector3(translation.x, translation.y, 1f);
    }

    public bool getCurrentAnimationScale(out Vector3 outScale)
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._scalePresetData == null)
        {
            outScale = Vector3.one;
            return false;
        }
        
        Vector2 currentScale = _currentAnimationPlayData._scalePresetData.evaulate(_animationTimeProcessor.getCurrentAnimationNormalizedTime());
        outScale = new Vector3(currentScale.x,currentScale.y,1f);
        return true;
    }

    public Vector3 getAnimationScalePerFrame()
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._scalePresetData == null)
            return Vector3.one;
        

        Vector2 scale = _currentAnimationPlayData._scalePresetData.getScaleValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc());
        return new Vector3(scale.x,scale.y,1f);
    }


    public Quaternion getCurrentAnimationRotation()
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._rotationPresetData == null)
            return Quaternion.identity;

        return Quaternion.Euler(0f,0f,_currentAnimationPlayData._rotationPresetData.evaulate(_animationTimeProcessor.getCurrentAnimationNormalizedTime()));
    }

    public Quaternion getAnimationRotationPerFrame()
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._rotationPresetData == null)
            return Quaternion.identity;
        
        return Quaternion.Euler(0f,0f,_currentAnimationPlayData._rotationPresetData.getRotateValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc()));
    }

    public FlipState getCurrentFlipState() 
    {
        if (_currentAnimationPlayData == null)
            return new FlipState { xFlip = false, yFlip = false };
        return _currentAnimationPlayData._flipState;
    }

    public Sprite getCurrentSprite(float currentAngleDegree = 0f)
    {
        if(_currentAnimationPlayData == null)
            return null;

        if(_currentAnimationSprites.Length <= _animationTimeProcessor.getCurrentIndex())
        {
            DebugUtil.assert(false, "스프라이트 Out Of Index 입니다. AnimationPreset이 잘못되어 있지는 않나요? End Frame을 확인해 주세요. [Length: {0}] [Index: {1}] [Loop: {2}] [Path: {3}]",_currentAnimationSprites.Length, _animationTimeProcessor.getCurrentIndex(), _animationTimeProcessor.isLoop(), _currentAnimationPlayData._path);
            return null;
        }

        if(_currentAnimationPlayData._isAngleBaseAnimation)
        {
            int index = angleToSectorNumberByAngleBaseSpriteCount(currentAngleDegree) + _animationTimeProcessor.getCurrentIndex();
            if(_currentAnimationSprites.Length <= index)
            {
                DebugUtil.assert(false, "잘못된 인덱스 입니다. 통보 요망 : [index : {0}] [angle : {1}] [timerProcessorIndex : {2}] [spriteCount : {3}]", index, currentAngleDegree,angleToSectorNumberByAngleBaseSpriteCount(currentAngleDegree), _animationTimeProcessor.getCurrentIndex() );
                return null;
            }

            return _currentAnimationSprites[angleToSectorNumberByAngleBaseSpriteCount(currentAngleDegree) + _animationTimeProcessor.getCurrentIndex()];
        }

        return _currentAnimationSprites[_animationTimeProcessor.getCurrentIndex()];
    }
}

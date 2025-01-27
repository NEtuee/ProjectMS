using System;
using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using System.IO;

public enum SequencerGraphEventType
{
    SpawnCharacter,
    WaitSecond,
    SetCameraTarget,
    SetCameraUVTarget,
    SetCameraPosition,
    SetAudioListner,
    SetCrossHair,
    SetHPSphere,
    WaitTargetDead,
    TeleportTargetTo,
    ApplyPostProcessProfile,
    SaveEventExecuteIndex,
    CallAIEvent,
    WaitSignal,
    SetCameraZoom,
    ZoomEffect,
    FadeIn,
    FadeOut,
    Fade,
    ForceQuit,
    BlockInput,
    BlockAI,
    SetAction,
    PlayAnimation,
    AIMove,
    QTEFence,
    DeadFence,
    SetHideUI,
    ShakeEffect,
    SetTimeScale,
    NextStage,
    ToastMessage,
    Task,
    LetterBoxShow,
    LetterBoxHide,
    TalkBalloon,
    CameraTrack,
    IsTrackEnd,
    TaskFence,
    SetDirection,
    BlockPointExit,
    EffectPreset,
    UnlockStageLimit,
    ActiveBossHp,
    DisableBossHp,
    SetBackgroundAnimationTrigger,
    SetHideCharacter,
    ApplyBuff,
    SpawnPrefab,
    DeletePrefab,
    AudioPlay,
    AudioParameter,
    StopSwitch,
    SetCameraBoundLock,
    KillEntity,
    KillAllStageEntity,
    ShowCursor,
    AudioBoardEvent,
    EntityCountFence,
    
    Count,
}

public abstract class SequencerGraphEventBase : SerializableDataType
{
    public abstract SequencerGraphEventType getSequencerGraphEventType();
    public abstract void Initialize(SequencerGraphProcessor processor);
    public abstract bool Execute(SequencerGraphProcessor processor, float deltaTime);
    public virtual void Exit(SequencerGraphProcessor processor) {}
    public abstract void loadXml(XmlNode node);
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        BinaryHelper.writeEnum<SequencerGraphEventType>(ref binaryWriter, getSequencerGraphEventType());
    }
#endif
    public static SequencerGraphEventBase buildSequencerGraphEvent(ref BinaryReader binaryReader)
    {
        SequencerGraphEventType eventType = BinaryHelper.readEnum<SequencerGraphEventType>(ref binaryReader);
        SequencerGraphEventBase sequencerGraphEvent = getSequencerGraphEventBase(eventType);

        sequencerGraphEvent.deserialize(ref binaryReader);

        return sequencerGraphEvent;
    }

    public static SequencerGraphEventBase getSequencerGraphEventBase(SequencerGraphEventType eventType)
    {
        SequencerGraphEventBase spawnEvent = null;
        if(eventType == SequencerGraphEventType.SpawnCharacter)
            spawnEvent = new SequencerGraphEvent_SpawnCharacter();
        else if(eventType == SequencerGraphEventType.WaitSecond)
            spawnEvent = new SequencerGraphEvent_WaitSecond(); 
        else if(eventType == SequencerGraphEventType.SetCameraTarget)
            spawnEvent = new SequencerGraphEvent_SetCameraTarget();
        else if(eventType == SequencerGraphEventType.SetCameraUVTarget)
            spawnEvent = new SequencerGraphEvent_SetCameraUVTarget();
        else if(eventType == SequencerGraphEventType.SetCameraPosition)
            spawnEvent = new SequencerGraphEvent_SetCameraPosition();
        else if(eventType == SequencerGraphEventType.SetAudioListner)
            spawnEvent = new SequencerGraphEvent_SetAudioListner();
        else if(eventType == SequencerGraphEventType.SetCrossHair)
            spawnEvent = new SequencerGraphEvent_SetCrossHair();
        else if(eventType == SequencerGraphEventType.SetHPSphere)
            spawnEvent = new SequencerGraphEvent_SetHPSphere();
        else if(eventType == SequencerGraphEventType.WaitTargetDead)
            spawnEvent = new SequencerGraphEvent_WaitTargetDead();
        else if(eventType == SequencerGraphEventType.TeleportTargetTo)
            spawnEvent = new SequencerGraphEvent_TeleportTargetTo();
        else if(eventType == SequencerGraphEventType.ApplyPostProcessProfile)
            spawnEvent = new SequencerGraphEvent_ApplyPostProcessProfile();
        else if(eventType == SequencerGraphEventType.SaveEventExecuteIndex)
            spawnEvent = new SequencerGraphEvent_SaveEventExecuteIndex();
        else if(eventType == SequencerGraphEventType.CallAIEvent)
            spawnEvent = new SequencerGraphEvent_CallAIEvent();
        else if(eventType == SequencerGraphEventType.WaitSignal)
            spawnEvent = new SequencerGraphEvent_WaitSignal();
        else if(eventType == SequencerGraphEventType.SetCameraZoom)
            spawnEvent = new SequencerGraphEvent_SetCameraZoom();
        else if(eventType == SequencerGraphEventType.FadeOut)
            spawnEvent = new SequencerGraphEvent_FadeIn();
        else if(eventType == SequencerGraphEventType.FadeIn)
            spawnEvent = new SequencerGraphEvent_FadeOut();
        else if(eventType == SequencerGraphEventType.Fade)
            spawnEvent = new SequencerGraphEvent_Fade();
        else if(eventType == SequencerGraphEventType.ForceQuit)
            spawnEvent = new SequencerGraphEvent_ForceQuit();
        else if(eventType == SequencerGraphEventType.BlockInput)
            spawnEvent = new SequencerGraphEvent_BlockInput();
        else if(eventType == SequencerGraphEventType.BlockAI)
            spawnEvent = new SequencerGraphEvent_BlockAI();
        else if(eventType == SequencerGraphEventType.SetAction)
            spawnEvent = new SequencerGraphEvent_SetAction();
        else if(eventType == SequencerGraphEventType.PlayAnimation)
            spawnEvent = new SequencerGraphEvent_PlayAnimation();
        else if(eventType == SequencerGraphEventType.AIMove)
            spawnEvent = new SequencerGraphEvent_AIMove();
        else if(eventType == SequencerGraphEventType.QTEFence)
            spawnEvent = new SequencerGraphEvent_QTEFence();
        else if(eventType == SequencerGraphEventType.DeadFence)
            spawnEvent = new SequencerGraphEvent_DeadFence();
        else if(eventType == SequencerGraphEventType.SetHideUI)
            spawnEvent = new SequencerGraphEvent_SetHideUI();
        else if(eventType == SequencerGraphEventType.SetTimeScale)
            spawnEvent = new SequencerGraphEvent_SetTimeScale();
        else if(eventType == SequencerGraphEventType.NextStage)
            spawnEvent = new SequencerGraphEvent_NextStage();
        else if(eventType == SequencerGraphEventType.ShakeEffect)
            spawnEvent = new SequencerGraphEvent_ShakeEffect();
        else if(eventType == SequencerGraphEventType.ZoomEffect)
            spawnEvent = new SequencerGraphEvent_ZoomEffect();
        else if(eventType == SequencerGraphEventType.ToastMessage)
            spawnEvent = new SequencerGraphEvent_ToastMessage();
        else if(eventType == SequencerGraphEventType.Task)
            spawnEvent = new SequencerGraphEvent_Task();
        else if(eventType == SequencerGraphEventType.LetterBoxShow)
            spawnEvent = new SequencerGraphEvent_LetterBoxShow();
        else if(eventType == SequencerGraphEventType.LetterBoxHide)
            spawnEvent = new SequencerGraphEvent_LetterBoxHide();
        else if(eventType == SequencerGraphEventType.TalkBalloon)
            spawnEvent = new SequencerGraphEvent_TalkBalloon();
        else if(eventType == SequencerGraphEventType.CameraTrack)
            spawnEvent = new SequencerGraphEvent_CameraTrack();
        else if(eventType == SequencerGraphEventType.TaskFence)
            spawnEvent = new SequencerGraphEvent_TaskFence();
        else if(eventType == SequencerGraphEventType.SetDirection)
            spawnEvent = new SequencerGraphEvent_SetDirection();
        else if(eventType == SequencerGraphEventType.BlockPointExit)
            spawnEvent = new SequencerGraphEvent_BlockPointExit();
        else if(eventType == SequencerGraphEventType.IsTrackEnd)
            spawnEvent = new SequencerGraphEvent_CameraTrackFence();
        else if(eventType == SequencerGraphEventType.EffectPreset)
            spawnEvent = new SequencerGraphEvent_EffectPreset();
        else if(eventType == SequencerGraphEventType.UnlockStageLimit)
            spawnEvent = new SequencerGraphEvent_UnlockStageLimit();
        else if(eventType == SequencerGraphEventType.ActiveBossHp)
            spawnEvent = new SequencerGraphEvent_ActiveBossHp();
        else if(eventType == SequencerGraphEventType.DisableBossHp)
            spawnEvent = new SequencerGraphEvent_DisableBossHp();
        else if(eventType == SequencerGraphEventType.SetBackgroundAnimationTrigger)
            spawnEvent = new SequencerGraphEvent_SetBackgroundAnimationTrigger();
        else if(eventType == SequencerGraphEventType.SetHideCharacter)
            spawnEvent = new SequencerGraphEvent_SetHideCharacter();
        else if(eventType == SequencerGraphEventType.ApplyBuff)
            spawnEvent = new SequencerGraphEvent_ApplyBuff();
        else if(eventType == SequencerGraphEventType.SpawnPrefab)
            spawnEvent = new SequencerGraphEvent_SpawnPrefab();
        else if(eventType == SequencerGraphEventType.DeletePrefab)
            spawnEvent = new SequencerGraphEvent_DeletePrefab();
        else if(eventType == SequencerGraphEventType.AudioPlay)
            spawnEvent = new SequencerGraphEvent_AudioPlay();
        else if(eventType == SequencerGraphEventType.StopSwitch)
            spawnEvent = new SequencerGraphEvent_StopSwitch();
        else if(eventType == SequencerGraphEventType.AudioParameter)
            spawnEvent = new SequencerGraphEvent_AudioParameter();
        else if(eventType == SequencerGraphEventType.SetCameraBoundLock)
            spawnEvent = new SequencerGraphEvent_SetCameraBoundLock();
        else if(eventType == SequencerGraphEventType.KillEntity)
            spawnEvent = new SequencerGraphEvent_KillEntity();
        else if(eventType == SequencerGraphEventType.KillAllStageEntity)
            spawnEvent = new SequencerGraphEvent_KillAllStageEntity();
        else if(eventType == SequencerGraphEventType.ShowCursor)
            spawnEvent = new SequencerGraphEvent_ShowCursor();
        else if(eventType == SequencerGraphEventType.AudioBoardEvent)
            spawnEvent = new SequencerGraphEvent_AudioBoardEvent();
        else if(eventType == SequencerGraphEventType.EntityCountFence)
            spawnEvent = new SequencerGraphEvent_EntityCountFence();

        return spawnEvent;
    }
}

public class SequencerGraphEvent_EntityCountFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.EntityCountFence;

    public enum CountConditionType
    {
        Less,
        LessEqual,
    }

    public string _uniqueGroupKey = "";
    public int _limitCount = 0;
    public CountConditionType _compareType = CountConditionType.Less;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        var list = processor.getUniqueGroup(_uniqueGroupKey,false);
        if(list == null)
            return true;
        
        int count = list.Count;

        switch(_compareType)
        {
            case CountConditionType.Less:
                return count < _limitCount;
            case CountConditionType.LessEqual:
                return count <= _limitCount;
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Condition")
                _compareType = (CountConditionType)System.Enum.Parse(typeof(CountConditionType), attrValue);
            else if(attrName == "Count")
                _limitCount = Int32.Parse(attrValue);
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        BinaryHelper.writeEnum<CountConditionType>(ref binaryWriter, _compareType);
        binaryWriter.Write(_limitCount);
        binaryWriter.Write(_uniqueGroupKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _compareType = BinaryHelper.readEnum<CountConditionType>(ref binaryReader);
        _limitCount = binaryReader.ReadInt32();
        _uniqueGroupKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_AudioBoardEvent : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.AudioBoardEvent;

    public string _eventName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.runAudioBoardEvent(_eventName);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Event")
                _eventName = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_eventName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _eventName = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_TaskFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TaskFence;

    public string _taskName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return processor.isTaskEnd(_taskName);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "TaskName")
                _taskName = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_taskName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _taskName = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_WaitSignal : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitSignal;

    public string _targetSignal = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return processor.checkSignal(_targetSignal);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Signal")
                _targetSignal = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_targetSignal);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _targetSignal = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_CameraTrackFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.IsTrackEnd;


    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return MasterManager.instance._stageProcessor.isTrackEnd();
    }

    public override void loadXml(XmlNode node)
    {
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_CameraTrack : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.CameraTrack;

    private string _trackName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.startCameraTrack(_trackName);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "TrackName")
                _trackName = attributes[i].Value;
        }

        if(_trackName == "")
            DebugUtil.assert(false,"Track Name이 존재하지 않습니다. 이거 필수임");
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_trackName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _trackName = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SetDirection : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetDirection;

    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";

    private DirectionType _directionType = DirectionType.AlwaysRight;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.setDirection(uniqueEntity.getDirectionFromType(_directionType));
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                item.setDirection(item.getDirectionFromType(_directionType));
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "DirectionType")
                _directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), attributes[i].Value);
            else if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        BinaryHelper.writeEnum<DirectionType>(ref binaryWriter,_directionType);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _directionType = BinaryHelper.readEnum<DirectionType>(ref binaryReader);
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_TalkBalloon : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TalkBalloon;

    private string _key = "";
    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";
    private bool _wait = false;

    private bool _executed = false;
    private bool _endFlag = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
        _executed = false;
        _endFlag = false;
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if (_executed == false)
        {
            var commandList = DialogTextManager.Instance().GetDialogCommand(_key);
            if (commandList == null)
            {
                DebugUtil.assert(false, "존재하지 않는 Dialog 커맨드 입니다. 데이터를 확인 해 주세요. [Key: {0}]", _key);
            }
        
            if (_uniqueKey != string.Empty)
            {
                GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
                if (uniqueEntity == null)
                {
                    return true;
                }
                else
                {
                    if (_wait == false)
                    {
                        GameUI.Instance.TextBubble.PlayCommand(commandList, uniqueEntity, null);
                    }
                    else
                    {
                        GameUI.Instance.TextBubble.PlayCommand(commandList, uniqueEntity, () => _endFlag = true);
                    }
                }
            }
        
            if (_uniqueGroupKey != string.Empty)
            {
                var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
                if (uniqueGroup == null)
                {
                    DebugUtil.assert(false, "대상 Unique Group이 존재하지 않습니다 : {0}", _uniqueGroupKey);
                }
                else
                {
                    foreach (var item in uniqueGroup)
                    {
                        GameUI.Instance.TextBubble.PlayCommand(commandList, item, null);
                    }
                }
            }

            _endFlag = false;
            _executed = true;
        }
        
        if (_wait == false)
        {
            return true;
        }

        if (_endFlag == false)
        {
            return false;
        }
        
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if (attributes[i].Name == "SimpleTalkKey")
            {
                _key = attributes[i].Value;
            }
            else if (attrName == "UniqueKey")
            {
                _uniqueKey = attrValue;
            }
            else if (attrName == "UniqueGroupKey")
            {
                _uniqueGroupKey = attrValue;
            }
            else if (attrName == "Wait")
            {
                _wait = Boolean.Parse(attrValue);
            }
        }

        if (_key == "")
        {
            DebugUtil.assert(false, "Dialog Text Key가 존재하지 않습니다. 이거 필수임");
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_key);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
        binaryWriter.Write(_wait);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _key = binaryReader.ReadString();
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
        _wait = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_LetterBoxHide : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.LetterBoxHide;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        LetterBox._instance.Hide();
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_LetterBoxShow : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.LetterBoxShow;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        LetterBox._instance.Show();
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_Task : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.Task;

    public SequencerGraphEventBase[] _eventList = null;
    public SequencerGraphProcessor.TaskProcessType taskProcessType = SequencerGraphProcessor.TaskProcessType.StepByStep;
    public string _taskName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
        foreach (var item in _eventList)
        {
            item.Initialize(processor);
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(taskProcessType == SequencerGraphProcessor.TaskProcessType.Count)
            return true;
            
        processor.addTask(_eventList,taskProcessType, _taskName);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;

        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "ProcessType")
                taskProcessType = (SequencerGraphProcessor.TaskProcessType)System.Enum.Parse(typeof(SequencerGraphProcessor.TaskProcessType), attributes[i].Value);
            else if(attributes[i].Name == "Name")
                _taskName = attributes[i].Value;
        }

        if(taskProcessType == SequencerGraphProcessor.TaskProcessType.Count)
        {
            DebugUtil.assert(false, "TaskProcessType은 필수입니다");
            return;
        }

        List<SequencerGraphEventBase> eventList = new List<SequencerGraphEventBase>();
        for(int index = 0; index < node.ChildNodes.Count; ++index)
        {
            eventList.Add(SequencrGraphLoader.readEventData(node.ChildNodes[index]));
        }
        _eventList = eventList.ToArray();
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        BinaryHelper.writeArray<SequencerGraphEventBase>(ref binaryWriter, _eventList);
        BinaryHelper.writeEnum<SequencerGraphProcessor.TaskProcessType>(ref binaryWriter, taskProcessType);
        binaryWriter.Write(_taskName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        int count = binaryReader.ReadInt32();
        _eventList = count == 0 ? null : new SequencerGraphEventBase[count];
        for(int i = 0; i < count; ++i)
        {
            _eventList[i] = SequencerGraphEventBase.buildSequencerGraphEvent(ref binaryReader);
        }
        taskProcessType = BinaryHelper.readEnum<SequencerGraphProcessor.TaskProcessType>(ref binaryReader);
        _taskName = binaryReader.ReadString();
    }
}


public class SequencerGraphEvent_ToastMessage : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ToastMessage;

    public string _text = "";
    public float _time = 1f;
    public Color _color = Color.white;


    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ToastMessage._instance.ShowToastMessage(_text,_time,_color);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Text")
                _text = attributes[i].Value;
            else if(attributes[i].Name == "Time")
                _time = float.Parse(attributes[i].Value);
            else if(attributes[i].Name == "Color")
                _color = XMLScriptConverter.valueToLinearColor(attributes[i].Value);
            
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_text);
        binaryWriter.Write(_time);
        BinaryHelper.writeColor(ref binaryWriter, _color);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _text = binaryReader.ReadString();
        _time = binaryReader.ReadSingle();
        _color = BinaryHelper.readColor(ref binaryReader);
    }
}

public class SequencerGraphEvent_NextStage : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.NextStage;

    public string _stageDataPath = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.requestStartStage(_stageDataPath, "");
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
                _stageDataPath = attributes[i].Value;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_stageDataPath);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _stageDataPath = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SetTimeScale : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetTimeScale;

    public float _targetTimeScale = 0f;
    public float _timeScalingTime = 0f;
    public float _timeScaleBlendTime = 0f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        Vector3Data data = MessageDataPooling.GetMessageData<Vector3Data>();
        data.value = new UnityEngine.Vector3(_targetTimeScale, _timeScalingTime, _timeScaleBlendTime);

        Message msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.entity_setTimeScale,0,data,null);
        MasterManager.instance.HandleMessage(msg);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _targetTimeScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Time")
                _timeScalingTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "BlendTime")
                _timeScaleBlendTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_targetTimeScale);
        binaryWriter.Write(_timeScalingTime);
        binaryWriter.Write(_timeScaleBlendTime);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _targetTimeScale = binaryReader.ReadSingle();
        _timeScalingTime = binaryReader.ReadSingle();
        _timeScaleBlendTime = binaryReader.ReadSingle();
    }
}

public class SequencerGraphEvent_ShakeEffect : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ShakeEffect;

    public float _shakeTime = 0f;
    public float _shakeScale = 0f;
    public float _shakeSpeed = 1f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setShake(_shakeScale, _shakeSpeed, _shakeTime);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _shakeScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Time")
                _shakeTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Speed")
                _shakeSpeed = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_shakeTime);
        binaryWriter.Write(_shakeScale);
        binaryWriter.Write(_shakeSpeed);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _shakeTime = binaryReader.ReadSingle();
        _shakeScale = binaryReader.ReadSingle();
        _shakeSpeed = binaryReader.ReadSingle();
    }
}


public class SequencerGraphEvent_SetHideUI : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetHideUI;

    public bool _value = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        HPSphereUIManager.Instance().setActive(_value == false);
        GameUI.Instance.SetActiveCrossHair(_value == false);
        ScreenDirector._instance.setActiveMainHud(_value == false);

        processor.getUniqueEntity("Player")?.setGraphicInterfaceActive(_value == false);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Hide")
                _value = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_value);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _value = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_QTEFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.QTEFence;

    private string _keyName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return ActionKeyInputManager.Instance().keyCheck(_keyName);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "KeyName")
                _keyName = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_keyName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _keyName = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_DeadFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.DeadFence;

    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        bool success = true;
        if(_uniqueKey != "")
        {
            GameEntityBase targetCharacter = processor.getUniqueEntity(_uniqueKey, false);
            success = targetCharacter == null || targetCharacter.isDead();
        }
        
        if(success && _uniqueGroupKey != "")
        {
            var list = processor.getUniqueGroup(_uniqueGroupKey, false);
            if(list != null)
                success = list.Count == 0;
        }
        
        return success;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
    }
}


public class SequencerGraphEvent_CallAIEvent : SequencerGraphEventBase
{
    public enum SequencerCallAIEventTargetType
    {
        UniqueTarget,
        Range,
    };

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.CallAIEvent;

    private string _customAiEventName = "";
    private string _uniqueKey = "";

    private SequencerCallAIEventTargetType _eventTargetType = SequencerCallAIEventTargetType.UniqueTarget;

    private List<CharacterEntityBase> _rangeSearchEntityList = new List<CharacterEntityBase>();
    private AllyTargetType _allyTarget = AllyTargetType.Count;

    private float _range = 0f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ObjectBase executeTargetEntity = processor.getUniqueEntity(_uniqueKey);
        if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
            return true;

        switch(_eventTargetType)
        {
            case SequencerCallAIEventTargetType.UniqueTarget:
            {
                (executeTargetEntity as GameEntityBase).executeCustomAIEvent(_customAiEventName);
            }
            break;
            case SequencerCallAIEventTargetType.Range:
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                _rangeSearchEntityList.Clear();
                sceneCharacterManager.targetSearchRange(executeTargetEntity.transform.position,_range,_allyTarget, executeTargetEntity.getAllyInfo(),ref _rangeSearchEntityList);

                foreach(var item in _rangeSearchEntityList)
                {
                    if(item == null || item is GameEntityBase == false)
                        continue;
                        
                    (item as GameEntityBase).executeCustomAIEvent(_customAiEventName);
                }
            }
            break;
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "EventName")
            {
                _customAiEventName = attributes[i].Value;
            }
            else if(attrName == "UniqueKey")
            {
                _uniqueKey = attributes[i].Value;
            }
            else if(attrName == "EventTargetType")
            {
                _eventTargetType = (SequencerCallAIEventTargetType)System.Enum.Parse(typeof(SequencerCallAIEventTargetType), attrValue);
            }
            else if(attrName == "Range")
            {
                _range = float.Parse(attrValue);
            }
            else if(attrName == "AllyTarget")
            {
                _allyTarget = (AllyTargetType)System.Enum.Parse(typeof(AllyTargetType), attrValue);
            }
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_customAiEventName);
        binaryWriter.Write(_uniqueKey);
        BinaryHelper.writeEnum<SequencerCallAIEventTargetType>(ref binaryWriter,_eventTargetType);
        binaryWriter.Write(_range);
        BinaryHelper.writeEnum<AllyTargetType>(ref binaryWriter,_allyTarget);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _customAiEventName = binaryReader.ReadString();
        _uniqueKey = binaryReader.ReadString();
        _eventTargetType = BinaryHelper.readEnum<SequencerCallAIEventTargetType>(ref binaryReader);
        _range = binaryReader.ReadSingle();
        _allyTarget = BinaryHelper.readEnum<AllyTargetType>(ref binaryReader);
    }
}

public class SequencerGraphEvent_FadeIn : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.FadeOut;

    private float _lambda = -1f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFadeIn(_lambda);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Lambda")
                _lambda = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_lambda);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _lambda = binaryReader.ReadSingle();
    }
}

public class SequencerGraphEvent_AIMove : SequencerGraphEventBase
{
    private enum AnimationState
    {
        Start,
        Loop,
        End
    };

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.AIMove;

    private string _uniqueKey = "";

    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private Vector3 _endAnimationStartPosition;

    private AnimationState _currentAnimationState = AnimationState.Start;

    private string _startAction = "";
    private string _loopAction = "";
    private string _endAction = "";

    private string _markerName = "";

    private int _startActionIndex = -1;
    private int _loopActionIndex = -1;
    private int _endActionIndex = -1;

    private float _totalAnimationPlayTime = 0f;

    private float _totalLoopAnimationPlayTime = 0f;

    private float _startAnimationPlayTime = 0f;
    private float _endAnimationPlayTime = 0f;
    private float _loopAnimationPlayTime = 0f;


    private float _processTimer = 0f;
    private bool _loopActionOnly = false;

    private bool _aiBlockState = false;

    private float _startActionDistance = 0f;
    private float _loopActionDistance = 0f;
    private float _endActionDistance = 0f;

    private float _totalLoopActionDistance = 0f;

    private bool _firstUpdate = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return;
        }

        if(_markerName != "")
        {
            if(_markerName == "Spawn")
            {
                _endPosition = uniqueEntity.getSpawnPosition();
            }
            else
            {
                MarkerItem item = processor.getMarker(_markerName);
                if(item != null)
                {
                    _endPosition = item._position + processor.getOwnerProcessor().getOffsetPosition();
                }
                else
                {
                    DebugUtil.assert(false,"대상 Marker가 존재하지 않습니다 : {0}",_markerName);
                }
            }
        }

        _aiBlockState = uniqueEntity.isAIBlocked();

        _totalAnimationPlayTime = 0f;
        _totalLoopAnimationPlayTime = 0f;

        _startActionDistance = 0f;
        _endActionDistance = 0f;
        _loopActionDistance = 0f;

        if(_startAction != "")
        {
            _startActionIndex = uniqueEntity.getActionIndex(_startAction);
            MovementGraphPresetData presetData = uniqueEntity.getMovementGraphPresetDataFromActionIndex(_startActionIndex);
            if(presetData == null)
            {
                DebugUtil.assert(false,"해당 액션에 MovementGraphPreset 설정이 안되어 있습니다. 확인 필요 [Action: {0}]", _startAction);
                return;
            }

            float moveScale = uniqueEntity.getMoveScaleFromActionIndex(_startActionIndex);

            _startActionDistance = presetData.getTotalMovement() * moveScale;
            _startAnimationPlayTime = uniqueEntity.getAnimationPlayTimeFromActionIndex(_startActionIndex) * (1f / moveScale);
            _totalAnimationPlayTime += _startAnimationPlayTime;
        }
        if(_endAction != "")
        {
            _endActionIndex = uniqueEntity.getActionIndex(_endAction);
            MovementGraphPresetData presetData = uniqueEntity.getMovementGraphPresetDataFromActionIndex(_endActionIndex);
            if(presetData == null)
            {
                DebugUtil.assert(false,"해당 액션에 MovementGraphPreset 설정이 안되어 있습니다. 확인 필요 [Action: {0}]", _endAction);
                return;
            }

            float moveScale = uniqueEntity.getMoveScaleFromActionIndex(_endActionIndex);

            _endActionDistance = presetData.getTotalMovement() * moveScale;
            _endAnimationPlayTime = uniqueEntity.getAnimationPlayTimeFromActionIndex(_endActionIndex) * (1f / moveScale);
            _totalAnimationPlayTime += _endAnimationPlayTime;
        }
        
        if(_startAction == "" && _endAction == "")
            _loopActionOnly = true;

        {
            _loopActionIndex = uniqueEntity.getActionIndex(_loopAction);
            MovementGraphPresetData presetData = uniqueEntity.getMovementGraphPresetDataFromActionIndex(_loopActionIndex);
            if(presetData == null)
            {
                DebugUtil.assert(false,"해당 액션에 MovementGraphPreset 설정이 안되어 있습니다. 확인 필요 [Action: {0}]", _loopAction);
                return;
            }

            float moveScale = uniqueEntity.getMoveScaleFromActionIndex(_loopActionIndex);

            _loopActionDistance = presetData.getTotalMovement() * moveScale;
            _loopAnimationPlayTime = uniqueEntity.getAnimationPlayTimeFromActionIndex(_loopActionIndex);
        }

        _firstUpdate = true;
        _processTimer = 0f;
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_loopAction == "")
            return true;

        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        if(_firstUpdate)
        {
            float startEndDistance = _startActionDistance + _endActionDistance;
            _startPosition = uniqueEntity.transform.position;

            float moveDistance = Vector3.Distance(_startPosition, _endPosition);
            if(_loopActionOnly || moveDistance < startEndDistance)
            {
                _currentAnimationState = AnimationState.Loop;

                float rate = moveDistance * (1f / _loopActionDistance);
                _totalLoopActionDistance = _loopActionDistance * rate;
                _totalLoopAnimationPlayTime = _loopAnimationPlayTime * rate;

                _totalAnimationPlayTime = 0f;
                _loopActionOnly = true;

                if(rate < 1f)
                {
                    _totalLoopAnimationPlayTime = _loopAnimationPlayTime;
                    uniqueEntity.setAdditionalMoveScale(rate);
                }
            }
            else
            {
                _currentAnimationState = AnimationState.Start;
                float rate = (moveDistance - startEndDistance) * (1f / _loopActionDistance);
                _totalLoopActionDistance = _loopActionDistance * rate;
                _totalLoopAnimationPlayTime = _loopAnimationPlayTime * rate;
            }

            _totalAnimationPlayTime += _totalLoopAnimationPlayTime;
            Vector3 direction = (_endPosition - _startPosition).normalized;
            uniqueEntity.blockAI(true);
            uniqueEntity.setDirection(direction);
            uniqueEntity.setAiDirection(direction);
            uniqueEntity.setDirectionType(DirectionType.AI);

            if(_loopActionOnly)
                uniqueEntity.setAction(_loopActionIndex);
            else
                uniqueEntity.setAction(_startActionIndex);
            
            _endAnimationStartPosition = _startPosition + direction * (_startActionDistance + _totalLoopActionDistance);

            _firstUpdate = false;
            return false;
        }

        _processTimer += deltaTime;
        
        switch(_currentAnimationState)
        {
            case AnimationState.Start:
            {
                if(_processTimer >= _startAnimationPlayTime)
                {
                    _processTimer = _startAnimationPlayTime;
                    uniqueEntity.setAction(_loopActionIndex);
                    _currentAnimationState = AnimationState.Loop;
                }
            }
            break;
            case AnimationState.Loop:
            {
                if(_loopActionOnly == false && _processTimer >= _startAnimationPlayTime + _totalLoopAnimationPlayTime)
                {
                    _processTimer = _startAnimationPlayTime + _totalLoopAnimationPlayTime;
                    uniqueEntity.setAction(_endActionIndex);
                    uniqueEntity.transform.position = _endAnimationStartPosition;
                    _currentAnimationState = AnimationState.End;
                }
            }
            break;
        }

        GizmoHelper.instance.drawCircle(_startPosition,0.2f,16,Color.green);
        GizmoHelper.instance.drawCircle(_endPosition,0.2f,16,Color.green);
        GizmoHelper.instance.drawLine(_startPosition,_endPosition,Color.green);
        
        if(_processTimer >= _totalAnimationPlayTime)
        {
            uniqueEntity.transform.position = _endPosition;
            uniqueEntity.resetAdditionalMoveScale();
            uniqueEntity.blockAI(_aiBlockState);
            uniqueEntity.setDefaultAction();
            return true;
        }

        return false;
    }

    public override void Exit(SequencerGraphProcessor processor)
    {
        base.Exit(processor);

        if(_processTimer < _totalAnimationPlayTime)
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if (uniqueEntity == null)
                return;

            uniqueEntity.resetAdditionalMoveScale();
            uniqueEntity.blockAI(_aiBlockState);
            uniqueEntity.setDefaultAction();
        }
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "StartAction")
                _startAction = attrValue;
            else if(attrName == "LoopAction")
                _loopAction = attrValue;
            else if(attrName == "EndAction")
                _endAction = attrValue;
            else if(attrName == "EndPosition")
                _endPosition = XMLScriptConverter.valueToVector3(attrValue);
            else if(attrName == "EndPositionMarker")
                _markerName = attrValue;
        }

        DebugUtil.assert(_loopAction != "", "Loop Action은 필수입니다. [Line: {0}]", XMLScriptConverter.getLineNumberFromXMLNode(node));
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_startAction);
        binaryWriter.Write(_loopAction);
        binaryWriter.Write(_endAction);
        BinaryHelper.writeVector3(ref binaryWriter, _endPosition);
        binaryWriter.Write(_markerName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _startAction = binaryReader.ReadString();
        _loopAction = binaryReader.ReadString();
        _endAction = binaryReader.ReadString();
        _endPosition = BinaryHelper.readVector3(ref binaryReader);
        _markerName = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_PlayAnimation : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.PlayAnimation;

    private string _animationPath;
    private string _uniqueKey;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        uniqueEntity.blockAI(true);
        uniqueEntity.setDummyAction();
        uniqueEntity.changeAnimationByPath(_animationPath);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "Path")
                _animationPath = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_animationPath);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _animationPath = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_UnlockStageLimit : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.UnlockStageLimit;

    private bool _enable = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.unlockLimit(_enable);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Enable")
                _enable = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_enable);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _enable = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_EffectPreset : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.EffectPreset;

    private string _effectPresetName = "";
    private string _uniqueKey = "";
    private string _markerName = "";
    private bool _isSwitch = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            if(_isSwitch)
            {
                EffectManager._instance.playEffectSwitch(uniqueEntity,null,_effectPresetName);
            }
            else
            {
                EffectInfoManager.Instance().requestEffect(_effectPresetName,uniqueEntity, null,CommonMaterial.Empty);
            }

        }
        else if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
            {
                if(_isSwitch)
                    DebugUtil.assert(false, "실행 주체가 없기 때문에 Switch로 실행 불가");

                EffectInfoManager.Instance().requestEffect(_effectPresetName,item._position + processor.getOwnerProcessor().getOffsetPosition());
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "EffectPreset")
                _effectPresetName = attrValue;
            else if(attrName == "PositionMarker")
                _markerName = attrValue;
            else if(attrName == "Switch")
                _isSwitch = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_effectPresetName);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_markerName);
        binaryWriter.Write(_isSwitch);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _effectPresetName = binaryReader.ReadString();
        _uniqueKey = binaryReader.ReadString();
        _markerName = binaryReader.ReadString();
        _isSwitch = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_SetAction : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetAction;

    private string _actionName;
    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.setAction(_actionName);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                item.setAction(_actionName);
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
            else if(attrName == "Action")
                _actionName = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
        binaryWriter.Write(_actionName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
        _actionName = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_BlockPointExit : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.BlockPointExit;

    public bool _value = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.blockPointExit(_value);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Enable")
                _value = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_value);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _value = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_BlockAI : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.BlockAI;

    public string _uniqueKey = "";
    public string _uniqueGroupKey = "";
    public bool _value = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_uniqueKey != "")
        {
            ObjectBase executeTargetEntity = processor.getUniqueEntity(_uniqueKey);
            if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
                return true;

            (executeTargetEntity as GameEntityBase).blockAI(_value);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
                return true;

            foreach(var item in uniqueGroup)
            {
                item.blockAI(_value);
            }
        }
        

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Enable")
                _value = bool.Parse(attrValue);
            else if(attrName == "UniqueKey")
                _uniqueKey = attributes[i].Value;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attributes[i].Value;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_value);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _value = binaryReader.ReadBoolean();
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_BlockInput : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.BlockInput;

    private bool _value = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase playerEntity = processor.getUniqueEntity("Player");
        if(playerEntity == null)
        {
            DebugUtil.assert(false,"플레이어가 존재하지 않습니다.");
            return true;
        }

        playerEntity.blockInput(_value);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Enable")
                _value = bool.Parse(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_value);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _value = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_ForceQuit : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ForceQuit;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return true;
    }

    public override void loadXml(XmlNode node)
    {

    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_Fade : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.Fade;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFade();
        return true;
    }

    public override void loadXml(XmlNode node)
    {

    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_FadeOut : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.FadeIn;

    private float _lambda = -1f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFadeOut(_lambda);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Lambda")
                _lambda = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_lambda);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _lambda = binaryReader.ReadSingle();
    }
}

public class SequencerGraphEvent_ZoomEffect : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ZoomEffect;

    private FloatEx _zoom = new FloatEx();

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().Zoom(_zoom);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Factor")
                _zoom.loadFromXML(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        _zoom.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _zoom.deserialize(ref binaryReader);
    }
}

public class SequencerGraphEvent_SetCameraZoom : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraZoom;

    private FloatEx _zoom = new FloatEx();
    private FloatEx _zoomSpeed = new FloatEx();
    private bool _force = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_zoom.getValue() <= 0f)
            CameraControlEx.Instance().setDefaultZoomSize();
        else
        {
            if(_force)
                CameraControlEx.Instance().setZoomSizeForce(_zoom,_zoomSpeed);
            else 
                CameraControlEx.Instance().setZoomSize(_zoom,_zoomSpeed);
        }
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Size")
                _zoom.loadFromXML(attributes[i].Value);
            else if(attributes[i].Name == "Speed")
                _zoomSpeed.loadFromXML(attributes[i].Value);
            else if(attributes[i].Name == "Force")
                _force = bool.Parse(attributes[i].Value);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        _zoom.serialize(ref binaryWriter);
        _zoomSpeed.serialize(ref binaryWriter);
        binaryWriter.Write(_force);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _zoom.deserialize(ref binaryReader);
        _zoomSpeed.deserialize(ref binaryReader);
        _force = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_SpawnCharacter : SequencerGraphEventBase
{
    private string                      _characterKey = "";

    private CharacterInfoData           _characterInfoData = null;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    private string                      _uniqueEntityKey = "";
    private string                      _uniqueGroupKey = "";
    private string                      _centerMarkerName = "";
    private string                      _centerUniqueEntityKey = "";
    private string                      _allyInfoKey = "";
    private string                      _startAIState = "";
    private Vector3                     _offset = Vector3.zero;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SpawnCharacter;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        _spawnDesc._allyInfo = AllyInfoManager.Instance().GetAllyInfoData(_allyInfoKey);
        _characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_characterKey);
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_centerMarkerName != "")
        {
            MarkerItem item = processor.getMarker(_centerMarkerName);
            if(item != null)
                _spawnDesc._position = item._position + processor.getOwnerProcessor().getOffsetPosition();
        }
        else if(_centerUniqueEntityKey != "")
        {
            var uniqueEntity = processor.getUniqueEntity(_centerUniqueEntityKey);
            if(uniqueEntity != null)
                _spawnDesc._position = uniqueEntity.transform.position;
        }
        else if(MasterManager.instance._stageProcessor.isValid())
        {
            _spawnDesc._position = MasterManager.instance._stageProcessor.getCurrentPointPosition();
        }

        _spawnDesc._position += _offset;

        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
        CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(_characterInfoData,_spawnDesc);

        if(createdCharacter == null)
            return true;

        if(_uniqueEntityKey != "")
            processor.addUniqueEntity(_uniqueEntityKey, createdCharacter);
        
        if(_uniqueGroupKey != "")
            processor.addUniqueGroupEntity(_uniqueGroupKey, createdCharacter);
        
        if(_startAIState != "")
            createdCharacter.setAINode(_startAIState);
        
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "CharacterKey")
            {
                _characterKey = attrValue;
            }
            else if(attrName == "Position")
            {
                _offset = XMLScriptConverter.valueToVector3(attrValue);
            }
            else if(attrName == "PositionMarker")
            {
                _centerMarkerName = attrValue;
            }
            else if(attrName == "PositionUniqueKey")
            {
                _centerUniqueEntityKey = attrValue;
            }
            else if(attrName == "AllyInfo")
            {
                _allyInfoKey = attrValue;
            }
            else if(attrName == "UniqueKey")
            {
                _uniqueEntityKey = attrValue;
            }
            else if(attrName == "UniqueGroupKey")
            {
                _uniqueGroupKey = attrValue;
            }
            else if(attrName == "StartAIState")
            {
                _startAIState = attrValue;
            }

        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_characterKey);
        BinaryHelper.writeVector3(ref binaryWriter, _offset);
        binaryWriter.Write(_centerMarkerName);
        binaryWriter.Write(_centerUniqueEntityKey);
        binaryWriter.Write(_allyInfoKey);
        binaryWriter.Write(_uniqueEntityKey);
        binaryWriter.Write(_uniqueGroupKey);
        binaryWriter.Write(_startAIState);

    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _characterKey = binaryReader.ReadString();
        _offset = BinaryHelper.readVector3(ref binaryReader);
        _centerMarkerName = binaryReader.ReadString();
        _centerUniqueEntityKey = binaryReader.ReadString();
        _allyInfoKey = binaryReader.ReadString();
        _uniqueEntityKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
        _startAIState = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_WaitSecond : SequencerGraphEventBase
{
    private float   _waitTime = 0f;
    private float   _timer = 0f;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitSecond;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        _timer = 0f;
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        _timer += deltaTime;
        return _waitTime <= _timer;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Time")
                _waitTime = XMLScriptConverter.valueToFloatExtend(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_waitTime);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _waitTime = binaryReader.ReadSingle();
    }
}

public class SequencerGraphEvent_SetHPSphere : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetHPSphere;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        HPSphereUIManager.Instance().release();
        HPSphereUIManager.Instance().initialize(uniqueEntity);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SetCrossHair : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCrossHair;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false, "대상 Unique Entity가 존재하지 않습니다 : {0}", _uniqueKey);
            return true;
        }
        
        GameUI.Instance.SetEntity(uniqueEntity);
        GameUI.Instance.SetActiveCrossHair(true);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_ActiveBossHp : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private string _key = "";
    private string _spritePath = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ActiveBossHp;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false, "대상 Unique Entity가 존재하지 않습니다 : {0}", _uniqueKey);
            return true;
        }
        
        Sprite sprite = _spritePath == "" ? null : ResourceContainerEx.Instance().GetSprite(_spritePath);

        GameUI.Instance.SetBossHpEntity(uniqueEntity,_key,sprite);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "NameKey")
                _key = attrValue;
            else if(attrName == "PortraitPath")
                _spritePath = attrValue;
                
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_key);
        binaryWriter.Write(_spritePath);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _key = binaryReader.ReadString();
        _spritePath = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_StopSwitch : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.StopSwitch;
   
    public enum StopSwitchType
    {
        None,
        Audio,
        Effect
    }

    public int _audioKey = 0;
    public string _effectKey = "";
    public string _uniqueKey = "Player";
    public StopSwitchType _stopSwitchType = StopSwitchType.None;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        switch(_stopSwitchType)
        {
            case StopSwitchType.Audio:
                FMODAudioManager.Instance().stopSwitch(processor.getUniqueEntity(_uniqueKey), _audioKey);
            break;
            case StopSwitchType.Effect:
                EffectManager._instance.stopEffectswitch(processor.getUniqueEntity(_uniqueKey), _effectKey);
            break;
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Switch")
            {
                _stopSwitchType = (StopSwitchType)System.Enum.Parse(typeof(StopSwitchType), attrValue);
            }
            else if(attrName == "Key")
            {
                switch(_stopSwitchType)
                {
                    case StopSwitchType.Audio:
                        _audioKey = int.Parse(attrValue);
                    break;
                    case StopSwitchType.Effect:
                        _effectKey = attrValue;
                    break;
                    default:
                        DebugUtil.assert_fileOpen(false, "Switch Type이 먼저 결정되어야 합니다.", node.BaseURI, XMLScriptConverter.getLineNumberFromXMLNode(node));
                    return;
                }
            }
            else if(attrName == "UniqueKey")
            {
                _uniqueKey = attrValue;
            }
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_audioKey);
        binaryWriter.Write(_effectKey);
        binaryWriter.Write(_uniqueKey);
        BinaryHelper.writeEnum<StopSwitchType>(ref binaryWriter,_stopSwitchType);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _audioKey = binaryReader.ReadInt32();
        _effectKey = binaryReader.ReadString();
        _uniqueKey = binaryReader.ReadString();
        _stopSwitchType = BinaryHelper.readEnum<StopSwitchType>(ref binaryReader);
    }
}

public class SequencerGraphEvent_AudioParameter : SequencerGraphEventBase
{
    enum ParameterType
    {
        Global,
        Local,
        Count
    }

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.AudioParameter;

    ParameterType _parameterType = ParameterType.Global;

    int _key = 0;

    public int[] _parameterID = null;
    public float[] _parameterValue = null;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        if(isValidParameter() == false)
            return true;

        for(int i = 0; i < _parameterID.Length; ++i)
        {
            switch(_parameterType)
            {
                case ParameterType.Global:
                    FMODAudioManager.Instance().SetGlobalParam(_parameterID[i],_parameterValue[i]);
                break;
                case ParameterType.Local:
                    FMODAudioManager.Instance().SetParam(_key,_parameterID[i],_parameterValue[i]);
                break;
            }
            
        }

        return true;
    }

    public bool isValidParameter()
    {
        return _parameterID != null && _parameterValue != null && _parameterID.Length == _parameterValue.Length;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;

        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "ParameterID")
            {
                string[] paramIDArray = attributes[i].Value.Split(' ');
                List<int> idList = new List<int>();
                foreach(var item in paramIDArray)
                {
                    idList.Add(int.Parse(item));
                }

                _parameterID = idList.ToArray();
            }
            else if(attributes[i].Name == "ParameterValue")
            {
                string[] paramValueArray = attributes[i].Value.Split(' ');
                List<float> valueList = new List<float>();
                foreach(var item in paramValueArray)
                {
                    valueList.Add(float.Parse(item));
                }

                _parameterValue = valueList.ToArray();
            }
            else if(attributes[i].Name == "ParameterType")
            {
                _parameterType = (ParameterType)System.Enum.Parse(typeof(ParameterType), attrValue);
            }
            else if(attributes[i].Name == "Key")
            {
                _key = int.Parse(attrValue);
            }
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        BinaryHelper.writeEnum<ParameterType>(ref binaryWriter,_parameterType);
        binaryWriter.Write(_key);
        BinaryHelper.writeArray(ref binaryWriter, _parameterID);
        BinaryHelper.writeArray(ref binaryWriter, _parameterValue);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _parameterType = BinaryHelper.readEnum<ParameterType>(ref binaryReader);
        _key = binaryReader.ReadInt32();
        _parameterID = BinaryHelper.readArrayInt(ref binaryReader);
        _parameterValue = BinaryHelper.readArrayFloat(ref binaryReader);
    }
}

public class SequencerGraphEvent_AudioPlay : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.AudioPlay;

    public int _audioID = 0;
    public bool _isSwitch = false;

    public int[] _parameterID = null;
    public float[] _parameterValue = null;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        FMODUnity.StudioEventEmitter eventEmitter = null;

        if(_isSwitch)
            eventEmitter = FMODAudioManager.Instance().playSwitch(processor.getUniqueEntity("Player"), _audioID, Vector3.zero, null);
        else
            eventEmitter = FMODAudioManager.Instance().Play(_audioID, Vector3.zero);

        if(eventEmitter != null && isValidParameter())
            FMODAudioManager.Instance().setParam(ref eventEmitter,_audioID,_parameterID,_parameterValue);

        return true;
    }

    public bool isValidParameter()
    {
        return _parameterID != null && _parameterValue != null && _parameterID.Length == _parameterValue.Length;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;

        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "ID")
            {
                _audioID = int.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "ParameterID")
            {
                string[] paramIDArray = attributes[i].Value.Split(' ');
                List<int> idList = new List<int>();
                foreach(var item in paramIDArray)
                {
                    idList.Add(int.Parse(item));
                }

                _parameterID = idList.ToArray();
            }
            else if(attributes[i].Name == "ParameterValue")
            {
                string[] paramValueArray = attributes[i].Value.Split(' ');
                List<float> valueList = new List<float>();
                foreach(var item in paramValueArray)
                {
                    valueList.Add(float.Parse(item));
                }

                _parameterValue = valueList.ToArray();
            }
            else if(attributes[i].Name == "Switch")
            {
                _isSwitch = bool.Parse(attributes[i].Value);
            }
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        binaryWriter.Write(_audioID);
        binaryWriter.Write(_isSwitch);
        BinaryHelper.writeArray(ref binaryWriter, _parameterID);
        BinaryHelper.writeArray(ref binaryWriter, _parameterValue);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _audioID = binaryReader.ReadInt32();
        _isSwitch = binaryReader.ReadBoolean();
        _parameterID = BinaryHelper.readArrayInt(ref binaryReader);
        _parameterValue = BinaryHelper.readArrayFloat(ref binaryReader);
    }
}

public class SequencerGraphEvent_DeletePrefab : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.DeletePrefab;

    public string _key = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        MasterManager.instance._stageProcessor.removeSpawnPrefab(_key);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Key")
                _key = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_key);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _key = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SpawnPrefab : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SpawnPrefab;

    public string _uniqueKey = "";
    public string _key = "";
    public float _lifeTime = 0f;

    public string _prefabPath = "";

    private GameObject _prefab = null;

    public override void Initialize(SequencerGraphProcessor processor)
    {
        if(_prefabPath != "")
            _prefab = ResourceContainerEx.Instance().GetPrefab(_prefabPath);
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        if(_prefab == null)
            return true;
        
        Vector3 position = Vector3.zero;
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            position = uniqueEntity.transform.position;
        }

        GameObject prefab = GameObject.Instantiate(_prefab, position,UnityEngine.Quaternion.identity);
        if(prefab == null)
            return true;
        
        if(_lifeTime > 0f)
            GameObject.Destroy(prefab, _lifeTime);
        
        if(_key != "")
            MasterManager.instance._stageProcessor.addSpawnPrefab(_key,prefab);

        prefab.transform.SetParent( MasterManager.instance._stageProcessor.getBackgroundObject()?.transform );
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Path")
            {
                _prefabPath = attrValue;
            }
            else if(attrName == "LifeTime")
                _lifeTime = float.Parse(attrValue);
            else if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "Key")
                _key = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_key);
        binaryWriter.Write(_lifeTime);
        binaryWriter.Write(_prefabPath);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _key = binaryReader.ReadString();
        _lifeTime = binaryReader.ReadSingle();
        _prefabPath = binaryReader.ReadString();
            
    }
}

public class SequencerGraphEvent_ApplyBuff : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ApplyBuff;

    public string _uniqueKey = "";
    public string _uniqueGroupKey = "";

    private int[] buffKeyList = null;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.applyActionBuffList(buffKeyList);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                item.applyActionBuffList(buffKeyList);
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Buff")
            {
                string[] buffList = attributes[i].Value.Split(' ');

                buffKeyList = new int[buffList.Length];
                for(int j = 0; j < buffList.Length; ++j)
                {
                    bool parse = int.TryParse(buffList[j],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[j]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert(false, "invalidBuff : {0}", buffList[j]);
                        continue;
                    }

                    buffKeyList[j] = buffKey;
                }

            }
            else if(attributes[i].Name == "UniqueKey")
                _uniqueKey = attributes[i].Value;
            else if(attributes[i].Name == "UniqueGroupKey")
                _uniqueGroupKey = attributes[i].Value;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
        BinaryHelper.writeArray(ref binaryWriter, buffKeyList);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
        buffKeyList = BinaryHelper.readArrayInt(ref binaryReader);
    }
}

public class SequencerGraphEvent_KillAllStageEntity : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.KillAllStageEntity;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        MasterManager.instance._stageProcessor.killAllCharacterWithoutKeepAliveCharacter();
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_ShowCursor : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ShowCursor;

    private bool _active = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        ActionKeyInputManager.setCursorVisible(_active);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Active")
                _active = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_active);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _active = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_KillEntity : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.KillEntity;

    public string _uniqueKey = "";
    public string _uniqueGroupKey = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey) as GameEntityBase;
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.deactive();
            uniqueEntity.DeregisterRequest();
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                item.deactive();
                item.DeregisterRequest();
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SetCameraBoundLock : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraBoundLock;

    public string _uniqueKey = "";
    public string _uniqueGroupKey = "";

    public bool _enable = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        if(_uniqueKey != "")
        {
            CharacterEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey) as CharacterEntityBase;
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.setCameraBoundLock(_enable);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                (item as CharacterEntityBase).setCameraBoundLock(_enable);
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
            else if(attrName == "Enable")
                _enable = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
        binaryWriter.Write(_enable);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
        _enable = binaryReader.ReadBoolean();
    }
}

public class SequencerGraphEvent_SetHideCharacter : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetHideCharacter;

    public string _uniqueKey = "";
    public string _uniqueGroupKey = "";

    public bool _hide = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.setActiveSelf(_hide == false, _hide);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                item.setActiveSelf(_hide == false, _hide);
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
            else if(attrName == "Hide")
                _hide = bool.Parse(attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_uniqueGroupKey);
        binaryWriter.Write(_hide);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _uniqueGroupKey = binaryReader.ReadString();
        _hide = binaryReader.ReadBoolean();
    }
}


public class SequencerGraphEvent_SetBackgroundAnimationTrigger : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetBackgroundAnimationTrigger;

    public string _animationTrigger = "";
    public string _key = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        MasterManager.instance._stageProcessor.setBackgroundAnimationTrigger(_key,_animationTrigger);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Trigger")
                _animationTrigger = attrValue;
            else if(attrName == "Key")
                _key = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_animationTrigger);
        binaryWriter.Write(_key);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _animationTrigger = binaryReader.ReadString();
        _key = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_DisableBossHp : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.DisableBossHp;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor, float deltaTime)
    {
        GameUI.Instance.DisableBossHp();

        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_WaitTargetDead : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitTargetDead;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
            return true;
        
        return unqueEntity.isDead();
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SaveEventExecuteIndex : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SaveEventExecuteIndex;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        
    }
}

public class SequencerGraphEvent_ApplyPostProcessProfile : SequencerGraphEventBase
{
    private string _path = "";
    private float _blendTime = 0f;
    private MathEx.EaseType _easeType = MathEx.EaseType.Linear;
    private PostProcessProfileApplyType _applyType = PostProcessProfileApplyType.BaseBlend;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ApplyPostProcessProfile;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScriptableObject profile = ResourceContainerEx.Instance().GetScriptableObject(_path);
        if(profile == null || (profile is PostProcessProfile) == false)
            return true;

        switch(_applyType)
        {
            case PostProcessProfileApplyType.BaseBlend:
                CameraControlEx.Instance().getPostProcessProfileControl().addBaseBlendProfile(profile as PostProcessProfile,_easeType,_blendTime);
            break;
            case PostProcessProfileApplyType.OneShot:
                CameraControlEx.Instance().getPostProcessProfileControl().setOneShotEffectProfile(profile as PostProcessProfile,_easeType,999,_blendTime);
            break;
            case PostProcessProfileApplyType.OneShotAdditional:
                CameraControlEx.Instance().getPostProcessProfileControl().setOneShotAdditionalEffectProfile(profile as PostProcessProfile,_easeType,999,_blendTime);
            break;
        }
        
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Path")
                _path = attrValue;
            else if(attrName == "BlendTime")
                _blendTime = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "ApplyType")
                _applyType = (PostProcessProfileApplyType)System.Enum.Parse(typeof(PostProcessProfileApplyType), attrValue);
            else if(attrName == "EaseType")
                _easeType = (MathEx.EaseType)System.Enum.Parse(typeof(MathEx.EaseType), attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_path);
        binaryWriter.Write(_blendTime);
        BinaryHelper.writeEnum<PostProcessProfileApplyType>(ref binaryWriter,_applyType);
        BinaryHelper.writeEnum<MathEx.EaseType>(ref binaryWriter,_easeType);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _path = binaryReader.ReadString();
        _blendTime = binaryReader.ReadSingle();
        _applyType = BinaryHelper.readEnum<PostProcessProfileApplyType>(ref binaryReader);
        _easeType = BinaryHelper.readEnum<MathEx.EaseType>(ref binaryReader);
    }
}

public class SequencerGraphEvent_TeleportTargetTo : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private string _markerName = "";
    private Vector3 _targetPosition;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TeleportTargetTo;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
                _targetPosition = item._position + processor.getOwnerProcessor().getOffsetPosition();
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }
        
        unqueEntity.updatePosition(_targetPosition);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "Position")
                _targetPosition = XMLScriptConverter.valueToVector3(attrValue);
            else if(attrName == "PositionMarker")
                _markerName = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        binaryWriter.Write(_markerName);
        BinaryHelper.writeVector3(ref binaryWriter, _targetPosition);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _markerName = binaryReader.ReadString();
        _targetPosition = BinaryHelper.readVector3(ref binaryReader);
    }
}

public class SequencerGraphEvent_SetAudioListner : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetAudioListner;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        FMODAudioManager.Instance().setListener(unqueEntity.transform);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
    }
}

public class SequencerGraphEvent_SetCameraUVTarget : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraUVTarget;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setCameraUVTarget(processor.getUniqueEntity(_uniqueKey));
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
    }
}


public class SequencerGraphEvent_SetCameraTarget : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private CameraModeType _cameraMode = CameraModeType.Count;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraTarget;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setCameraTarget(processor.getUniqueEntity(_uniqueKey));
        
        if(_cameraMode != CameraModeType.Count)
            CameraControlEx.Instance().setCameraMode(_cameraMode);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "CameraMode")
                _cameraMode = (CameraModeType)System.Enum.Parse(typeof(CameraModeType), attrValue);
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);
        binaryWriter.Write(_uniqueKey);
        BinaryHelper.writeEnum<CameraModeType>(ref binaryWriter, _cameraMode);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _uniqueKey = binaryReader.ReadString();
        _cameraMode = BinaryHelper.readEnum<CameraModeType>(ref binaryReader);
    }
}

public class SequencerGraphEvent_SetCameraPosition : SequencerGraphEventBase
{
    private Vector3 _cameraTargetPosition = Vector3.zero;
    private string _markerName = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraPosition;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
                _cameraTargetPosition = item._position + processor.getOwnerProcessor().getOffsetPosition();
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setCameraMode(CameraModeType.PositionMode);
        CameraControlEx.Instance().setCameraTargetPosition(_cameraTargetPosition);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "TargetPosition")
                _cameraTargetPosition = XMLScriptConverter.valueToVector3(attrValue);
            else if(attrName == "TargetPositionMarker")
                _markerName = attrValue;
        }
    }
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        base.serialize(ref binaryWriter);

        BinaryHelper.writeVector3(ref binaryWriter, _cameraTargetPosition);
        binaryWriter.Write(_markerName);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _cameraTargetPosition = BinaryHelper.readVector3(ref binaryReader);
        _markerName = binaryReader.ReadString();
    }
}

public enum SequencerGraphPhaseType
{
    Initialize = 0,
    Update,
    End,
    Count,
}

public class SequencerGraphPhaseData : SerializableDataType
{
    public SequencerGraphEventBase[]    _sequencerGraphEventList = null;
    public int                          _sequencerGraphEventCount = 0;
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        BinaryHelper.writeArray<SequencerGraphEventBase>(ref binaryWriter, _sequencerGraphEventList);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _sequencerGraphEventCount = binaryReader.ReadInt32();
        _sequencerGraphEventList = _sequencerGraphEventCount == 0 ? null : new SequencerGraphEventBase[_sequencerGraphEventCount];

        for(int i = 0; i < _sequencerGraphEventCount; ++i)
        {
            _sequencerGraphEventList[i] = SequencerGraphEventBase.buildSequencerGraphEvent(ref binaryReader);
        }
    }
}

public class SequencerGraphBaseData : SerializableDataType
{
    public string                       _sequencerName = "";

    public SequencerGraphPhaseData[]    _sequencerGraphPhase = new SequencerGraphPhaseData[(int)SequencerGraphPhaseType.Count];
#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_sequencerName);
        BinaryHelper.writeArray<SequencerGraphPhaseData>(ref binaryWriter, _sequencerGraphPhase);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _sequencerName = binaryReader.ReadString();
        _sequencerGraphPhase = BinaryHelper.readArray<SequencerGraphPhaseData>(ref binaryReader);
    }
}

public class SequencerGraphSetBaseData : SerializableDataType
{
    public int _bgmKey = -1;
    public int _startIndex = -1;

    public SequencerGraphBaseData[] _sequencerGraphSet = null;

    public SequencerGraphBaseData findSequencerGraph(string key)
    {
        foreach(var item in _sequencerGraphSet)
        {
            if(item._sequencerName == key)
                return item;
        }

        return null;
    }

#if UNITY_EDITOR
    public override void serialize(ref BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_bgmKey);
        binaryWriter.Write(_startIndex);
        BinaryHelper.writeArray<SequencerGraphBaseData>(ref binaryWriter, _sequencerGraphSet);
    }
#endif
    public override void deserialize(ref BinaryReader binaryReader)
    {
        _bgmKey = binaryReader.ReadInt32();
        _startIndex = binaryReader.ReadInt32();
        _sequencerGraphSet = BinaryHelper.readArray<SequencerGraphBaseData>(ref binaryReader);
    }
}

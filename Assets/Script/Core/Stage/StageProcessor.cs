using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
using UnityEngine.UIElements;

public class StageProcessor
{
    struct StartNextStageRequsetDesc
    {
        public bool _startNextStage;
        public string _stageDataPath;
        public string _startMarkerName;

        public Vector3 _keepAliveEntityPositionOffset;
    }

    public StageData _stageData = null;

    private int _currentPoint = 0;
    private bool _isEnd = false;
    private StartNextStageRequsetDesc _startNextStageRequestDesc = new StartNextStageRequsetDesc();

    private CameraControlEx _targetCameraControl;

    private Vector3 _smoothDampVelocity;

    private Vector3 _offsetPosition = Vector3.zero;
    private Vector3 _cameraPositionBlendStartPosition;
    private float _cameraPositionBlendTimeLeft = 0f;

    private GameEntityBase _playerEntity = null;
    private GameObject _stageBackgroundOjbect = null;
    private Dictionary<string, GameObject> _spawnPrefabMap = new Dictionary<string, GameObject>();
    
    private BackgroundAnimatorMaster _stageBackgroundAnimatorMaster = null;

    private MiniStageListItem   _miniStageInfo = null;

    private float               _miniStageTriggerBottomHitOffset;
    private BoundBox            _miniStageTrigger = new BoundBox(0f,0f,Vector3.zero);

    Dictionary<int,List<SequencerGraphProcessor.SpawnedCharacterEntityInfo>> _spawnedCharacterEntityDictionary = new Dictionary<int, List<SequencerGraphProcessor.SpawnedCharacterEntityInfo>>();
    Dictionary<string, CharacterEntityBase> _keepAliveMap = new Dictionary<string, CharacterEntityBase>();

    Dictionary<string, CharacterEntityBase> _keepUniqueMap = new Dictionary<string, CharacterEntityBase>();

    private Queue<StageProcessor> _miniStagePool = new Queue<StageProcessor>();
    private List<StageProcessor> _miniStageProcessor = new List<StageProcessor>();

    private List<BackgroundLayer> _backgroundLayerList = new List<BackgroundLayer>();

    private SequencerGraphProcessManager _sequencerProcessManager = new SequencerGraphProcessManager(null);
    private MovementTrackProcessor _trackProcessor = new MovementTrackProcessor();

    private Vector3         _cameraTrackPositionError = Vector3.zero;
    private float           _cameraTrackPositionErrorReduceTime = 1f;

    private bool            _blockPointExit = false;
    private bool            _unlockLimit = false;

    public StageProcessor()
    {
        _startNextStageRequestDesc._stageDataPath = "";
        _startNextStageRequestDesc._startNextStage = false;
        _startNextStageRequestDesc._startMarkerName = "";
        _startNextStageRequestDesc._keepAliveEntityPositionOffset = Vector3.zero;
    }

    public void requestStartStage(string stagePath, string markerName = "")
    {
        _startNextStageRequestDesc._startNextStage = true;
        _startNextStageRequestDesc._stageDataPath = stagePath;
        _startNextStageRequestDesc._startMarkerName = markerName;
        _startNextStageRequestDesc._keepAliveEntityPositionOffset = Vector3.zero;
    }

    public void requestStartStage(string stagePath, string markerName, Vector3 keepAliveOffset)
    {
        _startNextStageRequestDesc._startNextStage = true;
        _startNextStageRequestDesc._stageDataPath = stagePath;
        _startNextStageRequestDesc._startMarkerName = markerName;
        _startNextStageRequestDesc._keepAliveEntityPositionOffset = keepAliveOffset;
    }

    public void setTargetCameraControl(CameraControlEx target)
    {
        _targetCameraControl = target;
    }

    public void startMiniStage(MiniStageListItem data, Vector3 startPosition)
    {
        Vector3 worldPosition = startPosition + data._localStagePosition + data._overrideTriggerOffset;
        _miniStageInfo = data;
        _miniStageTrigger.setData(data._overrideTriggerWidth * 0.5f,data._overrideTriggerHeight * 0.5f,data._overrideTriggerOffset);
        _miniStageTrigger.updateBoundBox(worldPosition);

        startStage(data._data, worldPosition, Vector3.zero);
    }

    public void startStage(StageData data, Vector3 startPosition, Vector3 keepEntityPosition)
    {
        _sequencerProcessManager.initialize();
        _trackProcessor.clear();
        _backgroundLayerList.Clear();

        _cameraTrackPositionError = Vector3.zero;
        _cameraTrackPositionErrorReduceTime = 1f;

        _stageData = data;
        bool isMiniStage = _stageData._isMiniStage;

        _currentPoint = 0;
        _isEnd = false;
        _blockPointExit = false;
        _unlockLimit = false;

        if(_stageData._stagePointData.Count == 0)
            return;

        _offsetPosition = startPosition - _stageData._stagePointData[0]._stagePoint;

        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            item.Clear();
        }

        if(isMiniStage == false)
        {
            if(_stageData._stagePointData[0]._onEnterSequencerPath == null)
                return;

            CameraControlEx.Instance().clearCamera(_stageData._stagePointData[0]._stagePoint);
            CameraControlEx.Instance().setZoomSizeForce(_stageData._stagePointData[0]._cameraZoomSize,4.0f);

            ScreenDirector._instance.initialize();
            LetterBox._instance.clear();
            UIRepeater.Instance().initialize();
        }
        
        GameUI.Instance.InitializeBySceneStart();

        if(_stageData._backgroundPrefabPath != null)
        {
            _stageBackgroundOjbect = GameObject.Instantiate(_stageData._backgroundPrefabPath);
            _stageBackgroundOjbect.TryGetComponent<BackgroundAnimatorMaster>(out _stageBackgroundAnimatorMaster);

            _stageBackgroundOjbect.SetActive(true);
            _stageBackgroundOjbect.transform.position = startPosition;
        }

        _spawnPrefabMap.Clear();
        _keepUniqueMap.Clear();

        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            StagePointData stagePointData = _stageData._stagePointData[index];
            if(_spawnedCharacterEntityDictionary.ContainsKey(index) == false)
                _spawnedCharacterEntityDictionary.Add(index,new List<SequencerGraphProcessor.SpawnedCharacterEntityInfo>());

            foreach(var characterSpawnData in stagePointData._characterSpawnData)
            {
                if(_keepAliveMap.ContainsKey(characterSpawnData._uniqueKey))
                {
                    CharacterEntityBase characterEntity = _keepAliveMap[characterSpawnData._uniqueKey];
                    characterEntity.updatePosition((stagePointData._stagePoint + _offsetPosition) + characterSpawnData._localPosition);
                    characterEntity.clearCharacter();
                    if (characterSpawnData._startAIState != "")
                        characterEntity.setAINode(characterSpawnData._startAIState);

                    if (characterSpawnData._startAction != "")
                    {
                        characterEntity.setAction(characterSpawnData._startAction);
                        characterEntity.progress(0f);
                    }
                    else
                    {
                        characterEntity.setDefaultAction();
                    }

                    SequencerGraphProcessor.SpawnedCharacterEntityInfo keepEntityInfo = new SequencerGraphProcessor.SpawnedCharacterEntityInfo();
                    characterEntity.setKeepAliveEntity(characterSpawnData._keepAlive);
                    keepEntityInfo._uniqueKey = characterSpawnData._uniqueKey;
                    keepEntityInfo._characterEntity = characterEntity;

                    _spawnedCharacterEntityDictionary[index].Add(keepEntityInfo);

                    if(characterSpawnData._keepAlive)
                        _keepUniqueMap.Add(keepEntityInfo._uniqueKey, keepEntityInfo._characterEntity);

                    continue;
                }

                CharacterInfoData infoData = CharacterInfoManager.Instance().GetCharacterInfoData(characterSpawnData._characterKey);
                if(infoData == null)
                {
                    DebugUtil.assert(false,"CharacterInfo가 뭔가 잘못됐습니다. 통보 바람 [StageName: {0}]",data._stageName);
                    stopStage(true);
                    return;
                }

                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                SpawnCharacterOptionDesc spawnDesc = new SpawnCharacterOptionDesc();
                spawnDesc._position = (stagePointData._stagePoint + _offsetPosition) + characterSpawnData._localPosition;
                if(characterSpawnData._setDirection)
                    spawnDesc._direction = MathEx.angleToDirection(characterSpawnData._directionAngle * Mathf.Deg2Rad);
                else
                    spawnDesc._direction = characterSpawnData._flip ? Vector3.left : Vector3.right;
                spawnDesc._rotation = Quaternion.identity;
                spawnDesc._allyInfo = characterSpawnData._allyInfoKey == "" ? (infoData._allyInfoKey == "" ? null : AllyInfoManager.Instance().GetAllyInfoData(infoData._allyInfoKey)) : AllyInfoManager.Instance().GetAllyInfoData(characterSpawnData._allyInfoKey);
                spawnDesc._sortingOrder = characterSpawnData._sortingOrder;

                CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(infoData,spawnDesc);
                if(createdCharacter == null)
                {
                    DebugUtil.assert(false,"Character Spawn 실패! [StageName: {0}]",data._stageName);
                    stopStage(true);
                    return;
                }

                bool activeSelf = true;
                switch(characterSpawnData._activeType)
                {
                    case StageSpawnCharacterActiveType.Spawn:
                        activeSelf = true;
                    break;
                    case StageSpawnCharacterActiveType.PointActivated:
                    if(data._isMiniStage == false && index == 0)
                        activeSelf = true;
                    else
                        activeSelf = false;
                    break;
                }

                createdCharacter.setActiveSelf(activeSelf,characterSpawnData._hideWhenDeactive);

                SequencerGraphProcessor.SpawnedCharacterEntityInfo entityInfo = new SequencerGraphProcessor.SpawnedCharacterEntityInfo();
                createdCharacter.setKeepAliveEntity(characterSpawnData._keepAlive);
                entityInfo._uniqueKey = characterSpawnData._uniqueKey;
                entityInfo._characterEntity = createdCharacter;

                _spawnedCharacterEntityDictionary[index].Add(entityInfo);

                if(characterSpawnData._keepAlive)
                    _keepUniqueMap.Add(entityInfo._uniqueKey, entityInfo._characterEntity);

                if(characterSpawnData._startAIState != "")
                    createdCharacter.setAINode(characterSpawnData._startAIState);

                if(characterSpawnData._startAction != "")
                {
                    createdCharacter.setAction(characterSpawnData._startAction);
                    createdCharacter.progress(0f);
                }


                createdCharacter.updateFlipState(FlipType.Direction);
            }
        }


        if(isMiniStage == false)
        {
            if(_stageData._stagePointData[0]._onEnterSequencerPath != null && _stageData._stagePointData[0]._onEnterSequencerPath.Length != 0)
            {
                for(int index = 0; index < _stageData._stagePointData[0]._onEnterSequencerPath.Length; ++index)
                {
                    SequencerGraphProcessor processor = _sequencerProcessManager.startSequencerFromStage(_stageData._stagePointData[0]._onEnterSequencerPath[index],_stageData._stagePointData[0], ref _keepUniqueMap,_spawnedCharacterEntityDictionary[0],null,_stageData._markerData,false);

                    // if(_playerEntity != null && playerEntity != null)
                    // {
                    //     DebugUtil.assert(false,"Stage에 Player가 2명 이상 존재합니다. 데이터를 확인해 주세요. [StageName: {0}]",data._stageName);
                    //     stopStage();
                    //     return;
                    // }
                    // else 
                    if (_playerEntity == null)
                    {
                        setPlayEntity(processor?.getUniqueEntity("Player"));
                        UIRepeater.Instance().registerUniqueEntity("Player", getPlayerEntity());
                    }
                }

            }

            foreach(var item in _stageData._miniStageData)
            {
                StageProcessor processor = null;
                if(_miniStagePool.Count == 0)
                    processor = new StageProcessor();
                else
                    processor = _miniStagePool.Dequeue();
            
                processor.startMiniStage(item,startPosition);
                processor.setPlayEntity(_playerEntity);
                _miniStageProcessor.Add(processor);
            }
        }
    }

    public void setPlayEntity(GameEntityBase player)
    {
        _playerEntity = player;
    }

    public void playMiniStage(MiniStageListItem miniStage)
    {

    }

    public void registBackgroundLayer(BackgroundLayer backgroundLayer)
    {
        _backgroundLayerList.Add(backgroundLayer);
    }

    public void updateBackgroundLayer()
    {
        foreach(var item in _backgroundLayerList)
        {
            item.updateCameraPosition();
        }
    }

    public void stopStage(bool forceStop)
    {
        _sequencerProcessManager.initialize();
        _backgroundLayerList.Clear();
        _trackProcessor.clear();

        _cameraTrackPositionError = Vector3.zero;
        _cameraTrackPositionErrorReduceTime = 1f;

        bool isMiniStage = _stageData == null ? false : _stageData._isMiniStage;
        _stageData = null;
        _playerEntity = null;
        _miniStageInfo = null;
        _currentPoint = 0;
        _isEnd = false;
        _blockPointExit = false;
        _unlockLimit = false;

        _offsetPosition = Vector3.zero;
        if(_stageBackgroundOjbect != null)
        {
            GameObject.Destroy(_stageBackgroundOjbect);
            _stageBackgroundOjbect = null;
        }

        _spawnPrefabMap.Clear();

        _keepAliveMap.Clear();
        _keepUniqueMap.Clear();
        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            for(int i = 0; i < item.Count;)
            {
                if(forceStop == false && item[i]._characterEntity.isKeepAliveEntity())
                {
                    _keepAliveMap.Add(item[i]._uniqueKey,item[i]._characterEntity);
                    ++i;
                    continue;
                }

                item[i]._characterEntity.deactive();
                item[i]._characterEntity.DeregisterRequest();

                item.RemoveAt(i);
            }
        }

        if(isMiniStage == false)
        {
            foreach(var item in _miniStageProcessor)
            {
                item.stopStage(forceStop);
                _miniStagePool.Enqueue(item);
            }

            _miniStageProcessor.Clear();

            Message msg = MessagePool.GetMessage();
            BoolData data = MessageDataPooling.GetMessageData<BoolData>();
            data.value = forceStop;

            msg.Set(MessageTitles.game_stageEnd,MessageReceiver._boradcastNumber,data,null);
            MasterManager.instance.HandleBroadcastMessage(msg);
        }
    }

    public void processStage(float deltaTime)
    {
        if(isValid() == false)
            return;
        
        if(_startNextStageRequestDesc._startNextStage)
        {
            _startNextStageRequestDesc._startNextStage = false;
            stopStage(false);

            StageData stageData = ResourceContainerEx.Instance().GetStageData(_startNextStageRequestDesc._stageDataPath);
            if(stageData == null)
            {
                DebugUtil.assert(false,"대상 스테이지 데이터가 존재하지 않습니다. [Path: {0}]",_startNextStageRequestDesc._stageDataPath);
                return;
            }

            Vector3 markerPosition = Vector3.zero;
            if(_startNextStageRequestDesc._startMarkerName != "")
            {
                MarkerItem marker = stageData.findMarker(_startNextStageRequestDesc._startMarkerName);
                if(marker == null)
                {
                    DebugUtil.assert(false,"대상 마커가 존재하지 않습니다. [Marker: {0}] [Path: {1}]",_startNextStageRequestDesc._startMarkerName, _startNextStageRequestDesc._stageDataPath);
                    return;
                }

                markerPosition = marker._position + _startNextStageRequestDesc._keepAliveEntityPositionOffset;
            }

            startStage(stageData,Vector3.zero,markerPosition);

            return;
        }

        _sequencerProcessManager?.progress(deltaTime);

        if(_stageData._isMiniStage)
        {
            if(_isEnd == false)
            {
                _isEnd = _miniStageTrigger.intersection(_playerEntity.transform.position);
                if(_isEnd)
                {
                    _miniStageTriggerBottomHitOffset = _miniStageTrigger.getBottomCollisionOffset(_playerEntity.transform.position);
                    if(_stageData._stagePointData[0]._onEnterSequencerPath != null && _stageData._stagePointData[0]._onEnterSequencerPath.Length != 0)
                    {
                        for(int index = 0; index < _stageData._stagePointData[0]._onEnterSequencerPath.Length; ++index)
                        {
                            SequencerGraphProcessor processor = _sequencerProcessManager.startSequencerFromStage(_stageData._stagePointData[0]._onEnterSequencerPath[index],_stageData._stagePointData[0], ref _keepUniqueMap,_spawnedCharacterEntityDictionary[0],null,_stageData._markerData,false);
                        }
                    }

                    if(_spawnedCharacterEntityDictionary.ContainsKey(_currentPoint))
                    {
                        for(int index = 0; index < _spawnedCharacterEntityDictionary[_currentPoint].Count; ++index)
                        {
                            if(_stageData._stagePointData[_currentPoint]._characterSpawnData[index]._activeType == StageSpawnCharacterActiveType.PointActivated)
                                _spawnedCharacterEntityDictionary[_currentPoint][index]._characterEntity?.setActiveSelf(true,false);
                        }
                    }
                }
                
            }
            Color color = _isEnd ? Color.green : Color.red;
            GizmoHelper.instance.drawRectangle(_offsetPosition,new Vector3(_miniStageInfo._overrideTriggerWidth * 0.5f, _miniStageInfo._overrideTriggerHeight * 0.5f),color);
            return;
        }
    }

    public Vector3 getCameraCenterPosition(Vector3 position)
    {
        if(_stageData == null || _stageData._stagePointData == null)
            return position;
            
        Vector3 resultPoint;
        getLimitedFractionOnLine(_currentPoint, position, out resultPoint);

        return resultPoint;
    }

    public void cameraProcess(float deltaTime)
    {
        if(isValid() == false)
            return;
            
        Vector3 resultPoint;
        float fraction = getLimitedFractionOnLine(_currentPoint, _targetCameraControl.getVirtualCameraPosition(), out resultPoint);
        resultPoint = getCameraCenterPosition(_targetCameraControl.getCameraPosition());

        if(_stageData._stagePointData.Count - 1 > _currentPoint && _stageData._stagePointData[_currentPoint]._lerpCameraZoom)
        {
            float currentZoom = _stageData._stagePointData[_currentPoint]._cameraZoomSize;
            currentZoom = math.lerp(currentZoom, _stageData._stagePointData[_currentPoint + 1]._cameraZoomSize, fraction);
            CameraControlEx.Instance().setZoomSize(currentZoom,_stageData._stagePointData[_currentPoint]._cameraZoomSpeed);
        }

        if(_isEnd == false && _currentPoint < _stageData._stagePointData.Count - 1)
        {
            StagePointData stagePoint = _stageData._stagePointData[_currentPoint + 1];
            if(stagePoint._useTriggerBound)
            {
                GizmoHelper.instance.drawRectangle(stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,new Vector3(stagePoint._triggerWidth * 0.5f, stagePoint._triggerHeight * 0.5f),Color.green);
                bool intersectionResult = MathEx.intersectRect(_playerEntity.transform.position,stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,stagePoint._triggerWidth,stagePoint._triggerHeight);

                if(intersectionResult)
                {
                    fraction = 1f;
                    _cameraPositionBlendStartPosition = resultPoint;
                    _cameraPositionBlendTimeLeft = 1f;
                }
            }

            if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            {
                fraction = 1f;
                _playerEntity.transform.position = stagePoint._stagePoint + _offsetPosition;
                CameraControlEx.Instance().setCameraPosition(stagePoint._stagePoint + _offsetPosition);

                killAllCharacterWithoutKeepAliveCharacter();
            }
        }

        if(_isEnd == false && _blockPointExit == false && fraction >= 1f)
        {
            startExitSequencers(_stageData._stagePointData[_currentPoint],_currentPoint,_currentPoint != 0);

            if(getNextPointIndex(ref _currentPoint) == false)
            {
                _isEnd = true;
            }
            else
            {
                startEnterSequencers(_stageData._stagePointData[_currentPoint],_currentPoint,true);
                CameraControlEx.Instance().setZoomSize(_stageData._stagePointData[_currentPoint]._cameraZoomSize,_stageData._stagePointData[_currentPoint]._cameraZoomSpeed);

                if(_spawnedCharacterEntityDictionary.ContainsKey(_currentPoint))
                {
                    for(int index = 0; index < _spawnedCharacterEntityDictionary[_currentPoint].Count; ++index)
                    {
                        if(_stageData._stagePointData[_currentPoint]._characterSpawnData[index]._activeType == StageSpawnCharacterActiveType.PointActivated)
                            _spawnedCharacterEntityDictionary[_currentPoint][index]._characterEntity?.setActiveSelf(true,false);
                    }
                }

                _cameraPositionBlendStartPosition = resultPoint;
                _cameraPositionBlendTimeLeft = 1f;
            }
        }

        foreach(var item in _miniStageProcessor)
        {
            item.processStage(deltaTime);

            if(item._isEnd && item._miniStageInfo._isPortal)
            {
                Vector3 offset = Vector3.up * item._miniStageTriggerBottomHitOffset;
                requestStartStage(item._miniStageInfo._targetStageName,item._miniStageInfo._targetMarkerName,offset);
            }
        }

        if(_cameraPositionBlendTimeLeft != 0f)
        {
            resultPoint = Vector2.Lerp(_cameraPositionBlendStartPosition, resultPoint, MathEx.easeOutCubic(1f - _cameraPositionBlendTimeLeft) );
            _cameraPositionBlendTimeLeft -= deltaTime;
        }

        Vector2 trackPosition;
        if(_trackProcessor.isEnd() == false && _trackProcessor.processTrack(deltaTime,out trackPosition))
        {
            if(_trackProcessor.isEnd())
            {
                if(_trackProcessor.isEndBlend())
                {
                    _cameraTrackPositionError = (Vector3)trackPosition - resultPoint;
                    _cameraTrackPositionErrorReduceTime = 0f;
                }
                else
                {
                    _cameraTrackPositionErrorReduceTime = 1f;
                }
                
                _trackProcessor.clear();
            }
            else
            {
                resultPoint = trackPosition;
            }
        }

        if(_cameraTrackPositionErrorReduceTime < 1f)
        {
            _cameraTrackPositionErrorReduceTime += deltaTime;
            if(_cameraTrackPositionErrorReduceTime > 1f)
                _cameraTrackPositionErrorReduceTime = 1f;

            resultPoint = resultPoint + _cameraTrackPositionError * (1f - _cameraTrackPositionErrorReduceTime);
        }

        resultPoint.z = -10f;
        _targetCameraControl.setCameraPosition(resultPoint);
        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            Color targetColor = index < _currentPoint ? Color.green : ( index == _currentPoint ? Color.magenta : Color.red);
            GizmoHelper.instance.drawCircle(_stageData._stagePointData[index]._stagePoint + _offsetPosition, 0.3f, 12, targetColor);
        }

        for(int index = 0; index < _stageData._markerData.Count; ++index)
        {
            GizmoHelper.instance.drawCircle(_stageData._markerData[index]._position + _offsetPosition, 0.1f, 12, Color.yellow);
            
        }
    }

    public MovementTrackData getTrackData(string trackName)
    {
        if(_stageData == null)
            return null;
        foreach(MovementTrackData trackData in _stageData._trackData)
        {
            if(trackData._name == trackName)
                return trackData;
        }
        return null;
    }

    public void startCameraTrack(string trackName)
    {
        MovementTrackData trackData = getTrackData(trackName);
        if(trackData == null)
            return;
            
        startCameraTrack(trackData);
    }

    public bool isTrackEnd()
    {
        return (_trackProcessor.isEnd() || _trackProcessor.isTrackValid() == false) && (_cameraTrackPositionErrorReduceTime == 1f);
    }

    public void startCameraTrack(MovementTrackData trackData)
    {
        _trackProcessor.initialize(trackData);

        Vector2 trackStartPosition;
        if(_trackProcessor.getCurrentTrackStartPosition(out trackStartPosition) == false)
            return;

        Vector3 resultPosition = trackStartPosition;

        if(trackData._startBlend)
        {
            Vector3 resultPoint;
            getLimitedFractionOnLine(_currentPoint, _targetCameraControl.getCameraPosition(), out resultPoint);

            _cameraTrackPositionError = resultPoint - resultPosition;
            _cameraTrackPositionErrorReduceTime = 0f;
        }
        else
        {
            _cameraTrackPositionErrorReduceTime = 1f;
        }
    }

    public GameObject getBackgroundObject()
    {
        return _stageBackgroundOjbect;
    }

    public void setActiveBackground(bool value)
    {
        _stageBackgroundOjbect?.SetActive(value);
    }

    public void addSpawnPrefab(string key, GameObject prefab)
    {
        if(_spawnPrefabMap.ContainsKey(key))
        {
            DebugUtil.assert(false, "대상 Prefab이 이미 존재합니다. [Key: {0}]",key);
            return;
        }

        _spawnPrefabMap.Add(key,prefab);
    }

    public void removeSpawnPrefab(string key)
    {
        if(_spawnPrefabMap.ContainsKey(key))
        {
            GameObject.Destroy(_spawnPrefabMap[key]);
            _spawnPrefabMap.Remove(key);
        }
    }

    public void setBackgroundAnimationTrigger(string key, string trigger)
    {
        _stageBackgroundAnimatorMaster?.setTrigger(key,trigger);
    }

    public void killAllCharacterWithoutKeepAliveCharacter()
    {
        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            foreach(var characterInfo in item)
            {
                if(characterInfo._characterEntity == null || characterInfo._characterEntity.isDead())
                    continue;
                
                if(characterInfo._characterEntity.isKeepAliveEntity())
                    continue;
                
                if(characterInfo._characterEntity.isActiveSelf() == false)
                    continue;

                characterInfo._characterEntity.getStatusInfo().kill();
            }
        }

        (SceneCharacterManager._managerInstance as SceneCharacterManager).killAllCharacterWithoutKeepAliveCharacter();
    }

    public void addSequencerSignal(string signal)
    {
        _sequencerProcessManager.addSequencerSignal(signal);
    }

    public void startEnterSequencers(StagePointData pointData, int pointIndex, bool includePlayer)
    {
        if(pointData == null || pointData._onEnterSequencerPath == null)
            return;

        foreach(var path in pointData._onEnterSequencerPath)
        {
            SequencerGraphProcessor sequencerGraphProcessor = _sequencerProcessManager.startSequencerFromStage(path,pointData, ref _keepUniqueMap,_spawnedCharacterEntityDictionary[pointIndex],null,_stageData._markerData,includePlayer);
        }
    }

    public void startExitSequencers(StagePointData pointData,int pointIndex,bool includePlayer)
    {
        if(pointData == null || pointData._onExitSequencerPath == null)
            return;

        foreach(var path in pointData._onExitSequencerPath)
        {
            _sequencerProcessManager.startSequencerFromStage(path,pointData, ref _keepUniqueMap,_spawnedCharacterEntityDictionary[pointIndex],null,_stageData._markerData,includePlayer);
        }
    }

    public bool isValid()
    {
        return _stageData != null;
    }

    public void blockPointExit(bool value)
    {
        _blockPointExit = value;
    }

    public GameEntityBase getPlayerEntity()
    {
        return _playerEntity;
    }

    public bool getNextPointIndex(ref int resultIndex)
    {
        resultIndex++;
        if(resultIndex >= _stageData._stagePointData.Count)
        {
            resultIndex = _stageData._stagePointData.Count - 1;
            return false;
        }

        return true;
    }

    public bool getPrevPointIndex(ref int resultIndex)
    {
        resultIndex--;
        if(resultIndex < 0)
        {
            resultIndex = 0;
            return false;
        }

        return true;
    }

    private bool isInTriggerBound(int pointIndex, Vector3 position)
    {
        if(isValid() == false || pointIndex >= _stageData._stagePointData.Count - 1 || pointIndex < 0)
            return false;

        StagePointData stagePoint = _stageData._stagePointData[pointIndex + 1];
        if(stagePoint._useTriggerBound)
        {
            GizmoHelper.instance.drawRectangle(stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,new Vector3(stagePoint._triggerWidth * 0.5f, stagePoint._triggerHeight * 0.5f),Color.green);
            return MathEx.intersectRect(position,stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,stagePoint._triggerWidth,stagePoint._triggerHeight);
        }

        return false;
    }

    public void updatePointIndex(Vector3 position, ref int pointIndex)
    {
        if(isValid() == false)
            return;

        while(true)
        {
            float fraction = getLimitedFractionOnLine(pointIndex, position, out Vector3 resultPosition);
            if(fraction >= 1f && _blockPointExit == false)
            {
                if(getNextPointIndex(ref pointIndex) == false)
                    break;

                continue;
            }

            if(pointIndex < _currentPoint)
                pointIndex = _currentPoint;

            break;
        }
    }

    public bool isInCameraBound(Vector3 position)
    {
        Vector3 result;
        return isInCameraBound(_currentPoint, position, out result);
    }

    public bool isInCameraBound(int pointIndex, Vector3 position, out Vector3 resultPosition)
    {
        resultPosition = position;
        if(isValid() == false)
            return true;

        getLimitedFractionOnLine(pointIndex, position, out resultPosition);
        return CameraControlEx.Instance().IsInCameraBound(position, resultPosition, out resultPosition);
    }

    private float getLimitedFractionOnLine(int targetPointIndex, Vector3 targetPosition, out Vector3 resultPoint)
    {
        Vector3 targetPositionWithoutZ = MathEx.deleteZ(targetPosition);
        resultPoint = targetPosition;

        if(_stageData._stagePointData.Count == 0 || targetPointIndex >= _stageData._stagePointData.Count)
            return 0f;

        if(_unlockLimit)
            return 0f;

        bool isEndPoint = targetPointIndex == _stageData._stagePointData.Count - 1;
        StagePointData startPoint = _stageData._stagePointData[targetPointIndex];
        StagePointData nextPoint = isEndPoint ? startPoint : _stageData._stagePointData[targetPointIndex + 1];

        Vector3 onLinePosition = targetPositionWithoutZ;
        float resultFraction = 0f;
        float limitedDistance = 0f;
        float verticalLimitedDistance = 0f;
        float horizontalLimitedDistance = 0f;
        float cameraSize = startPoint._cameraZoomSize;

        if(startPoint._lockCameraInBound)
        {
            onLinePosition = startPoint._stagePoint + _offsetPosition;
            limitedDistance = startPoint._maxLimitedDistance;
            verticalLimitedDistance = startPoint._verticalLimitedDistance;
            horizontalLimitedDistance = startPoint._horizontalLimitedDistance;
        }
        else
        {
            onLinePosition = isEndPoint ? (startPoint._stagePoint + _offsetPosition) : MathEx.getPerpendicularPointOnLineSegment((startPoint._stagePoint + _offsetPosition), (nextPoint._stagePoint + _offsetPosition),targetPositionWithoutZ);
            resultFraction = isEndPoint ? 1f : (onLinePosition - (startPoint._stagePoint + _offsetPosition)).magnitude * (1f / ((nextPoint._stagePoint + _offsetPosition) - (startPoint._stagePoint + _offsetPosition)).magnitude);
            limitedDistance = Mathf.Lerp(startPoint._maxLimitedDistance, nextPoint._maxLimitedDistance, resultFraction);
            verticalLimitedDistance = Mathf.Lerp(startPoint._verticalLimitedDistance, nextPoint._verticalLimitedDistance, resultFraction);
            horizontalLimitedDistance = Mathf.Lerp(startPoint._horizontalLimitedDistance, nextPoint._horizontalLimitedDistance, resultFraction);
            cameraSize = Mathf.Lerp(cameraSize, nextPoint._cameraZoomSize, resultFraction);
        }

        float mainCamSize = Camera.main.orthographicSize;
        float currentCamHeight = mainCamSize * 2f;
        float currentcamWidth = currentCamHeight * (800f / 600f);

        float targetCamHeight = (cameraSize) * 2f;
		float targetCamWidth = targetCamHeight * (800f / 600f);

        verticalLimitedDistance += limitedDistance + (targetCamHeight - currentCamHeight) * 0.5f;
        horizontalLimitedDistance += limitedDistance + (targetCamWidth - currentcamWidth) * 0.5f;

        resultPoint = targetPositionWithoutZ;
        if(MathEx.distancef(targetPositionWithoutZ.y,onLinePosition.y) > verticalLimitedDistance)
        {
            if(targetPositionWithoutZ.y > onLinePosition.y)
                resultPoint.y = onLinePosition.y + verticalLimitedDistance;
            else
                resultPoint.y = onLinePosition.y - verticalLimitedDistance;
        }
        if(MathEx.distancef(targetPositionWithoutZ.x,onLinePosition.x) > horizontalLimitedDistance)
        {
            if(targetPositionWithoutZ.x > onLinePosition.x)
                resultPoint.x = onLinePosition.x + horizontalLimitedDistance;
            else
                resultPoint.x = onLinePosition.x - horizontalLimitedDistance;
        }

        return resultFraction;
    }

    public bool canTowardNext()
    {
        int nextPointIndex = 0;
        return _isEnd == false && _currentPoint >= 0 && _stageData._stagePointData.Count -1 > _currentPoint && _blockPointExit == false &&  _unlockLimit == false && getNextPointIndex(ref nextPointIndex);
    }

    public Vector3 getStagePoint(int index)
    {
        return _stageData._stagePointData[index]._stagePoint + _offsetPosition;
    }

    public Vector3 getCurrentPointPosition()
    {
        return getStagePoint(_currentPoint);
    }

    public bool getNextPoint(ref Vector3 point)
    {
        int nextPointIndex = _currentPoint;
        if(getNextPointIndex(ref nextPointIndex) == false)
            return false;

        point = getStagePoint(nextPointIndex);
        return true;
    }

    public bool getNextPointDirection(ref Vector3 direction )
    {
        if(canTowardNext() == false)
            return false;

        int nextPointIndex = _currentPoint;
        if(getNextPointIndex(ref nextPointIndex) == false)
            return false;

        direction = getStagePoint(nextPointIndex) - getStagePoint(_currentPoint);
        direction.Normalize();
        
        return true;
    }

    public void unlockLimit(bool value)
    {
        _unlockLimit = value;
        if(_unlockLimit)
        {
            _cameraPositionBlendStartPosition = Camera.main.transform.position;
            _cameraPositionBlendStartPosition.z = 0f;
            _cameraPositionBlendTimeLeft = 1f;
            killAllCharacterWithoutKeepAliveCharacter();
        }
    }
}


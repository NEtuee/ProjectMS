using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StageProcessor : Singleton<StageProcessor>
{
    public StageData _stageData = null;

    private int _currentPoint = 0;
    private bool _isEnd = false;

    private Transform _targetTransform;

    private Vector3 _clampTargetPosition;
    private Vector3 _smoothDampVelocity;

    private Vector3 _offsetPosition = Vector3.zero;

    private GameEntityBase _playerEntity = null;
    private GameObject _stageBackgroundOjbect = null;

    Dictionary<int,List<CharacterEntityBase>> _spawnedCharacterEntityDictionary = new Dictionary<int, List<CharacterEntityBase>>();

    private SequencerGraphProcessManager _sequencerProcessManager = new SequencerGraphProcessManager(null);

    public void setTargetTransform(Transform target)
    {
        _targetTransform = target;
    }

    public void startStage(StageData data, Vector3 startPosition)
    {
        _sequencerProcessManager.initialize();
        _stageData = data;

        _currentPoint = 0;
        _isEnd = false;

        if(_stageData._stagePointData.Count == 0)
            return;

        _offsetPosition = startPosition - _stageData._stagePointData[0]._stagePoint;
        _clampTargetPosition = _stageData._stagePointData[0]._stagePoint + _offsetPosition;

        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            item.Clear();
        }

        if(_stageData._stagePointData[0]._onEnterSequencerPath == null)
            return;

        CameraControlEx.Instance().clearCamera(_stageData._stagePointData[0]._stagePoint);
        CameraControlEx.Instance().setZoomSizeForce(_stageData._stagePointData[0]._cameraZoomSize);

        ScreenDirector._instance.initialize();

        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            StagePointData stagePointData = _stageData._stagePointData[index];
            if(_spawnedCharacterEntityDictionary.ContainsKey(index) == false)
                _spawnedCharacterEntityDictionary.Add(index,new List<CharacterEntityBase>());

            foreach(var characterSpawnData in stagePointData._characterSpawnData)
            {
                CharacterInfoData infoData = CharacterInfoManager.Instance().GetCharacterInfoData(characterSpawnData._characterKey);
                if(infoData == null)
                {
                    DebugUtil.assert(false,"CharacterInfo가 뭔가 잘못됐습니다. 통보 바람 [StageName: {0}]",data._stageName);
                    stopStage();
                    return;
                }

                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                SpawnCharacterOptionDesc spawnDesc = new SpawnCharacterOptionDesc();
                spawnDesc._position = (stagePointData._stagePoint + _offsetPosition) + characterSpawnData._localPosition;
                spawnDesc._direction = characterSpawnData._flip ? Vector3.left : Vector3.right;
                spawnDesc._rotation = Quaternion.identity;
                spawnDesc._searchIdentifier = characterSpawnData._searchIdentifier;

                CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(infoData,spawnDesc);
                if(createdCharacter == null)
                {
                    DebugUtil.assert(false,"Character Spawn 실패! [StageName: {0}]",data._stageName);
                    stopStage();
                    return;
                }

                bool activeSelf = true;
                switch(characterSpawnData._activeType)
                {
                    case StageSpawnCharacterActiveType.Spawn:
                        activeSelf = true;
                    break;
                    case StageSpawnCharacterActiveType.PointActivated:
                        activeSelf = false;
                    break;
                }

                createdCharacter.setActiveSelf(activeSelf);
                _spawnedCharacterEntityDictionary[index].Add(createdCharacter);

                if(characterSpawnData._startAction != "")
                {
                    createdCharacter.setAction(characterSpawnData._startAction);
                    createdCharacter.progress(0f);
                }
            }
        }

        if(_stageData._stagePointData[0]._onEnterSequencerPath != null && _stageData._stagePointData[0]._onEnterSequencerPath.Length != 0)
        {
            for(int index = 0; index < _stageData._stagePointData[0]._onEnterSequencerPath.Length; ++index)
            {
                SequencerGraphProcessor processor = _sequencerProcessManager.startSequencerFromStage(_stageData._stagePointData[0]._onEnterSequencerPath[index],_stageData._stagePointData[0],_spawnedCharacterEntityDictionary[0],null,false);

                // if(_playerEntity != null && playerEntity != null)
                // {
                //     DebugUtil.assert(false,"Stage에 Player가 2명 이상 존재합니다. 데이터를 확인해 주세요. [StageName: {0}]",data._stageName);
                //     stopStage();
                //     return;
                // }
                // else 
                if(_playerEntity == null)
                    _playerEntity = processor?.getUniqueEntity("Player");
            }
            
        }

        if(_stageData._backgroundPrefabPath != "")
        {
            GameObject prefabObject = ResourceContainerEx.Instance().GetPrefab(_stageData._backgroundPrefabPath);
            if(prefabObject == null)
                return;

            _stageBackgroundOjbect = GameObject.Instantiate(prefabObject);

            _stageBackgroundOjbect.SetActive(true);
            _stageBackgroundOjbect.transform.position = startPosition;
        }
    }

    public void stopStage()
    {
        _sequencerProcessManager.initialize();
        _stageData = null;
        _playerEntity = null;
        _currentPoint = 0;
        _isEnd = false;
        _offsetPosition = Vector3.zero;
        _clampTargetPosition = Vector3.zero;
        if(_stageBackgroundOjbect != null)
            GameObject.Destroy(_stageBackgroundOjbect);

        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            item.Clear();
        }
    }

    public void processStage(float deltaTime)
    {
        if(isValid() == false)
            return;

        _sequencerProcessManager?.progress(deltaTime);

        Vector3 resultPoint;
        float resultDistance;
        float fraction = getLimitedFractionOnLine(_targetTransform.position, out resultPoint, out resultDistance);
        if(_stageData._stagePointData.Count - 1 > _currentPoint && _stageData._stagePointData[_currentPoint]._lerpCameraZoom)
        {
            float currentZoom = _stageData._stagePointData[_currentPoint]._cameraZoomSize;
            currentZoom = math.lerp(currentZoom, _stageData._stagePointData[_currentPoint + 1]._cameraZoomSize, fraction);
            CameraControlEx.Instance().setZoomSize(currentZoom,_stageData._stagePointData[_currentPoint]._cameraZoomSpeed);
        }

        if(_isEnd == false && fraction >= 1f)
        {
            startExitSequencers(_stageData._stagePointData[_currentPoint],_currentPoint,_currentPoint != 0);

            ++_currentPoint;
            if(_currentPoint >= _stageData._stagePointData.Count)
            {
                _currentPoint = _stageData._stagePointData.Count - 1;
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
                            _spawnedCharacterEntityDictionary[_currentPoint][index]?.setActiveSelf(true);
                    }
                }
            }
        }

        resultPoint.z = -10f;
        _targetTransform.position = resultPoint;
        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            Color targetColor = index < _currentPoint ? Color.green : ( index == _currentPoint ? Color.magenta : Color.red);
            GizmoHelper.instance.drawCircle(_stageData._stagePointData[index]._stagePoint + _offsetPosition, 0.3f, 12, targetColor);
        }
    }

    public void startEnterSequencers(StagePointData pointData, int pointIndex, bool includePlayer)
    {
        if(pointData == null || pointData._onEnterSequencerPath == null)
            return;

        foreach(var path in pointData._onEnterSequencerPath)
        {
            _sequencerProcessManager.startSequencerFromStage(path,pointData,_spawnedCharacterEntityDictionary[pointIndex],null,includePlayer);
        }
    }

    public void startExitSequencers(StagePointData pointData,int pointIndex,bool includePlayer)
    {
        if(pointData == null || pointData._onExitSequencerPath == null)
            return;

        foreach(var path in pointData._onExitSequencerPath)
        {
            _sequencerProcessManager.startSequencerFromStage(path,pointData,_spawnedCharacterEntityDictionary[pointIndex],null,includePlayer);
        }
    }

    public bool isValid()
    {
        return _targetTransform != null && _stageData != null;
    }

    public GameEntityBase getPlayerEntity()
    {
        return _playerEntity;
    }

    private float getLimitedFractionOnLine(Vector3 targetPosition, out Vector3 resultPoint, out float distance)
    {
        Vector3 targetPositionWithoutZ = MathEx.deleteZ(targetPosition);
        resultPoint = targetPosition;
        distance = 0f;

        if(_stageData._stagePointData.Count == 0)
            return 0f;

        bool isEndPoint = _currentPoint == _stageData._stagePointData.Count - 1;
        StagePointData startPoint = _stageData._stagePointData[_currentPoint];
        StagePointData nextPoint = isEndPoint ? startPoint : _stageData._stagePointData[_currentPoint + 1];

        Vector3 perpendicular = isEndPoint ? (startPoint._stagePoint + _offsetPosition) : MathEx.getPerpendicularPointOnLineSegment((startPoint._stagePoint + _offsetPosition), (nextPoint._stagePoint + _offsetPosition),targetPositionWithoutZ);
        float resultFraction = isEndPoint ? 1f : (perpendicular - (startPoint._stagePoint + _offsetPosition)).magnitude * (1f / ((nextPoint._stagePoint + _offsetPosition) - (startPoint._stagePoint + _offsetPosition)).magnitude);
        float limitedDistance = Mathf.Lerp(startPoint._maxLimitedDistance, nextPoint._maxLimitedDistance, resultFraction);

        distance = Vector3.Distance(targetPositionWithoutZ, perpendicular);

        if(distance > limitedDistance)
            resultPoint = perpendicular + (targetPositionWithoutZ - perpendicular).normalized * limitedDistance;

        return resultFraction;
    }

}


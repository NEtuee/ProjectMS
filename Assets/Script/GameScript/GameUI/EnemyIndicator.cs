using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicator : IUIElement
{
    private EnemyIndicatorBinder _binder;

    private readonly float _magnitude = 0.9f;
    private readonly string _sortingLayer = "UI";
    private readonly int _sortingOrder = 1;

    private SceneCharacterManager _characterManager;

    private readonly Dictionary<CharacterEntityBase, EnemyIndicatorElement> _enabledCharacters = new Dictionary<CharacterEntityBase, EnemyIndicatorElement>();
    private readonly Queue<EnemyIndicatorElement> _indicatorPool = new Queue<EnemyIndicatorElement>();
    private Vector2 _cameraSizeHalf;
    private Vector3[,] _screenSectors = new Vector3[4,2];

    private bool _active = false;

    private Camera _mainCamera;

    private SceneCharacterManager.OnCharacterEnabled _onCharacterEnabled;
    private SceneCharacterManager.OnCharacterDisabled _onCharacterDisabled;

    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "인디케이터 ui 바인더가 셋팅되지 않았습니다";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as EnemyIndicatorBinder;
    }

    public void Initialize()
    {
        _onCharacterEnabled = AddCharacter;
        _onCharacterDisabled = DisableCharacter;
    }

    public void InitValue(Camera mainCamera)
    {
        _mainCamera = mainCamera;
        
        UpdateCameraSize();

        _active = true;
        
        _characterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;

        _characterManager.deleteCharacterEnableDelegate(_onCharacterEnabled);
        _characterManager.deleteCharacterDisableDelegate(_onCharacterDisabled);
        
        _characterManager.addCharacterEnableDelegate(_onCharacterEnabled);
        _characterManager.addCharacterDisableDelegate(_onCharacterDisabled);
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    public void UpdateByManager()
    {
        UpdateCameraSize();

        Vector3 center = _mainCamera.transform.position;
        CalculateScreenSectors(center);
        
        foreach(var item in _enabledCharacters)
        {
            if(item.Key.isActiveSelf() == false)
            {
                item.Value.gameObject.SetActive(false);
                item.Value._state = EnemyIndicatorElement.State.Inactive;
                continue;
            }
            
            bool isInCamera = IsInCameraSector(center, item.Key.transform.position);
            var isAppear = _active && isInCamera == false;

            if (isAppear && 
                item.Value.GetState() != EnemyIndicatorElement.State.AppearPlay &&
                item.Value.GetState() != EnemyIndicatorElement.State.Active)
            {
                item.Value.Appear();
            }
            else if (isAppear == false &&
                     item.Value.GetState() != EnemyIndicatorElement.State.DisappearPlay &&
                     item.Value.GetState() != EnemyIndicatorElement.State.Inactive
                    )
            {
                item.Value.Disappear(null);
            }

            if(isInCamera)
                continue;

            var position = item.Key.transform.position;
            Vector3 sectorPosition = GetSectorPosition(center,position);
            Vector3 direction = position - center;

            item.Value.transform.position = sectorPosition;
            item.Value.transform.rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(direction));
        }
    }

    private void AddCharacter(CharacterEntityBase character)
    {
        if(character.isIndicatorVisible() == false)
            return;
            
        var indicator = GetIndicator();

        Vector3 center = _mainCamera.transform.position;
        CalculateScreenSectors(center);
        if(IsInCameraSector(center, character.transform.position) == false)
        {
            indicator.Appear();
            Vector3 sectorPosition = GetSectorPosition(center, character.transform.position);
            indicator.transform.position = sectorPosition;
        }

        _enabledCharacters.Add(character, indicator);    
    }
    
    private void DisableCharacter(CharacterEntityBase character)
    {
        if(_enabledCharacters.ContainsKey(character) == false)
            return;

        _enabledCharacters[character].Disappear(null);
        ReturnToPool(_enabledCharacters[character]);
        _enabledCharacters.Remove(character); 
    }
    
    private void ReturnToPool(EnemyIndicatorElement indicator)
    {
        _indicatorPool.Enqueue(indicator);
    }
    
    private EnemyIndicatorElement GetIndicator()
    {
        if(_indicatorPool.Count <= 0)
            CreateIndicator();

        EnemyIndicatorElement indicator = _indicatorPool.Dequeue();
        return indicator;
    }
    
    private void CreateIndicator()
    {
        GameObject screenIndicator = new GameObject("EnemyIndicator");
        SpriteRenderer spriteRenderer = screenIndicator.AddComponent<SpriteRenderer>();

        spriteRenderer.sortingLayerName = _sortingLayer;
        spriteRenderer.sortingOrder = _sortingOrder;

        screenIndicator.SetActive(false);
        
        screenIndicator.transform.SetParent(_binder.transform);

        var indicatorElement = screenIndicator.AddComponent<EnemyIndicatorElement>();
        indicatorElement.Init();

        _indicatorPool.Enqueue(indicatorElement);
    }


    private void UpdateCameraSize()
    {
        if (_mainCamera == null)
        {
            return;
        }

        float orthoSize = _mainCamera.orthographicSize;
        float aspectRatio = _mainCamera.aspect;
        float height = orthoSize * 2f;
        float width = height * aspectRatio;

        _cameraSizeHalf.x = width * 0.5f;
        _cameraSizeHalf.y = height * 0.5f;
    }

    private void CalculateScreenSectors(Vector3 center)
    {
        Vector2 cameraSize = _cameraSizeHalf * _magnitude;

        //left
        _screenSectors[0,0] = new Vector3(-cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[0,1] = new Vector3(-cameraSize.x, -cameraSize.y, 0f) + center;

        //right
        _screenSectors[1,0] = new Vector3(cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[1,1] = new Vector3(cameraSize.x, -cameraSize.y, 0f) + center;

        //up
        _screenSectors[2,0] = new Vector3(-cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[2,1] = new Vector3(cameraSize.x, cameraSize.y, 0f) + center;

        //down
        _screenSectors[3,0] = new Vector3(-cameraSize.x, -cameraSize.y, 0f) + center;
        _screenSectors[3,1] = new Vector3(cameraSize.x, -cameraSize.y, 0f) + center;
    }
    
    private bool IsInCameraSector(Vector3 center, Vector3 point)
    {
        return (-_cameraSizeHalf.x + center.x <= point.x && 
                _cameraSizeHalf.x + center.x >= point.x) &&
               (-_cameraSizeHalf.y + center.y <= point.y && 
                _cameraSizeHalf.y + center.y >= point.y);
    }
    
    private Vector3 GetSectorPosition(Vector3 center, Vector3 targetPosition)
    {
        for(int index = 0; index < 4; ++index)
        {
            if(MathEx.findLineIntersection(_screenSectors[index,0],_screenSectors[index,1],center,targetPosition,out var outInterSectionPosition) == false)
                continue;
            
            return new Vector3(outInterSectionPosition.x,outInterSectionPosition.y,0f);
        }

        return center;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    
    //Overlay UI Binder
    public HpBpGageUIBinder HpBpGageUIBinder;
    public DashPointUIBinder DashPointBinder;
    public CrosshairUIBinder CrosshairBinder;
    public EnemyIndicatorBinder EnemyIndicatorBinder;
    public TargetFollowerBinder FollowerBinder;
    public TextBubblePoolBinder TextBubblePoolBinder;

    public TextBubble TextBubble { get; private set; }
    
    //Overlay UI
    private HpBpGageUI _hpBpGageUI;
    private DashPointUI _dashPointUI;
    private CrosshairUI _crossHairUI;
    private EnemyIndicator _enemyIndicator;
    private TargetFollower _targetFollower;
    private TextBubble _textBubble;

    private GameEntityBase _targetEntity;

    private void Awake()
    {
        Instance = this;
        SetBinder();
        CheckValidUI();
    }

    public void InitializeBySceneStart()
    {
        _enemyIndicator.InitValue(Camera.main);
    }

    public void SetEntity(GameEntityBase targetEntity) 
    {
        if (targetEntity == null)
        {
            return;
        }
        _targetEntity = targetEntity;
        
        _hpBpGageUI.InitValue(_targetEntity.getStatusPercentage("HP"), _targetEntity.getStatusPercentage("Blood"));
        _dashPointUI.InitValue(_targetEntity.getStatus("DashPoint"));
        _crossHairUI.InitValue(_targetEntity, _targetEntity.transform.position);
    }

    public void SetActiveCrossHair(bool active)
    {
        CrosshairBinder.HeadObject.SetActive(active);
    }

    private void Update()
    {
        if (_targetEntity == null)
        {
            return;
        }

        var deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        _hpBpGageUI.UpdateByManager(_targetEntity.getStatusPercentage("HP"), _targetEntity.getStatusPercentage("Blood"));
        _dashPointUI.UpdateByManager(deltaTime, _targetEntity.getStatus("DashPoint"), _targetEntity.getStatus("Blood"));
        _crossHairUI.UpdateByManager(_targetEntity, _targetEntity.isDead(), _targetEntity.transform.position);
        _enemyIndicator.UpdateByManager();
        _targetFollower.UpdateByManager(_targetEntity.transform.position);
        _textBubble.UpdateByManager();
    }

    private void SetBinder()
    {
        var binderList = new List<UIObjectBinder>();
        binderList.Add(HpBpGageUIBinder);
        binderList.Add(DashPointBinder);
        binderList.Add(CrosshairBinder);
        binderList.Add(EnemyIndicatorBinder);
        binderList.Add(FollowerBinder);
        binderList.Add(TextBubblePoolBinder);

        foreach (var binder in binderList)
        {
            if (binder == null)
            {             
                DebugUtil.assert(false, "연결되지 않은 바인더가 있습니다.");
                OnInitError();
                return;
            }
            
            if (binder.CheckValidLink(out string reason) == false)
            {
                DebugUtil.assert(false, reason);
                OnInitError();
                return;
            }
        }

        _hpBpGageUI = new HpBpGageUI();
        _dashPointUI = new DashPointUI();
        _crossHairUI = new CrosshairUI();
        _enemyIndicator = new EnemyIndicator();
        _targetFollower = new TargetFollower();
        _textBubble = new TextBubble();
        TextBubble = _textBubble;

        _hpBpGageUI.SetBinder(HpBpGageUIBinder);
        _dashPointUI.SetBinder(DashPointBinder);
        _crossHairUI.SetBinder(CrosshairBinder);
        _enemyIndicator.SetBinder(EnemyIndicatorBinder);
        _targetFollower.SetBinder(FollowerBinder);
        _textBubble.SetBinder(TextBubblePoolBinder);
    }

    private void CheckValidUI()
    {
        var uiElementList = new List<IUIElement>();
        uiElementList.Add(_hpBpGageUI);
        uiElementList.Add(_dashPointUI);
        uiElementList.Add(_crossHairUI);
        uiElementList.Add(_enemyIndicator);
        uiElementList.Add(_targetFollower);
        uiElementList.Add(_textBubble);

        foreach (var uiElement in uiElementList)
        {
            if (uiElement.CheckValidBinderLink(out string reason) == false)
            {
                DebugUtil.assert(false, reason);
                OnInitError();
                return;
            }
            
            uiElement.Initialize();
        }
    }

    private void OnInitError()
    {
#if UNITY_EDITOR
        Application.Quit();
#endif
    }
}

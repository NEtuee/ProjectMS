using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    [SerializeField] private IngameUI _ingameUI;
    public GameObject MainHUDRoot;
    public HpBpGaugeUIBinder HpBpGaugeUIBinder;
    public DashPointUIBinder DashPointBinder;
    public CrosshairUIBinder CrosshairBinder;
    public EnemyIndicatorBinder EnemyIndicatorBinder;
    public TargetFollowerBinder FollowerBinder;
    public TextBubblePoolBinder TextBubblePoolBinder;
    public EnemyHpBinder EnemyHpBinder;
    public BossHpUIBinder BossHpBinder;
    public TitleMenuUIBinder TitleMenuUIBinder;
    public PauseUIBinder PauseUIBinder;

    public OptionUIControl _optionUIControl;

    public GameObject _ratioCut;

    public TextBubble TextBubble { get; private set; }
    
    private HpBpGaugeUI _hpBpGaugeUI;
    private DashPointUI _dashPointUI;
    private CrosshairUI _crossHairUI;
    private EnemyIndicator _enemyIndicator;
    private TargetFollower _targetFollower;
    private TextBubble _textBubble;
    private EnemyHp _enemyHp;
    private BossHpUI _bossHp;
    private TitleMenuUI _titleMenuUI;
    private PauseUI _pauseUI;
    
    private GameEntityBase _targetEntity;

    private Dictionary<string, List<Action>> _notifySubscriber = new Dictionary<string, List<Action>>();

    private void Awake()
    {
        Instance = this;
        SetBinder();
        CheckValidUI();
        BindNotifySubscriber();

        _optionUIControl.initialize();
        _ratioCut.SetActive(true);
    }

    public void InitializeBySceneStart()
    {
        MainHUDRoot.SetActive(true);
        _enemyIndicator.InitValue(Camera.main);
        _enemyHp.InitValue();
        _bossHp.Disable();

        _ingameUI.ActivateUI();
    }

    public void StopByScene()
    {
        
    }

    public void SetEntity(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        if (targetEntity == null)
            return;

        _hpBpGaugeUI.InitValue(_targetEntity.getStatusPercentage("HP"), _targetEntity.getStatusPercentage("Blood"));
        _dashPointUI.InitValue(_targetEntity.getStatus("DashPoint"));
        _crossHairUI.InitValue(_targetEntity, _targetEntity.transform.position, _targetEntity.getStatus("DashPoint"));

        _ingameUI.SetMainUIEntity(_targetEntity);
    }

    public void SetActiveCrossHair(bool active)
    {
        CrosshairBinder.HeadObject.SetActive(active);
        CrosshairBinder.SubMarker.SetActive(active);
        CrosshairBinder.rangeMarker.SetActive(active);
    }

    public void ActiveTitleMenuUI(bool active)
    {
        _titleMenuUI.ActiveTitleMenu(active);
    }

    public void ActivePauseUI(bool active)
    {
        activeOptionUI(active);

        _pauseUI.ActivePauseUI(active);
        ActionKeyInputManager.setCursorVisible(active);

        FMODAudioManager.Instance().SetGlobalParam(21, active ? 1f : 0f);
    }

    public void activeOptionUI(bool active)
    {
        _optionUIControl.gameObject.SetActive(active);
    }

    public void SetBossHpEntity(GameEntityBase targetEntity, string key, Sprite portrait)
    {
        if (targetEntity == null)
            return;
        
        _bossHp.Active(targetEntity,key,portrait);
    }

    public void DisableBossHp()
    {
        _bossHp.Disable();
    }
    
    public void NotifyToUI(string key)
    {
        if (_notifySubscriber.TryGetValue(key, out var subscriberList) == false)
        {
            return;
        }

        foreach (var action in subscriberList)
        {
            action?.Invoke();
        }
    }

    private void LateUpdate()
    {
        _titleMenuUI.UpdateByManager();

        if (_targetEntity == null)
        {
            return;
        }

        if (_pauseUI.IsActive() == true)
        {
            return;
        }

        var deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        _hpBpGaugeUI.UpdateByManager(_targetEntity.getStatusPercentage("HP"), _targetEntity.getStatusPercentage("Blood"), _targetEntity.getStatusPercentage("IsCatched"), _targetEntity.getStatusPercentage("IsStun"));
        _dashPointUI.UpdateByManager(deltaTime, _targetEntity.getStatus("DashPoint"));
        _crossHairUI.UpdateByManager(_targetEntity, _targetEntity.isDead(), _targetEntity.transform.position, _targetEntity.getStatus("DashPoint"));
        _enemyIndicator.UpdateByManager();
        _targetFollower.UpdateByManager(_targetEntity.transform.position);
        _textBubble.UpdateByManager();
        _enemyHp.UpdateByManager(deltaTime);
        _bossHp.UpdateByManager();
    }

    private void SetBinder()
    {
        var binderList = new List<UIObjectBinder>();
        binderList.Add(HpBpGaugeUIBinder);
        binderList.Add(DashPointBinder);
        binderList.Add(CrosshairBinder);
        binderList.Add(EnemyIndicatorBinder);
        binderList.Add(FollowerBinder);
        binderList.Add(TextBubblePoolBinder);
        binderList.Add(EnemyHpBinder);
        binderList.Add(BossHpBinder);
        binderList.Add(TitleMenuUIBinder);
        binderList.Add(PauseUIBinder);

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

        _hpBpGaugeUI = new HpBpGaugeUI();
        _dashPointUI = new DashPointUI();
        _crossHairUI = new CrosshairUI();
        _enemyIndicator = new EnemyIndicator();
        _targetFollower = new TargetFollower();
        _textBubble = new TextBubble();
        _enemyHp = new EnemyHp();
        _bossHp = new BossHpUI();
        _titleMenuUI = new TitleMenuUI();
        _pauseUI = new PauseUI();
        
        TextBubble = _textBubble;

        _hpBpGaugeUI.SetBinder(HpBpGaugeUIBinder);
        _dashPointUI.SetBinder(DashPointBinder);
        _crossHairUI.SetBinder(CrosshairBinder);
        _enemyIndicator.SetBinder(EnemyIndicatorBinder);
        _targetFollower.SetBinder(FollowerBinder);
        _textBubble.SetBinder(TextBubblePoolBinder);
        _enemyHp.SetBinder(EnemyHpBinder);
        _bossHp.SetBinder(BossHpBinder);
        _titleMenuUI.SetBinder(TitleMenuUIBinder);
        _pauseUI.SetBinder(PauseUIBinder);
    }

    private void CheckValidUI()
    {
        var uiElementList = new List<IUIElement>();
        uiElementList.Add(_hpBpGaugeUI);
        uiElementList.Add(_dashPointUI);
        uiElementList.Add(_crossHairUI);
        uiElementList.Add(_enemyIndicator);
        uiElementList.Add(_targetFollower);
        uiElementList.Add(_textBubble);
        uiElementList.Add(_enemyHp);
        uiElementList.Add(_bossHp);
        uiElementList.Add(_titleMenuUI);
        uiElementList.Add(_pauseUI);

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

    private void BindNotifySubscriber()
    {
        _notifySubscriber.Add("Dash", new List<Action>());
        _notifySubscriber.Add("HitEnemy", new List<Action>());
        
        _notifySubscriber["Dash"].Add(_crossHairUI.OnDash);
        _notifySubscriber["HitEnemy"].Add(_crossHairUI.OnHitEnemy);
    }

    private void OnInitError()
    {
#if UNITY_EDITOR
        Application.Quit();
#endif
    }
}

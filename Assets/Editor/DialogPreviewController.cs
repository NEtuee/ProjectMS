using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogPreviewController
{
    // 인게임 해상도 비율 (1600x1200)
    private readonly float kGameAspectRatio = 1600f / 1200f;
    private readonly Vector2 kPreviewSize = new Vector2(400f, 300f); // 프리뷰 패널 크기 (게임 해상도의 1/4)
    
    private VisualElement _previewContainer;
    private VisualElement _gameViewContainer;
    private VisualElement _dialogUIContainer;
    private VisualElement _characterContainer;
    
    // 대화 UI 요소들
    private Label _characterNameLabel;
    private Label _dialogTextLabel;
    private VisualElement _dialogBox;
    
    // 캐릭터 스프라이트 컨테이너들
    private Dictionary<int, VisualElement> _characterSprites = new Dictionary<int, VisualElement>();
    
    // 프리뷰 컨트롤 요소들
    private DropdownField _entryDropdown;
    private Button _playButton;
    private Button _pauseButton;
    private Button _resetButton;
    private Button _nextButton;
    private Slider _speedSlider;
    
    // 시뮬레이션 상태
    private DialogData _currentDialogData;
    private DialogEventEntryData _currentEntry;
    private int _currentEventIndex = 0;
    private bool _isPlaying = false;
    private bool _isPaused = false;
    private float _playSpeed = 1.0f;
    
    // 텍스트 타이핑 애니메이션
    private string _currentFullText = "";
    private int _currentVisibleChars = 0;
    private double _lastTime = 0;
    private float _wordsPerSecond = 12f;
    
    // Static Data 로딩 상태
    private bool _isStaticDataLoaded = false;
    private Label _statusLabel;

    public VisualElement CreatePreviewPanel()
    {
        _previewContainer = new VisualElement();
        _previewContainer.style.width = kPreviewSize.x;
        _previewContainer.style.height = Length.Percent(100);
        _previewContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f));
        _previewContainer.style.borderLeftWidth = 2;
        _previewContainer.style.borderLeftColor = new StyleColor(Color.gray);
        _previewContainer.style.flexDirection = FlexDirection.Column;
        _previewContainer.style.marginTop = 25; // 툴바 높이만큼 여백 추가

        CreateControlPanel();
        CreateGameView();

        // Static Data 로딩 초기화
        InitializeStaticData();

        return _previewContainer;
    }

    private void CreateControlPanel()
    {
        var controlPanel = new VisualElement();
        controlPanel.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f));
        controlPanel.style.paddingTop = 5;
        controlPanel.style.paddingBottom = 5;
        controlPanel.style.paddingLeft = 5;
        controlPanel.style.paddingRight = 5;
        controlPanel.style.borderBottomWidth = 1;
        controlPanel.style.borderBottomColor = new StyleColor(Color.gray);

        // Entry 선택 드롭다운
        _entryDropdown = new DropdownField("Entry:");
        _entryDropdown.style.marginBottom = 5;
        _entryDropdown.RegisterValueChangedCallback(evt => OnEntrySelected(evt.newValue));
        controlPanel.Add(_entryDropdown);

        // 컨트롤 버튼들
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.marginBottom = 5;

        _playButton = new Button(OnPlayClicked) { text = "▶" };
        _playButton.style.width = 30;
        _playButton.style.marginRight = 2;
        buttonContainer.Add(_playButton);

        _pauseButton = new Button(OnPauseClicked) { text = "⏸" };
        _pauseButton.style.width = 30;
        _pauseButton.style.marginRight = 2;
        _pauseButton.SetEnabled(false);
        buttonContainer.Add(_pauseButton);

        _resetButton = new Button(OnResetClicked) { text = "⏹" };
        _resetButton.style.width = 30;
        _resetButton.style.marginRight = 5;
        buttonContainer.Add(_resetButton);

        _nextButton = new Button(OnNextClicked) { text = "Next" };
        _nextButton.style.flexGrow = 1;
        buttonContainer.Add(_nextButton);

        controlPanel.Add(buttonContainer);

        // 속도 조절 슬라이더
        _speedSlider = new Slider("Speed:", 0.1f, 3.0f);
        _speedSlider.value = 1.0f;
        _speedSlider.RegisterValueChangedCallback(evt => _playSpeed = evt.newValue);
        controlPanel.Add(_speedSlider);

        // 상태 표시 라벨
        _statusLabel = new Label("Initializing...");
        _statusLabel.style.color = Color.yellow;
        _statusLabel.style.fontSize = 10;
        _statusLabel.style.marginTop = 5;
        _statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        controlPanel.Add(_statusLabel);

        _previewContainer.Add(controlPanel);
    }

    private void CreateGameView()
    {
        _gameViewContainer = new VisualElement();
        _gameViewContainer.style.flexGrow = 1;
        _gameViewContainer.style.backgroundColor = new StyleColor(new Color(0.05f, 0.05f, 0.1f, 1f));
        _gameViewContainer.style.position = Position.Relative;

        // 캐릭터 스프라이트 컨테이너 (게임 화면 전체)
        _characterContainer = new VisualElement();
        _characterContainer.style.position = Position.Absolute;
        _characterContainer.style.width = Length.Percent(100);
        _characterContainer.style.height = Length.Percent(100);
        _gameViewContainer.Add(_characterContainer);

        // 대화 UI 컨테이너 (하단에 고정)
        _dialogUIContainer = new VisualElement();
        _dialogUIContainer.style.position = Position.Absolute;
        _dialogUIContainer.style.bottom = 0;
        _dialogUIContainer.style.left = 0;
        _dialogUIContainer.style.right = 0;
        _dialogUIContainer.style.height = 120; // 대화창 높이
        
        CreateDialogUI();
        _gameViewContainer.Add(_dialogUIContainer);
        
        _previewContainer.Add(_gameViewContainer);
    }

    private void CreateDialogUI()
    {
        // 대화창 배경
        _dialogBox = new VisualElement();
        _dialogBox.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.8f));
        _dialogBox.style.borderTopWidth = 2;
        _dialogBox.style.borderTopColor = new StyleColor(Color.white);
        _dialogBox.style.paddingTop = 10;
        _dialogBox.style.paddingBottom = 10;
        _dialogBox.style.paddingLeft = 15;
        _dialogBox.style.paddingRight = 15;
        _dialogBox.style.width = Length.Percent(100);
        _dialogBox.style.height = Length.Percent(100);

        // 캐릭터 이름
        _characterNameLabel = new Label("");
        _characterNameLabel.style.color = Color.yellow;
        _characterNameLabel.style.fontSize = 14;
        _characterNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _characterNameLabel.style.marginBottom = 5;
        _dialogBox.Add(_characterNameLabel);

        // 대화 텍스트
        _dialogTextLabel = new Label("");
        _dialogTextLabel.style.color = Color.white;
        _dialogTextLabel.style.fontSize = 12;
        _dialogTextLabel.style.whiteSpace = WhiteSpace.Normal;
        _dialogTextLabel.style.flexGrow = 1;
        _dialogBox.Add(_dialogTextLabel);

        _dialogUIContainer.Add(_dialogBox);
        
        // 초기에는 숨김
        _dialogUIContainer.style.display = DisplayStyle.None;
    }

    public void SetDialogData(DialogData dialogData)
    {
        _currentDialogData = dialogData;
        UpdateEntryDropdown();
        ResetPreview();
    }

    private void UpdateEntryDropdown()
    {
        if (_currentDialogData == null || _entryDropdown == null)
            return;

        var entryList = new List<string>();
        foreach (var entry in _currentDialogData._dialogEventEntryList)
        {
            entryList.Add(entry._entryKey);
        }
        
        _entryDropdown.choices = entryList;
        if (entryList.Count > 0)
        {
            _entryDropdown.value = entryList[0];
            OnEntrySelected(entryList[0]);
        }
    }

    private void OnEntrySelected(string entryKey)
    {
        if (_currentDialogData == null)
            return;

        _currentEntry = _currentDialogData._dialogEventEntryList.Find(e => e._entryKey == entryKey);
        ResetPreview();
        UpdateButtonStates();
    }

    private void OnPlayClicked()
    {
        if (_currentEntry == null)
            return;

        // Static Data가 로드되지 않았으면 로드 후 재시도
        if (!_isStaticDataLoaded)
        {
            UpdateStatusLabel("Loading Static Data...");
            EditorApplication.delayCall += () =>
            {
                LoadStaticData();
                // 로딩 완료 후 다시 시도
                EditorApplication.delayCall += OnPlayClicked;
            };
            return;
        }

        _isPlaying = true;
        _isPaused = false;
        _playButton.SetEnabled(false);
        _pauseButton.SetEnabled(true);
        
        StartDialogSimulation();
    }

    private void OnPauseClicked()
    {
        _isPaused = !_isPaused;
        _pauseButton.text = _isPaused ? "▶" : "⏸";
    }

    private void OnResetClicked()
    {
        ResetPreview();
    }

    private void OnNextClicked()
    {
        // Static Data가 로드되지 않았으면 먼저 로드
        if (!_isStaticDataLoaded)
        {
            UpdateStatusLabel("Loading Static Data...");
            EditorApplication.delayCall += () =>
            {
                LoadStaticData();
                // 로딩 완료 후 다시 시도
                EditorApplication.delayCall += OnNextClicked;
            };
            return;
        }

        if (_currentEntry != null && _currentEventIndex < _currentEntry._dialogEventList.Count)
        {
            ExecuteDialogEvent(_currentEntry._dialogEventList[_currentEventIndex]);
            _currentEventIndex++;
            
            if (_currentEventIndex >= _currentEntry._dialogEventList.Count)
            {
                _isPlaying = false;
                _playButton.SetEnabled(true);
                _pauseButton.SetEnabled(false);
            }
        }
    }

    private void ResetPreview()
    {
        _isPlaying = false;
        _isPaused = false;
        _currentEventIndex = 0;
        _currentVisibleChars = 0;
        _lastTime = EditorApplication.timeSinceStartup;
        
        _playButton.SetEnabled(true);
        _pauseButton.SetEnabled(false);
        _pauseButton.text = "⏸";
        
        // UI 초기화
        _characterNameLabel.text = "";
        _dialogTextLabel.text = "";
        _dialogUIContainer.style.display = DisplayStyle.None;
        
        // 캐릭터 스프라이트 제거
        ClearCharacterSprites();
    }

    private void StartDialogSimulation()
    {
        if (_currentEntry == null || _currentEntry._dialogEventList.Count == 0)
            return;

        _currentEventIndex = 0;
        ExecuteNextEvent();
    }

    private void ExecuteNextEvent()
    {
        if (_isPaused || !_isPlaying || _currentEntry == null || _currentEventIndex >= _currentEntry._dialogEventList.Count)
            return;

        var eventData = _currentEntry._dialogEventList[_currentEventIndex];
        ExecuteDialogEvent(eventData);
        
        _currentEventIndex++;
        
        // 다음 이벤트가 있으면 계속 진행 (Dialog 이벤트가 아닌 경우)
        if (eventData.getDialogEventType() != DialogEventType.Dialog && _currentEventIndex < _currentEntry._dialogEventList.Count)
        {
            EditorApplication.delayCall += ExecuteNextEvent;
        }
        else if (_currentEventIndex >= _currentEntry._dialogEventList.Count)
        {
            // 시뮬레이션 완료
            _isPlaying = false;
            _playButton.SetEnabled(true);
            _pauseButton.SetEnabled(false);
        }
    }

    private void ExecuteDialogEvent(DialogEventDataBase eventData)
    {
        switch (eventData.getDialogEventType())
        {
            case DialogEventType.Dialog:
                var dialogEvent = eventData as DialogEventData_Dialog;
                ShowDialog(dialogEvent);
                break;
                
            case DialogEventType.ActiveObject:
                var activeObjectEvent = eventData as DialogEventData_ActiveObject;
                ShowCharacterSprite(activeObjectEvent);
                break;
                
            case DialogEventType.SetSprite:
                var setSpriteEvent = eventData as DialogEventData_SetSprite;
                UpdateCharacterSprite(setSpriteEvent);
                break;
                
            case DialogEventType.SetPosition:
                var setPositionEvent = eventData as DialogEventData_SetPosition;
                UpdateCharacterPosition(setPositionEvent);
                break;
        }
    }

    private void ShowDialog(DialogEventData_Dialog dialogEvent)
    {
        _dialogUIContainer.style.display = DisplayStyle.Flex;
        
        // 캐릭터 이름 설정
        if (!string.IsNullOrEmpty(dialogEvent._displayCharacterKey))
        {
            try
            {
                var characterInfo = CharacterInfoManager.Instance()?.GetCharacterInfoData(dialogEvent._displayCharacterKey);
                _characterNameLabel.text = characterInfo?._displayName ?? dialogEvent._displayCharacterKey;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Dialog Preview: Failed to get character info for key '{dialogEvent._displayCharacterKey}': {e.Message}");
                _characterNameLabel.text = dialogEvent._displayCharacterKey;
            }
        }
        else
        {
            _characterNameLabel.text = "";
        }
        
        // 텍스트 데이터 가져오기
        try
        {
            if (LanguageManager._instance != null)
            {
                var textData = LanguageManager._instance.getStringKeyValueFromIndex(ref _currentDialogData._textDataName, dialogEvent._dialogIndex);
                _currentFullText = textData?._value ?? $"[Text Index: {dialogEvent._dialogIndex}]";
            }
            else
            {
                _currentFullText = $"[Preview Mode - Dialog Index: {dialogEvent._dialogIndex}]";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Dialog Preview: Failed to get text data: {e.Message}");
            _currentFullText = $"[Error loading text - Index: {dialogEvent._dialogIndex}]";
        }
        
        _wordsPerSecond = dialogEvent._wordPerSec * _playSpeed;
        
        // 타이핑 애니메이션 시작
        _currentVisibleChars = 0;
        _lastTime = EditorApplication.timeSinceStartup;
        StartTextAnimation();
    }

    private void StartTextAnimation()
    {
        EditorApplication.update += UpdateTextAnimation;
    }

    private void UpdateTextAnimation()
    {
        if (_isPaused || !_isPlaying)
            return;

        double currentTime = EditorApplication.timeSinceStartup;
        double deltaTime = currentTime - _lastTime;
        _lastTime = currentTime;
        
        int targetChars = Mathf.Min((int)(deltaTime * _wordsPerSecond * _playSpeed) + _currentVisibleChars, _currentFullText.Length);
        
        if (_currentVisibleChars < targetChars)
        {
            _currentVisibleChars = targetChars;
            _dialogTextLabel.text = _currentFullText.Substring(0, _currentVisibleChars);
        }
        
        // 텍스트 애니메이션 완료
        if (_currentVisibleChars >= _currentFullText.Length)
        {
            EditorApplication.update -= UpdateTextAnimation;
            
            // 잠시 후 다음 이벤트 실행
            EditorApplication.delayCall += () => 
            {
                if (_isPlaying && !_isPaused)
                {
                    EditorApplication.delayCall += ExecuteNextEvent;
                }
            };
        }
    }

    private void ShowCharacterSprite(DialogEventData_ActiveObject activeObjectEvent)
    {
        try
        {
            var sprite = Resources.Load<Sprite>(activeObjectEvent._spritePath);
            if (sprite != null)
            {
                CreateCharacterSprite(activeObjectEvent._objectId, sprite, activeObjectEvent._position, activeObjectEvent._flip);
            }
            else
            {
                Debug.LogWarning($"Dialog Preview: Sprite not found at path: {activeObjectEvent._spritePath}");
                // 플레이스홀더 스프라이트 생성
                CreatePlaceholderSprite(activeObjectEvent._objectId, activeObjectEvent._position, activeObjectEvent._flip);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Dialog Preview: Failed to load sprite '{activeObjectEvent._spritePath}': {e.Message}");
            CreatePlaceholderSprite(activeObjectEvent._objectId, activeObjectEvent._position, activeObjectEvent._flip);
        }
    }

    private void UpdateCharacterSprite(DialogEventData_SetSprite setSpriteEvent)
    {
        if (_characterSprites.ContainsKey(setSpriteEvent._objectId))
        {
            try
            {
                var sprite = Resources.Load<Sprite>(setSpriteEvent._spritePath);
                if (sprite != null)
                {
                    var spriteElement = _characterSprites[setSpriteEvent._objectId];
                    UpdateSpriteVisual(spriteElement, sprite, setSpriteEvent._flip);
                }
                else
                {
                    Debug.LogWarning($"Dialog Preview: Sprite not found for update: {setSpriteEvent._spritePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Dialog Preview: Failed to update sprite '{setSpriteEvent._spritePath}': {e.Message}");
            }
        }
    }

    private void UpdateCharacterPosition(DialogEventData_SetPosition setPositionEvent)
    {
        if (_characterSprites.ContainsKey(setPositionEvent._objectId))
        {
            var spriteElement = _characterSprites[setPositionEvent._objectId];
            UpdateSpritePosition(spriteElement, setPositionEvent._position, setPositionEvent._flip);
        }
    }

    private void CreateCharacterSprite(int objectId, Sprite sprite, Vector2 position, bool flip)
    {
        // 기존 스프라이트가 있으면 제거
        if (_characterSprites.ContainsKey(objectId))
        {
            _characterContainer.Remove(_characterSprites[objectId]);
        }

        var spriteElement = new VisualElement();
        spriteElement.style.position = Position.Absolute;
        spriteElement.style.backgroundImage = new StyleBackground(sprite.texture);
        spriteElement.style.width = 80; // 프리뷰 크기 조정
        spriteElement.style.height = 80;
        
        UpdateSpritePosition(spriteElement, position, flip);
        UpdateSpriteVisual(spriteElement, sprite, flip);
        
        _characterContainer.Add(spriteElement);
        _characterSprites[objectId] = spriteElement;
    }

    private void UpdateSpritePosition(VisualElement spriteElement, Vector2 position, bool flip)
    {
        // 게임 좌표를 프리뷰 좌표로 변환 (1600x1200 -> 프리뷰 크기)
        var containerRect = _characterContainer.contentRect;
        float scaleX = containerRect.width / 1600f;
        float scaleY = containerRect.height / 1200f;
        
        // 게임에서는 중앙이 (0,0), 하단이 기준점
        float previewX = (position.x * scaleX) + (containerRect.width * 0.5f);
        float previewY = containerRect.height - (position.y * scaleY) - 80; // 스프라이트 높이만큼 오프셋
        
        spriteElement.style.left = previewX;
        spriteElement.style.top = previewY;
    }

    private void UpdateSpriteVisual(VisualElement spriteElement, Sprite sprite, bool flip)
    {
        spriteElement.style.backgroundImage = new StyleBackground(sprite.texture);
        
        if (flip)
        {
            spriteElement.style.scale = new StyleScale(new Scale(new Vector3(-1, 1, 1)));
        }
        else
        {
            spriteElement.style.scale = new StyleScale(new Scale(Vector3.one));
        }
    }

    private void ClearCharacterSprites()
    {
        foreach (var sprite in _characterSprites.Values)
        {
            _characterContainer.Remove(sprite);
        }
        _characterSprites.Clear();
    }

    private void CreatePlaceholderSprite(int objectId, Vector2 position, bool flip)
    {
        // 기존 스프라이트가 있으면 제거
        if (_characterSprites.ContainsKey(objectId))
        {
            _characterContainer.Remove(_characterSprites[objectId]);
        }

        var spriteElement = new VisualElement();
        spriteElement.style.position = Position.Absolute;
        spriteElement.style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.8f));
        spriteElement.style.borderTopWidth = 2;
        spriteElement.style.borderBottomWidth = 2;
        spriteElement.style.borderLeftWidth = 2;
        spriteElement.style.borderRightWidth = 2;
        spriteElement.style.borderTopColor = Color.red;
        spriteElement.style.borderBottomColor = Color.red;
        spriteElement.style.borderLeftColor = Color.red;
        spriteElement.style.borderRightColor = Color.red;
        spriteElement.style.width = 80;
        spriteElement.style.height = 80;
        
        // "NO IMAGE" 텍스트 추가
        var label = new Label("NO\nIMAGE");
        label.style.color = Color.red;
        label.style.fontSize = 10;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.position = Position.Absolute;
        label.style.width = 80;
        label.style.height = 80;
        spriteElement.Add(label);
        
        UpdateSpritePosition(spriteElement, position, flip);
        
        if (flip)
        {
            spriteElement.style.scale = new StyleScale(new Scale(new Vector3(-1, 1, 1)));
        }
        
        _characterContainer.Add(spriteElement);
        _characterSprites[objectId] = spriteElement;
    }

    // Static Data 로딩 관련 메서드들
    private void InitializeStaticData()
    {
        if (!_isStaticDataLoaded)
        {
            UpdateStatusLabel("Loading Static Data...");
            EditorApplication.delayCall += LoadStaticData;
        }
        else
        {
            UpdateStatusLabel("Static Data Loaded");
        }
    }

    private void LoadStaticData()
    {
        try
        {
            UpdateStatusLabel("Initializing Resource Map...");
            
            // ResourceMap 인스턴스 확인
            if (ResourceMap.Instance() == null)
            {
                Debug.LogWarning("Dialog Preview: ResourceMap instance not available, using fallback");
            }

            UpdateStatusLabel("Loading Character Info...");
            
            // StaticDataLoader의 readStaticDataAll 메서드를 사용하여 모든 static data 로드
            try
            {
                StaticDataLoader.readStaticDataAll(ResourceMap.Instance());
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Dialog Preview: Failed to load some static data via StaticDataLoader: {e.Message}");
            }
            
            UpdateStatusLabel("Initializing Language Manager...");
            
            // Language Manager 초기화 (에디터 모드에서는 제한적)
            try
            {
                if (LanguageManager._instance == null)
                {
                    // 에디터 모드에서는 LanguageManager를 직접 생성하지 않고 경고만 표시
                    Debug.LogWarning("Dialog Preview: LanguageManager not available in editor mode - text display may be limited");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Dialog Preview: LanguageManager initialization issue: {e.Message}");
            }
            
            UpdateStatusLabel("Checking Character Info Manager...");
            
            // CharacterInfoManager 확인
            try
            {
                var characterManager = CharacterInfoManager.Instance();
                if (characterManager != null)
                {
                    Debug.Log("Dialog Preview: CharacterInfoManager is available");
                }
                else
                {
                    Debug.LogWarning("Dialog Preview: CharacterInfoManager not available");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Dialog Preview: CharacterInfoManager check failed: {e.Message}");
            }

            _isStaticDataLoaded = true;
            UpdateStatusLabel("Ready for Preview");
            
            Debug.Log("Dialog Preview: Static Data loading completed (with editor mode limitations)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Dialog Preview: Critical error loading static data - {e.Message}");
            UpdateStatusLabel("Static Data Load Failed");
            _isStaticDataLoaded = false;
        }
    }

    private void UpdateStatusLabel(string message)
    {
        if (_statusLabel != null)
        {
            _statusLabel.text = message;
            
            // 상태에 따라 라벨 색상 변경
            if (message.Contains("Loading") || message.Contains("Initializing"))
            {
                _statusLabel.style.color = Color.yellow;
            }
            else if (message.Contains("Failed") || message.Contains("Error"))
            {
                _statusLabel.style.color = Color.red;
            }
            else if (message.Contains("Ready") || message.Contains("Success"))
            {
                _statusLabel.style.color = Color.green;
            }
            else
            {
                _statusLabel.style.color = Color.white;
            }
        }
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (_playButton != null && _nextButton != null)
        {
            bool dataReady = _isStaticDataLoaded;
            bool hasEntry = _currentEntry != null;
            
            _playButton.SetEnabled(dataReady && hasEntry && !_isPlaying);
            _nextButton.SetEnabled(dataReady && hasEntry);
            
            // 툴팁 설정
            if (!dataReady)
            {
                _playButton.tooltip = "Static Data is loading...";
                _nextButton.tooltip = "Static Data is loading...";
            }
            else if (!hasEntry)
            {
                _playButton.tooltip = "Select an entry to preview";
                _nextButton.tooltip = "Select an entry to preview";
            }
            else
            {
                _playButton.tooltip = "";
                _nextButton.tooltip = "";
            }
        }
    }

    public bool IsStaticDataLoaded()
    {
        return _isStaticDataLoaded;
    }
}

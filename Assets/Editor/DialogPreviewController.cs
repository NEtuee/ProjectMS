using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogPreviewController
{
    // 프리뷰 해상도 (4:3 비율, 800x600)
    private readonly Vector2 kPreviewSize = new Vector2(800f, 600f);
    private readonly Vector2 kDialogBoxSize = new Vector2(800f, 143f);
    private readonly float kSpriteScale = 0.21f; // DialogManager의 kDefaultScale과 동일
    private readonly float kControlPanelHeight = 85f; // 컨트롤 패널의 고정 높이
    
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
    private Button _resetButton;
    private Button _nextButton;
    private Button _stepButton;
    private Slider _speedSlider;
    
    // 시뮬레이션 상태
    private DialogData _currentDialogData;
    private DialogEventEntryData _currentEntry;
    private int _currentEventIndex = 0;
    
    // Static Data 로딩 상태
    private bool _isStaticDataLoaded = false;
    private Label _statusLabel;

    public VisualElement CreatePreviewPanel()
    {
        _previewContainer = new VisualElement();
        _previewContainer.style.width = kPreviewSize.x;
        _previewContainer.style.height = kPreviewSize.y + kControlPanelHeight; // 컨트롤 패널 높이 포함
        _previewContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f));
        _previewContainer.style.borderLeftWidth = 2;
        _previewContainer.style.borderLeftColor = new StyleColor(Color.gray);
        _previewContainer.style.flexDirection = FlexDirection.Column;
        _previewContainer.style.position = Position.Absolute;
        _previewContainer.style.left = 0;
        _previewContainer.style.top = 20;

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
        controlPanel.style.height = kControlPanelHeight;
        controlPanel.style.width = kPreviewSize.x;

        // Entry 선택 드롭다운
        _entryDropdown = new DropdownField("Entry:");
        _entryDropdown.style.marginBottom = 5;
        _entryDropdown.RegisterValueChangedCallback(evt => OnEntrySelected(evt.newValue));
        controlPanel.Add(_entryDropdown);

        // 컨트롤 버튼들
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.marginBottom = 5;

        _resetButton = new Button(OnResetClicked) { text = "⏹" };
        _resetButton.style.width = 30;
        _resetButton.style.marginRight = 5;
        buttonContainer.Add(_resetButton);

        _nextButton = new Button(OnNextDialogClicked) { text = "Next" };
        _nextButton.style.width = 50;
        _nextButton.style.marginRight = 2;
        buttonContainer.Add(_nextButton);

        _stepButton = new Button(OnNextClicked) { text = "Step" };
        _stepButton.style.flexGrow = 1;
        buttonContainer.Add(_stepButton);

        controlPanel.Add(buttonContainer);

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
        // 게임 뷰 높이 = 전체 높이 - 컨트롤 패널 높이
        float gameViewHeight = kPreviewSize.y;// - kControlPanelHeight;
        
        _gameViewContainer = new VisualElement();
        _gameViewContainer.style.width = kPreviewSize.x;
        _gameViewContainer.style.height = gameViewHeight;
        _gameViewContainer.style.backgroundColor = new StyleColor(new Color(0.05f, 0.05f, 0.1f, 1f));
        _gameViewContainer.style.position = Position.Relative;
        // 컨트롤 패널 높이만큼 아래로 이동하여 가려지지 않도록 함
        _gameViewContainer.style.top = 0; // Relative position이므로 자동으로 컨트롤 패널 아래에 배치됨
        
        // 캐릭터 스프라이트 컨테이너 (게임 화면에서 대화창 제외한 영역)
        float characterAreaHeight = gameViewHeight - kDialogBoxSize.y;
        
        _characterContainer = new VisualElement();
        _characterContainer.style.position = Position.Absolute;
        _characterContainer.style.width = kPreviewSize.x;
        _characterContainer.style.height = characterAreaHeight;
        _characterContainer.style.top = 0;
        _characterContainer.style.left = 0;
        _gameViewContainer.Add(_characterContainer);

        // 대화 UI 컨테이너 (하단에 고정, 800x143)
        _dialogUIContainer = new VisualElement();
        _dialogUIContainer.style.position = Position.Absolute;
        _dialogUIContainer.style.bottom = 0;
        _dialogUIContainer.style.left = 0;
        _dialogUIContainer.style.width = kDialogBoxSize.x;
        _dialogUIContainer.style.height = kDialogBoxSize.y;
        
        CreateDialogUI();
        _gameViewContainer.Add(_dialogUIContainer);
        
        _previewContainer.Add(_gameViewContainer);
    }

    private void CreateDialogUI()
    {
        // 대화창 배경 (800x143 크기)
        _dialogBox = new VisualElement();
        _dialogBox.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.8f));
        _dialogBox.style.borderTopWidth = 2;
        _dialogBox.style.borderTopColor = new StyleColor(Color.white);
        _dialogBox.style.paddingTop = 10;
        _dialogBox.style.paddingBottom = 10;
        _dialogBox.style.paddingLeft = 15;
        _dialogBox.style.paddingRight = 15;
        _dialogBox.style.width = kDialogBoxSize.x;
        _dialogBox.style.height = kDialogBoxSize.y;
        _dialogBox.style.position = Position.Absolute;
        _dialogBox.style.bottom = 0;
        _dialogBox.style.left = 0;

        // 캐릭터 이름
        _characterNameLabel = new Label("");
        _characterNameLabel.style.color = Color.yellow;
        _characterNameLabel.style.fontSize = 14;
        _characterNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _characterNameLabel.style.marginBottom = 5;
        _characterNameLabel.style.height = 20;
        _dialogBox.Add(_characterNameLabel);

        // 대화 텍스트
        _dialogTextLabel = new Label("");
        _dialogTextLabel.style.color = Color.white;
        _dialogTextLabel.style.fontSize = 12;
        _dialogTextLabel.style.whiteSpace = WhiteSpace.Normal;
        _dialogTextLabel.style.flexGrow = 1;
        _dialogTextLabel.style.maxHeight = kDialogBoxSize.y - 50; // 여백 고려
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
            
            // 버튼 상태 업데이트
            UpdateButtonStates();
            
            if (_currentEventIndex >= _currentEntry._dialogEventList.Count)
            {
                // 완료됨 - 더 이상 할 일 없음
            }
        }
    }

    private void OnNextDialogClicked()
    {
        // Static Data가 로드되지 않았으면 먼저 로드
        if (!_isStaticDataLoaded)
        {
            UpdateStatusLabel("Loading Static Data...");
            EditorApplication.delayCall += () =>
            {
                LoadStaticData();
                // 로딩 완료 후 다시 시도
                EditorApplication.delayCall += OnNextDialogClicked;
            };
            return;
        }

        if (_currentEntry != null && _currentEventIndex < _currentEntry._dialogEventList.Count)
        {
            ExecuteToNextDialog();
        }
    }

    private void ExecuteToNextDialog()
    {
        bool foundDialog = false;
        
        // 현재 이벤트부터 시작하여 다음 Dialog 이벤트까지 실행
        while (_currentEventIndex < _currentEntry._dialogEventList.Count && !foundDialog)
        {
            var eventData = _currentEntry._dialogEventList[_currentEventIndex];
            ExecuteDialogEvent(eventData);
            _currentEventIndex++;
            
            // Dialog 이벤트를 찾았으면 중단
            if (eventData.getDialogEventType() == DialogEventType.Dialog)
            {
                foundDialog = true;
            }
        }
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
        
        // 모든 이벤트가 완료되었으면 자동으로 멈춤
        if (_currentEventIndex >= _currentEntry._dialogEventList.Count)
        {
            // 더 이상 진행할 이벤트가 없음
        }
    }

    private void ResetPreview()
    {
        _currentEventIndex = 0;
        
        // UI 초기화
        _characterNameLabel.text = "";
        _dialogTextLabel.text = "";
        _dialogUIContainer.style.display = DisplayStyle.None;
        
        // 캐릭터 스프라이트 제거
        ClearCharacterSprites();
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
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
        if (_currentEntry == null || _currentEventIndex >= _currentEntry._dialogEventList.Count)
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
            UpdateButtonStates();
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
            if (LanguageInstanceManager.Instance() != null)
            {
                var textData = LanguageInstanceManager.Instance().GetStringKeyValueFromIndex(_currentDialogData._textDataName, dialogEvent._dialogIndex);
                _dialogTextLabel.text = textData?._value ?? $"[Text Index: {dialogEvent._dialogIndex}]";
            }
            else
            {
                _dialogTextLabel.text = $"[Preview Mode - Dialog Index: {dialogEvent._dialogIndex}]";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Dialog Preview: Failed to get text data: {e.Message}");
            _dialogTextLabel.text = $"[Error loading text - Index: {dialogEvent._dialogIndex}]";
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
                    
                    // 새로운 스프라이트 크기 계산
                    float spriteWidth = sprite.texture.width * kSpriteScale;
                    float spriteHeight = sprite.texture.height * kSpriteScale;
                    
                    spriteElement.style.width = spriteWidth;
                    spriteElement.style.height = spriteHeight;
                    
                    UpdateSpriteVisual(spriteElement, sprite, setSpriteEvent._flip);
                    
                    // 현재 위치 정보를 가져와서 다시 설정 (크기가 바뀌었으므로)
                    // spriteElement의 현재 위치를 기반으로 게임 좌표를 역계산
                    float currentLeft = spriteElement.style.left.value.value;
                    float currentTop = spriteElement.style.top.value.value;
                    
                    // 게임 좌표로 역변환 (수정된 UpdateSpritePosition 로직에 맞춤)
                    float gameX = currentLeft - (kPreviewSize.x * 0.5f) + (spriteWidth * 0.5f);
                    float gameY = kPreviewSize.y - currentTop - spriteHeight;
                    
                    // 새로운 크기로 위치 재설정
                    UpdateSpritePosition(spriteElement, new Vector2(gameX, gameY), setSpriteEvent._flip, spriteWidth, spriteHeight);
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
            float spriteWidth = spriteElement.style.width.value.value;
            float spriteHeight = spriteElement.style.height.value.value;
            
            UpdateSpritePosition(spriteElement, setPositionEvent._position, setPositionEvent._flip, spriteWidth, spriteHeight);
            
            // Flip 상태 업데이트
            if (setPositionEvent._flip)
            {
                spriteElement.style.scale = new StyleScale(new Scale(new Vector3(-1, 1, 1)));
            }
            else
            {
                spriteElement.style.scale = new StyleScale(new Scale(Vector3.one));
            }
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
        
        // 스프라이트 크기 계산 (0.21배 스케일 적용)
        float spriteWidth = sprite.texture.width * kSpriteScale;
        float spriteHeight = sprite.texture.height * kSpriteScale;
        
        spriteElement.style.width = spriteWidth;
        spriteElement.style.height = spriteHeight;
        
        // 스프라이트 이미지 설정
        spriteElement.style.backgroundImage = new StyleBackground(sprite.texture);
        spriteElement.style.unityBackgroundImageTintColor = Color.white;
        
        UpdateSpritePosition(spriteElement, position, flip, spriteWidth, spriteHeight);
        UpdateSpriteVisual(spriteElement, sprite, flip);
        
        _characterContainer.Add(spriteElement);
        _characterSprites[objectId] = spriteElement;
    }

    private void UpdateSpritePosition(VisualElement spriteElement, Vector2 position, bool flip, float spriteWidth = 0, float spriteHeight = 0)
    {
        // 스프라이트 크기가 제공되지 않으면 현재 크기 사용
        if (spriteWidth == 0) spriteWidth = spriteElement.style.width.value.value;
        if (spriteHeight == 0) spriteHeight = spriteElement.style.height.value.value;
        
        // 게임 좌표계에서 프리뷰 좌표계로 변환
        // 게임: 중앙이 (0,0), Y축이 위쪽 양수, pivot이 bottom center
        // 프리뷰: 좌상단이 (0,0), Y축이 아래쪽 양수
        
        // 캐릭터 컨테이너의 실제 높이 (대화창 제외한 게임 영역)
        float characterAreaHeight = kPreviewSize.y - kControlPanelHeight - kDialogBoxSize.y;
        
        // X 좌표: 화면 중앙(400) + 게임 위치 - 스프라이트 폭의 절반 (bottom-center pivot)
        float previewX = (kPreviewSize.x * 0.5f) + position.x - (spriteWidth * 0.5f);
        
        // Y 좌표: 캐릭터 영역 하단에서 게임 위치만큼 위로 - 스프라이트 높이 (bottom pivot)
        // position.y가 양수일 때 화면 위쪽으로 이동해야 함
        float previewY = -spriteHeight + kPreviewSize.y - position.y;
        
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

        // 플레이스홀더 크기 (기본 크기에 0.21배 적용)
        float placeholderWidth = 200 * kSpriteScale;  // 200px 기본 크기
        float placeholderHeight = 300 * kSpriteScale; // 300px 기본 크기

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
        spriteElement.style.width = placeholderWidth;
        spriteElement.style.height = placeholderHeight;
        
        // "NO IMAGE" 텍스트 추가
        var label = new Label("NO\nIMAGE");
        label.style.color = Color.red;
        label.style.fontSize = (int)(12 * kSpriteScale); // 크기에 맞게 폰트 크기 조정
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.position = Position.Absolute;
        label.style.width = placeholderWidth;
        label.style.height = placeholderHeight;
        label.style.left = 0;
        label.style.top = 0;
        spriteElement.Add(label);
        
        UpdateSpritePosition(spriteElement, position, flip, placeholderWidth, placeholderHeight);
        
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
        if (_stepButton != null && _nextButton != null)
        {
            bool dataReady = _isStaticDataLoaded;
            bool hasEntry = _currentEntry != null;
            bool hasMoreEvents = hasEntry && _currentEventIndex < _currentEntry._dialogEventList.Count;
            
            // 다음 Dialog 이벤트가 있는지 확인
            bool hasNextDialog = false;
            if (hasEntry && _currentEventIndex < _currentEntry._dialogEventList.Count)
            {
                for (int i = _currentEventIndex; i < _currentEntry._dialogEventList.Count; i++)
                {
                    if (_currentEntry._dialogEventList[i].getDialogEventType() == DialogEventType.Dialog)
                    {
                        hasNextDialog = true;
                        break;
                    }
                }
            }
            
            _stepButton.SetEnabled(dataReady && hasMoreEvents);
            _nextButton.SetEnabled(dataReady && hasNextDialog);
            
            // 툴팁 설정
            if (!dataReady)
            {
                _stepButton.tooltip = "Static Data is loading...";
                _nextButton.tooltip = "Static Data is loading...";
            }
            else if (!hasEntry)
            {
                _stepButton.tooltip = "Select an entry to preview";
                _nextButton.tooltip = "Select an entry to preview";
            }
            else if (!hasMoreEvents)
            {
                _stepButton.tooltip = "No more events to execute";
                _nextButton.tooltip = "No more events to execute";
            }
            else if (!hasNextDialog)
            {
                _nextButton.tooltip = "No more dialog events";
            }
            else
            {
                _stepButton.tooltip = "";
                _nextButton.tooltip = "";
            }
        }
    }

    public bool IsStaticDataLoaded()
    {
        return _isStaticDataLoaded;
    }

    // 특정 이벤트 GUID까지 진행하는 메서드
    public void SetDialogDataAndPlayToEvent(DialogData dialogData, string entryKey, string eventGuid)
    {
        SetDialogData(dialogData);
        
        if (!string.IsNullOrEmpty(entryKey))
        {
            // 특정 엔트리 선택
            _entryDropdown.value = entryKey;
            OnEntrySelected(entryKey);
        }
        
        if (!string.IsNullOrEmpty(eventGuid))
        {
            // 해당 이벤트까지 자동 진행
            EditorApplication.delayCall += () => PlayToEventGuid(eventGuid);
        }
    }

    private void PlayToEventGuid(string targetEventGuid)
    {
        // Static Data가 로드되지 않았으면 먼저 로드
        if (!_isStaticDataLoaded)
        {
            UpdateStatusLabel("Loading Static Data...");
            EditorApplication.delayCall += () =>
            {
                LoadStaticData();
                // 로딩 완료 후 다시 시도
                EditorApplication.delayCall += () => PlayToEventGuid(targetEventGuid);
            };
            return;
        }

        if (_currentEntry == null)
            return;

        ResetPreview();

        // 대상 이벤트를 찾을 때까지 진행
        for (int i = 0; i < _currentEntry._dialogEventList.Count; i++)
        {
            var eventData = _currentEntry._dialogEventList[i];
            
            // 현재 이벤트 실행
            ExecuteDialogEvent(eventData);
            _currentEventIndex = i + 1;
            
            // 대상 이벤트에 도달했으면 중단
            if (eventData._editorGuidString == targetEventGuid)
            {
                break;
            }
        }
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
    }

    // 특정 엔트리와 이벤트 인덱스까지 진행하는 메서드 (GUID가 없을 경우 대안)
    public void SetDialogDataAndPlayToIndex(DialogData dialogData, string entryKey, int eventIndex)
    {
        SetDialogData(dialogData);
        
        if (!string.IsNullOrEmpty(entryKey))
        {
            // 특정 엔트리 선택
            _entryDropdown.value = entryKey;
            OnEntrySelected(entryKey);
        }
        
        // 해당 인덱스까지 자동 진행
        EditorApplication.delayCall += () => PlayToEventIndex(eventIndex);
    }

    private void PlayToEventIndex(int targetIndex)
    {
        // Static Data가 로드되지 않았으면 먼저 로드
        if (!_isStaticDataLoaded)
        {
            UpdateStatusLabel("Loading Static Data...");
            EditorApplication.delayCall += () =>
            {
                LoadStaticData();
                // 로딩 완료 후 다시 시도
                EditorApplication.delayCall += () => PlayToEventIndex(targetIndex);
            };
            return;
        }

        if (_currentEntry == null)
            return;

        ResetPreview();

        // 대상 인덱스까지 진행
        int maxIndex = Mathf.Min(targetIndex + 1, _currentEntry._dialogEventList.Count);
        for (int i = 0; i < maxIndex; i++)
        {
            var eventData = _currentEntry._dialogEventList[i];
            ExecuteDialogEvent(eventData);
            _currentEventIndex = i + 1;
        }
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
    }
}

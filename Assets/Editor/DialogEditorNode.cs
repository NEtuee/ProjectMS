using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class GameEventNode_Dialog : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.Dialog;

    private VisualElement dialogueListContainer;
    private TextField characterNameField;
    private TextField dialogTextField;
    private FloatField wordPerSecField;

    public string _dialogText = "Enter dialog text here...";

    public override void constructField()
    {
        style.width = 300;

        dialogueListContainer = new VisualElement();
        dialogueListContainer.style.flexDirection = FlexDirection.Column;
        dialogueListContainer.style.paddingBottom = 4;

        // 캐릭터 이름 필드
        characterNameField = new TextField("Character Name");
        if (_dialogEvent is DialogEventData_Dialog dialogData)
        {
            characterNameField.value = dialogData._displayCharacterKey ?? "";
            characterNameField.RegisterValueChangedCallback(evt =>
            {
                dialogData._displayCharacterKey = evt.newValue;
            });
        }
        characterNameField.style.marginBottom = 4;
        dialogueListContainer.Add(characterNameField);

        // 텍스트 출력 속도 필드
        wordPerSecField = new FloatField("Words Per Second");
        if (_dialogEvent is DialogEventData_Dialog dialogData2)
        {
            wordPerSecField.value = dialogData2._wordPerSec;
            wordPerSecField.RegisterValueChangedCallback(evt =>
            {
                dialogData2._wordPerSec = evt.newValue;
            });
        }
        wordPerSecField.style.marginBottom = 4;
        dialogueListContainer.Add(wordPerSecField);

        // 대화 텍스트 필드 (여러 줄)
        dialogTextField = new TextField("Dialog Text");
        dialogTextField.multiline = true;
        dialogTextField.style.flexGrow = 1;
        dialogTextField.style.whiteSpace = WhiteSpace.Normal;
        dialogTextField.value = "";
        dialogTextField.RegisterValueChangedCallback(evt =>
        {
            _dialogText = evt.newValue;
        });
        dialogTextField.style.marginBottom = 4;
        dialogueListContainer.Add(dialogTextField);

        mainContainer.Add(dialogueListContainer);
    }

    public override DialogEventDataBase exportGameEvent()
    {
        if (_dialogEvent is DialogEventData_Dialog dialogData)
        {
            // UI에서 데이터 업데이트
            dialogData._displayCharacterKey = characterNameField?.value ?? "";
            dialogData._wordPerSec = wordPerSecField?.value ?? 12f;
        }
        return _dialogEvent;
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        if (eventData is DialogEventData_Dialog dialogData)
        {
            _dialogEvent = dialogData;
            _dialogText = LanguageInstanceManager.Instance().GetTextFromFileFromIndex(_dialogData._textDataName, dialogData._dialogIndex);

            // UI 필드들이 이미 생성되어 있다면 값 업데이트
            if (characterNameField != null)
            {
                characterNameField.value = dialogData._displayCharacterKey ?? "";
            }
            
            if (wordPerSecField != null)
            {
                wordPerSecField.value = dialogData._wordPerSec;
            }
            
            if (dialogTextField != null)
            {
                dialogTextField.value = _dialogText;
            }
        }
    }

}

public class GameEventNode_Entry : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.Entry;
    public DialogEventEntryData _entryData;

    public GameEventNode_Entry()
    {
        title = "Entry";
        mainContainer.style.backgroundColor = new StyleColor(Color.green);

        _entryData = new DialogEventEntryData();
        _entryData._entryKey = "default";
        _entryData._editorGuidString = _guid;
    }

    public override void constructPort()
    {
        var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
        output.portName = "Next";
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override void constructField()
    {
        style.width = 250;

        var entryKeyField = new TextField("Entry Key");
        entryKeyField.value = _entryData._entryKey;
        entryKeyField.RegisterValueChangedCallback(evt =>
        {
            _entryData._entryKey = evt.newValue;
            title = _entryData._entryKey;
        });
        entryKeyField.style.backgroundColor = new StyleColor(Color.gray);
        mainContainer.Add(entryKeyField);
    }

    public override DialogEventDataBase exportGameEvent()
    {
        return null; // Entry 노드는 이벤트 데이터를 갖지 않음
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        // Entry 노드는 이벤트 데이터를 갖지 않음
    }

    public DialogEventEntryData exportEntryData()
    {
        return _entryData;
    }

    public void importEntryData(DialogEventEntryData entryData)
    {
        _entryData = entryData;
        title = _entryData._entryKey;
    }
}

public class GameEventNode_ActiveObject : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.ActiveObject;

    private VisualElement activeObjectContainer;
    private TextField objectIdField;
    private VisualElement spritePathContainer;
    private TextField spritePathField;
    private Button browseSpriteButton;
    private VisualElement spritePreviewContainer;
    private UnityEngine.UIElements.Image spritePreviewImage;
    private Vector2Field positionField;
    private Toggle flipToggle;

    public override void constructField()
    {
        style.width = 350;

        activeObjectContainer = new VisualElement();
        activeObjectContainer.style.flexDirection = FlexDirection.Column;
        activeObjectContainer.style.paddingBottom = 4;

        // 오브젝트 ID 필드 (에디터에서는 문자열로 입력)
        objectIdField = new TextField("Object ID");
        if (_dialogEvent is DialogEventData_ActiveObject activeObjectData)
        {
            objectIdField.value = activeObjectData._objectIdString ?? "";
            objectIdField.RegisterValueChangedCallback(evt =>
            {
                activeObjectData._objectIdString = evt.newValue;
                // 문자열을 해시로 변환하여 _objectId에 저장
                activeObjectData._objectId = IOControl.computeHash(evt.newValue);
            });
        }
        objectIdField.style.marginBottom = 4;
        activeObjectContainer.Add(objectIdField);

        // 스프라이트 경로 필드와 브라우즈 버튼
        spritePathContainer = new VisualElement();
        spritePathContainer.style.flexDirection = FlexDirection.Row;
        spritePathContainer.style.marginBottom = 4;

        spritePathField = new TextField("Sprite Path");
        spritePathField.style.flexGrow = 1;
        if (_dialogEvent is DialogEventData_ActiveObject activeObjectData2)
        {
            spritePathField.value = activeObjectData2._spritePath ?? "";
            spritePathField.RegisterValueChangedCallback(evt =>
            {
                activeObjectData2._spritePath = evt.newValue;
                UpdateSpritePreview(evt.newValue);
            });
        }
        spritePathContainer.Add(spritePathField);

        browseSpriteButton = new Button(() => BrowseSprite()) { text = "Browse" };
        browseSpriteButton.style.marginLeft = 4;
        browseSpriteButton.style.width = 60;
        spritePathContainer.Add(browseSpriteButton);

        activeObjectContainer.Add(spritePathContainer);

        // 스프라이트 미리보기 컨테이너
        spritePreviewContainer = new VisualElement();
        spritePreviewContainer.style.flexDirection = FlexDirection.Column;
        spritePreviewContainer.style.alignItems = Align.Center;
        spritePreviewContainer.style.marginBottom = 4;
        spritePreviewContainer.style.paddingTop = 4;
        spritePreviewContainer.style.paddingBottom = 4;
        spritePreviewContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.5f));
        spritePreviewContainer.style.borderTopLeftRadius = 4;
        spritePreviewContainer.style.borderTopRightRadius = 4;
        spritePreviewContainer.style.borderBottomLeftRadius = 4;
        spritePreviewContainer.style.borderBottomRightRadius = 4;

        var previewLabel = new Label("Sprite Preview");
        previewLabel.style.marginBottom = 4;
        previewLabel.style.fontSize = 12;
        previewLabel.style.color = new StyleColor(Color.gray);
        spritePreviewContainer.Add(previewLabel);

        spritePreviewImage = new UnityEngine.UIElements.Image();
        spritePreviewImage.style.width = 100;
        spritePreviewImage.style.height = 100;
        spritePreviewImage.style.backgroundImage = new StyleBackground();
        spritePreviewImage.scaleMode = ScaleMode.ScaleToFit;
        spritePreviewContainer.Add(spritePreviewImage);

        activeObjectContainer.Add(spritePreviewContainer);

        // 위치 필드
        positionField = new Vector2Field("Position");
        if (_dialogEvent is DialogEventData_ActiveObject activeObjectData3)
        {
            positionField.value = activeObjectData3._position;
            positionField.RegisterValueChangedCallback(evt =>
            {
                activeObjectData3._position = evt.newValue;
            });
        }
        positionField.style.marginBottom = 4;
        activeObjectContainer.Add(positionField);

        // 플립 토글
        flipToggle = new Toggle("Flip");
        if (_dialogEvent is DialogEventData_ActiveObject activeObjectData4)
        {
            flipToggle.value = activeObjectData4._flip;
            flipToggle.RegisterValueChangedCallback(evt =>
            {
                activeObjectData4._flip = evt.newValue;
            });
        }
        flipToggle.style.marginBottom = 4;
        activeObjectContainer.Add(flipToggle);

        mainContainer.Add(activeObjectContainer);

        // 초기 스프라이트 미리보기 업데이트
        if (_dialogEvent is DialogEventData_ActiveObject initialData && !string.IsNullOrEmpty(initialData._spritePath))
        {
            UpdateSpritePreview(initialData._spritePath);
        }
    }

    private void BrowseSprite()
    {
        // Resources/Sprites 폴더의 절대 경로 계산
        string spriteFolderPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Resources/Sprites");
        
        // 폴더가 존재하지 않으면 생성
        if (!System.IO.Directory.Exists(spriteFolderPath))
        {
            System.IO.Directory.CreateDirectory(spriteFolderPath);
            UnityEditor.AssetDatabase.Refresh();
        }

        // 파일 다이얼로그 열기
        string selectedPath = UnityEditor.EditorUtility.OpenFilePanel(
            "Select Sprite", 
            spriteFolderPath, 
            "png,jpg,jpeg,tga"
        );

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // 선택된 파일이 Resources/Sprites 폴더 내에 있는지 확인
            if (selectedPath.Contains("Resources/Sprites"))
            {
                // Resources 형식으로 경로 변환
                string resourcePath = ConvertToResourcePath(selectedPath);
                
                spritePathField.value = resourcePath;
                if (_dialogEvent is DialogEventData_ActiveObject activeObjectData)
                {
                    activeObjectData._spritePath = resourcePath;
                }
                UpdateSpritePreview(resourcePath);
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Invalid Selection", 
                    "Please select a sprite from the Resources/Sprites folder.", 
                    "OK"
                );
            }
        }
    }

    private string ConvertToResourcePath(string fullPath)
    {
        // Assets/Resources/ 부분을 찾아서 제거하고, 확장자도 제거
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            string relativePath = fullPath.Substring(resourcesIndex + "Resources/".Length);
            relativePath = relativePath.Replace("\\", "/");
            return System.IO.Path.ChangeExtension(relativePath, null); // 확장자 제거
        }
        return "";
    }

    private List<string> GetAvailableSpritePaths()
    {
        var spritePaths = new List<string>();
        var spriteFolder = "Assets/Resources/Sprites";
        
        if (System.IO.Directory.Exists(spriteFolder))
        {
            var files = System.IO.Directory.GetFiles(spriteFolder, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".tga"))
                {
                    var relativePath = file.Replace("Assets/Resources/", "").Replace("\\", "/");
                    relativePath = System.IO.Path.ChangeExtension(relativePath, null); // Remove extension for Resources.Load
                    spritePaths.Add(relativePath);
                }
            }
        }
        
        return spritePaths;
    }

    private void UpdateSpritePreview(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
        {
            spritePreviewImage.image = null;
            return;
        }

        // Resources.Load를 사용하여 스프라이트 로드
        var sprite = UnityEngine.Resources.Load<UnityEngine.Sprite>(spritePath);
        if (sprite != null)
        {
            spritePreviewImage.image = sprite.texture;
        }
        else
        {
            spritePreviewImage.image = null;
        }
    }

    public override DialogEventDataBase exportGameEvent()
    {
        if (_dialogEvent is DialogEventData_ActiveObject activeObjectData)
        {
            // UI에서 데이터 업데이트
            activeObjectData._objectIdString = objectIdField?.value ?? "";
            activeObjectData._objectId = IOControl.computeHash(activeObjectData._objectIdString);
            activeObjectData._spritePath = spritePathField?.value ?? "";
            activeObjectData._position = positionField?.value ?? Vector2.zero;
            activeObjectData._flip = flipToggle?.value ?? false;
        }
        return _dialogEvent;
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        if (eventData is DialogEventData_ActiveObject activeObjectData)
        {
            _dialogEvent = activeObjectData;
            
            // UI 필드들이 이미 생성되어 있다면 값 업데이트
            if (objectIdField != null)
            {
                objectIdField.value = activeObjectData._objectIdString ?? "";
            }
            
            if (spritePathField != null)
            {
                spritePathField.value = activeObjectData._spritePath ?? "";
                UpdateSpritePreview(activeObjectData._spritePath);
            }
            
            if (positionField != null)
            {
                positionField.value = activeObjectData._position;
            }
            
            if (flipToggle != null)
            {
                flipToggle.value = activeObjectData._flip;
            }
        }
    }
}

public class GameEventNode_SetSprite : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.SetSprite;

    private VisualElement setSpriteContainer;
    private TextField objectIdField;
    private VisualElement spritePathContainer;
    private TextField spritePathField;
    private Button browseSpriteButton;
    private VisualElement spritePreviewContainer;
    private UnityEngine.UIElements.Image spritePreviewImage;
    private Toggle flipToggle;

    public override void constructField()
    {
        style.width = 350;

        setSpriteContainer = new VisualElement();
        setSpriteContainer.style.flexDirection = FlexDirection.Column;
        setSpriteContainer.style.paddingBottom = 4;

        // 오브젝트 ID 필드 (에디터에서는 문자열로 입력)
        objectIdField = new TextField("Object ID");
        if (_dialogEvent is DialogEventData_SetSprite setSpriteData)
        {
            objectIdField.value = setSpriteData._objectIdString ?? "";
            objectIdField.RegisterValueChangedCallback(evt =>
            {
                setSpriteData._objectIdString = evt.newValue;
                // 문자열을 해시로 변환하여 _objectId에 저장
                setSpriteData._objectId = IOControl.computeHash(evt.newValue);
            });
        }
        objectIdField.style.marginBottom = 4;
        setSpriteContainer.Add(objectIdField);

        // 스프라이트 경로 필드와 브라우즈 버튼
        spritePathContainer = new VisualElement();
        spritePathContainer.style.flexDirection = FlexDirection.Row;
        spritePathContainer.style.marginBottom = 4;

        spritePathField = new TextField("Sprite Path");
        spritePathField.style.flexGrow = 1;
        if (_dialogEvent is DialogEventData_SetSprite setSpriteData2)
        {
            spritePathField.value = setSpriteData2._spritePath ?? "";
            spritePathField.RegisterValueChangedCallback(evt =>
            {
                setSpriteData2._spritePath = evt.newValue;
                UpdateSpritePreview(evt.newValue);
            });
        }
        spritePathContainer.Add(spritePathField);

        browseSpriteButton = new Button(() => BrowseSprite()) { text = "Browse" };
        browseSpriteButton.style.marginLeft = 4;
        browseSpriteButton.style.width = 60;
        spritePathContainer.Add(browseSpriteButton);

        setSpriteContainer.Add(spritePathContainer);

        // 스프라이트 미리보기 컨테이너
        spritePreviewContainer = new VisualElement();
        spritePreviewContainer.style.flexDirection = FlexDirection.Column;
        spritePreviewContainer.style.alignItems = Align.Center;
        spritePreviewContainer.style.marginBottom = 4;
        spritePreviewContainer.style.paddingTop = 4;
        spritePreviewContainer.style.paddingBottom = 4;
        spritePreviewContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.5f));
        spritePreviewContainer.style.borderTopLeftRadius = 4;
        spritePreviewContainer.style.borderTopRightRadius = 4;
        spritePreviewContainer.style.borderBottomLeftRadius = 4;
        spritePreviewContainer.style.borderBottomRightRadius = 4;

        var previewLabel = new Label("Sprite Preview");
        previewLabel.style.marginBottom = 4;
        previewLabel.style.fontSize = 12;
        previewLabel.style.color = new StyleColor(Color.gray);
        spritePreviewContainer.Add(previewLabel);

        spritePreviewImage = new UnityEngine.UIElements.Image();
        spritePreviewImage.style.width = 100;
        spritePreviewImage.style.height = 100;
        spritePreviewImage.style.backgroundImage = new StyleBackground();
        spritePreviewImage.scaleMode = ScaleMode.ScaleToFit;
        spritePreviewContainer.Add(spritePreviewImage);

        setSpriteContainer.Add(spritePreviewContainer);

        // Flip 토글 필드
        flipToggle = new Toggle("Flip");
        if (_dialogEvent is DialogEventData_SetSprite setSpriteData3)
        {
            flipToggle.value = setSpriteData3._flip;
            flipToggle.RegisterValueChangedCallback(evt =>
            {
                setSpriteData3._flip = evt.newValue;
            });
        }
        flipToggle.style.marginBottom = 4;
        setSpriteContainer.Add(flipToggle);

        mainContainer.Add(setSpriteContainer);

        // 초기 스프라이트 미리보기 업데이트
        if (_dialogEvent is DialogEventData_SetSprite initialData && !string.IsNullOrEmpty(initialData._spritePath))
        {
            UpdateSpritePreview(initialData._spritePath);
        }
    }

    private void BrowseSprite()
    {
        // Resources/Sprites 폴더의 절대 경로 계산
        string spriteFolderPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Resources/Sprites");
        
        // 폴더가 존재하지 않으면 생성
        if (!System.IO.Directory.Exists(spriteFolderPath))
        {
            System.IO.Directory.CreateDirectory(spriteFolderPath);
            UnityEditor.AssetDatabase.Refresh();
        }

        // 파일 다이얼로그 열기
        string selectedPath = UnityEditor.EditorUtility.OpenFilePanel(
            "Select Sprite", 
            spriteFolderPath, 
            "png,jpg,jpeg,tga"
        );

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // 선택된 파일이 Resources/Sprites 폴더 내에 있는지 확인
            if (selectedPath.Contains("Resources/Sprites"))
            {
                // Resources 형식으로 경로 변환
                string resourcePath = ConvertToResourcePath(selectedPath);
                
                spritePathField.value = resourcePath;
                if (_dialogEvent is DialogEventData_SetSprite setSpriteData)
                {
                    setSpriteData._spritePath = resourcePath;
                }
                UpdateSpritePreview(resourcePath);
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Invalid Selection", 
                    "Please select a sprite from the Resources/Sprites folder.", 
                    "OK"
                );
            }
        }
    }

    private string ConvertToResourcePath(string fullPath)
    {
        // Assets/Resources/ 부분을 찾아서 제거하고, 확장자도 제거
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            string relativePath = fullPath.Substring(resourcesIndex + "Resources/".Length);
            relativePath = relativePath.Replace("\\", "/");
            return System.IO.Path.ChangeExtension(relativePath, null); // 확장자 제거
        }
        return "";
    }

    private void UpdateSpritePreview(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
        {
            spritePreviewImage.image = null;
            return;
        }

        // Resources.Load를 사용하여 스프라이트 로드
        var sprite = UnityEngine.Resources.Load<UnityEngine.Sprite>(spritePath);
        if (sprite != null)
        {
            spritePreviewImage.image = sprite.texture;
        }
        else
        {
            spritePreviewImage.image = null;
        }
    }

    public override DialogEventDataBase exportGameEvent()
    {
        if (_dialogEvent is DialogEventData_SetSprite setSpriteData)
        {
            // UI에서 데이터 업데이트
            setSpriteData._objectIdString = objectIdField?.value ?? "";
            setSpriteData._objectId = IOControl.computeHash(setSpriteData._objectIdString);
            setSpriteData._spritePath = spritePathField?.value ?? "";
            setSpriteData._flip = flipToggle?.value ?? false;
        }
        return _dialogEvent;
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        if (eventData is DialogEventData_SetSprite setSpriteData)
        {
            _dialogEvent = setSpriteData;
            
            // UI 필드들이 이미 생성되어 있다면 값 업데이트
            if (objectIdField != null)
            {
                objectIdField.value = setSpriteData._objectIdString ?? "";
            }
            
            if (spritePathField != null)
            {
                spritePathField.value = setSpriteData._spritePath ?? "";
                UpdateSpritePreview(setSpriteData._spritePath);
            }
            
            if (flipToggle != null)
            {
                flipToggle.value = setSpriteData._flip;
            }
        }
    }
}

public class GameEventNode_SetPosition : GameEventNodeBase
{
    public override DialogEventType getEventType() => DialogEventType.SetPosition;

    private VisualElement setPositionContainer;
    private TextField objectIdField;
    private Vector2Field positionField;
    private Toggle flipToggle;

    public override void constructField()
    {
        style.width = 300;

        setPositionContainer = new VisualElement();
        setPositionContainer.style.flexDirection = FlexDirection.Column;
        setPositionContainer.style.paddingBottom = 4;

        // 오브젝트 ID 필드 (에디터에서는 문자열로 입력)
        objectIdField = new TextField("Object ID");
        if (_dialogEvent is DialogEventData_SetPosition setPositionData)
        {
            objectIdField.value = setPositionData._objectIdString ?? "";
            objectIdField.RegisterValueChangedCallback(evt =>
            {
                setPositionData._objectIdString = evt.newValue;
                // 문자열을 해시로 변환하여 _objectId에 저장
                setPositionData._objectId = IOControl.computeHash(evt.newValue);
            });
        }
        objectIdField.style.marginBottom = 4;
        setPositionContainer.Add(objectIdField);

        // 위치 필드
        positionField = new Vector2Field("Position");
        if (_dialogEvent is DialogEventData_SetPosition setPositionData2)
        {
            positionField.value = setPositionData2._position;
            positionField.RegisterValueChangedCallback(evt =>
            {
                setPositionData2._position = evt.newValue;
            });
        }
        positionField.style.marginBottom = 4;
        setPositionContainer.Add(positionField);

        // Flip 토글 필드
        flipToggle = new Toggle("Flip");
        if (_dialogEvent is DialogEventData_SetPosition setPositionData3)
        {
            flipToggle.value = setPositionData3._flip;
            flipToggle.RegisterValueChangedCallback(evt =>
            {
                setPositionData3._flip = evt.newValue;
            });
        }
        flipToggle.style.marginBottom = 4;
        setPositionContainer.Add(flipToggle);

        mainContainer.Add(setPositionContainer);
    }

    public override DialogEventDataBase exportGameEvent()
    {
        if (_dialogEvent is DialogEventData_SetPosition setPositionData)
        {
            // UI에서 데이터 업데이트
            setPositionData._objectIdString = objectIdField?.value ?? "";
            setPositionData._objectId = IOControl.computeHash(setPositionData._objectIdString);
            setPositionData._position = positionField?.value ?? Vector2.zero;
            setPositionData._flip = flipToggle?.value ?? false;
        }
        return _dialogEvent;
    }

    public override void importGameEvent(DialogEventDataBase eventData)
    {
        if (eventData is DialogEventData_SetPosition setPositionData)
        {
            _dialogEvent = setPositionData;
            
            // UI 필드들이 이미 생성되어 있다면 값 업데이트
            if (objectIdField != null)
            {
                objectIdField.value = setPositionData._objectIdString ?? "";
            }
            
            if (positionField != null)
            {
                positionField.value = setPositionData._position;
            }
            
            if (flipToggle != null)
            {
                flipToggle.value = setPositionData._flip;
            }
        }
    }
}


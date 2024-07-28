using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

public class LocalizationText : ObjectBase
{
    public enum IconDirection
    {
        Left,
        Right,
    }

    public string          _textKey = "";    
    
    public TextMeshProUGUI  _textMeshPro;
    public Image            _targetIcon;
    public IconDirection    _iconDirection = IconDirection.Left;

    public float            _iconGap;
    public bool             _useCenterAlign;
    public bool             _autoUpdate = true;

    protected override void Awake()
    {
        base.Awake();

        int managerID = QueryUniqueID("LanguageManager");

        var msg = MessagePack(MessageTitles.system_registerRequest,managerID,null);
        MasterManager.instance.ReceiveMessage(msg);

        if(_autoUpdate)
        {
            AddAction(MessageTitles.system_languageChanged,(msg)=>{
                updateLanguage();
            });
        }
    }

    public override void initialize()
    {
        base.initialize();
        updateLanguage();
    }

    public void setIcon(Sprite sprite)
    {
        _targetIcon.sprite = sprite;
    }

    public void updateLanguage()
    {
        _textMeshPro.font = LanguageManager._instance.getFont();
        if(_textKey != "")
            updateString(_textKey);
    }

    public void updateString(string key)
    {
        _textKey = key;
        _textMeshPro.text = LanguageManager._instance.getString(_textKey);
        _textMeshPro.ForceMeshUpdate();
        if (_useCenterAlign)
            centerAlign();
    }

    public void centerAlign()
    {
        if(_textMeshPro == null || _targetIcon == null)
            return;

        float imageBoundX = _targetIcon.rectTransform.rect.width;
        float textBoundX = _textMeshPro.textBounds.extents.x * 2f;

        float total = imageBoundX + textBoundX + _iconGap;
        float totalHalf = total * 0.5f;

        Vector3 iconLocalPosition = _targetIcon.rectTransform.localPosition;
        if(_iconDirection == IconDirection.Left)
            iconLocalPosition.x = -totalHalf + (imageBoundX * 0.5f);
        else if(_iconDirection == IconDirection.Right)
            iconLocalPosition.x = totalHalf - (imageBoundX * 0.5f);

        _targetIcon.rectTransform.localPosition = iconLocalPosition;

        Vector3 textLocalPosition = _textMeshPro.rectTransform.localPosition;
        if(_iconDirection == IconDirection.Left)
            textLocalPosition.x = totalHalf - (textBoundX * 0.5f);
        else if(_iconDirection == IconDirection.Right)
            textLocalPosition.x = -totalHalf + (textBoundX * 0.5f);
        
        _textMeshPro.rectTransform.localPosition = textLocalPosition;
    }
}

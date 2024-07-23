using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScript : MonoBehaviour
{
    public TitleMenuUIBinder _titleMenuUIBinder;
    private TitleMenuUI _titleMenuUI;

    private void Awake()
    {
        _titleMenuUI = new TitleMenuUI();
        _titleMenuUI.SetBinder(_titleMenuUIBinder);

        if (_titleMenuUI.CheckValidBinderLink(out string reason) == false)
        {
            DebugUtil.assert(false, reason);
            return;
        }
        
        _titleMenuUI.Initialize();
        _titleMenuUI.ActiveTitleMenu(true);
    }

    private void Start()
    {
        LetterBox._instance.clear();
        ScreenDirector._instance._screenFader.clear();
        Cursor.visible = true;        
    }

    private void LateUpdate()
    {
        _titleMenuUI.UpdateByManager();
    }
}

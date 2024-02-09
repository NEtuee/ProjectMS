using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTextManager : Singleton<DialogTextManager>
{
    private Dictionary<string, List<BubbleCommend>> _data;

    public void Init(Dictionary<string, List<BubbleCommend>> data)
    {
        _data = data;
    }

    public List<BubbleCommend> GetDialogCommand(string key)
    {
        if(_data == null)
        {
            DebugUtil.assert(false,"Subtitle Info 컨테이너가 null 입니다. 통보 요망");
            return null;
        }

        if (_data.TryGetValue(key, out var result) == false)
        {
            return null;
        }

        return result;
    }
}

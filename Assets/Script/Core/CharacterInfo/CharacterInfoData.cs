
[System.Serializable]
public class CharacterInfoData
{
    public string       _displayName;
    public string       _actionGraphPath;
    public string       _aiGraphPath;
    public string       _statusName;

    public float        _characterRadius;
    public float        _headUpOffset;

    public bool         _indicatorVisible = true;
    public bool         _useCameraBoundLock = true;
    public bool         _useHpInterface = false;
    public bool         _selfCollision = true;
    public bool         _immortalCharacter = false;

    public string       _allyInfoKey = "";
    
    public CommonMaterial   _defaultMaterial = CommonMaterial.Skin;
}

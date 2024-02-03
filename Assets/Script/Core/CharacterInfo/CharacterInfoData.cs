
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
    
    public SearchIdentifier _searchIdentifer = SearchIdentifier.Enemy;
    public CommonMaterial   _defaultMaterial = CommonMaterial.Skin;
}

public struct SubUIData : IPackedUIData
{
    public UIDataType UIDataType => UIDataType.NONE;
    public UIEventKey UIEventKey { get; }

    public SubUIData(UIEventKey key = UIEventKey.NONE)
    {
        UIEventKey = key;
    }
    public static bool operator ==(SubUIData left, SubUIData right) => left.UIEventKey == right.UIEventKey;
    public static bool operator !=(SubUIData left, SubUIData right) => !(left == right);
    public override bool Equals(object obj) => obj is SubUIData other && this == other;
    public override int GetHashCode() => UIEventKey.GetHashCode();
}
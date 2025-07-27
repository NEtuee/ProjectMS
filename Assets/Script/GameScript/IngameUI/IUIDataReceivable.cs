using System.Collections.Generic;

public interface IUIDataReceivable
{
    public UIDataType DataType { get; }
    public IPackedUIData ReceivedData { get; }
    public IReadOnlyCollection<UIEventKey> ValidEventKeys { get; }
    public SubUIData ReceivedSubData { get; }
    public void ReceiveUpdatedData(IPackedUIData data);
    public void ReceiveUpdatedSubData(IPackedUIData data);
}
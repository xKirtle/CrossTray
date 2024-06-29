namespace CrossTrayCore;

public class NotifyIconEventArgs(ClickTypes clickTypes) : EventArgs
{
    public ClickTypes ClickTypes { get; } = clickTypes;
}

[Flags]
public enum ClickTypes : uint
{
    Left = 0x01,
    DoubleLeft = 0x02,
    Right = 0x04
}

using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore;

public class ContextMenuItem(
    string text,
    Action action,
    MENU_ITEM_FLAGS flags = MENU_ITEM_FLAGS.MF_STRING,
    List<ContextMenuItem>? subItems = null,
    ContextMenuItem? parent = null)
{
    public int Id { get; set; }
    public PinnedString Text { get; } = new(text);
    public Action Action { get; } = action;
    public MENU_ITEM_FLAGS Flags { get; } = flags;
    public List<ContextMenuItem> SubItems { get; } = subItems ?? [];
    public ContextMenuItem? Parent { get; set; } = parent;
}

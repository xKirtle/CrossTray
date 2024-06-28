using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public abstract class ContextMenuItemBase(
    string text,
    MENU_ITEM_FLAGS flags,
    List<ContextMenuItemBase>? subItems = null,
    ContextMenuItemBase? parent = null)
{
    public int Id { get; set; }
    public PinnedString Text { get; } = new(text);
    public MENU_ITEM_FLAGS Flags { get; protected set; } = flags;
    public List<ContextMenuItemBase> SubItems { get; } = subItems ?? [];
    public ContextMenuItemBase? Parent { get; set; } = parent;

    public virtual void AddToMenu(HMENU hMenu)
    {
        PInvoke.AppendMenu(hMenu, Flags, (uint)Id, Text.Ptr);
    }
}
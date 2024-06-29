using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class SimpleMenuItem(string text, Action<ContextMenuItemBase>? action = default, bool isEnabled = true, ContextMenuItemBase? parent = null)
    : ContextMenuItemBase(text, MENU_ITEM_FLAGS.MF_STRING, isEnabled, null, parent)
{
    public Action<ContextMenuItemBase>? Action { get; } = action;
}
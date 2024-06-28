using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class SeparatorMenuItem(ContextMenuItemBase? parent = null)
    : ContextMenuItemBase(string.Empty, MENU_ITEM_FLAGS.MF_SEPARATOR, null, parent)
{
}
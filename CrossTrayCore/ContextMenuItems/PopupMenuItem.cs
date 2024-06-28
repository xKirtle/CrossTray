using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class PopupMenuItem(string text, List<ContextMenuItemBase>? subItems = null, ContextMenuItemBase? parent = null)
    : ContextMenuItemBase(text, MENU_ITEM_FLAGS.MF_POPUP, subItems, parent)
{
    public override void AddToMenu(HMENU hMenu)
    {
        if (SubItems.Count == 0)
        {
            return;
        }
        
        var hSubMenu = PInvoke.CreatePopupMenu();
        
        foreach (var subItem in SubItems)
        {
            subItem.AddToMenu(hSubMenu);
        }

        PInvoke.AppendMenu(hMenu, Flags, (nuint)hSubMenu.Value, Text.Ptr);
    }
}
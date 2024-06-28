using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class CheckableMenuItem(string text, Action<ContextMenuItemBase> action, bool isChecked = false, ContextMenuItemBase? parent = null) : SimpleMenuItem(text, action, parent)
{
    public bool IsChecked { get; private set; } = isChecked;
    
    public void Toggle()
    {
        IsChecked = !IsChecked;
        UpdateFlags();
    }
    
    private void UpdateFlags()
    {
        if (IsChecked) 
        {
            Flags |= MENU_ITEM_FLAGS.MF_CHECKED;
        } 
        else 
        {
            Flags &= ~MENU_ITEM_FLAGS.MF_CHECKED;
        }
    }
}
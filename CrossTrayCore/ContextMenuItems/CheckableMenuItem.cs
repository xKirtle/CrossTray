using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class CheckableMenuItem : SimpleMenuItem
{
    public CheckableMenuItem(string text, Action<ContextMenuItemBase>? action = default, bool isChecked = false, bool isEnabled = true, ContextMenuItemBase? parent = null) : base(
        text, action, isEnabled, parent)
    {
        IsChecked = isChecked;
        UpdateFlags();
    }
    
    public bool IsChecked { get; private set; }
    
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
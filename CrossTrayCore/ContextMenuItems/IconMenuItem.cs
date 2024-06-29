using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class IconMenuItem(string text, HICON icon, Action<ContextMenuItemBase>? action = default, bool isEnabled = true, ContextMenuItemBase? parent = null) 
    : SimpleMenuItem(text, action, isEnabled, parent)
{
    public HICON Icon { get; } = icon;
    private HBITMAP Bitmap { get; } = IconToBitmap(icon);
    
    private static HBITMAP IconToBitmap(HICON hIcon)
    {
        var bitmapPtr = System.Drawing.Bitmap.FromHicon(hIcon).GetHbitmap();
        return new HBITMAP(bitmapPtr);
    }

    public override void AddToMenu(HMENU hMenu)
    {
        base.AddToMenu(hMenu);
        PInvoke.SetMenuItemBitmaps(hMenu, (uint)Id, MENU_ITEM_FLAGS.MF_BYCOMMAND, Bitmap, Bitmap);
    }
}
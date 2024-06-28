using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class IconMenuItem(string text, Action<ContextMenuItemBase> action, HICON icon, ContextMenuItemBase? parent = null) : SimpleMenuItem(text, action, parent)
{
    public HICON Icon { get; } = icon;
    public HBITMAP Bitmap { get; } = IconToBitmap(icon);
    
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
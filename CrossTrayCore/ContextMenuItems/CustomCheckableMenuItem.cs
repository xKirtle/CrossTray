using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using System.Drawing;
using Windows.Win32;

namespace CrossTrayCore.ContextMenuItems;

public class CustomCheckableMenuItem : CheckableMenuItem
{
    private HBITMAP CheckedBitmap { get; }
    private HBITMAP UncheckedBitmap { get; }

    public CustomCheckableMenuItem(string text, Action<ContextMenuItemBase> action, HICON checkedHicon, HICON uncheckedHicon, bool isChecked = false, bool isEnabled = true, ContextMenuItemBase? parent = null)
        : base(text, action, isChecked, isEnabled, parent)
    {
        CheckedBitmap = IconToBitmap(checkedHicon);
        UncheckedBitmap = IconToBitmap(uncheckedHicon);
        Flags |= MENU_ITEM_FLAGS.MF_USECHECKBITMAPS;
    }

    private static HBITMAP IconToBitmap(HICON hIcon)
    {
        using var bitmap = Icon.FromHandle(hIcon).ToBitmap();
        return new HBITMAP(bitmap.GetHbitmap(Color.Transparent));
    }

    public override void AddToMenu(HMENU hMenu)
    {
        base.AddToMenu(hMenu);
        PInvoke.SetMenuItemBitmaps(hMenu, (uint)Id, MENU_ITEM_FLAGS.MF_BYCOMMAND, CheckedBitmap, UncheckedBitmap);
    }
}
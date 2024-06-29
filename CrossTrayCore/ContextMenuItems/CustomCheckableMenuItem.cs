using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using System.Drawing;
using Windows.Win32;

namespace CrossTrayCore.ContextMenuItems;

public class CustomCheckableMenuItem : CheckableMenuItem
{
    public HICON CheckedIcon { get; }
    public HICON UncheckedIcon { get; }
    private HBITMAP CheckedBitmap { get; }
    private HBITMAP UncheckedBitmap { get; }

    public CustomCheckableMenuItem(string text, HICON checkedHicon, HICON uncheckedHicon, Action<ContextMenuItemBase>? action = default, bool isChecked = false, bool isEnabled = true, ContextMenuItemBase? parent = null)
        : base(text, action, isChecked, isEnabled, parent)
    {
        CheckedIcon = checkedHicon;
        UncheckedIcon = uncheckedHicon;
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
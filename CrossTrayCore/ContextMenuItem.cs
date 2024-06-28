using System.Drawing;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore;

public class ContextMenuItem(
    string text,
    Action<ContextMenuItem> action,
    MENU_ITEM_FLAGS flags = MENU_ITEM_FLAGS.MF_STRING,
    List<ContextMenuItem>? subItems = null,
    ContextMenuItem? parent = null)
{
    public int Id { get; set; }
    public PinnedString Text { get; } = new(text);
    public Action<ContextMenuItem> Action { get; } = action;
    public MENU_ITEM_FLAGS Flags { get; protected set; } = flags;
    public List<ContextMenuItem> SubItems { get; } = subItems ?? [];
    public ContextMenuItem? Parent { get; set; } = parent;
}

public class CheckableMenuItem : ContextMenuItem
{
    public bool IsChecked { get; private set; }

    public CheckableMenuItem(string text, Action<ContextMenuItem> action, bool isChecked = false)
        : base(text, action, MENU_ITEM_FLAGS.MF_STRING)
    {
        IsChecked = isChecked;
        UpdateFlags();
    }

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

public class IconMenuItem(string text, Action<ContextMenuItem> action, HICON icon) : ContextMenuItem(text, action, MENU_ITEM_FLAGS.MF_STRING)
{
    public HICON Icon { get; } = icon;
    public HBITMAP Bitmap { get; } = IconToTransparentBitmap(icon);
        
    private static HBITMAP IconToBitmap(HICON hIcon)
    {
        var bitmapPtr = System.Drawing.Bitmap.FromHicon(hIcon).GetHbitmap();
        return new HBITMAP(bitmapPtr);
    }
    
    private static HBITMAP IconToTransparentBitmap(HICON hIcon)
    {
        using var icon = System.Drawing.Icon.FromHandle(hIcon);
        
        // Create a bitmap that has the same size as the icon
        var bitmap = new Bitmap(icon.Width, icon.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        // Use a graphics object to draw the icon onto the bitmap
        using (var graphics = Graphics.FromImage(bitmap))
        {
            // Clear the background to transparent
            graphics.Clear(Color.Transparent);

            // Draw the icon onto the bitmap
            graphics.DrawIcon(icon, 0, 0);
        }

        // Convert the bitmap to an HBITMAP
        return new HBITMAP(bitmap.GetHbitmap(Color.Transparent));
    }
}

namespace CrossTrayCore;

using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

[SupportedOSPlatform("windows5.1.2600")]
public class NotifyIconWrapper : IDisposable
{
    private readonly HWND _hwnd;
    private HICON _iconHandle;
    private readonly uint _uId = 1;
    private string _tooltip;
    private static readonly uint WM_TRAYICON = PInvoke.RegisterWindowMessage("WM_TRAYICON");

    public NotifyIconWrapper(string tooltip)
    {
        _tooltip = tooltip;
        _iconHandle = new HICON(SystemIcons.Application.Handle);
        _hwnd = CreateMinimalWindow();
    }

    public void SetIconFromFile(string filePath)
    {
        _iconHandle = LoadIconFromFile(filePath);
        ModifyIcon();
    }

    public void SetIconFromEmbeddedResource(string resourceName, Assembly resourceAssembly)
    {
        _iconHandle = LoadIconFromEmbeddedResource(resourceName, resourceAssembly);
        ModifyIcon();
    }

    public void UpdateTooltip(string newTooltip)
    {
        _tooltip = newTooltip;
        ModifyTooltip();
    }

    public void ShowBalloonTip(string title, string text, NOTIFY_ICON_INFOTIP_FLAGS infoFlags)
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = _uId,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_INFO,
            szInfo = text,
            szInfoTitle = title,
            dwInfoFlags = infoFlags
        };

        Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    public bool AddIcon()
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = _uId,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _iconHandle,
            szTip = _tooltip
        };

        return Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in nid);
    }

    public bool ModifyIcon()
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = _uId,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_ICON,
            hIcon = _iconHandle
        };

        return Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    public bool ModifyTooltip()
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = _uId,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            szTip = _tooltip
        };

        return Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in nid);
    }

    public bool RemoveIcon()
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = _uId
        };

        return Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in nid);
    }

    private static unsafe HICON LoadIconFromFile(string filePath)
    {
        fixed (char* iconPathPtr = filePath)
        {
            var hIcon = ExtractIcon(HINSTANCE.Null, new PCWSTR(iconPathPtr), 0);
            if (hIcon == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to load icon from file.");
            }

            return new HICON(hIcon);
        }
    }

    private static HICON LoadIconFromEmbeddedResource(string resourceName, Assembly resourceAssembly)
    {
        var iconPath = $"{resourceAssembly.GetName().Name}.{resourceName}";
        using var stream = resourceAssembly.GetManifestResourceStream(iconPath);

        if (stream == null)
        {
            throw new InvalidOperationException("Icon resource not found.");
        }

        using var bitmap = new Bitmap(stream);
        return new HICON(bitmap.GetHicon());
    }

    private static unsafe HWND CreateMinimalWindow()
    {
        fixed (char* classNamePtr = "MinimalWindowClass")
        {
            var wc = new WNDCLASSW
            {
                lpfnWndProc = WindowProc,
                lpszClassName = new PCWSTR(classNamePtr)
            };

            RegisterClass(in wc);

            return CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_NOACTIVATE,
                new PCWSTR(classNamePtr),
                new PCWSTR(classNamePtr),
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                0, 0, 0, 0,
                HWND.Null,
                HMENU.Null,
                HINSTANCE.Null,
                null);
        }
    }

    private static LRESULT WindowProc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        Console.WriteLine($"WindowProc: {uMsg}. WM_TRAYICON: {PInvoke.WM_COMMAND}. WM_COMMAND: {PInvoke.WM_COMMAND}");

        if (uMsg == WM_TRAYICON)
        {
            switch ((uint)lParam.Value)
            {
                case PInvoke.WM_LBUTTONDOWN:
                    OnLeftClick();
                    break;
                case PInvoke.WM_RBUTTONDOWN:
                    OnRightClick(hwnd);
                    break;
                case PInvoke.WM_LBUTTONDBLCLK:
                    OnDoubleClick();
                    break;
            }
        }
        else if (uMsg == PInvoke.WM_COMMAND)
        {
            switch ((uint)wParam.Value)
            {
                case 1:
                    Console.WriteLine("Item 1 clicked");
                    break;
                case 2:
                    Console.WriteLine("Item 2 clicked");
                    break;
                case 3:
                    Console.WriteLine("Exit clicked");
                    Environment.Exit(0);
                    break;
            }
        }
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    private static void OnLeftClick()
    {
        Console.WriteLine("Tray icon left-clicked");
    }

    private static void OnRightClick(HWND hwnd)
    {
        Console.WriteLine("Tray icon right-clicked");
        ShowContextMenu(hwnd);
    }

    private static void OnDoubleClick()
    {
        Console.WriteLine("Tray icon double-clicked");
    }

    private static unsafe void ShowContextMenu(HWND hwnd)
    {
        var hMenu = CreatePopupMenu();

        using (var item1 = new PinnedString("Item 1"))
        using (var item2 = new PinnedString("Item 2"))
        using (var exitItem = new PinnedString("Exit"))
        {
            AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UIntPtr)1, item1.Ptr);
            AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UIntPtr)2, item2.Ptr);
            AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_SEPARATOR, UIntPtr.Zero, (PCWSTR)null);
            AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UIntPtr)3, exitItem.Ptr);

            GetCursorPos(out var pt);
            SetForegroundWindow(hwnd);
            TrackPopupMenu(hMenu, TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN | TRACK_POPUP_MENU_FLAGS.TPM_RIGHTBUTTON, pt.X, pt.Y, 0, hwnd);
            PostMessage(hwnd, 0, 0, 0); // WM_NULL is 0
        }
    }

    public void Dispose()
    {
        RemoveIcon();
        PostMessage(_hwnd, PInvoke.WM_QUIT, 0, 0);
    }
}

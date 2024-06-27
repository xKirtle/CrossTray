using Windows.Win32.System.Threading;

namespace CrossTrayCore;

using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

[SupportedOSPlatform("windows5.1.2600")]
public class NotifyIconWrapper : IDisposable
{
    private HWND _hwnd;
    private HICON _iconHandle;
    private readonly uint _uId = 1;
    private string _tooltip;
    private readonly uint _wmTrayicon = RegisterWindowMessage("WM_TRAYICON");
    private readonly Thread _thread;
    private readonly ManualResetEvent _windowCreatedEvent;
    private bool _disposed = false;

    public Action OnLeftClickAction { get; set; }
    public Action OnDoubleClickAction { get; set; }
    public Action OnRightClickAction { get; set; }
    public Action<int> OnContextMenuItemClickAction { get; set; }

    public NotifyIconWrapper(string tooltip)
    {
        _tooltip = tooltip;
        _iconHandle = new HICON(SystemIcons.Application.Handle);
        _windowCreatedEvent = new ManualResetEvent(false);

        // Start the thread that will handle the NotifyIcon
        _thread = new Thread(ThreadProc);
        _thread.Start();

        // Wait for the window handle to be created
        _windowCreatedEvent.WaitOne();
    }

    private void ThreadProc()
    {
        // Create the window on this thread
        _hwnd = CreateMinimalWindow();
        _windowCreatedEvent.Set();

        while (GetMessage(out var msg, HWND.Null, 0, 0))
        {
            TranslateMessage(msg);
            DispatchMessage(msg);
        }
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
            uCallbackMessage = _wmTrayicon,
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

    private unsafe HWND CreateMinimalWindow()
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

    private LRESULT WindowProc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        Console.WriteLine($"WindowProc: {uMsg}. WM_TRAYICON: {_wmTrayicon}. WM_COMMAND: {WM_COMMAND}");

        if (uMsg == _wmTrayicon)
        {
            switch ((uint)lParam.Value)
            {
                case WM_LBUTTONDOWN:
                    OnLeftClickAction?.Invoke();
                    break;
                case WM_RBUTTONDOWN:
                    OnRightClickAction?.Invoke();
                    ShowContextMenu(hwnd);
                    break;
                case WM_LBUTTONDBLCLK:
                    OnDoubleClickAction?.Invoke();
                    break;
            }
        }
        else if (uMsg == WM_COMMAND)
        {
            int commandId = (int)wParam.Value;
            OnContextMenuItemClickAction?.Invoke(commandId);
        }
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    private static unsafe void ShowContextMenu(HWND hwnd)
    {
        var hMenu = CreatePopupMenu();

        using var item1 = new PinnedString("Item 1");
        using var item2 = new PinnedString("Item 2");
        using var exitItem = new PinnedString("Exit");
        AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UIntPtr)1, item1.Ptr);
        AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UIntPtr)2, item2.Ptr);
        AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_SEPARATOR, UIntPtr.Zero, (PCWSTR)null);
        AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_STRING, (UIntPtr)3, exitItem.Ptr);

        GetCursorPos(out var pt);
        SetForegroundWindow(hwnd);
        TrackPopupMenu(hMenu, TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN | TRACK_POPUP_MENU_FLAGS.TPM_RIGHTBUTTON, pt.X, pt.Y, 0, hwnd);
        PostMessage(hwnd, 0, 0, 0); // WM_NULL is 0
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            RemoveIcon();
            var threadHandle = OpenThread(THREAD_ACCESS_RIGHTS.THREAD_ALL_ACCESS, false, (uint)_thread.ManagedThreadId);
            PostThreadMessage(GetThreadId(threadHandle), WM_QUIT, 0, 0);
            CloseHandle(threadHandle);
        }

        _disposed = true;
    }
}

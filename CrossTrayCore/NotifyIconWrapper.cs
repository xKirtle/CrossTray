using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace CrossTrayCore;

[SupportedOSPlatform("windows6.0.6000")]
public class NotifyIconWrapper : INotifyIconWrapper, IDisposable
{
    private HWND _hwnd;
    private HICON _hicon;
    private string _tooltip;
    private const uint UId = 1; // What is this
    private readonly uint _wmTrayIcon = RegisterWindowMessage("WM_TRAYICON");

    private readonly Thread _thread;
    private readonly ManualResetEvent _windowCreatedEvent;
    private bool _disposed;
    
    private Dictionary<int, ContextMenuItem> _contextMenuItems = new();
    private int _nextMenuItemId = 1;
    
    public Action OnLeftClickAction { get; set; }
    public Action OnDoubleClickAction { get; set; }
    public Action OnRightClickAction { get; set; }

    public NotifyIconWrapper(string tooltip)
    {
        _tooltip = tooltip;
        _hicon = new HICON(SystemIcons.Application.Handle);
        _windowCreatedEvent = new ManualResetEvent(initialState: false);
        
        // Start the thread that will handle the NotifyIcon message loop
        _thread = new Thread(ThreadProc);
        _thread.Start();
        
        // and wait for the window handle to be created
        _windowCreatedEvent.WaitOne();
    }
    
    public void SetIconFromFile(string iconFilePath)
    {
        _hicon = LoadIconFromFile(iconFilePath);
        RefreshIcon();
    }

    public void SetIconFromEmbeddedResource(string resourceName, Assembly resourceAssembly)
    {
        _hicon = LoadIconFromEmbeddedResource(resourceName, resourceAssembly);
        RefreshIcon();
    }

    public void SetTooltip(string newTooltip)
    {
        _tooltip = newTooltip;
        RefreshIcon();
    }

    public void ShowBalloonTip(string title, string text, NOTIFY_ICON_INFOTIP_FLAGS infoFlags)
    {
        InternalShellNotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, NOTIFY_ICON_DATA_FLAGS.NIF_INFO, text, title, infoFlags);
    }

    public bool MountIcon()
    {
        return InternalShellNotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP);
    }

    public bool UnmountIcon()
    {
        return InternalShellNotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, 0);
    }

    public bool RefreshIcon()
    {
        return InternalShellNotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, NOTIFY_ICON_DATA_FLAGS.NIF_ICON);
    }

    public void AddContextMenuItem(string itemText, Action action)
    {
        var menuItem = new ContextMenuItem(itemText, action, MENU_ITEM_FLAGS.MF_STRING);
        _contextMenuItems.Add(_nextMenuItemId++, menuItem);
    }

    public void AddContextMenuSeparator()
    {
        var separator = new ContextMenuItem("", () => { }, MENU_ITEM_FLAGS.MF_SEPARATOR);
        _contextMenuItems.Add(_nextMenuItemId++, separator);
    }

    public void AddContextMenuSubmenu(string itemText, ICollection<(string subItemText, Action subItemAction)> submenuItems)
    {
        // TODO: Submenu must implement its own context menu with its own IDs... Think of a recursive solution
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnmountIcon();
        
        // Send a WM_QUIT message to the message loop to exit
        var threadHandle = OpenThread(THREAD_ACCESS_RIGHTS.THREAD_ALL_ACCESS, false, (uint)_thread.ManagedThreadId);
        PostThreadMessage(GetThreadId(threadHandle), WM_QUIT, 0, 0);
        CloseHandle(threadHandle);
        
        GC.SuppressFinalize(this);
        
        _disposed = true;
    }
    
    // internal/private methods
    
    private void ThreadProc()
    {
        _hwnd = CreateMinimalHiddenWindow();
        _windowCreatedEvent.Set();
        
        while (GetMessage(out var msg, HWND.Null, 0, 0))
        {
            TranslateMessage(msg);
            DispatchMessage(msg);
        }
    }
    
    private static HICON LoadIconFromFile(string filePath)
    {
        var hIcon = ExtractIcon(HINSTANCE.Null, StringToPCWSTR(filePath), 0);
        
        if (hIcon == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to load icon from file.");
        }

        return new HICON(hIcon);
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

    /// <summary>
    /// Use with caution! The returned <see cref="PCWSTR"/> instance will only be valid while the Garbage Collector doesn't move the string in memory.
    /// </summary>
    /// <param name="str">String to be converted.</param>
    /// <returns>A temporarily valid <see cref="PCWSTR"/> instance.</returns>
    private static unsafe PCWSTR StringToPCWSTR(string str)
    {
        fixed (char* strPtr = str)
        {
            return new PCWSTR(strPtr);
        }
    }

    private unsafe HWND CreateMinimalHiddenWindow()
    {
        var classNamePtr = StringToPCWSTR("MinimalHiddenWindowClass");
        
        var wc = new WNDCLASSW
        {
            lpfnWndProc = ProcessWindowMessages,
            lpszClassName = classNamePtr
        };
        
        RegisterClass(in wc);

        return CreateWindowEx(
            WINDOW_EX_STYLE.WS_EX_NOACTIVATE,
            classNamePtr,
            classNamePtr,
            WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
            0, 0, 0, 0,
            HWND.Null,
            HMENU.Null,
            HINSTANCE.Null,
            null);
    }

    /// <summary>
    /// Callback function that processes messages sent to a window
    /// </summary>
    /// <param name="hwnd">Handle to the window that is receiving the message.</param>
    /// <param name="uMsg">Message identifier. Specifies what kind of message is being received (e.g., a mouse click, a command, etc...).</param>
    /// <param name="wParam">Often used to pass specific data related to the message. In the case of WM_COMMAND, it holds the identifier of the menu item that was selected.</param>
    /// <param name="lParam">Additional message information, for instance, in the case of mouse messages, holds information about the mouse event (like which button was clicked).</param>
    /// <returns></returns>
    private LRESULT ProcessWindowMessages(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        // Check if the message is related to our tray icon
        if (uMsg == _wmTrayIcon)
        {
            switch ((uint)lParam.Value)
            {
                case WM_LBUTTONDOWN:
                    OnLeftClickAction?.Invoke();
                    break;
                case WM_RBUTTONDOWN:
                    OnRightClickAction?.Invoke();
                        CreateContextMenu(hwnd);
                    break;
                case WM_LBUTTONDBLCLK:
                    OnDoubleClickAction?.Invoke();
                    break;
            }
        }
        // Otherwise, check if the message is a command from the context menu
        else if (uMsg == WM_COMMAND)
        {
            var commandId = (int)wParam.Value;
            _contextMenuItems[commandId].Action.Invoke();
        }
        
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }
    
    private unsafe void CreateContextMenu(HWND hwnd)
    {
        var hMenu = CreatePopupMenu();

        foreach (var (menuItemId, contextMenuItem) in _contextMenuItems)
        {
            AppendMenu(hMenu, contextMenuItem.Flags, (nuint)menuItemId, contextMenuItem.Text.Ptr);

            // TODO: Add submenu support
            // if (contextMenuItem.SubItems.Count <= 0)
            // {
            //     continue;
            // }
            
            // var submenu = CreatePopupMenu();
            // foreach (var (subMenuItemId, subMenuItem) in contextMenuItem.SubItems.Select((item, i) => (i + 1, item)))
            // {
            //     AppendMenu(submenu, subMenuItem.Flags, (nuint)subMenuItemId, subMenuItem.Text.Ptr);
            // }
            // AppendMenu(hMenu, MENU_ITEM_FLAGS.MF_POPUP, (nuint)submenu, contextMenuItem.Text.Ptr);
        }

        GetCursorPos(out var point);
        SetForegroundWindow(hwnd);
        
        // Align popup to the left of the cursor and show it on right-click
        TrackPopupMenu(hMenu, TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN | TRACK_POPUP_MENU_FLAGS.TPM_RIGHTBUTTON, point.X, point.Y, 0, _hwnd);
        
        // Post a no-op message to the window to ensure the message loop processes any new messages
        PostMessage(hwnd, 0, 0, 0);
    }
    
    private bool InternalShellNotifyIcon(NOTIFY_ICON_MESSAGE nim, NOTIFY_ICON_DATA_FLAGS flags, string? info = null, string? infoTitle = null, NOTIFY_ICON_INFOTIP_FLAGS infoFlags = 0)
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = UId,
            uFlags = flags,
            szInfo = info,
            szInfoTitle = infoTitle,
            dwInfoFlags = infoFlags,
            uCallbackMessage = _wmTrayIcon,
            hIcon = _hicon,
            szTip = _tooltip
        };

        return Shell_NotifyIcon(nim, in nid);
    }
}
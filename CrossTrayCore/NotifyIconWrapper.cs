using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using CrossTrayCore.ContextMenuItems;
using static Windows.Win32.PInvoke;

namespace CrossTrayCore;

[SupportedOSPlatform("windows6.0.6000")]
public class NotifyIconWrapper : INotifyIconWrapper, IDisposable
{
    private HWND _hwnd;
    private readonly HICON _hIcon;
    private string _tooltip;
    private const uint UId = 1;
    private readonly uint _wmTrayIcon = RegisterWindowMessage("WM_TRAYICON");
    private readonly ClickTypes _showContextMenuClickType;

    private readonly Thread _thread;
    private readonly ManualResetEvent _windowCreatedEvent;
    private bool _disposed;
    
    private readonly Dictionary<int, ContextMenuItemBase> _contextMenuItems = new();
    private int _nextMenuItemId = 1;
    
    public event EventHandler<NotifyIconEventArgs>? OnLeftClick;
    public event EventHandler<NotifyIconEventArgs>? OnRightClick;
    public event EventHandler<NotifyIconEventArgs>? OnDoubleLeftClick;

    public NotifyIconWrapper(string tooltip, HICON hIcon = default, ClickTypes showContextMenuClickTypes = ClickTypes.Right)
    {
        _tooltip = tooltip;
        _hIcon = hIcon == default ? new HICON(SystemIcons.Application.Handle) : hIcon;
        _showContextMenuClickType = showContextMenuClickTypes;
        _windowCreatedEvent = new ManualResetEvent(initialState: false);
        
        // Start the thread that will handle the NotifyIcon message loop
        _thread = new Thread(ThreadProc);
        _thread.Start();
        
        // and wait for the window handle to be created
        _windowCreatedEvent.WaitOne();
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

    public static ContextMenuItemBase CreateSimpleMenuItem(string itemText, Action<ContextMenuItemBase> action, bool isEnabled = true)
    {
        return new SimpleMenuItem(itemText, action, isEnabled);
    }

    public static ContextMenuItemBase CreateSeparator()
    {
        return new SeparatorMenuItem();
    }

    public static ContextMenuItemBase CreatePopupMenuItem(string itemText, List<ContextMenuItemBase> submenuItems, bool isEnabled = true)
    {
        var submenuItem = new PopupMenuItem(itemText, isEnabled, submenuItems);
        
        foreach (var item in submenuItems)
        {
            item.Parent = submenuItem;
        }
        return submenuItem;
    }
    
    public static CheckableMenuItem CreateCheckableMenuItem(string itemText, Action<ContextMenuItemBase> action, bool isChecked = false, bool isEnabled = true)
    {
        return new CheckableMenuItem(itemText, action, isChecked, isEnabled);
    }
    
    public static CustomCheckableMenuItem CreateCustomCheckableMenuItem(string itemText, Action<ContextMenuItemBase> action, HICON checkedHicon, HICON uncheckedHicon, bool isChecked = false, bool isEnabled = true)
    {
        return new CustomCheckableMenuItem(itemText, action, checkedHicon, uncheckedHicon, isChecked, isEnabled);
    }
    
    public static IconMenuItem CreateIconMenuItem(string itemText, Action<ContextMenuItemBase> action, HICON icon, bool isEnabled = true)
    {
        return new IconMenuItem(itemText, action, icon, isEnabled);
    }

    public void CreateContextMenu(List<ContextMenuItemBase> contextMenuItems)
    {
        _contextMenuItems.Clear();
        _nextMenuItemId = 1;
        foreach (var item in contextMenuItems)
        {
            AddMenuItemRecursive(item, null);
        }
    }
    
    public static HICON LoadIconFromFile(string filePath)
    {
        var hIcon = ExtractIcon(HINSTANCE.Null, StringToPCWSTR(filePath), 0);
        
        if (hIcon == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to load icon from file.");
        }

        return new HICON(hIcon);
    }

    public static HICON LoadIconFromEmbeddedResource(string resourceName, Assembly resourceAssembly)
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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
    /// <returns><see cref="LRESULT"/></returns>
    private LRESULT ProcessWindowMessages(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        if (uMsg == _wmTrayIcon)
        {
            HandleTrayIconMessage(hwnd, (uint)lParam.Value);
        }
        else if (uMsg == WM_COMMAND)
        {
            HandleMenuItemClick((int)wParam.Value);
        }
        
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }
    
    private void HandleTrayIconMessage(HWND hwnd, uint mouseEventMessage)
    {
        void ShowContextMenuIfFlagSet(ClickTypes flag)
        {
            if (((uint)_showContextMenuClickType & (uint)flag) == (uint)flag)
            {
                ShowContextMenu(hwnd);
            }
        }
        
        switch (mouseEventMessage)
        {
            case WM_LBUTTONDOWN:
                OnLeftClick?.Invoke(this, new NotifyIconEventArgs(ClickTypes.Left));
                ShowContextMenuIfFlagSet(ClickTypes.Left);
                break;
            case WM_RBUTTONDOWN:
                OnRightClick?.Invoke(this, new NotifyIconEventArgs(ClickTypes.Right));
                ShowContextMenuIfFlagSet(ClickTypes.Right);
                break;
            case WM_LBUTTONDBLCLK:
                OnDoubleLeftClick?.Invoke(this, new NotifyIconEventArgs(ClickTypes.DoubleLeft));
                ShowContextMenuIfFlagSet(ClickTypes.DoubleLeft);
                break;
        }
    }
    
    private void HandleMenuItemClick(int menuItemId)
    {
        if (!_contextMenuItems.TryGetValue(menuItemId, out var menuItem))
        {
            return;
        }
        
        if (menuItem is CheckableMenuItem checkableMenuItem)
        {
            checkableMenuItem.Toggle();
        }
        
        if (menuItem is SimpleMenuItem simpleMenuItem)
        {
            simpleMenuItem.Action?.Invoke(menuItem);
        }
    }
    
    private unsafe void ShowContextMenu(HWND hwnd)
    {
        var hMenu = CreatePopupMenu();
        AddMenuItemsToMenu(hMenu, _contextMenuItems.Values.Where(item => item.Parent == null).ToList());

        GetCursorPos(out var point);
        SetForegroundWindow(hwnd);
        
        // Align popup to the left of the cursor and show it on right-click
        TrackPopupMenu(hMenu, TRACK_POPUP_MENU_FLAGS.TPM_LEFTALIGN | TRACK_POPUP_MENU_FLAGS.TPM_RIGHTBUTTON, point.X, point.Y, 0, _hwnd);
        
        PostMessage(hwnd, 0, 0, 0);
    }

    private static void AddMenuItemsToMenu(HMENU hMenu, List<ContextMenuItemBase> menuItems)
    {
        foreach (var contextMenuItem in menuItems)
        {
            contextMenuItem.AddToMenu(hMenu);
        }
    }


    private void AddMenuItemRecursive(ContextMenuItemBase menuItem, ContextMenuItemBase? parent)
    {
        menuItem.Id = _nextMenuItemId++;
        _contextMenuItems[menuItem.Id] = menuItem;

        if (parent != null)
        {
            menuItem.Parent = parent;
        }
        
        foreach (var subItem in menuItem.SubItems)
        {
            AddMenuItemRecursive(subItem, menuItem);
        }
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
            hIcon = _hIcon,
            szTip = _tooltip
        };

        return Shell_NotifyIcon(nim, in nid);
    }
    
    protected virtual void Dispose(bool disposing)
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
        
        _disposed = true;
    }
}

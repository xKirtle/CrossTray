using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace CrossTrayCore;

[SupportedOSPlatform("windows5.1.2600")]
public class NotifyIconWrapper(string tooltip)
{
    private readonly HWND _hwnd = CreateMinimalWindow();
    private HICON _iconHandle = new(SystemIcons.Application.Handle);
    private readonly uint _uId = 1;

    public void SetIconFromFile(string filePath)
    {
        _iconHandle = LoadIconFromFile(filePath);
    }
    
    public void SetIconFromEmbeddedResource(string resourceName, Assembly resourceAssembly)
    {
        _iconHandle = LoadIconFromEmbeddedResource(resourceName, resourceAssembly);
    }
    
    public bool AddIcon()
    {
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = _uId,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            uCallbackMessage = RegisterWindowMessage("WM_TRAYICON"),
            hIcon = _iconHandle,
            szTip = tooltip
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
        if (uMsg == RegisterWindowMessage("WM_TRAYICON"))
        {
            // Handle tray icon messages here (e.g., mouse clicks)
            Console.WriteLine("Tray icon message received.");
        }
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }
}
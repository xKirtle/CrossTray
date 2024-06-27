using System.Reflection;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using CrossTrayCore;

var tooltip = "CrossTray Test App";

// Create an instance of NotifyIconWrapper
using var notifyIcon = new NotifyIconWrapper(tooltip);

// Set the icon from the build output folder
// notifyIcon.SetIconFromFile("icon.ico");

// Set the icon from the embedded resource
notifyIcon.SetIconFromEmbeddedResource("icon.ico", Assembly.GetExecutingAssembly());

// Add the icon to the system tray
if (notifyIcon.AddIcon())
{
    Console.WriteLine("Icon added to the system tray.");
}
else
{
    Console.WriteLine("Failed to add icon to the system tray.");
}

// Show a balloon tip
notifyIcon.ShowBalloonTip("Hello", "This is a balloon tip!", NOTIFY_ICON_INFOTIP_FLAGS.NIIF_INFO);

// Message loop to keep the application running and processing messages
while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
{
    PInvoke.TranslateMessage(msg);
    PInvoke.DispatchMessage(msg);
}

notifyIcon.RemoveIcon();
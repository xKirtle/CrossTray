using System.Reflection;
using System.Runtime.Versioning;
using Windows.Win32.UI.Shell;
using CrossTrayCore;

namespace CrossTrayTestApp;

[SupportedOSPlatform("windows5.1.2600")]
public static class Program
{
    public static void Main()
    {
        var tooltip = "CrossTray Test App";

        // Create an instance of NotifyIconWrapper
        using var notifyIcon = new NotifyIconWrapper(tooltip);

        // Set the icon from the build output folder
        // notifyIcon.SetIconFromFile("icon.ico");

        // Set the icon from the embedded resource
        notifyIcon.SetIconFromEmbeddedResource("icon.ico", Assembly.GetExecutingAssembly());

        // Define actions for clicks
        notifyIcon.OnLeftClickAction = () => Console.WriteLine("Tray icon left-clicked");
        notifyIcon.OnDoubleClickAction = () => Console.WriteLine("Tray icon double-clicked");
        notifyIcon.OnRightClickAction = () => Console.WriteLine("Tray icon right-clicked");

        // Define actions for context menu items
        notifyIcon.OnContextMenuItemClickAction = commandId =>
        {
            switch (commandId)
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
        };

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

        // Keep the application running to see the tray icon
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
using System.Reflection;
using System.Runtime.Versioning;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using CrossTrayCore;

namespace CrossTrayTestApp;

[SupportedOSPlatform("windows6.0.6000")]
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
        notifyIcon.CreateContextMenu(
        [
            NotifyIconWrapper.CreateMenuItem("Item 1", (_) => Console.WriteLine("Item 1 clicked")),
            NotifyIconWrapper.CreateSubmenuItem("Popup 1",
            [
                NotifyIconWrapper.CreateMenuItem("Subitem 1.1", (_) => Console.WriteLine("Subitem 1.1 clicked")),
                NotifyIconWrapper.CreateMenuItem("Subitem 1.2", (_) => Console.WriteLine("Subitem 1.2 clicked")),
                NotifyIconWrapper.CreateSubmenuItem("Popup 2",
                [
                    NotifyIconWrapper.CreateMenuItem("Subitem 2.1", (_) => Console.WriteLine("Subitem 2.1 clicked")),
                    NotifyIconWrapper.CreateMenuItem("Subitem 2.2", (_) => Console.WriteLine("Subitem 2.2 clicked")),
                    NotifyIconWrapper.CreateSeparator(),
                    NotifyIconWrapper.CreateMenuItem("Subitem 2.3", (_) => Console.WriteLine("Subitem 2.3 clicked")),

                    NotifyIconWrapper.CreateSubmenuItem("Popup 3",
                    [
                        NotifyIconWrapper.CreateMenuItem("Subitem 3.1",
                            (_) => Console.WriteLine("Subitem 3.1 clicked")),
                        NotifyIconWrapper.CreateMenuItem("Subitem 3.2", (_) => Console.WriteLine("Subitem 3.2 clicked"))
                    ])
                ]),
                NotifyIconWrapper.CreateCheckableMenuItem("Checkable item",
                    (_) => Console.WriteLine("Checkable item clicked"))
            ]),

            NotifyIconWrapper.CreateSeparator(),
            NotifyIconWrapper.CreateCheckableMenuItem("Checkable item 2", (item) =>
            {
                var checkableItem = item as CheckableMenuItem;
                if (checkableItem.IsChecked)
                {
                    // ...
                }

                Console.WriteLine("Checkable item 2 clicked");
            }),

            NotifyIconWrapper.CreateIconMenuItem("Icon item", (_) => Console.WriteLine("Icon item clicked"),
                NotifyIconWrapper.LoadIconFromEmbeddedResource("icon.ico", Assembly.GetExecutingAssembly())),

            NotifyIconWrapper.CreateSeparator(),
            NotifyIconWrapper.CreateMenuItem("Exit", (_) => Environment.Exit(0))
        ]);

        // Add the icon to the system tray
        Console.WriteLine(notifyIcon.MountIcon()
            ? "Icon added to the system tray."
            : "Failed to add icon to the system tray.");

        // Show a balloon tip
        // notifyIcon.ShowBalloonTip("Hello", "This is a balloon tip!", NOTIFY_ICON_INFOTIP_FLAGS.NIIF_INFO);

        // Keep the application running to see the tray icon
        Console.WriteLine("Press Enter to unmount icon...");
        Console.ReadLine();

        notifyIcon.UnmountIcon();

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
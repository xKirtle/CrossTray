using System.Reflection;
using System.Runtime.Versioning;
using Windows.Win32.UI.Shell;
using CrossTrayCore;
using CrossTrayCore.ContextMenuItems;

namespace CrossTrayTestApp;

[SupportedOSPlatform("windows6.0.6000")]
public static class Program
{
    public static void Main()
    {
        const string tooltip = "CrossTray Test App";

        // Load the icon either from an embedded resource or a file.
        var redIcon = NotifyIconWrapper.LoadIconFromEmbeddedResource("red_icon.ico", Assembly.GetExecutingAssembly());
        var greenIcon = NotifyIconWrapper.LoadIconFromEmbeddedResource("green_icon.ico", Assembly.GetExecutingAssembly());
        var blueIcon = NotifyIconWrapper.LoadIconFromFile("blue_icon.ico");
      
        // Create an instance of NotifyIconWrapper.
        using var notifyIcon = new NotifyIconWrapper(tooltip, blueIcon, ClickTypes.Right | ClickTypes.DoubleLeft);

        // Define actions for clicks
        void HandleTrayIconClick(object? sender, NotifyIconEventArgs e)
        {
            Console.WriteLine($"Tray icon clicked with button: {e.ClickTypes.ToString()}");
        }
                
        notifyIcon.OnLeftClick += HandleTrayIconClick;
        notifyIcon.OnRightClick += HandleTrayIconClick;
        notifyIcon.OnDoubleLeftClick += HandleTrayIconClick;

        // Define actions for context menu items
        notifyIcon.CreateContextMenu(
        [
            NotifyIconWrapper.CreateSimpleMenuItem("One use item", item =>
            {
                Console.WriteLine("One use item clicked");
                item.IsEnabled = false;
            }),
            NotifyIconWrapper.CreatePopupMenuItem("Popup 0", []),
            NotifyIconWrapper.CreatePopupMenuItem("Popup 1",
            [
                NotifyIconWrapper.CreateSimpleMenuItem("Subitem 1.1", _ => Console.WriteLine("Subitem 1.1 clicked")),
                NotifyIconWrapper.CreateSimpleMenuItem("Subitem 1.2", _ => Console.WriteLine("Subitem 1.2 clicked")),
                NotifyIconWrapper.CreatePopupMenuItem("Popup 2",
                [
                    NotifyIconWrapper.CreateSimpleMenuItem("Subitem 2.1", _ => Console.WriteLine("Subitem 2.1 clicked")),
                    NotifyIconWrapper.CreateSimpleMenuItem("Subitem 2.2", _ => Console.WriteLine("Subitem 2.2 clicked")),
                    NotifyIconWrapper.CreateSeparator(),
                    NotifyIconWrapper.CreateSimpleMenuItem("Subitem 2.3", _ => Console.WriteLine("Subitem 2.3 clicked")),

                    NotifyIconWrapper.CreatePopupMenuItem("Popup 3",
                    [
                        NotifyIconWrapper.CreateSimpleMenuItem("Subitem 3.1", _ => Console.WriteLine("Subitem 3.1 clicked")),
                        NotifyIconWrapper.CreateSimpleMenuItem("Subitem 3.2", _ => Console.WriteLine("Subitem 3.2 clicked"))
                    ])
                ]),
                NotifyIconWrapper.CreateCheckableMenuItem("Checkable item 1.1",
                    _ => Console.WriteLine("Checkable item 1.1 clicked"))
            ]),

            NotifyIconWrapper.CreateSeparator(),
            NotifyIconWrapper.CreateCheckableMenuItem("Checkable item 1", item =>
            {
                var checkableItem = item as CheckableMenuItem;
                Console.WriteLine($"Checkable item 1 is {(checkableItem.IsChecked ? "checked" : "not checked")}");

                Console.WriteLine("Checkable item 1 clicked");
            }),
            
            NotifyIconWrapper.CreateCustomCheckableMenuItem("Custom checkable item", _ => Console.WriteLine("Custom checkable item clicked"),
                redIcon, greenIcon),

            NotifyIconWrapper.CreateSeparator(),
            
            NotifyIconWrapper.CreateIconMenuItem("Static Icon item", _ => Console.WriteLine("Static Icon item clicked"), greenIcon),
            NotifyIconWrapper.CreateSimpleMenuItem("Exit", _ => Environment.Exit(0))
        ]);

        // Add the icon to the system tray
        Console.WriteLine(notifyIcon.MountIcon()
            ? "Icon added to the system tray."
            : "Failed to add icon to the system tray.");

        // Show a balloon tip
        notifyIcon.ShowBalloonTip("Hello", "This is a balloon tip!", NOTIFY_ICON_INFOTIP_FLAGS.NIIF_INFO);

        // Keep the application running to see the tray icon
        Console.WriteLine("Press Enter to unmount icon...");
        Console.ReadLine();

        notifyIcon.UnmountIcon();

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
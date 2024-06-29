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
        void HandleMenuItemAction(ContextMenuItemBase item)
        {
            Console.WriteLine($"Menu item clicked: {item.Text}");
        }
        
        var contextMenuItems = new List<ContextMenuItemBase> 
        {
            new PopupMenuItem("Submenu", new List<ContextMenuItemBase>
            {
                new SimpleMenuItem("Sub item 1", HandleMenuItemAction),
                new SimpleMenuItem("Sub item 2", _ => { })
            }),
            new SeparatorMenuItem(),
            new IconMenuItem("Item with Icon", redIcon, HandleMenuItemAction),
            new SimpleMenuItem("Simple Item", _ => { }, isEnabled: false),
            new CheckableMenuItem("Checkable Item", item =>
            {
                var checkableItem = item as CheckableMenuItem;
                Console.WriteLine($"IsChecked: {checkableItem?.IsChecked}");
            }, isChecked: true)
        };
        
        notifyIcon.CreateContextMenu(contextMenuItems);

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
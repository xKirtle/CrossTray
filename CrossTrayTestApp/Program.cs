using System.Reflection;
using System.Runtime.Versioning;
using CrossTrayCore;

namespace CrossTrayTestApp;

[SupportedOSPlatform("windows5.1.2600")]
public static class Program
{
    static void Main(params string[] args)
    {
        const string iconName = "icon.ico";
        const string tooltip = "Test Tooltip";

        var notifyIcon = new NotifyIconWrapper(tooltip);
        notifyIcon.SetIconFromEmbeddedResource(iconName, Assembly.GetExecutingAssembly());
        
        if (notifyIcon.AddIcon())
        {
            Console.WriteLine("Icon added to the system tray.");
        }
        else
        {
            Console.WriteLine("Failed to add icon to the system tray.");
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        notifyIcon.RemoveIcon();
    }
}
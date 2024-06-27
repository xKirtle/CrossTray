using System.Reflection;
using Windows.Win32.UI.Shell;

namespace CrossTrayCore;

public interface INotifyIconWrapper
{
    void SetIconFromFile(string iconFilePath);
    void SetIconFromEmbeddedResource(string resourceName, Assembly resourceAssembly);
    void SetTooltip(string newTooltip);
    void ShowBalloonTip(string title, string text, NOTIFY_ICON_INFOTIP_FLAGS infoFlags);
    bool MountIcon();
    bool UnmountIcon();
    bool RefreshIcon();
    void AddContextMenuItem(string itemText, Action action);
    void AddContextMenuSeparator();
    void AddContextMenuSubmenu(string itemText, ICollection<(string subItemText, Action subItemAction)> submenuItems);
}
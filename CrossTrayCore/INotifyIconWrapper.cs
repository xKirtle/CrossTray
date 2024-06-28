using System.Reflection;
using Windows.Win32.UI.Shell;
using CrossTrayCore.ContextMenuItems;

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
    void CreateContextMenu(List<ContextMenuItemBase> contextMenuItems);
}
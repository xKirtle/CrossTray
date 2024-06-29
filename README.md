<h1 align="center">
  <br>
  <a href="#">
    <img src="./media/CrossTray.png" alt="CrossTray" width="256">
  </a>
  <br>
  CrossTray
  <br>
</h1>

<h4 align="center">A cross-platform .NET library for creating tray icons with customizable context menus.</h4>

## Key Features

- **Cross-Platform**: CrossTray is a .NET library that allows you to create tray icons with customizable context menus on Windows, Linux, and macOS.
- **Simple API**: CrossTray provides a simple API for creating tray icons and context menus with various types of menu items.
- **Lightweight**: CrossTray is a lightweight library with minimal dependencies that can be easily integrated into your .NET projects.
- **Open Source**: CrossTray is an open-source project released under the MIT License, so you can use it for free in your applications.

## How To Use

To clone and use this library, you'll need [Git](https://git-scm.com) and [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your computer. From your command line:

```bash
# Clone this repository
$ git clone https://github.com/xKirtle/CrossTray.git

# Go into the repository
$ cd CrossTray

# Build the library
$ dotnet build

# Reference the library in your project
# (Assuming you have a .NET project set up)
$ dotnet add reference ../crosstray/src/CrossTray/CrossTray.csproj
```
## Code Example

### Creating a Tray Icon
Creating a tray icon is very simple with CrossTray. All you need is an icon and a tooltip text.

```csharp
// Load the icon either from an embedded resource or a file.
var icon = NotifyIconWrapper.LoadIconFromEmbeddedResource("embedded_icon.ico", Assembly.GetExecutingAssembly());
// var icon = NotifyIconWrapper.LoadIconFromFile("file_icon.ico");

// NotifyIconWrapper implements IDisposable, so it is recommended to either use it within a using block or call Dispose() when you are done.
using var notifyIcon = new NotifyIconWrapper("Tooltip Text", icon);
notifyIcon.MountIcon();
```

### Defining Actions 

You can also define actions for left-click, right-click, and double left-click events on the tray icon.

```csharp
notifyIcon.OnLeftClick += (sender, args) => { ... };
notifyIcon.OnRightClick += (sender, args) => { ... };
notifyIcon.OnDoubleLeftClick += (sender, args) => { ... };
```

### Creating a Context Menu

You can also create a context menu for the tray icon by providing a list of menu items.
The library provides several types of menu items that can be added to the context menu:

- `SimpleMenuItem`: A simple menu item with text and an action.
- `IconMenuItem`: A menu item with text, an icon, and an action.
- `CheckableMenuItem`: A menu item with text, a checkable state, and an action.
- `CustomCheckableMenuItem`: A menu item with text, a checkable state with a custom icon for each state, and an action.
- `SeparatorMenuItem`: A separator between menu items.
- `PopupMenuItem`: A submenu with a list of menu items. (If the submenu is empty, the menu item will be ignored.)

To create a context menu, you just need to call the `CreateContextMenu` method with a list of menu items.

```csharp
var contextMenuItems = [
    new SimpleMenuItem("Simple Item", item => Console.WriteLine("Simple item clicked!")),
    new SeparatorMenuItem(),
    new SimpleMenuItem("Exit", item => Environment.Exit(0))
];

notifyIcon.CreateContextMenu(contextMenuItems);
```

If you decide to add a context menu to the tray icon, you might want to decide how the context menu is shown.

In the section <a href="#defining-actions">Defining Actions</a>, we showed how to define actions for left-click, right-click, and double-click events on the tray icon.
However, to modify how the context menu is shown, you can only change this behaviour by providing a custom flag to the constructor. By default, the context menu is shown on right-click.

```csharp
// Show the context menu on left-click, right-click, and double left-click.
var showContextMenuFlag = ClickTypes.Left | ClickTypes.Right | ClickTypes.DoubleLeft;
using var notifyIcon = new NotifyIconWrapper("Tooltip Text", icon, showContextMenuFlag);
```

### Example Windows Tray Icon

![Example Windows Tray Icon](media/ExampleTrayIcon.png)

## Download
You can download the latest version of CrossTray from the [releases page](https://github.com/xKirtle/CrossTray/releases).

## Credits
This library uses the following open source packages:

 - [.NET](https://dotnet.microsoft.com/)
 - [CsWin32](https://github.com/microsoft/CsWin32)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
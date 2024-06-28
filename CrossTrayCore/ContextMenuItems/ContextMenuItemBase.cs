using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public abstract class ContextMenuItemBase(
    string text,
    MENU_ITEM_FLAGS flags,
    List<ContextMenuItemBase>? subItems = null,
    ContextMenuItemBase? parent = null)
{
    public int Id { get; set; }
    public PinnedString Text { get; } = new(text);
    public MENU_ITEM_FLAGS Flags { get; protected set; } = flags;
    public List<ContextMenuItemBase> SubItems { get; } = subItems ?? [];
    public ContextMenuItemBase? Parent { get; set; } = parent;

    public virtual void AddToMenu(HMENU hMenu)
    {
        PInvoke.AppendMenu(hMenu, Flags, (uint)Id, Text.Ptr);
    }
    
    // TODO: Add a disabled property. If true, do Flags &= MENU_ITEM_FLAGS.MF_ENABLED on AddToMenu?
    // Hard to apply a disabled property to everything, since not all menu items are calling our base AddToMenu...
}

// MENU_ITEM_FLAGS Explanation:
//     MF_BYCOMMAND (0x00000000): Indicates that the menu item identifier is used to specify the menu item. This is the default.
//     MF_BYPOSITION (0x00000400): Indicates that the position of the menu item within the menu is used to specify the item.
//     MF_BITMAP (0x00000004): Specifies that the menu item is a bitmap. The lpNewItem parameter contains the handle to the bitmap.
//     MF_CHECKED (0x00000008): Checks the menu item. A check mark is displayed next to the item.
//     MF_DISABLED (0x00000002): Disables the menu item so that it cannot be selected, but it is not grayed out.
//     MF_ENABLED (0x00000000): Enables the menu item so that it can be selected. This is the default.
//     MF_GRAYED (0x00000001): Disables the menu item and grays it out so that it cannot be selected.
//     MF_MENUBARBREAK (0x00000020): Functions the same as MF_MENUBREAK for a menu bar. For a drop-down menu, submenu, or shortcut menu, the new column is separated from the old column by a vertical line.
//     MF_MENUBREAK (0x00000040): Places the item on a new line (for a menu bar) or in a new column (for a drop-down menu, submenu, or shortcut menu) without a vertical line.
//     MF_OWNERDRAW (0x00000100): Specifies that the item is an owner-drawn item. Before the menu is displayed, the system sends the window that owns the menu a WM_MEASUREITEM message to determine the width and height of the menu item, and it sends a WM_DRAWITEM message when the visual aspect of the menu item must be drawn.
//     MF_POPUP (0x00000010): Specifies that the item opens a drop-down menu or submenu. The lpNewItem parameter specifies a handle to the drop-down menu or submenu.
//     MF_SEPARATOR (0x00000800): Draws a horizontal dividing line. This flag is used only in a drop-down menu, submenu, or shortcut menu. The line cannot be selected.
//     MF_STRING (0x00000000): Specifies that the menu item is a text string; lpNewItem is a pointer to the string. This is the default.
//     MF_UNCHECKED (0x00000000): Does not check the menu item. This is the default.
//     MF_INSERT (0x00000000): Inserts the menu item at the position specified by uPosition. This is the default.
//     MF_CHANGE (0x00000080): Replaces the existing menu item at the position specified by uPosition.
//     MF_APPEND (0x00000100): Adds the menu item to the end of the menu. This flag is default if neither MF_INSERT nor MF_CHANGE is specified.
//     MF_DELETE (0x00000200): Deletes the menu item.
//     MF_REMOVE (0x00001000): Removes the menu item, but does not destroy the associated memory.
//     MF_USECHECKBITMAPS (0x00000200): Uses the bitmap for check marks. The system displays the appropriate bitmap next to the menu item. If the application uses this flag, it must provide a bitmap for both the checked and unchecked states.
//     MF_UNHILITE (0x00000000): Removes the highlighting from the menu item. This is the default.
//     MF_HILITE (0x00000080): Highlights the menu item.
//     MF_DEFAULT (0x00001000): Specifies that the menu item is the default. A menu can contain only one default menu item, which is displayed in bold.
//     MF_SYSMENU (0x00002000): Specifies that the item is part of a window menu (system menu).
//     MF_HELP (0x00004000): Specifies that the item is a Help item; when the user presses F1, the system sends a WM_HELP message to the window.
//     MF_RIGHTJUSTIFY (0x00004000): Right-justifies the menu item and any subsequent items. This flag is valid only if the menu item is in a menu bar.
//     MF_MOUSESELECT (0x00008000): The menu item is a menu item in a menu bar. The user selects the item using the mouse.
//     MF_END (0x00000080): This flag is not used.
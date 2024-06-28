﻿using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CrossTrayCore.ContextMenuItems;

public class SimpleMenuItem(string text, Action<ContextMenuItemBase> action, ContextMenuItemBase? parent = null)
    : ContextMenuItemBase(text, MENU_ITEM_FLAGS.MF_STRING, null, parent)
{
    public Action<ContextMenuItemBase> Action { get; } = action;
}
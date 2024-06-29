using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace CrossTrayCore;

public class PinnedString(string str) : IDisposable
{
    private GCHandle _handle = GCHandle.Alloc(str, GCHandleType.Pinned);
    private bool _disposed = false;

    public unsafe PCWSTR Ptr => new PCWSTR((char*)_handle.AddrOfPinnedObject());

    public void Dispose()
    {
        if (!_disposed)
        {
            _handle.Free();
            _disposed = true;
        }
    }

    public override string ToString() => str;
}
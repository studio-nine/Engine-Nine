namespace Nine.Studio.Controls
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.Xna.Framework.Graphics;

    [ComImport, Guid("D0223B96-BF7A-43fd-92BD-A43B0D82B9EB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDirect3DDevice9
    {
        void TestCooperativeLevel();
        void GetAvailableTextureMem();
        void EvictManagedResources();
        void GetDirect3D();
        void GetDeviceCaps();
        void GetDisplayMode();
        void GetCreationParameters();
        void SetCursorProperties();
        void SetCursorPosition();
        void ShowCursor();
        void CreateAdditionalSwapChain();
        void GetSwapChain();
        void GetNumberOfSwapChains();
        void Reset();
        void Present();
        int GetBackBuffer(uint swapChain, uint backBuffer, int type, out IntPtr backBufferPointer);
    }

    static class Interop
    {
        public static unsafe IntPtr GetDirect3DDevice(GraphicsDevice graphics)
        {
            FieldInfo comPtr = graphics.GetType().GetField("pComPtr", BindingFlags.NonPublic | BindingFlags.Instance);
            return new IntPtr(Pointer.Unbox(comPtr.GetValue(graphics)));
        }

        public static IntPtr GetBackBuffer(GraphicsDevice graphicsDevice)
        {
            IntPtr surfacePointer;
            var device = GetIUnknownObject<IDirect3DDevice9>(graphicsDevice);
            Marshal.ThrowExceptionForHR(device.GetBackBuffer(0, 0, 0, out surfacePointer));
            Marshal.ReleaseComObject(device);
            return surfacePointer;
        }

        public static T GetIUnknownObject<T>(object container)
        {
            unsafe
            {
                //Get the COM object pointer from the D3D object and marshal it as one of the interfaces defined below
                var deviceField = container.GetType().GetField("pComPtr", BindingFlags.NonPublic | BindingFlags.Instance);
                var devicePointer = new IntPtr(Pointer.Unbox(deviceField.GetValue(container)));
                return (T)Marshal.GetObjectForIUnknown(devicePointer);
            }
        }
    }
}

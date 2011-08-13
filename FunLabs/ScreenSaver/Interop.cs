#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace DirectX
{
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
        void GetRasterStatus();
        void SetDialogBoxMode();
        void SetGammaRamp();
        void GetGammaRamp();
        int CreateTexture(uint Width, uint Height, uint Levels, int Usage, int Format, int Pool, out IntPtr ppTexture, IntPtr pSharedHandle);
        void CreateVolumeTexture();
        void CreateCubeTexture();
        void CreateVertexBuffer();
        void CreateIndexBuffer();
        void CreateRenderTarget();
        void CreateDepthStencilSurface();
        int UpdateSurface(IntPtr pSourceSurface, IntPtr pSourceRect, IntPtr pDestinationSurface, IntPtr pDestinationPoint);
        void UpdateTexture();
        void GetRenderTargetData();
        int GetFrontBufferData(uint swapChain, IntPtr surface);
        void StretchRect();
        void ColorFill();
        int CreateOffscreenPlainSurface(uint Width, uint Height, int Format, int Pool, out IntPtr ppSurface, IntPtr pSharedHandle);
    }

    [ComImport, Guid("85C31227-3DE5-4f00-9B3A-F11AC38C18B5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDirect3DTexture9
    {
        void GetDevice();
        void SetPrivateData();
        void GetPrivateData();
        void FreePrivateData();
        void SetPriority();
        void GetPriority();
        void PreLoad();
        void GetType();
        void SetLOD();
        void GetLOD();
        void GetLevelCount();
        void SetAutoGenFilterType();
        void GetAutoGenFilterType();
        void GenerateMipSubLevels();
        void GetLevelDesc();
        int GetSurfaceLevel(uint level, out IntPtr surfacePointer);
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

        static IntPtr frontBufferSurfacePointer;

        public static Texture2D CreateFrontBufferTexture(GraphicsDevice graphics)
        {
            var device = GetIUnknownObject<IDirect3DDevice9>(graphics);

            Marshal.ThrowExceptionForHR(device.CreateOffscreenPlainSurface(
               (uint)Screen.PrimaryScreen.Bounds.Width, (uint)Screen.PrimaryScreen.Bounds.Height, 21, 3, out frontBufferSurfacePointer, IntPtr.Zero));


            //return new Texture2D(graphics, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, false, SurfaceFormat.Color);

            IntPtr texturePointer;
            Marshal.ThrowExceptionForHR(device.CreateTexture(
               (uint)Screen.PrimaryScreen.Bounds.Width, (uint)Screen.PrimaryScreen.Bounds.Height, 1, 0, 21, 1, out texturePointer, IntPtr.Zero));

            var d3dTexture = (IDirect3DTexture9)Marshal.GetObjectForIUnknown(texturePointer);

            var cons = typeof(Texture2D).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First();
            var result = (Texture2D)cons.Invoke(new object[] { texturePointer, graphics }); ;

            //Marshal.ReleaseComObject(d3dTexture);
            //Marshal.ReleaseComObject(device);
            return result;
        }

        public static void GetFrontBuffer(Texture2D texture)
        {
            var device = GetIUnknownObject<IDirect3DDevice9>(texture.GraphicsDevice);
            Marshal.ThrowExceptionForHR(device.GetFrontBufferData(0, frontBufferSurfacePointer));
            
            IntPtr surfacePointer;
            var d3dTexture = GetIUnknownObject<IDirect3DTexture9>(texture);

            Marshal.ThrowExceptionForHR(d3dTexture.GetSurfaceLevel(0, out surfacePointer));

            Marshal.ThrowExceptionForHR(device.UpdateSurface(frontBufferSurfacePointer, IntPtr.Zero, surfacePointer, IntPtr.Zero));

            //Marshal.Release(surfacePointer);
            //Marshal.ReleaseComObject(device);
            //Marshal.ReleaseComObject(d3dTexture);
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

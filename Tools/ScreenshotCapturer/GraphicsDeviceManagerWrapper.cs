#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Components;
#endregion

namespace Nine.Tools.ScreenshotCapturer
{
    class GameExitException : Exception { }

    class GraphicsDeviceManagerWrapper : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
    {
        Nine.Components.ScreenshotCapturer capturer = null;
        IGraphicsDeviceManager manager;
        IGraphicsDeviceService service;
        Game game;
        ScreenCaptureTask task;
        int frame;

        public GraphicsDeviceManagerWrapper(Game game, ScreenCaptureTask task)
        {
            this.game = game;
            this.task = task;
            
            Form gameForm = (Form)Form.FromHandle(game.Window.Handle);
            gameForm.TopLevel = false;
            gameForm.TopMost = false;
            gameForm.ShowInTaskbar = false;
            gameForm.SendToBack();
            gameForm.FormBorderStyle = FormBorderStyle.None;
            gameForm.Shown += delegate(object sender, EventArgs e) { gameForm.Hide(); };
            
            manager = game.Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
            service = game.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;

            if (manager is GraphicsDeviceManager)
            {
                ((GraphicsDeviceManager)manager).GraphicsProfile = GraphicsProfile.HiDef;
            }
            
            game.Services.RemoveService(typeof(IGraphicsDeviceManager));
            game.Services.RemoveService(typeof(IGraphicsDeviceService));

            service.DeviceCreated += (o, e) => { if (DeviceCreated != null) DeviceCreated(this, e); };
            service.DeviceDisposing += (o, e) => { if (DeviceDisposing != null) DeviceDisposing(this, e); };
            service.DeviceReset += (o, e) => { if (DeviceReset != null) DeviceReset(this, e); };
            service.DeviceResetting += (o, e) => { if (DeviceResetting != null) DeviceResetting(this, e); };

            game.Services.RemoveService(typeof(IGraphicsDeviceManager));
            game.Services.RemoveService(typeof(IGraphicsDeviceService));

            game.Services.AddService(typeof(IGraphicsDeviceManager), this);
            game.Services.AddService(typeof(IGraphicsDeviceService), this);

            game.Components.Add(capturer = new Nine.Components.ScreenshotCapturer(GraphicsDevice));
        }
        
        public bool BeginDraw()
        {
            if (frame == 0)
                SetParameters(game, task);
            return manager.BeginDraw();
        }

        public void CreateDevice()
        {
            manager.CreateDevice();
        }

        public void EndDraw()
        {
            if (frame++ == task.Frame)
            {
                ResizeAndSave(capturer.Capture());

                // Game.Exist prevents the current process from creating a new Game,
                // so raise an exception instead.
                throw new GameExitException();
            }
            manager.EndDraw();
        }

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return service.GraphicsDevice; }
        }

        public void Dispose()
        {
            IDisposable md = manager as IDisposable;
            if (md != null)
                md.Dispose();

            IDisposable sd = service as IDisposable;
            if (sd != null)
                sd.Dispose();
        }

        private static void SetParameters(Game game, ScreenCaptureTask task)
        {
            foreach (var pair in task.Parameters)
            {
                try
                {
                    SetValue(game, pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error setting property {0} to {1}", pair.Key, pair.Value));
                }
            }
        }

        private static void SetValue(object Target, string TargetProperty, string value)
        {
            MemberInfo member = null;

            if (Target != null && !string.IsNullOrEmpty(TargetProperty))
            {
                // Extract a valid property target
                member = Target.GetType().GetProperty(TargetProperty);

                if (member == null)
                    member = Target.GetType().GetField(TargetProperty);

                if (member == null)
                {
                    throw new ArgumentException(
                        "Type " + Target.GetType().Name +
                        " does not have a valid public property or field: " + TargetProperty);
                }
            }

            if (Target != null)
            {
                if (member is FieldInfo)
                {
                    (member as FieldInfo).SetValue(Target, Convert.ChangeType(value, (member as FieldInfo).FieldType));
                }
                else if (member is PropertyInfo)
                {
                    (member as PropertyInfo).SetValue(Target, Convert.ChangeType(value, (member as PropertyInfo).PropertyType), null);
                }
            }
        }
        
        private void ResizeAndSave(Texture2D screenshot)
        {
            using (RenderTarget2D renderTarget = new RenderTarget2D(GraphicsDevice, task.Width, task.Height))
            {
                GraphicsDevice.SetRenderTarget(renderTarget);
                SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.AnisotropicClamp, null, null);

                float scale = Math.Max(1.0f * task.Width / screenshot.Width, 1.0f * task.Height / screenshot.Height);
                Vector2 position = new Vector2();
                position.X = GraphicsDevice.Viewport.Bounds.GetCenter().X;
                position.Y = GraphicsDevice.Viewport.Bounds.GetCenter().Y;
                Vector2 origin = new Vector2();
                origin.X = screenshot.Bounds.GetCenter().X;
                origin.Y = screenshot.Bounds.GetCenter().Y;

                spriteBatch.Draw(screenshot, position, null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
                using (FileStream savedFile = new FileStream(task.OutputFilename, FileMode.OpenOrCreate))
                {
                    renderTarget.SaveAsPng(savedFile, task.Width, task.Height);
                    Console.WriteLine("Screenshot Saved: " + Path.GetFullPath(task.OutputFilename));
                }
            }
        }
    }
}

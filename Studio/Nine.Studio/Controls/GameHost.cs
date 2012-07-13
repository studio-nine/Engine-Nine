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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Components;
#endregion

namespace Nine.Studio.Controls
{
    public partial class GameHost : ContentControl
    {
        // TODO: Cannot change this profile after the surface is loaded
        public GraphicsProfile GraphicsProfile { get; set; }

        public Game Game
        {
            get { return (Game)GetValue(GameProperty); }
            set { SetValue(GameProperty, value); }
        }

        public event EventHandler<EventArgs> GameLoaded;

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("Game", typeof(Game), typeof(GameHost), new UIPropertyMetadata(OnGameChanged));

        private InputComponent inputComponent;
        private bool gameHasChanged = true;
        private bool suppressUpdate = false;
        private DrawingSurface Surface;

        public GameHost()
        {
            GraphicsProfile = GraphicsProfile.HiDef;
            Loaded += new RoutedEventHandler(GameHost_Loaded);
        }

        private void GameHost_Loaded(object sender, RoutedEventArgs e)
        {
            if (Surface == null)
            {
                AddChild(Surface = new DrawingSurface() { GraphicsProfile = GraphicsProfile });
                Surface.Draw += Surface_Draw;
                RedirectInput();
            }
        }

        public void SuppressUpdate()
        {
            suppressUpdate = true;
        }

        private void Surface_Draw(object sender, DrawEventArgs e)
        {
            if (Game == null)
                return;

            if (gameHasChanged && Game != null)
            {
                gameHasChanged = false;

                HideGameWindow(Game);
                SetInteropGraphics(Game);
                SetGameInput(Game);
                StartGame(Game);

                if (GameLoaded != null)
                    GameLoaded(this, EventArgs.Empty);
            }

            UpdateGameWindow(Game);
            UpdateMouseWheel();
            Activate(Game);
            Game.Tick();

            if (!suppressUpdate)
                e.InvalidateSurface();
            else
                suppressUpdate = false;
        }

        private static void OnGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameHost)d).gameHasChanged = true;
            ((GameHost)d).OnGameChanged(e);
        }

        protected virtual void OnGameChanged(DependencyPropertyChangedEventArgs e) { }

        private void SetInteropGraphics(Microsoft.Xna.Framework.Game Game)
        {
            Game.Services.RemoveService(typeof(IGraphicsDeviceService));
            Game.Services.AddService(typeof(IGraphicsDeviceService), Surface.GraphicsDeviceService);
        }

        private void HideGameWindow(Game Game)
        {
            System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)
                System.Windows.Forms.Control.FromHandle((Game.Window.Handle));

            gameForm.Visible = false;
            gameForm.TopLevel = false;
            gameForm.TopMost = false;
            gameForm.ShowInTaskbar = false;
            gameForm.SendToBack();
            gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gameForm.Shown += (sender, e) => { gameForm.Hide(); };
            gameForm.VisibleChanged += (sender, e) => { gameForm.Visible = false; };
            gameForm.Show();
        }

        private void UpdateGameWindow(Game Game)
        {
            System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle((Game.Window.Handle));

            if (Application.Current.MainWindow != null)
            {
                System.Windows.Point position = Surface.Image.PointToScreen(new System.Windows.Point(0, 0));

                if (form.Left != (int)(position.X))
                    form.Left = (int)(position.X);
                if (form.Top != (int)(position.Y))
                    form.Top = (int)(position.Y);
            }
        }

        private void SetGameInput(Game Game)
        {
            inputComponent = Game.Components.FirstOrDefault(component => component is InputComponent) as InputComponent;
            if (inputComponent == null)
                Game.Components.Add(inputComponent = new InputComponent());
            //inputComponent.HostingEnabled = true;
            Microsoft.Xna.Framework.Input.Mouse.WindowHandle = new WindowInteropHelper(GetTopLevelControl<Window>(this)).Handle;
        }

        private T GetTopLevelControl<T>(DependencyObject control) where T : DependencyObject
        {
            DependencyObject tmp = control;
            while ((tmp = VisualTreeHelper.GetParent(tmp)) != null && !(tmp is T)) { }
            return tmp as T;
        }

        private void StartGame(Game Game)
        {
            MethodInfo mi;
            mi = typeof(Game).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(Game, null);
            mi = typeof(Game).GetMethod("BeginRun", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(Game, null);
        }

        private void Activate(Game Game)
        {
            FieldInfo fi = typeof(Game).GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(Game, true);
        }

        #region Redirect Input
        private bool leftDown = false;
        private bool rightDown = false;
        private bool middleDown = false;
        private int mouseX;
        private int mouseY;
        private int wheel;

        private void RedirectInput()
        {
            Surface.MouseDown += new MouseButtonEventHandler(Surface_MouseDown);
            Surface.MouseMove += new MouseEventHandler(Surface_MouseMove);
            Surface.MouseUp += new MouseButtonEventHandler(Surface_MouseUp);
            Surface.KeyDown += new KeyEventHandler(Surface_KeyDown);
            Surface.KeyUp += new KeyEventHandler(Surface_KeyUp);
            Surface.MouseWheel += new MouseWheelEventHandler(Surface_MouseWheel);
        }

        void Surface_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (inputComponent != null)
            {
                MouseEventArgs args;
                var cursor = Microsoft.Xna.Framework.Input.Mouse.GetState();
                inputComponent.Wheel(args = new MouseEventArgs(MouseButtons.Middle,
                                                    (int)cursor.X, (int)cursor.Y, (float)e.Delta));
                e.Handled = args.Handled;
            }
        }

        void UpdateMouseWheel()
        {
            if (inputComponent != null)
            {
                var cursor = Microsoft.Xna.Framework.Input.Mouse.GetState();
                if (cursor.ScrollWheelValue == wheel)
                    return;
                MouseEventArgs args = new MouseEventArgs(MouseButtons.Middle, (int)cursor.X, (int)cursor.Y,
                                                    (float)cursor.ScrollWheelValue - wheel);
                args.IsLeftButtonDown = leftDown;
                args.IsRightButtonDown = rightDown;
                args.IsMiddleButtonDown = middleDown;
                inputComponent.Wheel(args);
                wheel = cursor.ScrollWheelValue;
            }
        }

        void Surface_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (inputComponent != null)
            {
                // This produces more smooth result then the MouseMove event triggered by wpf.
                var cursor = Microsoft.Xna.Framework.Input.Mouse.GetState();

                MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, (int)cursor.X, (int)cursor.Y, 0);
                args.IsLeftButtonDown = leftDown;
                args.IsRightButtonDown = rightDown;
                args.IsMiddleButtonDown = middleDown;
                inputComponent.MouseMove(args);
                mouseX = cursor.X;
                mouseY = cursor.Y;
            }
        }

        void Surface_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (inputComponent != null)
            {
                Surface.ReleaseMouseCapture();

                if (e.ChangedButton == MouseButton.Left)
                    leftDown = false;
                else if (e.ChangedButton == MouseButton.Right)
                    rightDown = false;
                else if (e.ChangedButton == MouseButton.Middle)
                    middleDown = false;

                MouseEventArgs args;
                var cursor = Microsoft.Xna.Framework.Input.Mouse.GetState();
                inputComponent.MouseUp(args = new MouseEventArgs(ConvertButton(e.ChangedButton), (int)cursor.X, (int)cursor.Y, 0));
                e.Handled = args.Handled;
            }
        }

        void Surface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (inputComponent != null)
            {
                Surface.CaptureMouse();

                if (e.ChangedButton == MouseButton.Left)
                    leftDown = true;
                else if (e.ChangedButton == MouseButton.Right)
                    rightDown = true;
                else if (e.ChangedButton == MouseButton.Middle)
                    middleDown = true;

                MouseEventArgs args;
                var cursor = Microsoft.Xna.Framework.Input.Mouse.GetState();
                inputComponent.MouseDown(args = new MouseEventArgs(ConvertButton(e.ChangedButton), (int)cursor.X, (int)cursor.Y, 0));
                e.Handled = args.Handled;
            }
        }

        void Surface_KeyUp(object sender, KeyEventArgs e)
        {
            if (inputComponent != null)
            {
                KeyboardEventArgs args;
                inputComponent.KeyUp(args = new KeyboardEventArgs(ConvertKeys(e.Key)));
                e.Handled = args.Handled;
            }
        }

        void Surface_KeyDown(object sender, KeyEventArgs e)
        {
            if (inputComponent != null)
            {
                KeyboardEventArgs args;
                inputComponent.KeyDown(args = new KeyboardEventArgs(ConvertKeys(e.Key)));
                e.Handled = args.Handled;
            }
        }

        private MouseButtons ConvertButton(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    return MouseButtons.Left;
                case MouseButton.Middle:
                    return MouseButtons.Middle;
                case MouseButton.Right:
                    return MouseButtons.Right;
                default:
                    return MouseButtons.Left;
            }
        }


        private Microsoft.Xna.Framework.Input.Keys ConvertKeys(Key key)
        {
            // FIXME: This is not correct. Avoid using it!!!
            return (Microsoft.Xna.Framework.Input.Keys)key;
        }
        #endregion
    }
}

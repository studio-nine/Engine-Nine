#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// EventArgs used by ScreenshotCapturer.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScreenshotCapturedEventArgs : EventArgs
    {
        /// <summary>
        /// The captured screenshot texture.
        /// </summary>
        public Texture2D Screenshot { get; internal set; }

        /// <summary>
        /// The filename of the saved screenshot.
        /// </summary>
        public string Filename { get; internal set; }
    }

    /// <summary>
    /// Screenshot capturer component that captures screenshots.
    /// </summary>
    public class ScreenshotCapturer : GameComponent
    {
        #region Variables
        /// <summary>
        /// Internal screenshot number (will increase by one each screenshot)
        /// </summary>
        private int screenshotNum = 0;
        private bool pressedLastFrame = false;

        private string screenshotsDirectory;

        /// <summary>
        /// Gets or sets the directory where the screenshot files will be stored.
        /// </summary>
        public string ScreenshotsDirectory 
        {
            get { return screenshotsDirectory; }

            set 
            {
                screenshotsDirectory = value; 
                screenshotNum = GetCurrentScreenshotNum(); 
            }
        }

        /// <summary>
        /// Gets or sets the key used to capture a screenshot.
        /// </summary>
        public Keys CaptureKey { get; set; }

        /// <summary>
        /// Occurs when a new screenshot is captured.
        /// </summary>
        public event EventHandler<ScreenshotCapturedEventArgs> Captured;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of ScreenshotCapturer.
        /// </summary>
        public ScreenshotCapturer(Game game) : base(game)
        {
            ScreenshotsDirectory = "Screenshots";
            screenshotNum = GetCurrentScreenshotNum();
            CaptureKey = Keys.PrintScreen;
        }
        #endregion

        #region Make screenshot
        #region Screenshot name builder
        /// <summary>
        /// Screenshot name builder
        /// </summary>
        /// <param name="num">Num</param>
        /// <returns>String</returns>
        private string ScreenshotNameBuilder(int num)
        {
            return ScreenshotsDirectory + "/" +
                Game.Window.Title + " Screenshot " +
                num.ToString("0000") + ".png";
        }
        #endregion

        #region Get current screenshot num
        /// <summary>
        /// Get current screenshot num
        /// </summary>
        /// <returns>Int</returns>
        private int GetCurrentScreenshotNum()
        {
            // We must search for last screenshot we can found in list using own
            // fast filesearch
            int i = 0, j = 0, k = 0, l = -1;
            // First check if at least 1 screenshot exist
            if (File.Exists(ScreenshotNameBuilder(0)) == true)
            {
                // First scan for screenshot num/1000
                for (i = 1; i < 10; i++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000)) == false)
                        break;
                }

                // This i*1000 does not exist, continue scan next level
                // screenshotnr/100
                i--;
                for (j = 1; j < 10; j++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100)) == false)
                        break;
                }

                // This i*1000+j*100 does not exist, continue scan next level
                // screenshotnr/10
                j--;
                for (k = 1; k < 10; k++)
                {
                    if (File.Exists(ScreenshotNameBuilder(
                            i * 1000 + j * 100 + k * 10)) == false)
                        break;
                }

                // This i*1000+j*100+k*10 does not exist, continue scan next level
                // screenshotnr/1
                k--;
                for (l = 1; l < 10; l++)
                {
                    if (File.Exists(ScreenshotNameBuilder(
                            i * 1000 + j * 100 + k * 10 + l)) == false)
                        break;
                }

                // This i*1000+j*100+k*10+l does not exist, we have now last
                // screenshot nr!!!
                l--;
            }

            return i * 1000 + j * 100 + k * 10 + l;
        }
        #endregion
        
        /// <summary>
        /// Takes a new Screenshot of the current backbuffer.
        /// </summary>
        public Texture2D Capture()
        {
            string filename;
            return Capture(false, out filename);
        }

        /// <summary>
        /// Takes a new Screenshot of the current backbuffer and save it to local storage.
        /// </summary>
        public string CaptureAndSave()
        {
            string filename = null;
#if WINDOWS
            Capture(true, out filename);
#endif
            return filename;
        }

        private Texture2D Capture(bool save, out string filename)
        {
            filename = null;
            Texture2D screenshot = null;

            try
            {
                int width = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
                int height = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;

                // Get data with help of the resolve method
                Color[] backbuffer = new Color[width * height];
                Game.GraphicsDevice.GetBackBufferData<Color>(backbuffer);

                screenshot = new Texture2D(Game.GraphicsDevice, width, height);
                screenshot.SetData<Color>(backbuffer);
#if WINDOWS
                screenshotNum++;
                
                if (save)
                {
                    // Make sure screenshots directory exists
                    if (Directory.Exists(ScreenshotsDirectory) == false)
                        Directory.CreateDirectory(ScreenshotsDirectory);

                    using (FileStream savedFile = new FileStream(filename = ScreenshotNameBuilder(screenshotNum), FileMode.OpenOrCreate))
                    {
                        screenshot.SaveAsPng(savedFile, width, height);
                    }
                    return null;
                }
#endif
                OnCaptured(new ScreenshotCapturedEventArgs() { Filename = filename, Screenshot = screenshot });
                return screenshot;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Raised when a new screenshot is taken.
        /// </summary>
        protected virtual void OnCaptured(ScreenshotCapturedEventArgs e)
        {
            if (Captured != null)
                Captured(this, e);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool pressed = Keyboard.GetState().IsKeyDown(CaptureKey);

            if (pressedLastFrame && !pressed)
            {
                pressedLastFrame = false;
                CaptureAndSave();
            }

            pressedLastFrame = pressed;
        }
        #endregion
    }
}
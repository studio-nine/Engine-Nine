namespace Nine.Graphics.UI.Controls
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Media;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    /// <summary>
    /// A Control that displays Video's.
    /// </summary>
    public class MediaElement : UIElement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Playing Video.
        /// </summary>
        public Video Source
        {
            get { return video; }
            set 
            { 
                video = value; 
                Play(); // Not sure if this should be placed here.
            }
        }
        private Video video;

        /// <summary>
        /// Gets or sets, if the video should loop.
        /// </summary>
        public bool Loop
        {
            get { return player.IsLooped; }
            set { player.IsLooped = value; }
        }

        /// <summary>
        /// Gets or sets, if the video is muted.
        /// </summary>
        public bool IsMuted
        {
            get { return player.IsLooped; }
            set { player.IsLooped = value; }
        }

        /// <summary>
        /// Gets the current video position.
        /// </summary>
        public TimeSpan Position
        {
            get { return player.PlayPosition; }
        }

        /// <summary>
        /// Gets the videos length.
        /// </summary>
        public TimeSpan VideoLength
        {
            get { return PlayerVideo.Duration; }
        }

        /// <summary>
        /// Gets the State.
        /// </summary>
        public MediaState State
        {
            get { return player.State; }
        }

        /// <summary>
        /// Gets the Current Playing Video.
        /// </summary>
        public Video PlayerVideo
        {
            get { return player.Video; }
        }

        /// <summary>
        /// Gets or sets the video Volume.
        /// </summary>
        public float Volume
        {
            get { return player.Volume; }
            set { player.Volume = value; }
        }

        #endregion

        private VideoPlayer player;
        private Texture2D Texture;

        /// <summary>
        /// Initializes a new instance <see cref="MediaElement">MeidaElement</see> where the video is not set.
        /// </summary>
        public MediaElement() : this(null) { }

        /// <summary>
        /// Initializes a new instance <see cref="MediaElement">MeidaElement</see>.
        /// </summary>
        /// <param name="video">Video</param>
        public MediaElement(Video video)
        {
            player = new VideoPlayer();
            if (video != null)
                this.Source = video;
        }

        #region Methods

        /// <summary>
        /// Plays the Video.
        /// </summary>
        public void Play()
        {
            if (Source == null)
                throw new ArgumentNullException();
            player.Play(Source);
        }
        
        /// <summary>
        /// Resumes the video, if the state is Paused.
        /// </summary>
        public void Resume()
        {
            if (player.State == MediaState.Paused)
                player.Resume();
        }

        /// <summary>
        /// Pause the video, if the state is Playing.
        /// </summary>
        public void Pause()
        {
            if (player.State == MediaState.Playing)
                player.Pause();
        }

        /// <summary>
        /// Stops the video, if it isn't already stopped.
        /// </summary>
        public void Stop()
        {
            if (player.State != MediaState.Stopped)
                player.Stop();
        }

        protected internal override void OnRender(DynamicPrimitive dynamicPrimitive)
        {
            base.OnRender(dynamicPrimitive);

            if (player.State != MediaState.Stopped)
                Texture = player.GetTexture();

            if (Texture != null)
            {
                dynamicPrimitive.AddRectangle(AbsoluteRenderTransform, new ImageBrush() { Source = Texture }, null);
            }
        }

        #endregion
    }
}

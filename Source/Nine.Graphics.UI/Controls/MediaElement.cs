namespace Nine.Graphics.UI.Controls
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Media;

    public class MediaElement : UIElement
    {
        #region Properties

        public Video Source
        {
            get { return video; }
            set 
            { 
                video = value; 
                Play(); 
                // Not sure if this should be placed here.
            }
        }
        private Video video;

        public bool Loop
        {
            get { return player.IsLooped; }
            set { player.IsLooped = value; }
        }

        public bool IsMuted
        {
            get { return player.IsLooped; }
            set { player.IsLooped = value; }
        }

        public TimeSpan Position
        {
            get { return player.PlayPosition; }
        }

        public TimeSpan VideoLength
        {
            get { return PlayerVideo.Duration; }
        }

        public MediaState State
        {
            get { return player.State; }
        }

        public Video PlayerVideo
        {
            get { return player.Video; }
        }

        public float Volume
        {
            get { return player.Volume; }
            set { player.Volume = value; }
        }

        #endregion

        private VideoPlayer player;
        private Texture2D Texture;

        public MediaElement() : this(null) { }
        public MediaElement(Video video)
        {
            player = new VideoPlayer();
            if (video != null)
                this.Source = video;
        }

        #region Methods

        public void Play()
        {
            if (Source == null)
                throw new ArgumentNullException();
            player.Play(Source);
        }
        
        public void Resume()
        {
            if (player.State == MediaState.Playing)
                player.Resume();
        }

        public void Pause()
        {
            if (player.State == MediaState.Playing)
                player.Pause();
        }

        public void Stop()
        {
            if (player.State != MediaState.Stopped)
                player.Stop();
        }

        protected internal override void OnRender(SpriteBatch spriteBatch)
        {
            base.OnRender(spriteBatch);

            if (player.State != MediaState.Stopped)
                Texture = player.GetTexture();

            if (Texture != null)
            {
                spriteBatch.Draw(Texture, AbsoluteRenderTransform, Color.White);
            }
        }

        #endregion
    }
}

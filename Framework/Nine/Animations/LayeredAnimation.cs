#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Animations
{
    /// <summary>
    /// Contains several animation clips that are played concurrently.
    /// The animation completes when all of its containing animations
    /// had finished playing.
    /// </summary>
    public class LayeredAnimation : Animation, IEnumerable<SequentialAnimation>
    {
        List<SequentialAnimation> layers = new List<SequentialAnimation>();

        /// <summary>
        /// Gets all the layers in the animation.
        /// </summary>
        public IList<SequentialAnimation> Layers { get { return layers; } }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c>.
        /// </summary>
        public LayeredAnimation() { }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> with the specified
        /// number of layers created initially.
        /// </summary>
        public LayeredAnimation(int numLayers)
        {
            for (int i = 0; i < numLayers; i++)
                layers.Add(new SequentialAnimation());
        }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public LayeredAnimation(IEnumerable<IAnimation> animations)
        {
            foreach (IAnimation animation in animations)
                layers.Add(new SequentialAnimation(animation));
        }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public LayeredAnimation(params IAnimation[] animations)
        {
            foreach (IAnimation animation in animations)
                layers.Add(new SequentialAnimation(animation));
        }

        protected override void OnStarted()
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Play();
            base.OnStarted();
        }
        
        protected override void OnStopped()
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Stop();

            base.OnStopped();
        }

        protected override void OnPaused()
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Pause();

            base.OnPaused();
        }

        protected override void OnResumed()
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Resume();

            base.OnResumed();
        }

        public override event EventHandler Completed;

        public override void  Update(GameTime gameTime)
        {
            if (State == AnimationState.Playing)
            {
                bool allStopped = true;

                for (int i = 0; i < layers.Count; i++)
                {
                    IUpdateObject update = layers[i] as IUpdateObject;

                    if (update != null)
                        update.Update(gameTime);

                    if (layers[i].State == AnimationState.Playing)
                    {
                        allStopped = false;
                    }
                }

                if (allStopped)
                {
                    OnCompleted();
                    OnStopped();
                }
            }
 	    
            base.Update(gameTime);
        }

        protected virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public IEnumerator<SequentialAnimation> GetEnumerator()
        {
            return layers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return layers.GetEnumerator();
        }
    }
}
#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Transitions.Curves;
#endregion

namespace Isles.Transitions
{
    #region TransitionCallback<T>
    internal sealed class TransitionCallback<T> where T : struct
    {
        private object Object;
        private MemberInfo Member;
        private Action<T> Callback;

        public TransitionCallback(object target, string member)
        {
            Member = target.GetType().GetProperty(member);

            if (Member == null)
                Member = target.GetType().GetField(member);

            if (Member == null)
            {
                throw new InvalidDataException("Object of type " + target.GetType().Name +
                                               " do not have a valid property or field: " + member);
            }

            Object = target;
        }

        public TransitionCallback(Action<T> callback)
        {
            if (callback == null)
                throw new ArgumentNullException();

            Callback = callback;
        }

        public T? GetValue()
        {
            if (Object != null)
            {
                if (Member is FieldInfo)
                {
                    return (T)(Member as FieldInfo).GetValue(Object);
                }
                
                if (Member is PropertyInfo)
                {
                    return (T)(Member as PropertyInfo).GetValue(Object, null);
                }
            }

            return null;
        }

        public void SetValue(T value)
        {
            if (Object != null)
            {
                if (Member is FieldInfo)
                {
                    (Member as FieldInfo).SetValue(Object, value);
                }
                else if (Member is PropertyInfo)
                {
                    (Member as PropertyInfo).SetValue(Object, value, null);
                }
            }
            else if (Callback != null)
            {
                Callback(value);
            }
        }

        public static bool operator ==(TransitionCallback<T> x, TransitionCallback<T> y)
        {
            return (x.Callback != null && x.Callback == y.Callback) ||
                   (x.Object != null && x.Object == y.Object && x.Member == y.Member);
        }

        public static bool operator !=(TransitionCallback<T> x, TransitionCallback<T> y)
        {
            return !((x.Callback != null && x.Callback == y.Callback) ||
                     (x.Object != null && x.Object == y.Object && x.Member == y.Member));
        }
    }
    #endregion

    #region TransitionChannel<T>
    internal sealed class TransitionChannel<T> where T : struct
    {
        public bool UseFutureFromValue;
        public TimeSpan Delay;         
        public ITweener<T> Tweener; 
        public TransitionCallback<T> Callback;
        public TransitionChannel<T> Queued;
    }
    #endregion

    #region Transition<T>
    internal sealed class Transition<T> : IUpdateObject where T : struct
    {
        EnumerationCollection<TransitionChannel<T>> activeTransitions = new EnumerationCollection<TransitionChannel<T>>();
        EnumerationCollection<TransitionChannel<T>> delayedTransitions = new EnumerationCollection<TransitionChannel<T>>();


        public void Start(ITweener<T> tweener, TimeSpan delay, bool useFutureFromValue, TransitionCallback<T> callback)
        {
            TransitionChannel<T> channel = new TransitionChannel<T>();

            channel.Tweener = tweener;
            channel.Delay = delay;
            channel.UseFutureFromValue = useFutureFromValue;
            channel.Callback = callback;

            if (tweener is IAnimation)
                (tweener as IAnimation).Complete += new EventHandler(Transition_Complete);
            
            delayedTransitions.Add(channel);
        }

        public void Queue(ITweener<T> tweener, TimeSpan delay, bool useFutureFromValue, TransitionCallback<T> callback)
        {
            TransitionChannel<T> channel = new TransitionChannel<T>();

            channel.Tweener = tweener;
            channel.Delay = delay;
            channel.UseFutureFromValue = useFutureFromValue;
            channel.Callback = callback;

            if (tweener is IAnimation)
                (tweener as IAnimation).Complete += new EventHandler(Transition_Complete);

            
            // Search for delayed transitions from back to front
            for (int i = delayedTransitions.Count - 1; i >= 0; i--)
            {
                if (delayedTransitions[i].Callback == callback)
                {
                    TransitionChannel<T> link = delayedTransitions[i];

                    while (link.Queued != null)
                        link = link.Queued;

                    link.Queued = channel;
                    return;
                }
            }

            // Search for active transitions
            for (int i = activeTransitions.Count - 1; i >= 0; i--)
            {
                if (activeTransitions[i].Callback == callback)
                {
                    TransitionChannel<T> link = activeTransitions[i];

                    while (link.Queued != null)
                        link = link.Queued;

                    link.Queued = channel;
                    return;
                }
            }

            // Nothing to be queued
            Start(tweener, delay, useFutureFromValue, callback);
        }

        private void Transition_Complete(object sender, EventArgs e)
        {
            for (int i = 0; i < activeTransitions.Count; i++)
            {
                if (activeTransitions[i].Tweener == sender)
                {
                    // Process queued transitions
                    if (activeTransitions[i].Queued != null)
                    {
                        delayedTransitions.Add(activeTransitions[i].Queued);
                        activeTransitions[i].Queued = null;
                    }
   
                    activeTransitions.RemoveAt(i);
                    break;
                }
            }
        }

        public void Remove(TransitionCallback<T> callback)
        {
            delayedTransitions.RemoveAll(o => o.Callback == callback);
            activeTransitions.RemoveAll(o => o.Callback == callback);
        }

        public void Update(GameTime time)
        {
            // Process delayed transitions
            foreach (TransitionChannel<T> channel in delayedTransitions)
            {
                channel.Delay -= time.ElapsedGameTime;

                if (channel.Delay <= TimeSpan.Zero)
                {
                    // Update tweener from
                    if (channel.UseFutureFromValue && channel.Tweener is Tweener<T>)
                    {
                        T? value = channel.Callback.GetValue();

                        if (value.HasValue)
                            (channel.Tweener as Tweener<T>).From = value.Value;
                    }

                    // Remove delayed transitions
                    delayedTransitions.RemoveAll(o => o.Callback == channel.Callback && o.Delay <= TimeSpan.Zero);
                    activeTransitions.RemoveAll(o => o.Callback == channel.Callback);
                    activeTransitions.Add(channel);
                }
            }

            // Process active transitions
            foreach (TransitionChannel<T> channel in activeTransitions)
            {
                channel.Callback.SetValue(channel.Tweener.Update(time));
            }
        }
    }
    #endregion
}
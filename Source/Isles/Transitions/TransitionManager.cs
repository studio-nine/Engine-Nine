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
    public sealed class TransitionManager : IUpdateObject
    {
        Dictionary<Type, IUpdateObject> transitions = new Dictionary<Type, IUpdateObject>();
        

        #region Start Reflection
        public void Start<TValue>(ITweener<TValue> tweener, object target, string member)
            where TValue : struct
        {
            GetTransition<TValue>().Start(tweener, TimeSpan.Zero, false, new TransitionCallback<TValue>(target, member));
        }

        public Tweener<TValue> Start<TValue, TCurve>(float duration, TValue? from, TValue to, object target, string member)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Start<TValue, TCurve>(TimeSpan.Zero, TimeSpan.FromSeconds(duration), from, to, LoopStyle.None, Easing.In, target, member);
        }

        public Tweener<TValue> Start<TValue, TCurve>(TimeSpan duration, TValue? from, TValue to, object target, string member)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Start<TValue, TCurve>(TimeSpan.Zero, duration, from, to, LoopStyle.None, Easing.In, target, member);
        }

        public Tweener<TValue> Start<TValue, TCurve>(TimeSpan delay, TimeSpan duration, TValue? from, TValue to,
                                                LoopStyle style, Easing easing, object target, string member)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            Transition<TValue> transition = GetTransition<TValue>();

            Tweener<TValue> tweener = new Tweener<TValue>();

            tweener.Curve = new TCurve();
            tweener.Duration = duration;
            tweener.Style = style;
            tweener.Easing = easing;
            tweener.From = from.HasValue ? from.Value : default(TValue);
            tweener.To = to;

            transition.Start(tweener, delay, !from.HasValue, new TransitionCallback<TValue>(target, member));

            return tweener;
        }
        #endregion
        
        #region Start Delegate
        public void Start<TValue>(ITweener<TValue> tweener, Action<TValue> action)
            where TValue : struct
        {
            GetTransition<TValue>().Start(tweener, TimeSpan.Zero, false, new TransitionCallback<TValue>(action));
        }

        public Tweener<TValue> Start<TValue, TCurve>(float duration, TValue from, TValue to, Action<TValue> action)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Start<TValue, TCurve>(TimeSpan.Zero, TimeSpan.FromSeconds(duration), from, to, LoopStyle.None, Easing.In, action);
        }

        public Tweener<TValue> Start<TValue, TCurve>(TimeSpan duration, TValue from, TValue to, Action<TValue> action)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Start<TValue, TCurve>(TimeSpan.Zero, duration, from, to, LoopStyle.None, Easing.In, action);
        }

        public Tweener<TValue> Start<TValue, TCurve>(TimeSpan delay, TimeSpan duration, TValue from, TValue to,
                                                     LoopStyle style, Easing easing, Action<TValue> action)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            Transition<TValue> transition = GetTransition<TValue>();

            Tweener<TValue> tweener = new Tweener<TValue>();

            tweener.Curve = new TCurve();
            tweener.Duration = duration;
            tweener.Style = style;
            tweener.Easing = easing;
            tweener.From = from;
            tweener.To = to;

            transition.Start(tweener, delay, false, new TransitionCallback<TValue>(action));

            return tweener;
        }
        #endregion

        #region Queue Reflection
        public void Queue<TValue>(ITweener<TValue> tweener, object target, string member)
            where TValue : struct
        {
            GetTransition<TValue>().Queue(tweener, TimeSpan.Zero, false, new TransitionCallback<TValue>(target, member));
        }

        public Tweener<TValue> Queue<TValue, TCurve>(float duration, TValue? from, TValue to, object target, string member)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Queue<TValue, TCurve>(TimeSpan.Zero, TimeSpan.FromSeconds(duration), from, to, LoopStyle.None, Easing.In, target, member);
        }

        public Tweener<TValue> Queue<TValue, TCurve>(TimeSpan duration, TValue? from, TValue to, object target, string member)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Queue<TValue, TCurve>(TimeSpan.Zero, duration, from, to, LoopStyle.None, Easing.In, target, member);
        }

        public Tweener<TValue> Queue<TValue, TCurve>(TimeSpan delay, TimeSpan duration, TValue? from, TValue to,
                                                LoopStyle style, Easing easing, object target, string member)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            Transition<TValue> transition = GetTransition<TValue>();

            Tweener<TValue> tweener = new Tweener<TValue>();

            tweener.Curve = new TCurve();
            tweener.Duration = duration;
            tweener.Style = style;
            tweener.Easing = easing;
            tweener.From = from.HasValue ? from.Value : default(TValue);
            tweener.To = to;

            transition.Queue(tweener, delay, !from.HasValue, new TransitionCallback<TValue>(target, member));

            return tweener;
        }
        #endregion

        #region Queue Delegate
        public void Queue<TValue>(ITweener<TValue> tweener, Action<TValue> action)
            where TValue : struct
        {
            GetTransition<TValue>().Queue(tweener, TimeSpan.Zero, false, new TransitionCallback<TValue>(action));
        }

        public Tweener<TValue> Queue<TValue, TCurve>(float duration, TValue from, TValue to, Action<TValue> action)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Queue<TValue, TCurve>(TimeSpan.Zero, TimeSpan.FromSeconds(duration), from, to, LoopStyle.None, Easing.In, action);
        }

        public Tweener<TValue> Queue<TValue, TCurve>(TimeSpan duration, TValue from, TValue to, Action<TValue> action)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            return Queue<TValue, TCurve>(TimeSpan.Zero, duration, from, to, LoopStyle.None, Easing.In, action);
        }

        public Tweener<TValue> Queue<TValue, TCurve>(TimeSpan delay, TimeSpan duration, TValue from, TValue to,
                                                     LoopStyle style, Easing easing, Action<TValue> action)
            where TValue : struct
            where TCurve : ICurve, new()
        {
            Transition<TValue> transition = GetTransition<TValue>();

            Tweener<TValue> tweener = new Tweener<TValue>();

            tweener.Curve = new TCurve();
            tweener.Duration = duration;
            tweener.Style = style;
            tweener.Easing = easing;
            tweener.From = from;
            tweener.To = to;

            transition.Queue(tweener, delay, false, new TransitionCallback<TValue>(action));

            return tweener;
        }
        #endregion

        #region Remove
        public void Remove<TValue>(object target, string member) where TValue : struct            
        {
            IUpdateObject updateObject;

            if (transitions.TryGetValue(typeof(TValue), out updateObject))
            {
                (updateObject as Transition<TValue>).Remove(new TransitionCallback<TValue>(target, member));
            }
        }

        public void Remove<TValue>(Action<TValue> action) where TValue : struct
        {
            IUpdateObject updateObject;

            if (transitions.TryGetValue(typeof(TValue), out updateObject))
            {
                (updateObject as Transition<TValue>).Remove(new TransitionCallback<TValue>(action));
            }
        }
        #endregion


        private Transition<TValue> GetTransition<TValue>() where TValue : struct
        {
            IUpdateObject updateObject;

            if (!transitions.TryGetValue(typeof(TValue), out updateObject))
            {
                transitions.Add(typeof(TValue), updateObject = new Transition<TValue>());
            }

            return updateObject as Transition<TValue>;
        }

        public void Update(GameTime time)
        {
            foreach (IUpdateObject update in transitions.Values)
            {
                update.Update(time);
            }
        }
    }
}
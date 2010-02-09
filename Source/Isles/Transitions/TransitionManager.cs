#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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
#endregion


namespace Isles.Transitions
{
    public sealed class TransitionManager
    {
        sealed class Entry
        {
            public IAnimation Animation;
            public ITransition Transition;
            public TimeSpan Delay;
            public MemberInfo Info;
            public object Target;
            public string Member;
            public bool Smooth;
        }

        List<Entry> delayedTransitions = new List<Entry>();
        EnumerationCollection<Entry, List<Entry>> transitions = new EnumerationCollection<Entry, List<Entry>>();


        public void Start(ITransition transition, object target, string member)
        {
            Start(transition, target, member, TimeSpan.Zero, false);
        }

        /// <summary>
        /// Starts a new transition on the specified member of the target.
        /// </summary>
        /// <param name="transition">The transition to be played.</param>
        /// <param name="target">The target object.</param>
        /// <param name="member">Name of the target field or property. Must be both gettable and settable.</param>
        /// <param name="delay">Delay the transition for the specifed amount of time.</param>
        /// <param name="smooth">
        /// Whether the transition will use the current value of the target field when the transition starts.
        /// </param>
        public void Start(ITransition transition, object target, string member, TimeSpan delay, bool smooth)
        {
            if (transition == null)
                throw new ArgumentNullException();

            // Remove existing transitions
            if (delay > TimeSpan.Zero)
            {
                for (int i = 0; i < delayedTransitions.Count; i++)
                {
                    if (delayedTransitions[i].Target == target &&
                        delayedTransitions[i].Member == member)
                    {
                        delayedTransitions.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    if (transitions.Elements[i].Target == target &&
                        transitions.Elements[i].Member == member)
                    {
                        transitions.Elements.RemoveAt(i);
                        break;
                    }
                }
            }
                        

            // Add new transition
            MemberInfo info;

            info = target.GetType().GetProperty(member);

            if (info == null)
                info = target.GetType().GetField(member);

            if (info != null)
            {
                Entry e = new Entry();

                e.Transition = transition;
                e.Animation = transition as IAnimation;
                e.Delay = delay;
                e.Info = info;
                e.Target = target;
                e.Member = member;
                e.Smooth = smooth;

                if (e.Animation != null)
                    e.Animation.Complete += new EventHandler(Animation_Complete);

                if (delay >= TimeSpan.Zero)
                    delayedTransitions.Add(e);
                else
                    transitions.Add(e);
            }
        }

        public void Clear()
        {
            transitions.Clear();
        }

        void Animation_Complete(object sender, EventArgs e)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions.Elements[i].Animation == sender)
                {
                    transitions.Elements.RemoveAt(i);
                    break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < delayedTransitions.Count; i++)
            {
                Entry e = delayedTransitions[i];

                e.Delay -= gameTime.ElapsedGameTime;

                if (e.Delay <= TimeSpan.Zero)
                {
                    delayedTransitions.RemoveAt(i);
                    transitions.Add(e);
                    i--;

                    if (e.Smooth) 
                    {
                        object value = null;

                        if (e.Info is FieldInfo)
                        {
                            value = (e.Info as FieldInfo).GetValue(e.Target);
                        }
                        else if (e.Info is PropertyInfo)
                        {
                            value = (e.Info as PropertyInfo).GetValue(e.Target, null);
                        }

                        PropertyInfo property = e.Transition.GetType().GetProperty("Start");
                        
                        if (property != null)
                            property.SetValue(e.Transition, value, null);
                    }
                }
            }


            foreach (Entry e in transitions)
            {
                e.Transition.Update(gameTime);

                if (e.Info is FieldInfo)
                {
                    (e.Info as FieldInfo).SetValue(e.Target, e.Transition.Value);
                }
                else if (e.Info is PropertyInfo)
                {
                    (e.Info as PropertyInfo).SetValue(e.Target, e.Transition.Value, null);
                }
            }
        }
    }
}
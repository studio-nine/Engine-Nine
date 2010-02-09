#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Isles.Graphics;
using Isles.Graphics.Models;
#endregion


namespace Isles.Components
{
    public class WorldObject : IDisplayObject, IPickable
    {
        struct Binding
        {
            public int Bone;
            public IDisplayObject Target;
        }

        private Model model;
        private ModelSkinning skinning;
        private List<Binding> bindings;


        #region Components
        [Loader("Components", Serializer=typeof(ComponentsLoader))]
        private EnumerationCollection<object, List<object>> components = 
            new EnumerationCollection<object, List<object>>();
        
        internal class ComponentsLoader : IXmlLoader
        {
            public object Load(XmlElement input, IServiceProvider services)
            {
                EnumerationCollection<object, List<object>> components =
                    new EnumerationCollection<object, List<object>>();

                XmlLoader loader = new XmlLoader();

                foreach (XmlNode childNode in input.ChildNodes)
                {
                    if (childNode is XmlElement)
                    {
                        object child = loader.Load(childNode as XmlElement, services);

                        if (child != null)
                            components.Add(child);
                    }
                }

                return components;
            }
        }
        #endregion


        [Loader(IsService=true)]
        public ModelBatch ModelBatch { get; set; }
        public Matrix Transform { get; set; }
        public ModelEffect Effect { get; set; }
        public ICollection<object> Components { get { return components; } }
        public ModelAnimation Animation { get; internal set; }
        public Geometry Collision { get; set; }

        [Loader(IsContent=true)]
        public Model Model
        {
            get { return model; }
            set
            {
                model = value;

                // Invalidate bindings
                if (bindings != null)
                    bindings.Clear();

                // Update skinning and animation
                if (model != null)
                {
                    skinning = ModelExtensions.GetSkinning(model);
                    Animation.Model = model;
                    Animation.AnimationClip = ModelExtensions.GetAnimation(model, 0);
                    Animation.Play();
                }
                else
                {
                    skinning = null;
                    Animation.Model = null;
                    Animation.AnimationClip = null;
                }
            }
        }


        public WorldObject()
        {
            Animation = new ModelAnimation();
        }

        public void Bind(string boneName, IDisplayObject attachment)
        {
            ModelBone bone;

            if (model != null && model.Bones.TryGetValue(boneName, out bone))
            {
                Bind(bone.Index, attachment);
            }
        }

        public void Bind(int bone, IDisplayObject attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException();
            
            Unbind(attachment);

            Binding binding;

            binding.Target = attachment;
            binding.Bone = bone;

            bindings.Add(binding);
        }

        public void Unbind(IDisplayObject attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException();

            if (bindings == null)
                bindings = new List<Binding>();
            
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].Target == attachment)
                {
                    bindings.RemoveAt(i);
                    break;
                }
            }
        }

        public T GetComponent<T>()
        {
            foreach (object obj in components)
                if (obj is T)
                    return (T)obj;

            return default(T);
        }

        public IEnumerable<T> GetComponents<T>()
        {
            foreach (object obj in components)
                if (obj is T)
                    yield return (T)obj;
        }

        public virtual void Update(GameTime gameTime)
        {
            Animation.Update(gameTime);

            foreach (object o in components)
            {
                IUpdateObject tick = o as IUpdateObject;

                if (tick != null)
                    tick.Update(gameTime);
            }
        }

        public virtual void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (ModelBatch != null && model != null)
            {
                if (skinning == null)
                {
                    if (Effect == null)
                        ModelBatch.Draw(model, Transform, view, projection);
                    else
                        ModelBatch.Draw(model, Effect, gameTime, Transform, view, projection);
                }
                else
                {
                    Matrix[] bones = skinning.GetBoneTransform(model, Transform);

                    if (Effect == null)
                        ModelBatch.Draw(model, bones, view, projection);
                    else
                        ModelBatch.Draw(model, Effect, gameTime, bones, view, projection);
                }
            }


            foreach (object o in components)
            {
                IDisplayObject disp = o as IDisplayObject;

                if (disp != null)
                    disp.Draw(gameTime, view, projection);
            }
        }

        public object Pick(Vector3 point)
        {
            if (Collision == null)
                return null;

            if (PickEngine.Intersects(Collision, point, Collision.BoundingSphere))
                return this;

            return null;
        }

        public object Pick(Ray ray, out float? distance)
        {
            distance = null;

            if (Collision == null)
                return null;

            distance = PickEngine.Intersects(Collision, ray, Collision.BoundingSphere);

            if (distance != null && distance.HasValue)
                return this;

            return null;
        }
    }
}
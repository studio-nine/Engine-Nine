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
using Isles.Transitions;
#endregion


namespace Isles.Game.World
{
    public class ModelObject : IDisplayObject
    {
        Model model;
        ModelSkinning skinning;


        public ModelAnimation Animation { get; internal set; }
        public ModelBatch ModelBatch { get; set; }
        public Matrix Transform { get; set; }
        public ModelEffect Effect { get; set; }


        public Model Model
        {
            get { return model; }
            set
            {
                model = value;

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


        public ModelObject()
        {
            Animation = new ModelAnimation();
        }

        public void Update(GameTime gameTime)
        {
            Animation.Update(gameTime);
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection)
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
        }
    }


    [XmlLoader(typeof(ModelObject))]
    public class ModelObjectLoader : IXmlLoader
    {
        public object Load(XmlElement input, IServiceProviderEx services)
        {
            ModelObject model = new ModelObject();

            model.Model = services.GetService<ContentManager>(null).Load<Model>(input.GetAttribute("Model"));
            model.Transform = ParseHelper.ToMatrix(input.GetAttribute("Transform"));
            model.ModelBatch = services.GetService<ModelBatch>(null);

            return model;
        }
    }
}
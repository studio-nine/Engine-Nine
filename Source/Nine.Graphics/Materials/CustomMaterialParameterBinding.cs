namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    struct CustomMaterialParameterBinding
    {
        public bool IsGlobal;
        public Action<EffectParameter, DrawingContext, Material> OnApply;
        
        /// <summary>
        /// This array has to be synchronized with CustomEffectSemantics
        /// </summary>
        public static Dictionary<string, CustomMaterialParameterBinding> Bindings = new Dictionary<string,CustomMaterialParameterBinding>()
        {
            { "World", Bind((parameter, context, material) => parameter.SetValue(material.world)) },            
            { "WorldInverse", Bind((parameter, context, material) => parameter.SetValue(Matrix.Invert(material.world))) },
            { "WorldTranspose", Bind((parameter, context, material) => parameter.SetValueTranspose(material.world)) },            
            { "WorldInverseTranspose", Bind((parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(material.world))) },

            { "View", BindGlobal((parameter, context, material) => parameter.SetValue(context.matrices.view)) },
            { "ViewInverse", BindGlobal((parameter, context, material) => parameter.SetValue(context.matrices.viewInverse)) },
            { "ViewTranspose", BindGlobal((parameter, context, material) => parameter.SetValueTranspose(context.matrices.view)) },
            { "ViewInverseTranspose", BindGlobal((parameter, context, material) => parameter.SetValueTranspose(context.matrices.viewInverse)) },

            { "Projection", BindGlobal((parameter, context, material) => parameter.SetValue(context.matrices.projection)) },            
            { "ProjectionInverse", BindGlobal((parameter, context, material) => parameter.SetValue(context.matrices.ProjectionInverse)) },
            { "ProjectionTranspose", BindGlobal((parameter, context, material) => parameter.SetValueTranspose(context.matrices.projection)) },
            { "ProjectionInverseTranspose", BindGlobal((parameter, context, material) => parameter.SetValueTranspose(context.matrices.ProjectionInverse)) },
            
            { "WorldView", Bind((parameter, context, material) => parameter.SetValue(material.world * context.matrices.view)) },
            { "WorldViewInverse", Bind((parameter, context, material) => parameter.SetValue(Matrix.Invert(material.world * context.matrices.view))) },
            { "WorldViewTranspose", Bind((parameter, context, material) => parameter.SetValueTranspose(material.world * context.matrices.view)) },
            { "WorldViewInverseTranspose", Bind((parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(material.world * context.matrices.view))) },
            
            { "WorldViewProjection", Bind((parameter, context, material) => parameter.SetValue(material.world * context.matrices.ViewProjection)) },
            { "WorldViewProjectionInverse", Bind((parameter, context, material) => parameter.SetValue(Matrix.Invert(material.world * context.matrices.ViewProjection))) },
            { "WorldViewProjectionTranspose", Bind((parameter, context, material) => parameter.SetValueTranspose(material.world * context.matrices.ViewProjection)) },
            { "WorldViewProjectionInverseTranspose", Bind((parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(material.world * context.matrices.ViewProjection))) },
            
            { "ViewProjection", BindGlobal((parameter, context, material) => parameter.SetValue(context.matrices.ViewProjection)) },
            { "ViewProjectionInverse", BindGlobal((parameter, context, material) => parameter.SetValue(context.matrices.ViewProjectionInverse)) },
            { "ViewProjectionTranspose", BindGlobal((parameter, context, material) => parameter.SetValueTranspose(context.matrices.ViewProjection)) },
            { "ViewProjectionInverseTranspose", BindGlobal((parameter, context, material) => parameter.SetValueTranspose(context.matrices.ViewProjectionInverse)) },

            /*
            // Global Directional Light
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.DirectionalLight.Direction) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.DirectionalLight.DiffuseColor) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.DirectionalLight.SpecularColor) },
            */

            { "HalfPixelSize", BindGlobal((parameter, context, material) => { var viewport = context.graphics.Viewport; parameter.SetValue(new Vector2(0.5f / viewport.Width, 0.5f / viewport.Height)); }) },            
            
            { "EyePosition", BindGlobal((parameter, context, material) => parameter.SetValue(context.CameraPosition)) },

            { "Time", BindGlobal((parameter, context, material) => parameter.SetValue((float)context.totalSeconds)) },
            { "TimeElapsed", BindGlobal((parameter, context, material) => parameter.SetValue((float)context.elapsedTime)) },

            { "Alpha", BindGlobal((parameter, context, material) => { parameter.SetValue(material.alpha); }) },
            { "Opacity", BindGlobal((parameter, context, material) => { parameter.SetValue(material.alpha); }) },
            { "Diffuse", BindGlobal((parameter, context, material) => { if (parameter.ParameterType == EffectParameterType.Texture || parameter.ParameterType == EffectParameterType.Texture2D) parameter.SetValue(material.texture); }) },
        };

        public static CustomMaterialParameterBinding Bind(Action<EffectParameter, DrawingContext, Material> bindMethod)
        {
            return new CustomMaterialParameterBinding { OnApply = bindMethod };
        }

        public static CustomMaterialParameterBinding BindGlobal(Action<EffectParameter, DrawingContext, Material> bindMethod)
        {
            return new CustomMaterialParameterBinding { OnApply = bindMethod, IsGlobal = true };
        }
    }
}
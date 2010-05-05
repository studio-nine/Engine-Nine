#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Isles.Graphics.Effects
{
    public partial class ShadowEffect : IEffectMatrices, IEffectFog, IEffectLights, IEffectMaterial
    {
        public bool FogEnabled
        {
            get { return fogMask > 0.5f; }
            set { fogMask = (value ? 1.0f : 0.0f); }
        }
        
        public DirectionalLight DirectionalLight0 { get; private set; }
        public DirectionalLight DirectionalLight1 { get; private set; }
        public DirectionalLight DirectionalLight2 { get; private set; }

        public bool LightingEnabled { get; set; }

        public void EnableDefaultLighting()
        {
            LightingEnabled = true;

            DirectionalLight0.Direction = Vector3.Normalize(-Vector3.One);
            DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            DirectionalLight0.SpecularColor = Color.White.ToVector3();
        }

        public ShadowEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();

            DirectionalLight0 = new DirectionalLight(_lightDirection, _lightDiffuseColor, _lightSpecularColor, null);
            DirectionalLight1 = new DirectionalLight(_lightDirection, _lightDiffuseColor, _lightSpecularColor, null);
            DirectionalLight2 = new DirectionalLight(_lightDirection, _lightDiffuseColor, _lightSpecularColor, null);
        }

        protected override void OnApply()
        {
            farClip = Math.Abs(LightProjection.M43 / (Math.Abs(LightProjection.M33) - 1));
            eyePosition = Matrix.Invert(View).Translation;

            base.OnApply();
        }
    }
}

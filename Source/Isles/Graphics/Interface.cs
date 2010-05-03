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
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics
{
    public interface IEffectSkinned
    {
        bool SkinningEnabled { set; }
        void SetBoneTransforms(Matrix[] boneTransforms);
    }

    public interface IEffectMaterial
    {
        Vector3 AmbientLightColor { get; set; }
        Vector3 DiffuseColor { get; set; }
        Vector3 EmissiveColor { get; set; }
        Vector3 SpecularColor { get; set; }
        float SpecularPower { get; set; }
        Texture2D Texture { get; set; }
    }
}
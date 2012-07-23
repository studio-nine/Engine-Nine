#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xaml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Content;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.Materials
{
    /// <summary>
    /// Represents a local copy of settings of the specified effect.
    /// </summary>
    public abstract class Material : Object
    {
        #region Fields
        private const int TransparencySortOrderFlag = 1 << 25;
        private const int TwoSidedSortOrderFlag = 1 << 24;

        /// <summary>
        /// A mask that is applied to the sort order. 
        /// Materials with the same type always have the same sort order mask.
        /// This sort mask applies to the first 8 bits from most significant bits.
        /// </summary>
        private int sortOrderTypeMask;

        /// <summary>
        /// The order for material sorting.
        /// </summary>
        internal int SortOrder;

        private static int NextSortOrderTypeMask = 0;
        private static Dictionary<Type, int> SortOrderTypeMasks = new Dictionary<Type, int>();

        /// <summary>
        /// Stores materials used for different purposes.
        /// </summary>
        private Dictionary<MaterialUsage, Material> materialUsages;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the world transformation for this material.
        /// The property provides a fast access to the world parameter without
        /// having to query for IEffectMatrices interface using the Find method.
        /// </summary>
        /// <remarks>
        /// Derived materials should support both this property and the
        /// IEffectMatrices interface.
        /// </remarks>
        [ContentSerializerIgnore]
        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }
        internal Matrix world = Matrix.Identity;

        /// <summary>
        /// Gets or sets the default diffuse texture for this material.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets or sets the alpha value of this material.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set { alpha = MathHelper.Clamp(value, 0, 1); }
        }
        internal float alpha = 1;

        /// <summary>
        /// Gets or sets whether this material is transparent.
        /// </summary>
        public bool IsTransparent
        {
            get { return isTransparent || isAdditive || alpha < 1; }
            set { isTransparent = value; }
        }
        private bool isTransparent;

        /// <summary>
        /// Gets or sets whether this material will blend with other materials using additive blending.
        /// </summary>
        public bool IsAdditive
        {
            get { return isAdditive; }
            set { isAdditive = value; }
        }
        internal bool isAdditive;

        /// <summary>
        /// Gets or sets a value indicating whether the underlying object rendered using
        /// this material is double sided when it is transparent.
        /// </summary>
        public bool TwoSided { get; set; }

        /// <summary>
        /// Gets or sets the next material to form a multi pass material chain.
        /// </summary>
        public Material NextMaterial { get; set; }

        /// <summary>
        /// Occurs when a material usage is not found.
        /// </summary>
        public event Func<MaterialUsage, Material, Material> MaterialResolve;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class.
        /// </summary>
        protected Material()
        {
            // Creates a unique material sort type based on current material type.
            var type = GetType();
            if (!SortOrderTypeMasks.TryGetValue(type, out sortOrderTypeMask))
                SortOrderTypeMasks.Add(type, sortOrderTypeMask = ((NextSortOrderTypeMask++) & 0xFF) << 24);
        }

        /// <summary>
        /// Queries the material for the specified feature T.
        /// </summary>
        public virtual T Find<T>() where T : class
        {
            return this as T;
        }

        /// <summary>
        /// Queries the material for all the components that implements interface T.
        /// </summary>
        public virtual void FindAll<T>(ICollection<T> result) where T : class
        {
            if (this is T)
                result.Add(this as T);
        }

        /// <summary>
        /// Gets or sets the <see cref="Nine.Graphics.Materials.Material"/> with the specified usage.
        /// </summary>
        public Material this[MaterialUsage usage]
        {
            get 
            {
                Material result = null;
                if (materialUsages != null && materialUsages.TryGetValue(usage, out result))
                    return result;
                return (result = ResolveMaterial(usage)) != null ? this[usage] = result : null; 
            }            
            set
            {
                if (materialUsages == null)
                    materialUsages = new Dictionary<MaterialUsage, Material>();
                materialUsages[usage] = value;
            }
        }

        private Material ResolveMaterial(MaterialUsage usage)
        {
            var result = OnResolveMaterial(usage);
            if (result == null && MaterialResolve != null)
            {
                var listeners = MaterialResolve.GetInvocationList();
                for (int i = 0; i < listeners.Length; i++)
                {
                    var resolve = (Func<MaterialUsage, Material, Material>)listeners[i];
                    if (resolve != null && (result = resolve(usage, this)) != null)
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the material with the specified usage that is attached to this material.
        /// </summary>
        protected virtual Material OnResolveMaterial(MaterialUsage usage)
        {
            return null;
        }
        
        /// <summary>
        /// Sets the texture based on the texture usage.
        /// </summary>
        public virtual void SetTexture(TextureUsage textureUsage, Texture texture) { }

        /// <summary>
        /// Applies all the shader parameters before drawing any primitives.
        /// </summary>
        public void BeginApply(DrawingContext context) 
        {
            OnBeginApply(context, context.PreviousMaterial);
        }

        /// <summary>
        /// Restores any shader parameters changes after drawing the promitive.
        /// </summary>
        public void EndApply(DrawingContext context) 
        {
            OnEndApply(context);
            context.PreviousMaterial = this;
        }

        /// <summary>
        /// Applies all the shader parameters before drawing any primitives.
        /// </summary>
        protected abstract void OnBeginApply(DrawingContext context, Material previousMaterial);

        /// <summary>
        /// Applies all the shader parameters before drawing any primitives.
        /// </summary>
        protected abstract void OnEndApply(DrawingContext context);

        /// <summary>
        /// Creates a copy of this material.
        /// </summary>
        public virtual Material Clone() 
        {
            // TODO:
            return null; 
        }
        #endregion
    }
}
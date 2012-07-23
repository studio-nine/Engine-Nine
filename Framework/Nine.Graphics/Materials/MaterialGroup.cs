namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material that is grouped by material fragments.
    /// </summary>
    [NotContentSerializable]
    [ContentProperty("MaterialParts")]
    public class MaterialGroup : Material
    {
        internal Effect Effect;
        internal string Reference;
        internal Dictionary<MaterialUsage, MaterialGroup> ExtendedMaterials;
                
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGroup"/> class.
        /// </summary>
        internal MaterialGroup()
        {
            materialParts = new MaterialPartCollection();
            materialParts.Bind(this);
        }

        /// <summary>
        /// Gets a collection holding all the material parts.
        /// </summary>
        public IList<MaterialPart> MaterialParts
        {
            get { return materialParts; }
        }
        private MaterialPartCollection materialParts;
        private Dictionary<Type, MaterialPart> materialPartDictionary;

        /// <summary>
        /// Queries the material for the specified feature T.
        /// </summary>
        public override T Find<T>()
        {
            MaterialPart part;
            if (materialPartDictionary == null)
                materialPartDictionary = new Dictionary<Type, MaterialPart>();
            if (materialPartDictionary.TryGetValue(typeof(T), out part))
                return part as T;

            var count = materialParts.Count;
            for (int i = 0; i < count; i++)
            {
                if (materialParts[i] is T)
                {
                    part = materialParts[i];
                    break;
                }
            }
            materialPartDictionary.Add(typeof(T), part);
            return part as T;
        }

        /// <summary>
        /// Queries the material for all the components that implements interface T.
        /// </summary>
        public override void FindAll<T>(ICollection<T> result)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; i++)
            {
                var part = materialParts[i] as T;
                if (part != null)
                    result.Add(part);
            }
        }

        /// <summary>
        /// Sets the texture based on the texture usage.
        /// </summary>
        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; i++)
                materialParts[i].SetTexture(textureUsage, texture);

            if (ExtendedMaterials != null)
                foreach (var material in ExtendedMaterials.Values)
                    material.SetTexture(textureUsage, texture);
        }

        /// <summary>
        /// Applies all the shader parameters before drawing any primitives.
        /// </summary>
        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            var count = materialParts.Count;
            var previous = previousMaterial as MaterialGroup;
            if (previous == null || previous.Effect != Effect)
            {
                for (int i = 0; i < count; i++)
                    materialParts[i].ApplyGlobalParameters(context);
            }

            for (int i = 0; i < count; i++)
                materialParts[i].BeginApplyLocalParameters(context, this);

            Effect.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Restores any shader parameters changes after drawing the promitive.
        /// </summary>
        protected override void OnEndApply(DrawingContext context)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; i++)
                materialParts[i].EndApplyLocalParameters();
        }

        /// <summary>
        /// Gets the material with the specified usage that is attached to this material.
        /// </summary>
        protected override Material OnResolveMaterial(MaterialUsage usage)
        {
            if (usage == MaterialUsage.Default)
                return this;

            MaterialGroup result = null;
            if (ExtendedMaterials != null)
                ExtendedMaterials.TryGetValue(usage, out result);
            return result;
        }

        /// <summary>
        /// Called when the shader has changed.
        /// </summary>
        internal void OnShaderChanged()
        {
#if WINDOWS
            if (!MaterialPart.IsContentBuild && !MaterialPart.IsContentRead)
#endif
                throw new InvalidOperationException("The material state can only be modified at content build time.");
        }

        /// <summary>
        /// Creates a deep copy of this material.
        /// </summary>
        public override Material Clone()
        {
            var count = materialParts.Count;
            var result = new MaterialGroup();
            
            result.Effect = Effect;
            result.TwoSided = TwoSided;
            result.IsTransparent = IsTransparent;
            result.ExtendedMaterials = ExtendedMaterials;

            for (int i = 0; i < count; i++)
            {
                var clonedPart = materialParts[i].Clone();
                if (clonedPart == null)
                    throw new InvalidOperationException("MaterialPart.Clone cannot return null.");
                result.materialParts.Add(clonedPart);
            }            
            return result;
        }
    }

    class MaterialPartCollection : Collection<MaterialPart>
    {
        MaterialGroup materialGroup;

        public void Bind(MaterialGroup materialGroup)
        {
            if (materialGroup == null)
                throw new ArgumentNullException("materialGroup");

            this.materialGroup = materialGroup;

            var copiedMaterialParts = new MaterialPart[Count];
            CopyTo(copiedMaterialParts, 0);
            for (var i = 0; i < copiedMaterialParts.Length; i++)
            {
                copiedMaterialParts[i].MaterialGroup = materialGroup;
                copiedMaterialParts[i].OnBind();
            }
        }

        protected override void InsertItem(int index, MaterialPart item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            base.InsertItem(index, item);
            item.MaterialGroup = materialGroup;
            if (!MaterialPart.IsContentBuild && materialGroup != null)
                item.OnBind();
            if (materialGroup != null)
                materialGroup.OnShaderChanged();
        }

        protected override void SetItem(int index, MaterialPart item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            base.SetItem(index, item);
            item.MaterialGroup = materialGroup;
            if (!MaterialPart.IsContentBuild && materialGroup != null)
                item.OnBind();
            if (materialGroup != null)
                materialGroup.OnShaderChanged();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            if (materialGroup != null)
                materialGroup.OnShaderChanged();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (materialGroup != null)
                materialGroup.OnShaderChanged();
        }
    }

    class MaterialGroupReader : ContentTypeReader<MaterialGroup>
    {
        internal static int Index = -1;
        protected override MaterialGroup Read(ContentReader input, MaterialGroup existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new MaterialGroup();

            int count = 0;
            existingInstance.Texture = input.ReadObject<Microsoft.Xna.Framework.Graphics.Texture2D>();
            existingInstance.IsTransparent = input.ReadBoolean();
            existingInstance.IsAdditive = input.ReadBoolean();
            existingInstance.TwoSided = input.ReadBoolean();
            existingInstance.NextMaterial = input.ReadObject<Nine.Graphics.Materials.Material>();
            existingInstance.AttachedProperties = input.ReadObject<Dictionary<System.Xaml.AttachableMemberIdentifier, object>>();

            existingInstance.Effect = input.ReadExternalReference<Effect>();            

            try
            {
                MaterialPart.IsContentRead = true;
                count = input.ReadInt32();
                for (Index = 0; Index < count; Index++)
                    existingInstance.MaterialParts.Add(input.ReadObject<MaterialPart>());
            }
            finally
            {
                Index = -1;
                MaterialPart.IsContentRead = false;
            }
            existingInstance.ExtendedMaterials = input.ReadObject<Dictionary<MaterialUsage, MaterialGroup>>();
            return existingInstance;
        }
    }
}
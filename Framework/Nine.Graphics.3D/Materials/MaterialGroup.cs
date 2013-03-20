namespace Nine.Graphics.Materials
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Serialization;

    /// <summary>
    /// Defines a material that is grouped by material fragments.
    /// </summary>
    [ContentProperty("MaterialParts")]
    public class MaterialGroup : Material, ISupportInitialize
    {
        private bool initializing;

        internal Effect Effect;
        internal string Reference;
        internal MaterialGroup[] ExtendedMaterials;
                
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGroup"/> class.
        /// </summary>
        internal MaterialGroup() : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGroup"/> class.
        /// </summary>
        internal MaterialGroup(IServiceProvider serviceProvider)
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
            for (int i = 0; i < count; ++i)
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
            for (int i = 0; i < count; ++i)
            {
                var part = materialParts[i] as T;
                if (part != null)
                    result.Add(part);
            }
        }

        /// <summary>
        /// Gets the textures that are required by this material.
        /// </summary>
        public override void GetDependentPasses(ICollection<Type> passTypes)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].GetDependentPasses(passTypes);
        }

        /// <summary>
        /// Sets the texture based on the texture usage.
        /// </summary>
        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].SetTexture(textureUsage, texture);

            if (ExtendedMaterials != null)
            {
                for (int i = 0; i < ExtendedMaterials.Length; i++)
                {
                    var material = ExtendedMaterials[i];
                    if (material != null)
                    material.SetTexture(textureUsage, texture);
        }
            }
        }

        /// <summary>
        /// Applies all the shader parameters before drawing any primitives.
        /// </summary>
        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            var count = materialParts.Count;
            var previous = previousMaterial as MaterialGroup;
            var context3D = context as DrawingContext3D;
            if (previous == null || previous.Effect != Effect)
            {
                for (int i = 0; i < count; ++i)
                    materialParts[i].ApplyGlobalParameters(context3D);
            }

            for (int i = 0; i < count; ++i)
                materialParts[i].BeginApplyLocalParameters(context3D, this);

            Effect.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Restores any shader parameters changes after drawing the primitive.
        /// </summary>
        protected override void OnEndApply(DrawingContext context)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].EndApplyLocalParameters(context as DrawingContext3D);
        }

        /// <summary>
        /// Gets the material with the specified usage that is attached to this material.
        /// </summary>
        protected override Material OnResolveMaterial(MaterialUsage usage, Material existingInstance)
        {
            var result = existingInstance as MaterialGroup;
            if (result == null && ExtendedMaterials != null)
                result = ExtendedMaterials[(int)usage];
            if (result == null)
                return null;
            
                var srcCount = materialParts.Count;
                var destCount = result.materialParts.Count;
                
                if (srcCount > 0 && destCount > 0)
                {
                    var src = 0;
                    var dest = 0;
                    var srcPart = materialParts[0];
                    var destPart = result.materialParts[0];
                    var srcType = srcPart.GetType();
                    var destType = destPart.GetType();

                    // Source material parts is a super set of destination material parts.
                    while (dest < destCount)
                    {
                        destPart = result.materialParts[dest++];
                        destType = destPart.GetType();

                        while (src < srcCount)
                        {
                            srcPart = materialParts[src++];
                            srcType = srcPart.GetType();
                            if (srcType == destType)
                            {
                                srcPart.OnResolveMaterialPart(usage, destPart);
                                break;
                            }
                        }
                    }
                }
            return result;
        }

        /// <summary>
        /// Called when the shader has changed.
        /// </summary>
        internal void OnShaderChanged()
        {
            if (!initializing)
                UpdateShader();
        }

        /// <summary>
        /// Creates a deep copy of this material.
        /// </summary>
        public MaterialGroup Clone()
        {
            var count = materialParts.Count;
            var result = new MaterialGroup();
            
            result.Effect = Effect;
            result.TwoSided = TwoSided;
            result.IsTransparent = IsTransparent;
            result.ExtendedMaterials = ExtendedMaterials;

            for (int i = 0; i < count; ++i)
            {
                var clonedPart = materialParts[i].Clone();
                if (clonedPart == null)
                    throw new InvalidOperationException("MaterialPart.Clone cannot return null.");
                result.materialParts.Add(clonedPart);
            }            
            return result;
        }

        private void UpdateShader()
        {
            if (Effect == null)
            {
                TryInvokeContentPipelineMethod("MaterialGroupBuilder", "Build", out Effect);
            }
        }

        void ISupportInitialize.BeginInit() { initializing = true; }
        void ISupportInitialize.EndInit()
        {
            if (initializing)
            {
                UpdateShader();
                initializing = false;
            }
        }

        internal static bool TryInvokeContentPipelineMethod<T>(string className, string methodName, out T result, params object[] paramters)
            {
            result = default(T);
#if WINDOWS
            if (PipelineAssembly == null)
                return false;
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            result = (T)PipelineAssembly.GetTypes().Single(type => type.Name == className).InvokeMember(methodName, flags, null, null, paramters);
            return true;
#else
            return false;
#endif
        }
        static Assembly PipelineAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "Nine.Content");
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
            for (var i = 0; i < copiedMaterialParts.Length; ++i)
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
#if WINDOWS
            if (!MaterialPart.IsContentBuild && materialGroup != null)
#else
            if (materialGroup != null)
#endif
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
#if WINDOWS
            if (!MaterialPart.IsContentBuild && materialGroup != null)
#else
            if (materialGroup != null)
#endif
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
            /*
            if (existingInstance == null)
                existingInstance = new MaterialGroup();

            int count = 0;
            existingInstance.texture = input.ReadObject<Microsoft.Xna.Framework.Graphics.Texture2D>();
            existingInstance.IsTransparent = input.ReadBoolean();
            existingInstance.IsAdditive = input.ReadBoolean();
            existingInstance.TwoSided = input.ReadBoolean();
            existingInstance.NextMaterial = input.ReadObject<Nine.Graphics.Materials.Material>();
            existingInstance.AttachedProperties = input.ReadObject<AttachableMemberIdentifierCollection>();

            existingInstance.Effect = input.ReadExternalReference<Effect>();

            MaterialPart.IsContentRead = true;

            count = input.ReadInt32();
            for (Index = 0; Index < count; Index++)
                existingInstance.MaterialParts.Add(input.ReadObject<MaterialPart>());
            Index = -1;
            MaterialPart.IsContentRead = false;

            existingInstance.ExtendedMaterials = input.ReadObject<Dictionary<MaterialUsage, MaterialGroup>>();
            return existingInstance;
             */
            return null;
        }
    }
}
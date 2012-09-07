namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    #region IEffectParameterProvider
    interface IEffectParameterProvider
    {
        EffectParameter GetParameter(string name);
        IEnumerable<EffectParameter> GetParameters();
    }
    #endregion

    #region CustomMaterialParameterCollection
    /// <summary>
    /// Defines a collection of parameters used by custom materials.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomMaterialParameterCollection : IDictionary<string, object>
    {
        internal class Entry
        {
            public object Value;
            public object DefaultValue;
            public EffectParameter Parameter;
        }        
        private List<Entry> values;

        private IEffectParameterProvider effect;
        private Dictionary<string, int> nameToIndex;

        private List<KeyValuePair<CustomEffectSemantics, EffectParameter>> globalParameters;
        private List<KeyValuePair<CustomEffectSemantics, EffectParameter>> localParameters;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public int Count
        {
            get { return nameToIndex != null ? nameToIndex.Count : 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMaterialParameterCollection"/> class.
        /// </summary>
        internal CustomMaterialParameterCollection() { }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        public void Add(string key, object value)
        {
            this[key] = value;
        }

        /// <summary>
        /// Binds all the parameters.
        /// </summary>
        internal void Bind(IEffectParameterProvider effectParameterProvider)
        {
            effect = effectParameterProvider;

            if (globalParameters != null)
                globalParameters.Clear();
            if (localParameters != null)
                localParameters.Clear();

            if (effect == null)
                return;
            
            foreach (var parameter in effect.GetParameters())
            {
                CustomEffectSemantics semantics;
                if (TryGetSemantic(parameter.Semantic, out semantics))
                {
                    var index = (int)semantics;
                    if (CustomEffectParameterBinding.Bindings[index].IsGlobal)
                    {
                        if (globalParameters == null)
                            globalParameters = new List<KeyValuePair<CustomEffectSemantics, EffectParameter>>();
                        globalParameters.Add(new KeyValuePair<CustomEffectSemantics, EffectParameter>(semantics, parameter));
                    }
                    else
                    {
                        if (localParameters == null)
                            localParameters = new List<KeyValuePair<CustomEffectSemantics, EffectParameter>>();
                        localParameters.Add(new KeyValuePair<CustomEffectSemantics, EffectParameter>(semantics, parameter));
                    }
                }
            }

            if (globalParameters != null)
                globalParameters.TrimExcess();
            if (localParameters != null)
                localParameters.TrimExcess();
        }

        private bool TryGetSemantic(string sematics, out CustomEffectSemantics semantic)
        {
#if XBOX
            try
            {
                // TODO: Try catch is slow, move this to the content pipeline?
                semantic = (EffectSemantics)Enum.Parse(typeof(EffectSemantics), sematics, true);
                return true;
            }
            catch (Exception)
            {
                semantic = EffectSemantics.Ambient;
                return false;
            }
#elif SILVERLIGHT
            throw new NotSupportedException();
#else
            return Enum.TryParse<CustomEffectSemantics>(sematics, true, out semantic);
#endif
        }

        internal void ApplyGlobalParameters(DrawingContext context, Material material)
        {
            if (globalParameters != null)
            {
                var parameterCount = globalParameters.Count;
                for (int i = 0; i < parameterCount; ++i)
                    CustomEffectParameterBinding.Bindings[(int)globalParameters[i].Key].Bind(globalParameters[i].Value, context, material);
            }
        }

        internal void BeginApplyLocalParameters(DrawingContext context, Material material)
        {
            var parameterCount = 0;

            if (localParameters != null)
            {
                parameterCount = localParameters.Count;
                for (int i = 0; i < parameterCount; ++i)
                    CustomEffectParameterBinding.Bindings[(int)localParameters[i].Key].Bind(localParameters[i].Value, context, material);
            }

            if (values != null)
            {
                parameterCount = values.Count;
                for (int i = 0; i < parameterCount; ++i)
                {
                    var entry = values[i];
                    EffectExtensions.SetValue(entry.Parameter, entry.Value);
                }
            }
        }

        internal void EndApplyLocalParameters()
        {
            if (values != null)
            {
                var parameterCount = values.Count;
                for (int i = 0; i < parameterCount; ++i)
                {
                    var entry = values[i];
                    EffectExtensions.SetValue(entry.Parameter, entry.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of a parameter with the specified name.
        /// </summary>
        public object this[string name]
        {
            get
            {
                int index = 0;
                if (nameToIndex != null && nameToIndex.TryGetValue(name, out index))
                    return values[index].Value;
                return null;
            }
            set 
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("name");

                EffectParameter parameter = null;
                if (effect != null && (parameter = effect.GetParameter(name)) == null)
                    throw new InvalidOperationException("Cannot find parameter " + name);

                if (values == null)
                    values = new List<Entry>();

                Entry entry = new Entry();
                if ((entry.Parameter = parameter) != null)
                {
#if !SILVERLIGHT
                    entry.DefaultValue = parameter.GetValue();
#endif
                }
                entry.Value = value;
                values.Add(entry);

                if (nameToIndex == null)
                    nameToIndex = new Dictionary<string,int>();
                nameToIndex.Add(name, values.Count - 1);
            }
        }

        #region Dictionary
        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return nameToIndex != null && nameToIndex.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return nameToIndex != null ? (ICollection<string>)nameToIndex.Keys : new string[0]; }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            throw new NotSupportedException();
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { throw new NotSupportedException(); }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            this[item.Key] = item.Value;
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            if (nameToIndex != null)
                foreach (var pair in nameToIndex)
                    yield return new KeyValuePair<string, object>(pair.Key, values[pair.Value].Value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (nameToIndex != null)
                foreach (var pair in nameToIndex)
                    yield return new KeyValuePair<string, object>(pair.Key, values[pair.Value].Value);
        }
        #endregion
    }
    #endregion

    #region CustomEffectSematics
    /// <summary>
    /// Defines a list of effect parameter annotation supported by the rendering system.
    /// </summary>
    enum CustomEffectSemantics : int
    {
        World,
        View,
        Projection,

        WorldTranspose,
        ViewTranspose,
        ProjectionTranspose,

        WorldView,
        WorldViewProjection,

        WorldInverse,
        ViewInverse,
        ProjectionInverse,

        WorldInverseTranspose,
        ViewInverseTranspose,
        ProjectionInverseTranspose,

        WorldViewInverse,
        WorldViewTranspose,
        WorldViewInverseTranspose,

        WorldViewProjectionInverse,
        WorldViewProjectionTranspose,
        WorldViewProjectionInverseTranspose,

        ViewProjection,
        ViewProjectionInverse,
        ViewProjectionTranspose,
        ViewProjectionInverseTranspose,

        LightDirection,
        LightDiffuse,
        LightSpecular,
        
        HalfPixelSize,
        EyePosition,

        Time,
        ElapsedTime,
        
        Alpha,
        Opacity,
        Diffuse,
    }
    #endregion

    #region CustomEffectParameterBinding
    struct CustomEffectParameterBinding
    {
        public bool IsGlobal;
        public Action<EffectParameter, DrawingContext, Material> Bind;

        /// <summary>
        /// This array has to be sychronized with CustomEffectSemantics
        /// </summary>
        public static CustomEffectParameterBinding[] Bindings = new CustomEffectParameterBinding[]
        {
            // Matrices
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValue(material.World) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.View) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.Projection) },

            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValueTranspose(material.World) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.Matrices.ViewTranspose) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.Matrices.ProjectionInverseTranspose) },
                        
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValue(material.World * context.Matrices.View) },
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValue(material.World * context.Matrices.ViewProjection) },
            
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValue(Matrix.Invert(material.World)) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.Matrices.ViewInverse) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.Matrices.ProjectionInverse) },
            
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(material.World)) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValueTranspose(context.Matrices.ViewInverse) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValueTranspose(context.Matrices.ProjectionInverse) },
                        
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValue(Matrix.Invert(material.World * context.View)) },
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValueTranspose(material.World * context.View) },
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(material.World * context.View)) },
                        
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValue(Matrix.Invert(material.World * context.Matrices.ViewProjection)) },
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValueTranspose(material.World * context.Matrices.ViewProjection) },
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(material.World * context.Matrices.ViewProjection)) },
                        
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.Matrices.ViewProjection) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(Matrix.Invert(context.Matrices.ViewProjection)) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValueTranspose(context.Matrices.ViewProjection) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValueTranspose(Matrix.Invert(context.Matrices.ViewProjection)) },                        

            // Global Directional Light
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.DirectionalLight.Direction) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.DirectionalLight.DiffuseColor) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.DirectionalLight.SpecularColor) },

            // Half Pixel
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => { var viewport = context.graphics.Viewport; parameter.SetValue(new Vector2(0.5f / viewport.Width, 0.5f / viewport.Height)); } },
            
            // Eye Position
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue(context.CameraPosition) },

            // Time
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue((float)context.totalSeconds) },
            new CustomEffectParameterBinding { IsGlobal = true, Bind = (parameter, context, material) => parameter.SetValue((float)context.elapsedSeconds) },

            // Materials and textures
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => { parameter.SetValue(material.alpha); } },
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => { parameter.SetValue(material.alpha); } },
#if SILVERLIGHT
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => { parameter.SetValue(material.texture); } },
#else
            new CustomEffectParameterBinding { Bind = (parameter, context, material) => { if (parameter.ParameterType == EffectParameterType.Texture2D) parameter.SetValue(material.texture); } },
#endif
        };
    }
    #endregion
}
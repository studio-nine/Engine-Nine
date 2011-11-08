using System.Collections.Generic;
using System.IO;
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides support for .fx files.
    /// </summary>
    public class SilverlightEffect : Effect
    {
        #region Instance Data

        readonly SilverlightEffectParametersCollection parameters;

        #endregion

        #region IDisposable

        ///// <summary>
        ///// Dispose(bool disposing) executes in two distinct scenarios.
        ///// If disposing equals true, the method has been called directly
        ///// or indirectly by a user's code. Managed and unmanaged resources
        ///// can be disposed.
        ///// If disposing equals false, the method has been called by the 
        ///// runtime from inside the finalizer and you should not reference 
        ///// other objects. Only unmanaged resources can be disposed.
        ///// </summary>
        //protected override void Dispose(bool disposing)
        //{
        //    if (IsDisposed) 
        //        return;

        //    base.Dispose(disposing);

        //    if (disposing)
        //    {
        //        if (vertexShader != null)
        //        {
        //            vertexShader.Dispose();
        //            vertexShader = null;
        //        }
        //        if (pixelShader != null)
        //        {
        //            pixelShader.Dispose();
        //            pixelShader = null;
        //        }
        //    }
        //}

        #endregion

        #region Creation
        /// <summary>
        /// Initializes a new instance of the <see cref="SilverlightEffect"/> class.
        /// </summary>
        protected internal SilverlightEffect(GraphicsDevice graphics, byte[] effectCode)
            : base(Initialize(graphics, effectCode))
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;

            Dictionary<string, SilverlightEffectParameter> tempParameters = new Dictionary<string, SilverlightEffectParameter>();

            foreach (var technique in Techniques)
            {
                foreach (SilverlightEffectPass pass in technique.Passes)
                {
                    pass.ParentEffect = this;

                    foreach (SilverlightEffectInternalParameter parameter in pass.Parameters)
                    {
                        if (!tempParameters.ContainsKey(parameter.Name))
                        {
                            tempParameters.Add(parameter.Name, new SilverlightEffectParameter(parameter.Name));
                        }
                    }
                }
            }

            parameters = new SilverlightEffectParametersCollection(tempParameters.Values);
        }

        private static EffectTechnique[] Initialize(GraphicsDevice graphics, byte[] effectCode)
        {
            using (var stream = new MemoryStream(effectCode))
            {
                var input = new BinaryReader(stream);
                int techniquesCount = input.ReadInt32();
                EffectTechnique[] techniques = new EffectTechnique[techniquesCount];

                for (int techniqueIndex = 0; techniqueIndex < techniquesCount; techniqueIndex++)
                {
                    int passesCount = input.ReadInt32();
                    EffectPass[] passes = new EffectPass[passesCount];

                    for (int passIndex = 0; passIndex < passesCount; passIndex++)
                    {
                        string passName = input.ReadString();

                        MemoryStream vertexShaderCodeStream = null;
                        MemoryStream pixelShaderCodeStream = null;
                        MemoryStream vertexShaderParametersStream = null;
                        MemoryStream pixelShaderParametersStream = null;

                        // Vertex shader
                        int vertexShaderByteCodeLength = input.ReadInt32();
                        if (vertexShaderByteCodeLength > 0)
                        {
                            byte[] vertexShaderByteCode = input.ReadBytes(vertexShaderByteCodeLength);
                            int vertexShaderParametersLength = input.ReadInt32();
                            byte[] vertexShaderParameters = input.ReadBytes(vertexShaderParametersLength);
                            
                            vertexShaderCodeStream = new MemoryStream(vertexShaderByteCode);
                            vertexShaderParametersStream = new MemoryStream(vertexShaderParameters);
                        }

                        // Pixel shader
                        int pixelShaderByteCodeLength = input.ReadInt32();
                        if (pixelShaderByteCodeLength > 0)
                        {
                            byte[] pixelShaderByteCode = input.ReadBytes(pixelShaderByteCodeLength);
                            int pixelShaderParametersLength = input.ReadInt32();
                            byte[] pixelShaderParameters = input.ReadBytes(pixelShaderParametersLength);

                            pixelShaderCodeStream = new MemoryStream(pixelShaderByteCode);
                            pixelShaderParametersStream = new MemoryStream(pixelShaderParameters);
                        }

                        // Instanciate pass
                        SilverlightEffectPass currentPass = new SilverlightEffectPass(passName, graphics, vertexShaderCodeStream, pixelShaderCodeStream, vertexShaderParametersStream, pixelShaderParametersStream);
                        passes[passIndex] = currentPass;

                        if (vertexShaderCodeStream != null)
                            vertexShaderCodeStream.Dispose();
                        if (pixelShaderCodeStream != null)
                            pixelShaderCodeStream.Dispose();
                        if (vertexShaderParametersStream != null)
                            vertexShaderParametersStream.Dispose();
                        if (pixelShaderParametersStream != null)
                            pixelShaderParametersStream.Dispose();

                        // Render states
                        int renderStatesCount = input.ReadInt32();

                        for (int renderStateIndex = 0; renderStateIndex < renderStatesCount; renderStateIndex++)
                        {
                            currentPass.AppendState(input.ReadString(), input.ReadString());
                        }
                    }

                    // Instanciate technique
                    techniques[techniqueIndex] = new EffectTechnique(passes);
                }
                return techniques;
            }
        }

        #endregion

        #region Internal methods

        internal void Apply(bool force)
        {
            OnApply();

            foreach (SilverlightEffectParameter parameter in parameters)
            {
                if (parameter.IsDirty || force)
                {
                    // The SilverlightEffectParameters must transmit data to internal parameters if they are dirty
                    foreach (var technique in Techniques)
                    {
                        foreach (SilverlightEffectPass pass in technique.Passes)
                        {
                            parameter.Apply(pass.Parameters);
                        }
                    }
                }
            }
        }

        protected virtual void OnApply()
        {

        }

        #endregion

        #region Public properties

        /// <summary>
        /// Get the parameters list.
        /// </summary>
        public SilverlightEffectParametersCollection Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }
        #endregion
    }
}

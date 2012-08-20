// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Windows.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides support for .fx files.
    /// </summary>
    public class SilverlightEffect : Microsoft.Xna.Framework.Graphics.Effect
    {
        #region Instance Data

        readonly EffectParametersCollection parameters;

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
        /// Creates a new SilverlightEffect with default parameter settings.
        /// </summary>
        internal SilverlightEffect(EffectTechnique[] techniques) : base(techniques)
        {
            Dictionary<string, EffectParameter> tempParameters = new Dictionary<string, EffectParameter>();

            foreach (var technique in techniques)
            {
                foreach (SilverlightEffectPass pass in technique.Passes)
                {
                    pass.ParentEffect = this;

                    foreach (SilverlightEffectInternalParameter parameter in pass.Parameters)
                    {
                        if (!tempParameters.ContainsKey(parameter.Name))
                        {
                            tempParameters.Add(parameter.Name, new EffectParameter(parameter.Name));
                        }
                    }
                }
            }

            parameters = new EffectParametersCollection(tempParameters.Values);
        }

        public SilverlightEffect(byte[] effectCode) : this(new BinaryReader(new MemoryStream(effectCode)))
        {

        }

        internal SilverlightEffect(BinaryReader input) : this(CreateTechniques(input))
        {

        }

        private static EffectTechnique[] CreateTechniques(BinaryReader input)
        {
            int techniquesCount = input.ReadInt32();
            EffectTechnique[] techniques = new EffectTechnique[techniquesCount];

            for (int techniqueIndex = 0; techniqueIndex < techniquesCount; techniqueIndex++)
            {
                int passesCount = input.ReadInt32();
                EffectPass[] passes = new EffectPass[passesCount];

                for (int passIndex = 0; passIndex < passesCount; passIndex++)
                {
                    string passName = input.ReadString();

                    // Vertex shader
                    int vertexShaderByteCodeLength = input.ReadInt32();
                    byte[] vertexShaderByteCode = input.ReadBytes(vertexShaderByteCodeLength);
                    int vertexShaderParametersLength = input.ReadInt32();
                    byte[] vertexShaderParameters = input.ReadBytes(vertexShaderParametersLength);

                    // Pixel shader
                    int pixelShaderByteCodeLength = input.ReadInt32();
                    byte[] pixelShaderByteCode = input.ReadBytes(pixelShaderByteCodeLength);
                    int pixelShaderParametersLength = input.ReadInt32();
                    byte[] pixelShaderParameters = input.ReadBytes(pixelShaderParametersLength);

                    MemoryStream vertexShaderCodeStream = new MemoryStream(vertexShaderByteCode);
                    MemoryStream pixelShaderCodeStream = new MemoryStream(pixelShaderByteCode);
                    MemoryStream vertexShaderParametersStream = new MemoryStream(vertexShaderParameters);
                    MemoryStream pixelShaderParametersStream = new MemoryStream(pixelShaderParameters);

                    // Instanciate pass
                    SilverlightEffectPass currentPass = new SilverlightEffectPass(passName, GraphicsDeviceManager.Current.GraphicsDevice, vertexShaderCodeStream, pixelShaderCodeStream, vertexShaderParametersStream, pixelShaderParametersStream);
                    passes[passIndex] = currentPass;

                    vertexShaderCodeStream.Dispose();
                    pixelShaderCodeStream.Dispose();
                    vertexShaderParametersStream.Dispose();
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

        #endregion

        #region Internal methods

        internal void Apply(bool force)
        {
            var count = parameters.Count;
            for (var i = 0; i < parameters.Count; ++i)
            {
                var parameter = parameters[i];
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

        #endregion

        #region Public properties

        /// <summary>
        /// Get the parameters list.
        /// </summary>
        public EffectParametersCollection Parameters
        {
            get { return parameters; }
        }

        #endregion
    }
}

// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Internal parameter class for SilverlightEffect
    /// </summary>
    [DebuggerDisplay("{Name}")]
    internal class SilverlightEffectInternalParameter
    {
        #region Instance Data

        readonly GraphicsDevice device;

        bool isDirty = true;
        Vector4[] data;
        int dataLength;

        #endregion

        #region Internal properties

        internal int VertexShaderRegisterIndex;

        internal int PixelShaderRegisterIndex;

        internal int RegisterCount;

        internal void SetData(Vector4[] data, int length)
        {
            isDirty = true;
            this.data = data;
            this.dataLength = length;
        }

        #endregion

        #region Creation

        /// <summary>Creates a new effect parameter</summary>
        internal SilverlightEffectInternalParameter(GraphicsDevice device, string name)
        {
            this.device = device;
            Name = name;
            VertexShaderRegisterIndex = -1;
            PixelShaderRegisterIndex = -1;
            RegisterCount = 1;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Get or set the name of the parameter
        /// </summary>
        public string Name { get; internal set; }
        
        #endregion

        #region Internal methods

        internal void Apply()
        {
            // Checking dirty state
            if (!isDirty)
            {
                return;
            }

            if (data == null)
                return;

            // Compute correct register size
            int size = Math.Min(RegisterCount, dataLength);

            // Transmit to device
            for (int index = 0; index < size; index++)
            {
                if (VertexShaderRegisterIndex >= 0)
                    device.SetVertexShaderConstantFloat4(VertexShaderRegisterIndex + index, ref data[index]);
                if (PixelShaderRegisterIndex >= 0)
                    device.SetPixelShaderConstantFloat4(PixelShaderRegisterIndex + index, ref data[index]);
            }

            isDirty = false;
        }

        #endregion

    }
}

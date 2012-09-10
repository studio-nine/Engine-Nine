// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum EffectParameterClass
    {
        Scalar = 0,
        Vector = 1,
        Matrix = 2,
        Object = 3,
        Struct = 4,
    }

    public enum EffectParameterType
    {
        Void = 0,
        Bool = 1,
        Int32 = 2,
        Single = 3,
        String = 4,
        Texture = 5,
        Texture1D = 6,
        Texture2D = 7,
        Texture3D = 8,
        TextureCube = 9,
    }

    /// <summary>
    /// Parameter class for SilverlightEffect
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class EffectParameter
    {
        bool isDirty = true;
        int dataLength;
        Vector4[] data;
        SilverlightEffectInternalParameter internalParameter;

        #region Creation

        /// <summary>Creates a new effect parameter</summary>
        internal EffectParameter(SilverlightEffectInternalParameter parameter)
        {
            internalParameter = parameter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the name of the parameter
        /// </summary>
        public string Name
        {
            get { return internalParameter.Name; }
        }

        public EffectParameterClass ParameterClass
        {
            get { return internalParameter.ParameterClass; }
        }

        public EffectParameterType ParameterType
        {
            get { return internalParameter.ParameterType; }
        }

        internal EffectRegisterSet RegisterSet
        {
            get { return internalParameter.RegisterSet; }
        }

        internal int SamplerIndex
        {
            get { return internalParameter.SamplerIndex; }
        }

        // TODO:
        public string Semantic { get; internal set; }

        internal bool IsDirty
        {
            get { return isDirty; }
        }

        #endregion

        #region Public methods
        
        private void EnsureDataSize(int size)
        {
            if (data == null || data.Length < size)
                data = new Vector4[size];
            dataLength = size;
        }

        public void SetValue(Texture texture)
        {
            internalParameter.device.Textures[internalParameter.SamplerIndex] = texture;
        }

        /// <summary>
        /// Affect a single value to the parameter
        /// </summary>
        public void SetValue(float value)
        {
            EnsureDataSize(1);
            data[0].X = value;
            isDirty = true;
        }

        public void SetValue(float[] value)
        {
            EnsureDataSize(value.Length / 4 + Math.Max(value.Length % 4, 1));
            var upper = value.Length - 1;
            for (var i = 0; i <= upper; i += 4)
            {
                var d = i / 4;
                data[d].X = value[i];
                data[d].Y = value[Math.Min(i + 1, upper)];
                data[d].Z = value[Math.Min(i + 2, upper)];
                data[d].W = value[Math.Min(i + 3, upper)];
            }
            isDirty = true;
        }

        /// <summary>
        /// Affect a Vector2 value to the parameter
        /// </summary>
        public void SetValue(Vector2 value)
        {
            EnsureDataSize(1);
            data[0].X = value.X;
            data[0].Y = value.Y;
            isDirty = true;
        }

        public void SetValue(Vector2[] value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Affect a Vector3 value to the parameter
        /// </summary>
        public void SetValue(Vector3 value)
        {
            EnsureDataSize(1);
            data[0].X = value.X;
            data[0].Y = value.Y;
            data[0].Z = value.Z;
            isDirty = true;
        }

        /// <summary>
        /// Affect a Vector4 value to the parameter
        /// </summary>
        public void SetValue(Vector4 value)
        {
            EnsureDataSize(1);
            data[0].X = value.X;
            data[0].Y = value.Y;
            data[0].Z = value.Z;
            data[0].W = value.W;
            isDirty = true;
        }

        public void SetValue(Vector4[] value)
        {
            EnsureDataSize(value.Length);
            Array.Copy(value, data, value.Length);
            isDirty = true;
        }

        /// <summary>
        /// Affect a Matrix value to the parameter
        /// </summary>
        public void SetValue(Matrix value)
        {
            EnsureDataSize(4);
            data[0].X = value.M11; data[0].Y = value.M21; data[0].Z = value.M31; data[0].W = value.M41;
            data[1].X = value.M12; data[1].Y = value.M22; data[1].Z = value.M32; data[1].W = value.M42;
            data[2].X = value.M13; data[2].Y = value.M23; data[2].Z = value.M33; data[2].W = value.M43;
            data[3].X = value.M14; data[3].Y = value.M24; data[3].Z = value.M34; data[3].W = value.M44;
            isDirty = true;
        }

        /// <summary>
        /// Affect a Matrix value to the parameter
        /// </summary>
        public void SetValueTranspose(Matrix value)
        {
            EnsureDataSize(4);
            data[0].X = value.M11; data[0].Y = value.M12; data[0].Z = value.M13; data[0].W = value.M14;
            data[1].X = value.M21; data[1].Y = value.M22; data[1].Z = value.M23; data[1].W = value.M24;
            data[2].X = value.M31; data[2].Y = value.M32; data[2].Z = value.M33; data[2].W = value.M34;
            data[3].X = value.M41; data[3].Y = value.M42; data[3].Z = value.M43; data[3].W = value.M44;
            isDirty = true;
        }

        /// <summary>
        /// Affect an array of Matrix value to the parameter
        /// </summary>
        public void SetValue(Matrix[] value)
        {
            EnsureDataSize(4 * value.Length);
            for (var i = 0; i < value.Length; ++i)
            {
                var m = i * 4;
                data[0 + m].X = value[i].M11; data[0 + m].Y = value[i].M21; data[0 + m].Z = value[i].M31; data[0 + m].W = value[i].M41;
                data[1 + m].X = value[i].M12; data[1 + m].Y = value[i].M22; data[1 + m].Z = value[i].M32; data[1 + m].W = value[i].M42;
                data[2 + m].X = value[i].M13; data[2 + m].Y = value[i].M23; data[2 + m].Z = value[i].M33; data[2 + m].W = value[i].M43;
                data[3 + m].X = value[i].M14; data[3 + m].Y = value[i].M24; data[3 + m].Z = value[i].M34; data[3 + m].W = value[i].M44;
            }
            isDirty = true;
        }

        /// <summary>
        /// Affect a color value to the parameter
        /// </summary>
        public void SetValue(Color color)
        {
            EnsureDataSize(1);
            data[0].X = color.R / 255.0f;
            data[0].Y = color.G / 255.0f;
            data[0].Z = color.B / 255.0f;
            data[0].W = color.A / 255.0f;
            isDirty = true;
        }

        #endregion

        #region Internal methods

        internal void Apply()
        {
            isDirty = false;
            internalParameter.SetData(data, dataLength);
        }

        #endregion        
    }
}

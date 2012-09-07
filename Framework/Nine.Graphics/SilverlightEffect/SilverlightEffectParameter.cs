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
        public void SetValue(float single)
        {
            EnsureDataSize(1);
            data[0].X = single;
            isDirty = true;
        }

        public void SetValue(float[] value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Affect a Vector2 value to the parameter
        /// </summary>
        public void SetValue(Vector2 vector2)
        {
            EnsureDataSize(1);
            data[0].X = vector2.X;
            data[0].Y = vector2.Y;
            isDirty = true;
        }

        public void SetValue(Vector2[] vector2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Affect a Vector3 value to the parameter
        /// </summary>
        public void SetValue(Vector3 vector3)
        {
            EnsureDataSize(1);
            data[0].X = vector3.X;
            data[0].Y = vector3.Y;
            data[0].Z = vector3.Z;
            isDirty = true;
        }

        public void SetValue(Vector3[] vector2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Affect a Vector4 value to the parameter
        /// </summary>
        public void SetValue(Vector4 vector4)
        {
            EnsureDataSize(1);
            data[0].X = vector4.X;
            data[0].Y = vector4.Y;
            data[0].Z = vector4.Z;
            data[0].W = vector4.W;
            isDirty = true;
        }

        public void SetValue(Vector4[] vector2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Affect a Matrix value to the parameter
        /// </summary>
        public void SetValue(Matrix matrix)
        {
            EnsureDataSize(4);
            data[0] = new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41);
            data[1] = new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42);
            data[2] = new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43);
            data[3] = new Vector4(matrix.M14, matrix.M24, matrix.M34, matrix.M44);
            isDirty = true;
        }

        /// <summary>
        /// Affect a Matrix value to the parameter
        /// </summary>
        public void SetValueTranspose(Matrix matrix)
        {
            EnsureDataSize(4);
            data[0] = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
            data[1] = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
            data[2] = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            data[3] = new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
            isDirty = true;
        }

        /// <summary>
        /// Affect an array of Matrix value to the parameter
        /// </summary>
        public void SetValue(Matrix[] matrices)
        {
            EnsureDataSize(4 * matrices.Length);
            for (int i = 0; i < matrices.Length; i++)
            {
                data[4 * i] = new Vector4(matrices[i].M11, matrices[i].M12, matrices[i].M13, matrices[i].M14);
                data[1 + 4 * i] = new Vector4(matrices[i].M21, matrices[i].M22, matrices[i].M23, matrices[i].M24);
                data[2 + 4 * i] = new Vector4(matrices[i].M31, matrices[i].M32, matrices[i].M33, matrices[i].M34);
                data[3 + 4 * i] = new Vector4(matrices[i].M41, matrices[i].M42, matrices[i].M43, matrices[i].M44);
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

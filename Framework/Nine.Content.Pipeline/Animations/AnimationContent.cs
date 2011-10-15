#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace Nine.Content.Pipeline.Animations
{
    [ContentProperty("Animations")]
    partial class AnimationPlayerContent : Dictionary { }

    [ContentProperty("Animations")]
    partial class LayeredAnimationContent { }

    [ContentProperty("Animations")]
    partial class SequentialAnimationContent { }

    partial class TweenAnimationContent<T> where T : struct { }
    partial class TweenAnimationContentWriter<T> where T : struct { }

    public class CharAnimationContent : TweenAnimationContent<char> { }
    public class ByteAnimationContent : TweenAnimationContent<byte> { }
    public class Int16AnimationContent : TweenAnimationContent<short> { }
    public class Int32AnimationContent : TweenAnimationContent<int> { }
    public class Int64AnimationContent : TweenAnimationContent<long> { }
    public class BooleanAnimationContent : TweenAnimationContent<bool> { }
    public class DoubleAnimationContent : TweenAnimationContent<double> { }
    public class SingleAnimationContent : TweenAnimationContent<float> { }
    public class DecimalAnimationContent : TweenAnimationContent<decimal> { }
    public class Vector2AnimationContent : TweenAnimationContent<Vector2> { }
    public class Vector3AnimationContent : TweenAnimationContent<Vector3> { }
    public class Vector4AnimationContent : TweenAnimationContent<Vector4> { }
    public class ColorAnimationContent : TweenAnimationContent<Color> { }
    public class QuaternionAnimationContent : TweenAnimationContent<Quaternion> { }
    public class MatrixAnimationContent : TweenAnimationContent<Matrix> { }
    public class PointAnimationContent : TweenAnimationContent<Point> { }
    public class RectangleAnimationContent : TweenAnimationContent<Rectangle> { }

    [ContentTypeWriter] class CharAnimationContentWriter : TweenAnimationContentWriter<char, CharAnimationContent> { }
    [ContentTypeWriter] class ByteAnimationContentWriter : TweenAnimationContentWriter<byte, ByteAnimationContent> { }
    [ContentTypeWriter] class Int16AnimationContentWriter : TweenAnimationContentWriter<short, Int16AnimationContent> { }
    [ContentTypeWriter] class Int32AnimationContentWriter : TweenAnimationContentWriter<int, Int32AnimationContent> { }
    [ContentTypeWriter] class Int64AnimationContentWriter : TweenAnimationContentWriter<long, Int64AnimationContent> { }
    [ContentTypeWriter] class BooleanAnimationContentWriter : TweenAnimationContentWriter<bool, BooleanAnimationContent> { }
    [ContentTypeWriter] class DoubleAnimationContentWriter : TweenAnimationContentWriter<double, DoubleAnimationContent> { }
    [ContentTypeWriter] class SingleAnimationContentWriter : TweenAnimationContentWriter<float, SingleAnimationContent> { }
    [ContentTypeWriter] class DecimalAnimationContentWriter : TweenAnimationContentWriter<decimal, DecimalAnimationContent> { }
    [ContentTypeWriter] class Vector2AnimationContentWriter : TweenAnimationContentWriter<Vector2, Vector2AnimationContent> { }
    [ContentTypeWriter] class Vector3AnimationContentWriter : TweenAnimationContentWriter<Vector3, Vector3AnimationContent> { }
    [ContentTypeWriter] class Vector4AnimationContentWriter : TweenAnimationContentWriter<Vector4, Vector4AnimationContent> { }
    [ContentTypeWriter] class ColorAnimationContentWriter : TweenAnimationContentWriter<Color, ColorAnimationContent> { }
    [ContentTypeWriter] class QuaternionAnimationContentWriter : TweenAnimationContentWriter<Quaternion, QuaternionAnimationContent> { }
    [ContentTypeWriter] class MatrixAnimationContentWriter : TweenAnimationContentWriter<Matrix, MatrixAnimationContent> { }
    [ContentTypeWriter] class PointAnimationContentWriter : TweenAnimationContentWriter<Point, PointAnimationContent> { }
    [ContentTypeWriter] class RectangleAnimationContentWriter : TweenAnimationContentWriter<Rectangle, RectangleAnimationContent> { }
    
    class TweenAnimationContentWriter<T, TContent> : ContentTypeWriter<TContent> where T : struct
    {
        ContentTypeWriter writer = new TweenAnimationContentWriter<T>();

        protected override void Write(ContentWriter output, TContent value)
        {
            output.WriteRawObject<TweenAnimationContent<T>>((TweenAnimationContent<T>)(object)value, writer);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return writer.GetRuntimeReader(targetPlatform);
        }
    }

    partial class TransformAnimationContentWriter
    {
        partial void BeginWrite(ContentWriter output, TransformAnimationContent value)
        {
            // Convert from degrees to radius
            if (value.RotationX != null)
            {
                if (value.RotationX.From != null)
                    value.RotationX.From = MathHelper.ToRadians(value.RotationX.From.Value);
                if (value.RotationX.To != null)
                    value.RotationX.To = MathHelper.ToRadians(value.RotationX.To.Value);
                if (value.RotationX.By != null)
                    value.RotationX.By = MathHelper.ToRadians(value.RotationX.By.Value);
            }
            if (value.RotationY != null)
            {
                if (value.RotationY.From != null)
                    value.RotationY.From = MathHelper.ToRadians(value.RotationY.From.Value);
                if (value.RotationY.To != null)
                    value.RotationY.To = MathHelper.ToRadians(value.RotationY.To.Value);
                if (value.RotationY.By != null)
                    value.RotationY.By = MathHelper.ToRadians(value.RotationY.By.Value);
            }
            if (value.RotationZ != null)
            {
                if (value.RotationZ.From != null)
                    value.RotationZ.From = MathHelper.ToRadians(value.RotationZ.From.Value);
                if (value.RotationZ.To != null)
                    value.RotationZ.To = MathHelper.ToRadians(value.RotationZ.To.Value);
                if (value.RotationZ.By != null)
                    value.RotationZ.By = MathHelper.ToRadians(value.RotationZ.By.Value);
            }
        }
    }
}

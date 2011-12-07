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

    public class TransformAnimationContent : TweenAnimationContent<Transform> { }
    
    [ContentTypeWriter]
    class TransformAnimationContentWriter : ContentTypeWriter<TransformAnimationContent>
    {
        ContentTypeWriter writer = new TweenAnimationContentWriter<Matrix>();

        protected override void Write(ContentWriter output, TransformAnimationContent value)
        {
            var tween = new TweenAnimationContent<Matrix>
            {
                AutoReverse = value.AutoReverse,
                Curve = value.Curve,
                Direction = value.Direction,
                Duration = value.Duration,
                Easing = value.Easing,
                Position = value.Position,
                Repeat = value.Repeat,
                Speed = value.Speed,
                StartupDirection = value.StartupDirection,
                TargetProperty = value.TargetProperty,
            };

            if (value.From.HasValue)
            {
                var transform = value.From.Value;
                transform.Rotation = transform.Rotation * MathHelper.Pi / 180;
                tween.From = transform.ToMatrix();
            }

            if (value.To.HasValue)
            {
                var transform = value.To.Value;
                transform.Rotation = transform.Rotation * MathHelper.Pi / 180;
                tween.To = transform.ToMatrix();
            }

            if (value.By.HasValue)
            {
                var transform = value.By.Value;
                transform.Rotation = transform.Rotation * MathHelper.Pi / 180;
                tween.By = transform.ToMatrix();
            }

            output.WriteRawObject<TweenAnimationContent<Matrix>>(tween, writer);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return writer.GetRuntimeReader(targetPlatform);
        }
    }
}

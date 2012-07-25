namespace Nine.Animations
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;

    // Simplify Xaml authoring.

    [EditorBrowsable(EditorBrowsableState.Never)]    
    public class CharAnimation : TweenAnimation<char> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ByteAnimation : TweenAnimation<byte> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Int16Animation : TweenAnimation<short> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Int32Animation : TweenAnimation<int> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Int64Animation : TweenAnimation<long> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BooleanAnimation : TweenAnimation<bool> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DoubleAnimation : TweenAnimation<double> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SingleAnimation : TweenAnimation<float> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DecimalAnimation : TweenAnimation<decimal> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Vector2Animation : TweenAnimation<Vector2> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Vector3Animation : TweenAnimation<Vector3> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Vector4Animation : TweenAnimation<Vector4> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorAnimation : TweenAnimation<Color> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class QuaternionAnimation : TweenAnimation<Quaternion> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MatrixAnimation : TweenAnimation<Matrix> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PointAnimation : TweenAnimation<Point> { }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RectangleAnimation : TweenAnimation<Rectangle> { }
}
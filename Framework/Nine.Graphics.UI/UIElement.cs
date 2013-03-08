#region License
/* The MIT License
 *
 * Copyright (c) 2011 Red Badger Consulting
 * Copyright (c) 2012 Yufei Huang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/
#endregion

namespace Nine.Graphics.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Graphics;
    using Nine.Graphics.UI.Input;
    using Nine.Graphics.UI.Internal;

    public abstract class UIElement : ReactiveObject, IContainer, IComponent
    {
        #region Properties
        public static readonly ReactiveProperty<object> DataContextProperty =
            ReactiveProperty<object>.Register("DataContext", typeof(UIElement), DataContextChanged);

        public static readonly ReactiveProperty<float> HeightProperty = ReactiveProperty<float>.Register(
            "Height", typeof(UIElement), float.NaN, ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<HorizontalAlignment> HorizontalAlignmentProperty =
            ReactiveProperty<HorizontalAlignment>.Register(
                "HorizontalAlignment", 
                typeof(UIElement), 
                HorizontalAlignment.Stretch, 
                ReactivePropertyChangedCallbacks.InvalidateArrange);

        public static readonly ReactiveProperty<bool> IsMouseCapturedProperty =
            ReactiveProperty<bool>.Register("IsMouseCaptured", typeof(UIElement));

        public static readonly ReactiveProperty<Thickness> MarginProperty =
            ReactiveProperty<Thickness>.Register(
                "Margin", typeof(UIElement), new Thickness(), ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<float> MaxHeightProperty =
            ReactiveProperty<float>.Register(
                "MaxHeight", 
                typeof(UIElement), 
                float.PositiveInfinity, 
                ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<float> MaxWidthProperty = ReactiveProperty<float>.Register(
            "MaxWidth", typeof(UIElement), float.PositiveInfinity, ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<float> MinHeightProperty =
            ReactiveProperty<float>.Register(
                "MinHeight", typeof(UIElement), ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<float> MinWidthProperty = ReactiveProperty<float>.Register(
            "MinWidth", typeof(UIElement), ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<VerticalAlignment> VerticalAlignmentProperty =
            ReactiveProperty<VerticalAlignment>.Register(
                "VerticalAlignment", 
                typeof(UIElement), 
                VerticalAlignment.Stretch, 
                ReactivePropertyChangedCallbacks.InvalidateArrange);

        public static readonly ReactiveProperty<float> WidthProperty = ReactiveProperty<float>.Register(
            "Width", typeof(UIElement), float.NaN, ReactivePropertyChangedCallbacks.InvalidateMeasure);
        

        internal bool isArrangeValid;
        internal bool isMeasureValid;

        private Vector2 previousAvailableSize;

        private BoundingRectangle previousFinalRect;

        private bool isClippingRequired;
        private Vector2 unclippedSize;
        private Vector2 visualOffset;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        private bool visible = true;

        public float Width
        {
            get { return this.GetValue(WidthProperty); }
            set { this.SetValue(WidthProperty, value); }
        }

        public float Height
        {
            get { return this.GetValue(HeightProperty); }
            set { this.SetValue(HeightProperty, value); }
        }

        public float MaxHeight
        {
            get { return this.GetValue(MaxHeightProperty); }
            set { this.SetValue(MaxHeightProperty, value); }
        }

        public float MaxWidth
        {
            get { return this.GetValue(MaxWidthProperty); }
            set { this.SetValue(MaxWidthProperty, value); }
        }

        public float MinHeight
        {
            get { return this.GetValue(MinHeightProperty); }
            set { this.SetValue(MinHeightProperty, value); }
        }

        public float MinWidth
        {
            get { return this.GetValue(MinWidthProperty); }
            set { this.SetValue(MinWidthProperty, value); }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get { return this.GetValue(HorizontalAlignmentProperty); }
            set { this.SetValue(HorizontalAlignmentProperty, value); }
        }

        public VerticalAlignment VerticalAlignment
        {
            get { return this.GetValue(VerticalAlignmentProperty); }
            set { this.SetValue(VerticalAlignmentProperty, value); }
        }

        public Vector2 RenderSize { get; private set; }
        public Vector2 DesiredSize { get; private set; }

        public float ActualWidth
        {
            get { return this.RenderSize.X; }
        }

        public float ActualHeight
        {
            get { return this.RenderSize.Y; }
        }

        public BoundingRectangle? Clip
        {
            get { return this.clip; }
        }
        private BoundingRectangle? clip = null;

        public object DataContext
        {
            get { return this.GetValue(DataContextProperty); }
            set { this.SetValue(DataContextProperty, value); }
        }

        public bool IsMouseCaptured
        {
            get { return this.GetValue(IsMouseCapturedProperty); }
            private set { this.SetValue(IsMouseCapturedProperty, value); }
        }

        public Thickness Margin
        {
            get { return this.GetValue(MarginProperty); }
            set { this.SetValue(MarginProperty, value); }
        }

        // TODO: RenderTransform
        public Vector2 VisualOffset
        {
            get { return this.visualOffset; }
        }

        public UIElement Parent { get; set; }

        IContainer IComponent.Parent 
        {
            get { return Parent; }
            set { Parent = value as UIElement; }
        }
        #endregion

        public bool CaptureMouse()
        {
            Window rootElement;
            if (!this.IsMouseCaptured && this.TryGetRootElement(out rootElement))
            {
                this.IsMouseCaptured = rootElement.CaptureMouse(this);
            }
            return this.IsMouseCaptured;
        }

        public void ReleaseMouseCapture()
        {
            Window rootElement;
            if (this.IsMouseCaptured && this.TryGetRootElement(out rootElement))
            {
                rootElement.ReleaseMouseCapture(this);
                this.IsMouseCaptured = false;
            }
        }

        public virtual void OnApplyTemplate()
        {

        }
            
        public virtual IList<UIElement> GetChildren()
        {
            return null;
        }

        System.Collections.IList IContainer.Children 
        {
            get { return GetChildren() as System.Collections.IList; } 
        }

        #region Measure and Arrange
        public void Measure(Vector2 availableSize)
        {
            if (float.IsNaN(availableSize.X) || float.IsNaN(availableSize.Y))
            {
                throw new InvalidOperationException("AvailableSize X or Y cannot be NaN");
            }

            if (!this.isMeasureValid || availableSize.IsDifferentFrom(this.previousAvailableSize))
            {
                Vector2 size = this.MeasureCore(availableSize);

                if (float.IsPositiveInfinity(size.X) || float.IsPositiveInfinity(size.Y))
                {
                    throw new InvalidOperationException("The implementing element returned a PositiveInfinity");
                }

                if (float.IsNaN(size.X) || float.IsNaN(size.Y))
                {
                    throw new InvalidOperationException("The implementing element returned NaN");
                }

                this.previousAvailableSize = availableSize;
                this.isMeasureValid = true;
                this.DesiredSize = size;
            }
        }

        /// <summary>
        ///     Implements basic measure-pass layout system behavior.
        /// </summary>
        /// <remarks>
        ///     In WPF this method is definded on UIElement as protected virtual and returns an empty Size.
        ///     FrameworkElement (which derrives from UIElement) then creates a sealed implementation similar to the below.
        ///     In XPF UIElement and FrameworkElement have been collapsed into a single class.
        /// </remarks>
        /// <param name = "availableSize">The available size that the parent element can give to the child elements.</param>
        /// <returns>The desired size of this element in layout.</returns>
        private Vector2 MeasureCore(Vector2 availableSize)
        {
            // TODO:
            //this.ResolveDeferredBindings(this.GetNearestDataContext());
            this.OnApplyTemplate();

            Thickness margin = this.Margin;
            Vector2 availableSizeWithoutMargins = availableSize.Deflate(margin);

            var minMax = new MinMax(this);

            availableSizeWithoutMargins.X = MathHelper.Clamp(availableSizeWithoutMargins.X, minMax.MinWidth, minMax.MaxWidth);
            availableSizeWithoutMargins.Y = MathHelper.Clamp(availableSizeWithoutMargins.Y, minMax.MinHeight, minMax.MaxHeight);

            Vector2 size = this.MeasureOverride(availableSizeWithoutMargins);
            size.X = Math.Max(size.X, minMax.MinWidth);
            size.Y = Math.Max(size.Y, minMax.MinHeight);

            Vector2 unclippedSize = size;
            isClippingRequired = false;

            if (size.X > minMax.MaxWidth)
            {
                size.X = minMax.MaxWidth;
                isClippingRequired = true;
            }

            if (size.Y > minMax.MaxHeight)
            {
                size.Y = minMax.MaxHeight;
                isClippingRequired = true;
            }

            Vector2 desiredSizeWithMargins = size.Inflate(margin);

            if (desiredSizeWithMargins.X > availableSize.X)
            {
                desiredSizeWithMargins.X = availableSize.X;
                isClippingRequired = true;
            }

            if (desiredSizeWithMargins.Y > availableSize.Y)
            {
                desiredSizeWithMargins.Y = availableSize.Y;
                isClippingRequired = true;
            }

            this.unclippedSize = isClippingRequired ? unclippedSize : Vector2.Zero;
            return desiredSizeWithMargins;
        }

        /// <summary>
        ///     When overridden in a derived class, measures the size in layout required for child elements and determines a size for the UIElement-derived class.
        /// </summary>
        /// <param name = "availableSize">
        ///     The available size that this element can give to child elements.
        ///     Infinity can be specified as a value to indicate that the element will size to whatever content is available.
        /// </param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected virtual Vector2 MeasureOverride(Vector2 availableSize)
        {
            return Vector2.Zero;
        }

        public void Arrange(BoundingRectangle finalRect)
        {
            if (float.IsNaN(finalRect.Width) || float.IsNaN(finalRect.Height))
            {
                throw new InvalidOperationException("X and Y must be numbers");
            }

            if (float.IsPositiveInfinity(finalRect.Width) || float.IsPositiveInfinity(finalRect.Height))
            {
                throw new InvalidOperationException("X and Y must be less than infinity");
            }

            if (!this.isArrangeValid || finalRect.IsDifferentFrom(this.previousFinalRect))
            {
                IRenderer renderer;
                IDrawingContext drawingContext = null;

                bool hasRenderer = this.TryGetRenderer(out renderer);
                if (hasRenderer)
                {
                    drawingContext = renderer.GetDrawingContext(this);
                }

                this.ArrangeCore(finalRect);
                this.clip = this.GetClippingRect(finalRect.Size);

                if (hasRenderer && drawingContext != null)
                {
                    this.OnRender(drawingContext);
                }

                this.previousFinalRect = finalRect;
                this.isArrangeValid = true;
            }
        }

        /// <summary>
        ///     Defines the template for core-level arrange layout definition.
        /// </summary>
        /// <remarks>
        ///     In WPF this method is defined on UIElement as protected virtual and has a base implementation.
        ///     FrameworkElement (which derrives from UIElement) creates a sealed implemention, similar to the below,
        ///     which discards UIElement's base implementation.
        /// </remarks>
        /// <param name = "finalRect">The final area within the parent that element should use to arrange itself and its child elements.</param>
        private void ArrangeCore(BoundingRectangle finalRect)
        {
            Thickness margin = this.Margin;

            Vector2 unclippedDesiredSize = isClippingRequired ? unclippedSize : DesiredSize.Deflate(margin);
            Vector2 finalSize = new Vector2(finalRect.Width, finalRect.Height);
            finalSize = finalSize.Deflate(margin);

            isClippingRequired = false;

            if (finalSize.X < unclippedDesiredSize.X)
            {
                isClippingRequired = true;
                finalSize.X = unclippedDesiredSize.X;
            }

            if (finalSize.Y < unclippedDesiredSize.Y)
            {
                isClippingRequired = true;
                finalSize.Y = unclippedDesiredSize.Y;
            }

            if (HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                finalSize.X = unclippedDesiredSize.X;
            }

            if (VerticalAlignment != VerticalAlignment.Stretch)
            {
                finalSize.Y = unclippedDesiredSize.Y;
            }

            var minMax = new MinMax(this);

            float largestWidth = Math.Max(unclippedDesiredSize.X, minMax.MaxWidth);
            if (largestWidth < finalSize.X)
            {
                finalSize.X = largestWidth;
            }

            float largestHeight = Math.Max(unclippedDesiredSize.Y, minMax.MaxHeight);
            if (largestHeight < finalSize.Y)
            {
                finalSize.Y = largestHeight;
            }

            Vector2 renderSize = this.ArrangeOverride(finalSize);
            this.RenderSize = renderSize;

            Vector2 inkSize = new Vector2(
                Math.Min(renderSize.X, minMax.MaxWidth), Math.Min(renderSize.Y, minMax.MaxHeight));

            isClippingRequired |= inkSize.X < renderSize.X || inkSize.Y < renderSize.Y;

            Vector2 clientSize = finalRect.Size.Deflate(margin);

            isClippingRequired |= clientSize.X < inkSize.X || clientSize.Y < inkSize.Y;

            Vector2 offset = this.ComputeAlignmentOffset(clientSize, inkSize);
            offset.X += finalRect.X + margin.Left;
            offset.Y += finalRect.Y + margin.Top;

            this.visualOffset = offset;
        }

        /// <summary>
        ///     When overridden in a derived class, positions child elements and determines a size for a UIElement derived class.
        /// </summary>
        /// <param name = "finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected virtual Vector2 ArrangeOverride(Vector2 finalSize)
        {
            return finalSize;
        }

        public void InvalidateArrange()
        {
            var visualParent = this;
            while (visualParent != null)
            {
                visualParent.isArrangeValid = false;
                visualParent = visualParent.Parent;
            }
        }

        public void InvalidateMeasure()
        {
            var visualParent = this;
            while (visualParent != null)
            {
                visualParent.isMeasureValid = false;
                visualParent.isArrangeValid = false;
                visualParent = visualParent.Parent;
            }
        }
        #endregion

        public bool HitTest(Vector2 point)
        {
            Vector2 absoluteOffset = Vector2.Zero;
            UIElement currentElement = this;

            while (currentElement != null)
            {
                absoluteOffset += currentElement.VisualOffset;
                currentElement = currentElement.Parent;
            }

            var hitTestRect = new BoundingRectangle(absoluteOffset.X, absoluteOffset.Y, this.ActualWidth, this.ActualHeight);            
            return hitTestRect.Contains(point.X, point.Y) == ContainmentType.Contains;
        }

        public bool TryGetRenderer(out IRenderer renderer)
        {
            Window rootElement;
            if (this.TryGetRootElement(out rootElement))
            {
                renderer = null;
                return true;
            }

            renderer = null;
            return false;
        }

        // TODO: Cache visual root
        public bool TryGetRootElement(out Window rootElement)
        {
            var element = this as Window;
            if (element != null)
            {
                rootElement = element;
                return true;
            }

            if (this.Parent != null)
            {
                return this.Parent.TryGetRootElement(out rootElement);
            }

            rootElement = null;
            return false;
        }

        protected virtual BoundingRectangle? GetClippingRect(Vector2 finalSize)
        {
            if (!this.isClippingRequired)
            {
                return null;
            }

            var max = new MinMax(this);
            Vector2 renderSize = this.RenderSize;

            float maxWidth = float.IsPositiveInfinity(max.MaxWidth) ? renderSize.X : max.MaxWidth;
            float maxHeight = float.IsPositiveInfinity(max.MaxHeight) ? renderSize.Y : max.MaxHeight;

            bool isClippingRequiredDueToMaxSize = maxWidth.IsLessThan(renderSize.X) ||
                                                  maxHeight.IsLessThan(renderSize.Y);

            renderSize.X = Math.Min(renderSize.X, max.MaxWidth);
            renderSize.Y = Math.Min(renderSize.Y, max.MaxHeight);

            Thickness margin = this.Margin;
            float horizontalMargins = margin.Left + margin.Right;
            float verticalMargins = margin.Top + margin.Bottom;

            var clientSize = new Vector2(
                (finalSize.X - horizontalMargins).EnsurePositive(), 
                (finalSize.Y - verticalMargins).EnsurePositive());

            bool isClippingRequiredDueToClientSize = clientSize.X.IsLessThan(renderSize.X) ||
                                                     clientSize.Y.IsLessThan(renderSize.Y);

            if (isClippingRequiredDueToMaxSize && !isClippingRequiredDueToClientSize)
            {
                return new BoundingRectangle(0f, 0f, maxWidth, maxHeight);
            }

            if (!isClippingRequiredDueToClientSize)
            {
                return BoundingRectangle.Empty;
            }

            Vector2 offset = this.ComputeAlignmentOffset(clientSize, renderSize);

            var clipRect = new BoundingRectangle(-offset.X, -offset.Y, clientSize.X, clientSize.Y);

            if (isClippingRequiredDueToMaxSize)
            {
                //clipRect.Intersect(new BoundingRectangle(0f, 0f, maxWidth, maxHeight));
            }
            
            return clipRect;
        }

        internal void NotifyGesture(Gesture gesture)
        {
            OnNextGesture(gesture);
        }

        protected virtual void OnNextGesture(Gesture gesture)
        {
        }

        protected virtual void OnRender(IDrawingContext drawingContext)
        {
        }

        private static void DataContextChanged(ReactiveObject source, ReactivePropertyChangeEventArgs<object> args)
        {
            ((UIElement)source).InvalidateMeasureOnDataContextInheritors();
        }

        private Vector2 ComputeAlignmentOffset(Vector2 clientSize, Vector2 inkSize)
        {
            var vector = new Vector2();
            HorizontalAlignment horizontalAlignment = this.HorizontalAlignment;
            VerticalAlignment verticalAlignment = this.VerticalAlignment;

            if (horizontalAlignment == HorizontalAlignment.Stretch && inkSize.X > clientSize.X)
            {
                horizontalAlignment = HorizontalAlignment.Left;
            }

            if (verticalAlignment == VerticalAlignment.Stretch && inkSize.Y > clientSize.Y)
            {
                verticalAlignment = VerticalAlignment.Top;
            }

            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    vector.X = (clientSize.X - inkSize.X) * 0.5f;
                    break;
                case HorizontalAlignment.Left:
                    vector.X = 0;
                    break;
                case HorizontalAlignment.Right:
                    vector.X = clientSize.X - inkSize.X;
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    vector.Y = (clientSize.Y - inkSize.Y) * 0.5f;
                    return vector;
                case VerticalAlignment.Bottom:
                    vector.Y = clientSize.Y - inkSize.Y;
                    return vector;
                case VerticalAlignment.Top:
                    vector.Y = 0;
                    break;
            }

            return vector;
        }

        private object GetNearestDataContext()
        {
            UIElement curentElement = this;
            object dataContext;

            do
            {
                dataContext = curentElement.DataContext;
                curentElement = curentElement.Parent;
            }
            while (dataContext == null && curentElement != null);

            return dataContext;
        }

        private void InvalidateMeasureOnDataContextInheritors()
        {
            IEnumerable<UIElement> children = this.GetChildren();
            if (children.Count() == 0)
            {
                this.InvalidateMeasure();
            }
            else
            {
                IEnumerable<UIElement> childrenInheritingDataContext =
                    children.OfType<UIElement>().Where(element => element.DataContext == null);

                foreach (UIElement element in childrenInheritingDataContext)
                {
                    element.InvalidateMeasureOnDataContextInheritors();
                }
            }
        }
    }
}

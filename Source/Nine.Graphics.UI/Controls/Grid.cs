#region License
/* The MIT License
 *
 * Copyright (c) 2013 Engine Nine
 * Copyright (c) 2011 Red Badger Consulting
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

namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xaml;

    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Internal;

    /// <summary>
    /// A Grid layout panel consisting of columns and rows.
    /// </summary>
    public class Grid : Panel
    {
        private enum Dimension { X, Y }
        private enum UpdateMinLengths { SkipHeights, SkipWidths, WidthsAndHeights }

        private readonly bool[] hasAuto = new bool[2];
        private readonly bool[] hasStar = new bool[2];

        private readonly LinkedList<Cell> noStars = new LinkedList<Cell>();
        private readonly LinkedList<Cell> allStars = new LinkedList<Cell>();
        private readonly LinkedList<Cell> starHeightAutoPixelWidth = new LinkedList<Cell>();
        private readonly LinkedList<Cell> autoPixelHeightStarWidth = new LinkedList<Cell>();
        private readonly IList<ColumnDefinition> columnDefinitions = new List<ColumnDefinition>();
        private readonly IList<RowDefinition> rowDefinitions = new List<RowDefinition>();

        #region Properties

        /// <summary>
        /// Gets the collection of column definitions.
        /// </summary>
        /// <value>The column definitions collection.</value>
        public IList<ColumnDefinition> ColumnDefinitions
        {
            get { return this.columnDefinitions; }
        }

        /// <summary>
        /// Gets the collection of row definitions.
        /// </summary>
        /// <value>The row definitions collection.</value>
        public IList<RowDefinition> RowDefinitions
        {
            get { return this.rowDefinitions; }
        }

        public bool ShowGridLines { get; set; }

        #endregion

        #region Fields

        private Cell[] cells;
        private DefinitionBase[] columns;
        private DefinitionBase[] rows;

        #endregion

        #region Methods

        protected internal override void OnRender(Graphics.Primitives.DynamicPrimitive dynamicPrimitive)
        {
            base.OnRender(dynamicPrimitive);
            // TODO: Render GridLines
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            SetFinalLength(this.columns, finalSize.X);
            SetFinalLength(this.rows, finalSize.Y);

            if (this.cells == null)
                return finalSize;

            for (int i = 0; i < this.cells.Length; i++)
            {
                UIElement child = this.Children[i];
                if (child != null)
                {
                    int columnIndex = this.cells[i].ColumnIndex;
                    int rowIndex = this.cells[i].RowIndex;

                    var finalRect = new BoundingRectangle(
                        this.columns[columnIndex].FinalOffset,
                        this.rows[rowIndex].FinalOffset,
                        this.columns[columnIndex].FinalLength,
                        this.rows[rowIndex].FinalLength);

                    child.Arrange(finalRect);
                }
            }

            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            this.columns = this.columnDefinitions.Count == 0
                               ? new DefinitionBase[] { new ColumnDefinition() }
                               : this.columnDefinitions.ToArray();

            this.rows = this.rowDefinitions.Count == 0
                            ? new DefinitionBase[] { new RowDefinition() }
                            : this.rowDefinitions.ToArray();

            bool treatStarAsAutoWidth = float.IsPositiveInfinity(availableSize.X);
            bool treatStarAsAutoHeight = float.IsPositiveInfinity(availableSize.Y);

            this.InitializeMeasureData(this.columns, treatStarAsAutoWidth, Dimension.X);
            this.InitializeMeasureData(this.rows, treatStarAsAutoHeight, Dimension.Y);

            this.CreateCells();
            this.MeasureCells(availableSize);

            return new Vector2(
                this.columns.Sum(definition => definition.MinLength), 
                this.rows.Sum(definition => definition.MinLength));
        }

        private void CreateCells()
        {
            this.cells = new Cell[this.Children.Count];
            this.noStars.Clear();
            this.autoPixelHeightStarWidth.Clear();
            this.starHeightAutoPixelWidth.Clear();
            this.allStars.Clear();

            int i = 0;
            foreach (UIElement child in this.Children)
            {
                if (child != null)
                {
                    int columnIndex = Math.Min(GetColumn(child), this.columns.Length - 1);
                    int rowIndex = Math.Min(GetRow(child), this.rows.Length - 1);
                    var cell = new Cell
                    {
                        ColumnIndex = columnIndex,
                        RowIndex = rowIndex,
                        Child = child,
                        WidthType = this.columns[columnIndex].LengthType,
                        HeightType = this.rows[rowIndex].LengthType,
                    };

                    if (cell.HeightType == GridUnitType.Star)
                    {
                        if (cell.WidthType == GridUnitType.Star)
                        {
                            this.allStars.AddLast(cell);
                        }
                        else
                        {
                            this.starHeightAutoPixelWidth.AddLast(cell);
                        }
                    }
                    else
                    {
                        if (cell.WidthType == GridUnitType.Star)
                        {
                            this.autoPixelHeightStarWidth.AddLast(cell);
                        }
                        else
                        {
                            this.noStars.AddLast(cell);
                        }
                    }

                    this.cells[i] = cell;
                }

                i++;
            }
        }

        private void InitializeMeasureData(
            IEnumerable<DefinitionBase> definitions, bool treatStarAsAuto, Dimension dimension)
        {
            foreach (DefinitionBase definition in definitions)
            {
                definition.MinLength = 0f;
                float availableLength = 0f;
                float userMinLength = definition.UserMinLength;
                float userMaxLength = definition.UserMaxLength;

                switch (definition.UserLength.GridUnitType)
                {
                    case GridUnitType.Auto:
                        definition.LengthType = GridUnitType.Auto;
                        availableLength = float.PositiveInfinity;
                        this.hasAuto[(int)dimension] = true;
                        break;
                    case GridUnitType.Pixel:
                        definition.LengthType = GridUnitType.Pixel;
                        availableLength = definition.UserLength.Value;
                        userMinLength = MathHelper.Clamp(availableLength, userMinLength, userMaxLength);
                        break;
                    case GridUnitType.Star:
                        definition.LengthType = treatStarAsAuto ? GridUnitType.Auto : GridUnitType.Star;
                        availableLength = float.PositiveInfinity;
                        this.hasStar[(int)dimension] = true;
                        break;
                }

                definition.UpdateMinLength(userMinLength);
                definition.AvailableLength = MathHelper.Clamp(availableLength, userMinLength, userMaxLength);
            }
        }

        private void MeasureCell(Cell cell, UIElement child, bool shouldChildBeMeasuredWithInfiniteHeight)
        {
            if (child != null)
            {
                float x = cell.WidthType == GridUnitType.Auto
                               ? float.PositiveInfinity
                               : this.columns[cell.ColumnIndex].AvailableLength;

                float y = cell.HeightType == GridUnitType.Auto || shouldChildBeMeasuredWithInfiniteHeight
                               ? float.PositiveInfinity
                               : this.rows[cell.RowIndex].AvailableLength;

                child.Measure(new Vector2(x, y));
            }
        }

        private void MeasureCells(Vector2 availableSize)
        {
            if (this.noStars.Count > 0)
            {
                this.MeasureCells(this.noStars, UpdateMinLengths.WidthsAndHeights);
            }

            if (!this.hasAuto[(int)Dimension.Y])
            {
                if (this.hasStar[(int)Dimension.Y])
                {
                    AllocateProportionalSpace(this.rows, availableSize.Y);
                }

                this.MeasureCells(this.starHeightAutoPixelWidth, UpdateMinLengths.WidthsAndHeights);

                if (this.hasStar[(int)Dimension.X])
                {
                    AllocateProportionalSpace(this.columns, availableSize.X);
                }

                this.MeasureCells(this.autoPixelHeightStarWidth, UpdateMinLengths.WidthsAndHeights);
            }
            else if (!this.hasAuto[(int)Dimension.X])
            {
                if (this.hasStar[(int)Dimension.X])
                {
                    AllocateProportionalSpace(this.columns, availableSize.X);
                }

                this.MeasureCells(this.autoPixelHeightStarWidth, UpdateMinLengths.WidthsAndHeights);

                if (this.hasStar[(int)Dimension.Y])
                {
                    AllocateProportionalSpace(this.rows, availableSize.Y);
                }

                this.MeasureCells(this.starHeightAutoPixelWidth, UpdateMinLengths.WidthsAndHeights);
            }
            else
            {
                this.MeasureCells(this.starHeightAutoPixelWidth, UpdateMinLengths.SkipHeights);

                if (this.hasStar[(int)Dimension.X])
                {
                    AllocateProportionalSpace(this.columns, availableSize.X);
                }

                this.MeasureCells(this.autoPixelHeightStarWidth, UpdateMinLengths.WidthsAndHeights);

                if (this.hasStar[(int)Dimension.Y])
                {
                    AllocateProportionalSpace(this.rows, availableSize.Y);
                }

                this.MeasureCells(this.starHeightAutoPixelWidth, UpdateMinLengths.SkipWidths);
            }

            if (this.allStars.Count > 0)
            {
                this.MeasureCells(this.allStars, UpdateMinLengths.WidthsAndHeights);
            }
        }

        private void MeasureCells(IEnumerable<Cell> cells, UpdateMinLengths updateMinLengths)
        {
            foreach (Cell cell in cells)
            {
                bool shouldChildBeMeasuredWithInfiniteHeight = updateMinLengths == UpdateMinLengths.SkipHeights;

                this.MeasureCell(cell, cell.Child, shouldChildBeMeasuredWithInfiniteHeight);

                if (updateMinLengths != UpdateMinLengths.SkipWidths)
                {
                    DefinitionBase widthDefinition = this.columns[cell.ColumnIndex];
                    widthDefinition.UpdateMinLength(
                        Math.Min(cell.Child.DesiredSize.X, widthDefinition.UserMaxLength));
                }

                if (updateMinLengths != UpdateMinLengths.SkipHeights)
                {
                    DefinitionBase heightDefinition = this.rows[cell.RowIndex];
                    heightDefinition.UpdateMinLength(
                        Math.Min(cell.Child.DesiredSize.Y, heightDefinition.UserMaxLength));
                }
            }
        }

        #endregion

        #region Static Methods

        static readonly AttachableMemberIdentifier RowMember = new AttachableMemberIdentifier(typeof(int), "Row");
        static readonly AttachableMemberIdentifier ColumnMember = new AttachableMemberIdentifier(typeof(int), "Column");

        /// <summary>
        /// Gets the value of the Column attached property for the specified element.
        /// </summary>
        /// <param name="element">The element for which to read the proerty value.</param>
        /// <returns>The value of the Column attached property.</returns>
        public static int GetColumn(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.AttachedProperties.ContainsKey(ColumnMember))
                return (int)element.AttachedProperties[ColumnMember];
            else
                return 0; // Default Value
        }

        /// <summary>
        /// Gets the value of the Row attached property for the specified element.
        /// </summary>
        /// <param name="element">The element for which to read the proerty value.</param>
        /// <returns>The value of the Row attached property.</returns>
        public static int GetRow(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.AttachedProperties.ContainsKey(RowMember))
                return (int)element.AttachedProperties[RowMember];
            else
                return 0; // Default Value
        }

        /// <summary>
        /// Sets the value of the Column attached property for the specified element.
        /// </summary>
        /// <param name="element">The element for which to write the proerty value.</param>
        /// <param name="value">The value of the Column attached property.</param>
        public static void SetColumn(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[ColumnMember] = value;
        }

        /// <summary>
        /// Sets the value of the Row attached property for the specified element.
        /// </summary>
        /// <param name="element">The element for which to write the proerty value.</param>
        /// <param name="value">The value of the Row attached property.</param>
        public static void SetRow(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.AttachedProperties[RowMember] = value;
        }

        private static void AllocateProportionalSpace(IEnumerable<DefinitionBase> definitions, float availableLength)
        {
            float occupiedLength = 0f;

            var stars = new LinkedList<DefinitionBase>();

            foreach (DefinitionBase definition in definitions)
            {
                switch (definition.LengthType)
                {
                    case GridUnitType.Auto:
                        occupiedLength += definition.MinLength;
                        break;
                    case GridUnitType.Pixel:
                        occupiedLength += definition.AvailableLength;
                        break;
                    case GridUnitType.Star:
                        float numerator = definition.UserLength.Value;
                        if (numerator.IsCloseTo(0f))
                        {
                            definition.Numerator = 0f;
                            definition.StarAllocationOrder = 0f;
                        }
                        else
                        {
                            definition.Numerator = numerator;
                            definition.StarAllocationOrder = Math.Max(definition.MinLength, definition.UserMaxLength) /
                                                             numerator;
                        }

                        stars.AddLast(definition);
                        break;
                }
            }

            if (stars.Count > 0)
            {
                DefinitionBase[] sortedStars = stars.OrderBy(o => o.StarAllocationOrder).ToArray();

                float denominator = 0f;
                foreach (DefinitionBase definition in sortedStars.Reverse())
                {
                    denominator += definition.Numerator;
                    definition.Denominator = denominator;
                }

                foreach (DefinitionBase definition in sortedStars)
                {
                    float length;
                    if (definition.Numerator.IsCloseTo(0f))
                    {
                        length = definition.MinLength;
                    }
                    else
                    {
                        float remainingLength = (availableLength - occupiedLength).EnsurePositive();
                        length = remainingLength * (definition.Numerator / definition.Denominator);
                        length = MathHelper.Clamp(length, definition.MinLength, definition.UserMaxLength);
                    }

                    occupiedLength += length;
                    definition.AvailableLength = length;
                }
            }
        }

        private static void SetFinalLength(DefinitionBase[] definitions, float gridFinalLength)
        {
            if (definitions == null)
                return;

            float occupiedLength = 0f;

            var stars = new LinkedList<DefinitionBase>();
            var nonStarDefinitions = new LinkedList<DefinitionBase>();

            foreach (DefinitionBase definition in definitions)
            {
                float minLength;

                switch (definition.UserLength.GridUnitType)
                {
                    case GridUnitType.Auto:
                        minLength = definition.MinLength;

                        definition.FinalLength = MathHelper.Clamp(minLength, definition.MinLength, definition.UserMaxLength);

                        occupiedLength += definition.FinalLength;
                        nonStarDefinitions.AddFirst(definition);
                        break;
                    case GridUnitType.Pixel:
                        minLength = definition.UserLength.Value;

                        definition.FinalLength = MathHelper.Clamp(minLength, definition.MinLength, definition.UserMaxLength);

                        occupiedLength += definition.FinalLength;
                        nonStarDefinitions.AddFirst(definition);
                        break;
                    case GridUnitType.Star:
                        float numerator = definition.UserLength.Value;
                        if (numerator.IsCloseTo(0f))
                        {
                            definition.Numerator = 0f;
                            definition.StarAllocationOrder = 0f;
                        }
                        else
                        {
                            definition.Numerator = numerator;
                            definition.StarAllocationOrder = Math.Max(definition.MinLength, definition.UserMaxLength) /
                                                             numerator;
                        }

                        stars.AddLast(definition);
                        break;
                }
            }

            if (stars.Count > 0)
            {
                DefinitionBase[] sortedStars = stars.OrderBy(o => o.StarAllocationOrder).ToArray();

                float denominator = 0f;
                foreach (DefinitionBase definitionBase in sortedStars.Reverse())
                {
                    denominator += definitionBase.Numerator;
                    definitionBase.Denominator = denominator;
                }

                foreach (DefinitionBase definition in sortedStars)
                {
                    float length;
                    if (definition.Numerator.IsCloseTo(0f))
                    {
                        length = definition.MinLength;
                    }
                    else
                    {
                        float remainingLength = (gridFinalLength - occupiedLength).EnsurePositive();
                        length = remainingLength * (definition.Numerator / definition.Denominator);
                        length = MathHelper.Clamp(length, definition.MinLength, definition.UserMaxLength);
                    }

                    occupiedLength += length;
                    definition.FinalLength = length;
                }
            }

            if (occupiedLength.IsGreaterThan(gridFinalLength))
            {
                IOrderedEnumerable<DefinitionBase> sortedDefinitions =
                    stars.Concat(nonStarDefinitions).OrderBy(o => o.FinalLength - o.MinLength);

                float excessLength = occupiedLength - gridFinalLength;
                int i = 0;
                foreach (DefinitionBase definitionBase in sortedDefinitions)
                {
                    float finalLength = definitionBase.FinalLength - (excessLength / (definitions.Length - i));
                    finalLength = MathHelper.Clamp(finalLength, definitionBase.MinLength, definitionBase.FinalLength);
                    excessLength -= definitionBase.FinalLength - finalLength;

                    definitionBase.FinalLength = finalLength;
                    i++;
                }
            }

            definitions[0].FinalOffset = 0f;
            for (int i = 1; i < definitions.Length; i++)
            {
                DefinitionBase previousDefinition = definitions[i - 1];
                definitions[i].FinalOffset = previousDefinition.FinalOffset + previousDefinition.FinalLength;
            }
        }

        #endregion

        private struct Cell
        {
            public UIElement Child;
            public int ColumnIndex;
            public GridUnitType HeightType;
            public int RowIndex;
            public GridUnitType WidthType;
        }
    }
}
